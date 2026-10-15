using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairPro.App.Localization;

namespace CrosshairPro.App.ViewModels;

/// <summary>
/// 电脑优化条目（仅提供说明与对应系统管理页的快捷入口，所有更改由用户手动完成，
/// 本 ViewModel 不写入/修改任何系统设置）
/// </summary>
public sealed class PcOptimizeItem
{
    /// <summary>条目标识（本地化键 = PcOptimize.{Id} / PcOptimize.{Id}Desc）</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>标题本地化键（PcOptimize.{Id}）</summary>
    public string TitleKey => $"PcOptimize.{Id}";

    /// <summary>说明本地化键（PcOptimize.{Id}Desc）</summary>
    public string DescKey => $"PcOptimize.{Id}Desc";

    /// <summary>期望收益本地化键（PcOptimize.GainHigh / GainMedium / GainLow / GainVaries）</summary>
    public string GainKey { get; init; } = string.Empty;

    /// <summary>
    /// 打开目标：exe/cpl/msc 文件名或 ms-settings: URI（UseShellExecute 按名解析）。
    /// 注意 ShellExecute 单字符串模式不拆分空格参数，带参控制面板须拆为
    /// FileName="control" + <see cref="OpenArguments"/>（如 "powercfg.cpl,,3"）。
    /// </summary>
    public string OpenTarget { get; init; } = string.Empty;

    /// <summary>控制面板程序参数（<see cref="OpenTarget"/> 为 control 时使用）</summary>
    public string OpenArguments { get; init; } = string.Empty;

    /// <summary>
    /// 候选可执行文件全路径探测（显卡厂商面板如 nvcplui.exe 不在 PATH，
    /// 只能按全路径启动；非空时逐个探测 File.Exists，命中才打开）
    /// </summary>
    public string[] ProbePaths { get; init; } = [];

    /// <summary>是否存在快捷入口（决定「打开」按钮可见性）</summary>
    public bool HasEntry => !string.IsNullOrEmpty(OpenTarget) || ProbePaths.Length > 0;
}

/// <summary>电脑优化分类（卡片标题 + 条目列表）</summary>
public sealed class PcOptimizeCategory
{
    /// <summary>分类标题本地化键（PcOptimize.Cat.*）</summary>
    public string TitleKey { get; init; } = string.Empty;

    public ObservableCollection<PcOptimizeItem> Items { get; } = new();

    public PcOptimizeCategory(string titleKey) => TitleKey = titleKey;
}

/// <summary>
/// 电脑优化页 ViewModel：静态定义优化项清单（含 Windows 管理页打开命令），
/// 不依赖任何服务、无持久化。设计依据（来源引用与验证记录）见
/// design/pc-optimize-design.md。
/// </summary>
public partial class PcOptimizeViewModel : ObservableObject
{
    public ObservableCollection<PcOptimizeCategory> Categories { get; } = new();

    /// <summary>Toast 通知事件，页面层订阅后渲染</summary>
    public event EventHandler<string>? ToastRequested;

    public PcOptimizeViewModel()
    {
        Categories.Add(NewCat("PcOptimize.Cat.System",
            Item("GameMode", "PcOptimize.GainVaries", "ms-settings:gaming-gamemode"),
            Item("GameBar", "PcOptimize.GainLow", "ms-settings:gaming-gamebar"),
            Item("BackgroundRecording", "PcOptimize.GainMedium", "ms-settings:gaming-gamedvr"),
            Item("StartupApps", "PcOptimize.GainMedium", "ms-settings:startupapps"),
            Item("NotificationsDnd", "PcOptimize.GainMedium", "ms-settings:notifications"),
            Item("MemoryIntegrity", "PcOptimize.GainVaries", "windowsdefender://coreisolation"),
            Item("UpdateActiveHours", "PcOptimize.GainLow", "ms-settings:windowsupdate-activehours"),
            Item("UninstallUnusedApps", "PcOptimize.GainVaries", "ms-settings:appsfeatures")));

        Categories.Add(NewCat("PcOptimize.Cat.Power",
            Item("PowerPlan", "PcOptimize.GainMedium", "powercfg.cpl"),
            // 高级电源设置直达：单字符串 "control powercfg.cpl,,3" 会被整体当文件名解析，必须拆参
            Item("UsbSelectiveSuspend", "PcOptimize.GainVaries", "control", "powercfg.cpl,,3"),
            Item("PcieLinkStatePm", "PcOptimize.GainLow", "control", "powercfg.cpl,,3"),
            Item("PowerSleepTimeouts", "PcOptimize.GainLow", "ms-settings:powersleep")));

        Categories.Add(NewCat("PcOptimize.Cat.Display",
            Item("GraphicsHags", "PcOptimize.GainVaries", "ms-settings:display-advancedgraphics-default"),
            Item("FullscreenOptimizations", "PcOptimize.GainVaries", "ms-settings:display-advancedgraphics"),
            Item("RefreshRateMax", "PcOptimize.GainHigh", "ms-settings:display-advanced"),
            Item("HdrSettings", "PcOptimize.GainVaries", "ms-settings:display-hdr"),
            Item("DisplayScaling", "PcOptimize.GainVaries", "ms-settings:display"),
            Item("VisualEffects", "PcOptimize.GainLow", "SystemPropertiesPerformance.exe")));

        Categories.Add(NewCat("PcOptimize.Cat.Network",
            Item("NicPowerManagement", "PcOptimize.GainVaries", "ncpa.cpl"),
            Item("ProxyVpn", "PcOptimize.GainVaries", "ms-settings:network-proxy"),
            Item("DeliveryOptimization", "PcOptimize.GainLow", "ms-settings:delivery-optimization")));

        Categories.Add(NewCat("PcOptimize.Cat.Storage",
            Item("StorageSense", "PcOptimize.GainLow", "ms-settings:storagesense"),
            Item("OptimizeDrives", "PcOptimize.GainLow", "dfrgui.exe"),
            Item("GameInstallSsd", "PcOptimize.GainHigh", "ms-settings:savelocations")));

        Categories.Add(NewCat("PcOptimize.Cat.Driver",
            Item("GpuDriverUpdate", "PcOptimize.GainMedium", "devmgmt.msc"),
            Item("OptionalDriverUpdates", "PcOptimize.GainLow", "ms-settings:windowsupdate-optionalupdates"),
            new PcOptimizeItem
            {
                Id = "VendorGpuPanel",
                GainKey = "PcOptimize.GainMedium",
                // NVIDIA/AMD 面板 exe 不在 PATH，必须按全路径探测启动；均未命中时提示从开始菜单打开
                ProbePaths =
                [
                    @"C:\Program Files\NVIDIA Corporation\Control Panel Client\nvcplui.exe",
                    @"C:\Program Files\AMD\CNext\CNext\RadeonSoftware.exe"
                ]
            }));

        Categories.Add(NewCat("PcOptimize.Cat.InputAudio",
            // 指针选项选项卡直达：同上拆参（main.cpl,,2）
            Item("PointerPrecision", "PcOptimize.GainMedium", "control", "main.cpl,,2"),
            Item("AudioEnhancements", "PcOptimize.GainLow", "mmsys.cpl")));
    }

    // ── 命令 ─────────────────────────────────────────────────────

    /// <summary>
    /// 打开条目对应的系统管理页（ms-settings URI / cpl / msc / exe）。
    /// 仅启动页面本身，不做任何更改；失败经 Toast 提示。
    /// </summary>
    [RelayCommand]
    private void OpenItem(PcOptimizeItem? item)
    {
        if (item == null || !item.HasEntry)
        {
            return;
        }

        try
        {
            // 厂商面板：按全路径探测，命中才启动
            if (item.ProbePaths.Length > 0)
            {
                var exePath = item.ProbePaths.FirstOrDefault(File.Exists);
                if (exePath == null)
                {
                    ToastRequested?.Invoke(this, LocalizationProvider.Get("PcOptimize.VendorNotFound"));
                    return;
                }
                Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                return;
            }

            Process.Start(new ProcessStartInfo(item.OpenTarget)
            {
                UseShellExecute = true,
                Arguments = item.OpenArguments
            });
        }
        catch (Exception ex)
        {
            var msg = LocalizationProvider.GetFormatted("PcOptimize.OpenFailed", ex.Message);
            ToastRequested?.Invoke(this, msg);
        }
    }

    // ── 工具方法 ─────────────────────────────────────────────────

    private static PcOptimizeCategory NewCat(string titleKey, params PcOptimizeItem[] items)
    {
        var cat = new PcOptimizeCategory(titleKey);
        foreach (var item in items)
        {
            cat.Items.Add(item);
        }
        return cat;
    }

    private static PcOptimizeItem Item(string id, string gainKey, string openTarget, string? openArguments = null)
        => new() { Id = id, GainKey = gainKey, OpenTarget = openTarget, OpenArguments = openArguments ?? string.Empty };
}
