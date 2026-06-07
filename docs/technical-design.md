# 系统技术设计文档

## 文档信息

**产品名称：** Crosshair Pro - 外置准心覆盖软件
**文档版本：** v1.0
**创建日期：** 2026-06-07
**状态：** 草稿

---

## 1. 技术选型

### 1.1 技术栈总览

| 层级 | 技术选择 | 版本 | 选择理由 |
|------|---------|------|---------|
| 前端框架 | WPF | .NET 8 | 原生Windows支持，透明窗口实现简单，内存占用低 |
| 后端框架 | .NET | 8.0 | 高性能，AOT编译支持，与WPF统一技术栈 |
| 覆盖层技术 | WPF Layered Window | - | 原生透明窗口支持，硬件加速渲染 |
| 配置存储 | JSON文件 | - | 易读易编辑，便于用户分享配置 |
| 热键系统 | Windows Raw Input API | - | 全局热键支持，游戏前台也能响应 |
| 日志框架 | Serilog | 4.x | 结构化日志，多输出目标支持 |
| DI容器 | Microsoft.Extensions.DependencyInjection | 8.x | .NET官方DI容器 |

### 1.2 技术选型详情

#### 1.2.1 前端框架选择：WPF + .NET 8

**选择理由：**
- WPF原生支持透明窗口（AllowsTransparency）、置顶窗口（Topmost属性）、分层窗口渲染
- .NET 8提供优秀的性能（AOT编译支持）和低内存占用
- WPF的XAML数据绑定机制简化UI开发，丰富的动画系统支持准心动画效果
- 相比Electron（内存100MB+），WPF应用内存占用可控制在30-50MB
- 相比WinForms，WPF的DirectX渲染管线更适合透明覆盖层
- 相比C++/Qt，开发效率更高且无需额外授权费用

**备选方案：**
| 方案 | 优点 | 缺点 | 结论 |
|------|------|------|------|
| Electron + React | 跨平台、开发效率高 | 内存占用大（100MB+）、启动慢 | 不采纳 |
| Qt + QML | 性能优秀、跨平台 | 商业授权费用、学习曲线陡峭 | 不采纳 |
| WinForms | 简单直接 | 透明窗口支持差、UI现代化困难 | 不采纳 |
| Flutter Windows | 现代UI、热重载 | Windows生态不成熟、系统集成困难 | 不采纳 |

#### 1.2.2 覆盖层技术方案：WPF Layered Window

**技术实现：**
```
窗口属性配置：
├─ WindowStyle: None          // 无边框
├─ AllowsTransparency: true   // 允许透明
├─ Topmost: true              // 始终置顶
├─ ShowInTaskbar: false       // 不显示任务栏
├─ ResizeMode: NoResize       // 不可调整大小
├─ Background: Transparent    // 透明背景
└─ IsHitTestVisible: false    // 鼠标点击穿透
```

**Windows扩展样式：**
- `WS_EX_LAYERED` - 分层窗口，支持透明度
- `WS_EX_TRANSPARENT` - 鼠标点击穿透
- `WS_EX_TOPMOST` - 置顶窗口
- `WS_EX_TOOLWINDOW` - 不在Alt+Tab中显示

**性能指标：**
- 渲染延迟：< 1ms
- CPU占用：< 0.5%
- 内存占用：< 30MB

#### 1.2.3 配置存储方案：JSON文件

**存储结构：**
```
%APPDATA%/CrosshairPro/
├── config.json           // 主配置文件
├── presets/              // 预设配置目录
│   ├── default.json
│   ├── cs2_pro.json
│   └── valorant_dot.json
├── custom_images/        // 自定义准心图片
└── logs/                 // 日志目录
    └── app.log
```

**选择理由：**
- 人类可读，便于调试和用户分享
- 导入导出便捷（直接复制文件）
- 无额外依赖，应用体积小
- 跨版本兼容性好（JSON Schema版本控制）

#### 1.2.4 热键系统方案：Windows Raw Input API + RegisterHotKey

**技术架构：**
```
┌─────────────────────────────────────────┐
│           HotkeyManager                 │
│  - 注册/注销热键                        │
│  - 解析快捷键字符串                    │
│  - 触发热键回调                        │
│  - 冲突检测                            │
└─────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│       Windows Raw Input API             │
│  (底层键盘输入捕获)                     │
└─────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────────────────┐
│          Win32 API                      │
│  RegisterHotKey / UnregisterHotKey      │
└─────────────────────────────────────────┘
```

**选择理由：**
- 全局响应，游戏前台也能响应
- 低延迟（< 100ms）
- 支持复杂组合键
- 支持冲突检测

---

## 2. 系统架构设计

### 2.1 架构模式：MVVM + 分层架构

**选择理由：**
1. MVVM与WPF天然契合：WPF的数据绑定机制、命令系统与MVVM模式完美配合
2. 分层架构职责清晰：表现层、业务逻辑层、服务层、基础设施层各司其职
3. 松耦合设计：通过依赖注入实现模块间解耦，支持单元测试

### 2.2 系统架构图

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           表现层 (Presentation)                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐        │
│  │   MainWindow    │  │  OverlayWindow  │  │  SettingsWindow │        │
│  │   (主界面)      │  │   (覆盖层)      │  │   (设置窗口)    │        │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘        │
│           │                    │                    │                  │
│  ┌────────┴────────────────────┴────────────────────┴────────┐        │
│  │                    ViewModels (视图模型层)                 │        │
│  │  MainViewModel | OverlayViewModel | SettingsViewModel      │        │
│  │  CrosshairDesignerViewModel | PresetManagementViewModel   │        │
│  └───────────────────────────────────────────────────────────┘        │
└───────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        业务逻辑层 (Application)                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                        Application Services                       │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────────┐ │   │
│  │  │ Crosshair   │ │   Preset    │ │   Hotkey    │ │   Game     │ │   │
│  │  │ Service     │ │  Service    │ │  Service    │ │ Detector   │ │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          服务层 (Services)                              │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                        Domain Services                            │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────────┐ │   │
│  │  │  Rendering  │ │   Config    │ │   Window    │ │   System   │ │   │
│  │  │  Engine     │ │  Manager    │ │  Manager    │ │   Tray     │ │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        基础设施层 (Infrastructure)                      │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐ │
│  │    Win32    │ │   File      │ │    DI       │ │    Logging      │ │
│  │   Wrapper   │ │    I/O      │ │  Container  │ │    Service      │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────────┘ │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                    │
│  │   Hotkey    │ │  Process    │ │   Monitor   │                    │
│  │    API      │ │   Monitor   │ │   Helper    │                    │
│  └─────────────┘ └─────────────┘ └─────────────┘                    │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.3 模块划分

```
CrosshairPro.sln
│
├── src/
│   ├── CrosshairPro.Core/                    # 核心业务模块
│   │   ├── Models/                           # 领域模型
│   │   │   ├── CrosshairConfig.cs
│   │   │   ├── CrosshairStyle.cs
│   │   │   ├── Preset.cs
│   │   │   ├── HotkeyBinding.cs
│   │   │   └── GameProfile.cs
│   │   ├── Enums/
│   │   ├── Events/
│   │   └── Interfaces/
│   │
│   ├── CrosshairPro.Services/                # 业务服务模块
│   │   ├── Crosshair/
│   │   ├── Configuration/
│   │   ├── Hotkey/
│   │   ├── GameDetection/
│   │   └── SystemTray/
│   │
│   ├── CrosshairPro.Infrastructure/          # 基础设施模块
│   │   ├── Win32/
│   │   ├── Hotkey/
│   │   ├── IO/
│   │   └── Logging/
│   │
│   └── CrosshairPro.App/                     # WPF应用模块
│       ├── Views/
│       ├── ViewModels/
│       ├── Converters/
│       └── App.xaml
│
└── tests/
    ├── CrosshairPro.Core.Tests/
    ├── CrosshairPro.Services.Tests/
    └── CrosshairPro.UI.Tests/
```

---

## 3. 核心模块详细设计

### 3.1 准心渲染模块

#### 3.1.1 类设计

```csharp
namespace CrosshairPro.Services.Crosshair;

/// <summary>
/// 准心渲染器 - 核心渲染引擎
/// </summary>
public sealed class CrosshairRenderer : ICrosshairRenderer, IDisposable
{
    private readonly ILogger _logger;
    private readonly IGeometryCache _geometryCache;
    private readonly IBrushCache _brushCache;

    // 渲染组件
    private DrawingContext? _drawingContext;
    private CrosshairConfig? _currentConfig;
    private DateTime _lastRenderTime;

    // 性能指标
    private readonly Queue<double> _renderTimes = new(100);
    private double _averageRenderTime;

    public event EventHandler<RenderCompletedEventArgs>? RenderCompleted;

    public CrosshairRenderer(
        ILogger logger,
        IGeometryCache geometryCache,
        IBrushCache brushCache)
    {
        _logger = logger;
        _geometryCache = geometryCache;
        _brushCache = brushCache;
    }

    /// <summary>
    /// 渲染准心
    /// </summary>
    public void Render(DrawingContext dc, CrosshairConfig config, Size size)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _drawingContext = dc;
            _currentConfig = config;

            var center = new Point(size.Width / 2, size.Height / 2);

            // 根据样式渲染
            switch (config.Style)
            {
                case CrosshairStyle.Cross:
                    RenderCross(center, config);
                    break;
                case CrosshairStyle.Dot:
                    RenderDot(center, config);
                    break;
                case CrosshairStyle.Circle:
                    RenderCircle(center, config);
                    break;
                case CrosshairStyle.TShape:
                    RenderTShape(center, config);
                    break;
                case CrosshairStyle.XShape:
                    RenderXShape(center, config);
                    break;
                case CrosshairStyle.CustomImage:
                    RenderCustomImage(center, config, size);
                    break;
            }

            stopwatch.Stop();
            RecordRenderTime(stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "准心渲染失败");
        }
    }

    /// <summary>
    /// 渲染十字准心
    /// </summary>
    private void RenderCross(Point center, CrosshairConfig config)
    {
        var pen = GetOrCreatePen(config);
        var halfLength = config.Size / 2.0;
        var halfGap = config.Gap / 2.0;

        // 四条线：上、下、左、右
        var lines = new[]
        {
            new Line(center.X, center.Y - halfGap, center.X, center.Y - halfGap - halfLength), // 上
            new Line(center.X, center.Y + halfGap, center.X, center.Y + halfGap + halfLength), // 下
            new Line(center.X - halfGap, center.Y, center.X - halfGap - halfLength, center.Y), // 左
            new Line(center.X + halfGap, center.Y, center.X + halfGap + halfLength, center.Y)  // 右
        };

        foreach (var line in lines)
        {
            // 绘制阴影（如果启用）
            if (config.Effects.Shadow.Enabled)
            {
                var shadowPen = GetOrCreateShadowPen(config.Effects.Shadow);
                _drawingContext!.DrawLine(shadowPen,
                    new Point(line.X1 + config.Effects.Shadow.OffsetX,
                             line.Y1 + config.Effects.Shadow.OffsetY),
                    new Point(line.X2 + config.Effects.Shadow.OffsetX,
                             line.Y2 + config.Effects.Shadow.OffsetY));
            }

            // 绘制描边（如果启用）
            if (config.Effects.Outline.Enabled)
            {
                var outlinePen = GetOrCreateOutlinePen(config.Effects.Outline);
                _drawingContext!.DrawLine(outlinePen,
                    new Point(line.X1, line.Y1),
                    new Point(line.X2, line.Y2));
            }

            // 绘制主线
            _drawingContext!.DrawLine(pen,
                new Point(line.X1, line.Y1),
                new Point(line.X2, line.Y2));
        }
    }

    /// <summary>
    /// 渲染点状准心
    /// </summary>
    private void RenderDot(Point center, CrosshairConfig config)
    {
        var brush = GetOrCreateBrush(config.Color, config.Opacity);
        var radius = config.CenterSize / 2.0;

        // 阴影
        if (config.Effects.Shadow.Enabled)
        {
            var shadowBrush = GetOrCreateBrush(
                config.Effects.Shadow.Color,
                config.Effects.Shadow.Opacity);

            _drawingContext!.DrawEllipse(shadowBrush, null,
                new Point(center.X + config.Effects.Shadow.OffsetX,
                         center.Y + config.Effects.Shadow.OffsetY),
                radius, radius);
        }

        // 描边
        if (config.Effects.Outline.Enabled)
        {
            var outlinePen = GetOrCreateOutlinePen(config.Effects.Outline);
            _drawingContext!.DrawEllipse(null, outlinePen, center, radius, radius);
        }

        // 填充
        _drawingContext!.DrawEllipse(brush, null, center, radius, radius);
    }

    /// <summary>
    /// 获取或创建画笔
    /// </summary>
    private Pen GetOrCreatePen(CrosshairConfig config)
    {
        var key = $"{config.Color}_{config.Thickness}_{config.Opacity}";
        return _penCache.GetOrCreate(key, () =>
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.Color));
            brush.Opacity = config.Opacity / 100.0;
            brush.Freeze();
            var pen = new Pen(brush, config.Thickness);
            pen.Freeze();
            return pen;
        });
    }
}
```

#### 3.1.2 渲染流程

```
用户调整参数
     │
     ▼
┌──────────────────┐
│ ViewModel更新    │
│ CrosshairConfig  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ 触发重新渲染     │
│ InvalidateVisual │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  OnRender调用    │
│  (WPF渲染管线)   │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│ CrosshairRenderer│
│ .Render()        │
└────────┬─────────┘
         │
         ├──────────────┬──────────────┐
         │              │              │
         ▼              ▼              ▼
    ┌─────────┐   ┌─────────┐   ┌─────────┐
    │ 获取缓存│   │ 计算几何│   │ 绘制效果│
    │ Pen/Brush│  │ 坐标点  │   │ 描边/阴影│
    └─────────┘   └─────────┘   └─────────┘
         │              │              │
         └──────────────┴──────────────┘
                        │
                        ▼
              ┌──────────────────┐
              │ DrawingContext   │
              │ 绘制到屏幕       │
              └──────────────────┘
```

#### 3.1.3 性能优化策略

| 策略 | 描述 | 效果 |
|------|------|------|
| Geometry缓存 | 缓存常用的几何图形对象 | 减少50%内存分配 |
| Brush缓存 | 缓存画笔和画刷对象 | 减少70%对象创建 |
| Freeze对象 | 冻结不可变的WPF对象 | 减少内存泄漏风险 |
| 延迟渲染 | 使用防抖机制，100ms内只渲染最后一次 | 减少无效渲染 |

---

### 3.2 配置管理模块

#### 3.2.1 数据模型

```csharp
namespace CrosshairPro.Core.Models;

/// <summary>
/// 准心配置
/// </summary>
public class CrosshairConfig : ObservableObject
{
    private string _id = Guid.NewGuid().ToString();
    private string _name = "默认配置";
    private CrosshairStyle _style = CrosshairStyle.Cross;
    private int _size = 20;
    private int _gap = 4;
    private int _thickness = 2;
    private string _color = "#00FF00";
    private int _opacity = 100;
    private int _centerSize = 4;
    private int _rotation = 0;
    private EffectsConfig _effects = new();
    private DisplayConfig _display = new();

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public CrosshairStyle Style
    {
        get => _style;
        set => SetProperty(ref _style, value);
    }

    public int Size
    {
        get => _size;
        set => SetProperty(ref _size, Math.Clamp(value, 1, 100));
    }

    public int Gap
    {
        get => _gap;
        set => SetProperty(ref _gap, Math.Clamp(value, 0, 50));
    }

    // ... 其他属性
}

/// <summary>
/// 效果配置
/// </summary>
public class EffectsConfig : ObservableObject
{
    private OutlineConfig _outline = new();
    private ShadowConfig _shadow = new();
    private GlowConfig _glow = new();
    private AnimationConfig _animation = new();

    public OutlineConfig Outline
    {
        get => _outline;
        set => SetProperty(ref _outline, value);
    }
    // ... 其他属性
}

/// <summary>
/// 预设配置
/// </summary>
public class Preset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "新预设";
    public CrosshairConfig Config { get; set; } = new();
    public string? GameAssociation { get; set; }
    public string? HotkeyBinding { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDefault { get; set; }
}
```

#### 3.2.2 配置服务接口

```csharp
namespace CrosshairPro.Services.Configuration;

/// <summary>
/// 配置服务接口
/// </summary>
public interface IConfigService
{
    Task<CrosshairConfig> LoadConfigAsync();
    Task SaveConfigAsync(CrosshairConfig config);
    Task ResetToDefaultAsync();
    Task ExportConfigAsync(string filePath);
    Task<CrosshairConfig> ImportConfigAsync(string filePath);
}

/// <summary>
/// 预设服务接口
/// </summary>
public interface IPresetService
{
    Task<IReadOnlyList<Preset>> LoadPresetsAsync();
    Task SavePresetAsync(Preset preset);
    Task DeletePresetAsync(string presetId);
    Task<Preset?> GetPresetAsync(string presetId);
    Task SwitchPresetAsync(string presetId);
    Task ExportPresetAsync(string presetId, string filePath);
    Task<Preset> ImportPresetAsync(string filePath);
    string GenerateShareCode(Preset preset);
    Preset ParseShareCode(string code);
}
```

#### 3.2.3 配置文件格式

```json
{
  "version": "1.0",
  "crosshair": {
    "style": "cross",
    "size": 20,
    "thickness": 2,
    "gap": 4,
    "opacity": 100,
    "color": "#00FF00",
    "centerSize": 4,
    "rotation": 0
  },
  "effects": {
    "outline": {
      "enabled": true,
      "color": "#000000",
      "thickness": 1
    },
    "shadow": {
      "enabled": false,
      "color": "#000000",
      "blur": 3,
      "offsetX": 0,
      "offsetY": 2
    },
    "glow": {
      "enabled": false,
      "color": "#00FFFF",
      "intensity": 50,
      "range": 10
    }
  },
  "display": {
    "monitor": "primary",
    "clickThrough": true,
    "alwaysOnTop": true,
    "positionX": 0,
    "positionY": 0
  },
  "hotkeys": {
    "toggle": "Ctrl+Shift+X",
    "resetPosition": "Ctrl+Shift+R"
  }
}
```

---

### 3.3 热键管理模块

#### 3.3.1 类设计

```csharp
namespace CrosshairPro.Services.Hotkey;

/// <summary>
/// 热键服务
/// </summary>
public sealed class HotkeyService : IHotkeyService, IDisposable
{
    private readonly ILogger _logger;
    private readonly IHotkeyProvider _hotkeyProvider;
    private readonly IConflictDetector _conflictDetector;

    private readonly Dictionary<string, HotkeyBinding> _bindings = new();
    private readonly Dictionary<int, string> _registeredHotkeys = new();

    public event EventHandler<HotkeyTriggeredEventArgs>? HotkeyTriggered;

    public HotkeyService(
        ILogger logger,
        IHotkeyProvider hotkeyProvider,
        IConflictDetector conflictDetector)
    {
        _logger = logger;
        _hotkeyProvider = hotkeyProvider;
        _conflictDetector = conflictDetector;

        _hotkeyProvider.HotkeyPressed += OnHotkeyPressed;
    }

    /// <summary>
    /// 注册热键
    /// </summary>
    public bool RegisterHotkey(HotkeyBinding binding)
    {
        if (binding == null || string.IsNullOrEmpty(binding.Id))
            return false;

        // 检测冲突
        var conflict = _conflictDetector.DetectConflict(binding.Combo);
        if (conflict != null)
        {
            _logger.Warning("热键冲突: {Binding} 与 {Conflict} 冲突",
                binding.Name, conflict);
            return false;
        }

        // 解析组合键
        var keyCombo = ParseHotkey(binding.Combo);

        // 注册到系统
        var id = GenerateHotkeyId();
        if (!_hotkeyProvider.Register(id, keyCombo))
        {
            _logger.Error("注册热键失败: {Combo}", binding.Combo);
            return false;
        }

        // 保存绑定
        _bindings[binding.Id] = binding;
        _registeredHotkeys[id] = binding.Id;

        _logger.Information("热键已注册: {Name} = {Combo}", binding.Name, binding.Combo);
        return true;
    }

    /// <summary>
    /// 注销热键
    /// </summary>
    public bool UnregisterHotkey(string bindingId)
    {
        var entry = _registeredHotkeys.FirstOrDefault(x => x.Value == bindingId);
        if (entry.Key == 0)
            return false;

        _hotkeyProvider.Unregister(entry.Key);
        _registeredHotkeys.Remove(entry.Key);
        _bindings.Remove(bindingId);

        return true;
    }

    /// <summary>
    /// 解析热键字符串
    /// </summary>
    private KeyCombo ParseHotkey(string combo)
    {
        var parts = combo.ToLowerInvariant().Split('+', StringSplitOptions.RemoveEmptyEntries);

        return new KeyCombo
        {
            Ctrl = parts.Contains("ctrl"),
            Shift = parts.Contains("shift"),
            Alt = parts.Contains("alt"),
            Win = parts.Contains("win"),
            Key = parts.LastOrDefault() ?? ""
        };
    }

    /// <summary>
    /// 热键按下事件处理
    /// </summary>
    private void OnHotkeyPressed(object? sender, HotkeyPressedEventArgs e)
    {
        if (!_registeredHotkeys.TryGetValue(e.HotkeyId, out var bindingId))
            return;

        if (!_bindings.TryGetValue(bindingId, out var binding))
            return;

        _logger.Debug("热键触发: {Name}", binding.Name);

        HotkeyTriggered?.Invoke(this, new HotkeyTriggeredEventArgs(binding));
    }
}

/// <summary>
/// 热键绑定定义
/// </summary>
public class HotkeyBinding
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Combo { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public Action? Callback { get; set; }
}

/// <summary>
/// 按键组合
/// </summary>
public struct KeyCombo
{
    public bool Ctrl;
    public bool Shift;
    public bool Alt;
    public bool Win;
    public string Key;

    public int Modifiers =>
        (Ctrl ? 0x0002 : 0) |
        (Shift ? 0x0004 : 0) |
        (Alt ? 0x0001 : 0) |
        (Win ? 0x0008 : 0);
}
```

#### 3.3.2 Win32热键提供者

```csharp
namespace CrosshairPro.Infrastructure.Hotkey;

/// <summary>
/// Windows热键提供者
/// </summary>
public sealed class WinHotkeyProvider : IHotkeyProvider, IDisposable
{
    private readonly IntPtr _hwnd;
    private readonly Dictionary<int, KeyCombo> _registeredKeys = new();
    private int _nextId = 1;

    private const int WM_HOTKEY = 0x0312;

    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public WinHotkeyProvider()
    {
        // 创建消息窗口
        _hwnd = CreateMessageWindow();
    }

    /// <summary>
    /// 注册热键
    /// </summary>
    public bool Register(int id, KeyCombo combo)
    {
        var virtualKey = KeyToVirtualKey(combo.Key);
        if (virtualKey == 0)
            return false;

        if (!RegisterHotKey(_hwnd, id, combo.Modifiers, virtualKey))
        {
            var error = Marshal.GetLastWin32Error();
            return false;
        }

        _registeredKeys[id] = combo;
        return true;
    }

    /// <summary>
    /// 注销热键
    /// </summary>
    public bool Unregister(int id)
    {
        if (!UnregisterHotKey(_hwnd, id))
            return false;

        _registeredKeys.Remove(id);
        return true;
    }

    /// <summary>
    /// 创建消息窗口用于接收热键消息
    /// </summary>
    private IntPtr CreateMessageWindow()
    {
        var wndClass = new WNDCLASS
        {
            lpfnWndProc = WndProc,
            lpszClassName = "CrosshairProHotkeyWindow"
        };

        RegisterClass(ref wndClass);

        return CreateWindowEx(
            0, wndClass.lpszClassName, "",
            0, 0, 0, 0, 0,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }

    /// <summary>
    /// 窗口过程
    /// </summary>
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_HOTKEY)
        {
            var id = wParam.ToInt32();
            HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(id));
            return IntPtr.Zero;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
```

---

### 3.4 游戏检测模块

#### 3.4.1 类设计

```csharp
namespace CrosshairPro.Services.GameDetection;

/// <summary>
/// 游戏检测服务
/// </summary>
public sealed class GameDetectorService : IGameDetector, IDisposable
{
    private readonly IProcessWatcher _processWatcher;
    private readonly IGameProfileManager _profileManager;
    private readonly IPresetService _presetService;
    private readonly ILogger _logger;

    private readonly List<GameProfile> _profiles = new();
    private GameInfo? _currentGame;
    private bool _isMonitoring;

    public event EventHandler<GameDetectedEventArgs>? GameStarted;
    public event EventHandler<GameDetectedEventArgs>? GameExited;

    public GameInfo? CurrentGame => _currentGame;

    public GameDetectorService(
        IProcessWatcher processWatcher,
        IGameProfileManager profileManager,
        IPresetService presetService,
        ILogger logger)
    {
        _processWatcher = processWatcher;
        _profileManager = profileManager;
        _presetService = presetService;
        _logger = logger;

        _processWatcher.ProcessStarted += OnProcessStarted;
        _processWatcher.ProcessExited += OnProcessExited;
    }

    /// <summary>
    /// 初始化服务
    /// </summary>
    public async Task InitializeAsync()
    {
        var profiles = await _profileManager.LoadProfilesAsync();
        _profiles.Clear();
        _profiles.AddRange(profiles);

        // 添加内置游戏配置
        foreach (var builtIn in GameProfile.BuiltIn.GetAll())
        {
            if (!_profiles.Any(p => p.ProcessName.Equals(builtIn.ProcessName,
                StringComparison.OrdinalIgnoreCase)))
            {
                _profiles.Add(builtIn);
            }
        }

        _logger.Information("已加载 {Count} 个游戏配置", _profiles.Count);
    }

    /// <summary>
    /// 开始监控
    /// </summary>
    public void StartMonitoring()
    {
        if (_isMonitoring) return;

        _isMonitoring = true;
        _processWatcher.StartWatching();

        _logger.Information("游戏检测已启动");
    }

    /// <summary>
    /// 进程启动事件处理
    /// </summary>
    private void OnProcessStarted(object? sender, ProcessEventArgs e)
    {
        var matchedProfile = DetectGame(e.ProcessName);
        if (matchedProfile == null) return;

        var gameInfo = new GameInfo(
            e.ProcessName,
            matchedProfile.DisplayName,
            e.ProcessId,
            DateTime.UtcNow);

        _currentGame = gameInfo;

        _logger.Information("检测到游戏启动: {Name} (PID: {Id})",
            matchedProfile.DisplayName, e.ProcessId);

        // 自动切换预设
        if (matchedProfile.AutoSwitch && !string.IsNullOrEmpty(matchedProfile.PresetId))
        {
            _ = _presetService.SwitchPresetAsync(matchedProfile.PresetId);
        }

        GameStarted?.Invoke(this, new GameDetectedEventArgs(gameInfo, isStarting: true));
    }

    /// <summary>
    /// 检测游戏
    /// </summary>
    private GameProfile? DetectGame(string processName)
    {
        return _profiles
            .Where(p => p.Matches(processName))
            .OrderByDescending(p => p.Priority)
            .FirstOrDefault();
    }
}
```

#### 3.4.2 内置游戏配置

```csharp
namespace CrosshairPro.Core.Models;

/// <summary>
/// 游戏配置
/// </summary>
public class GameProfile
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool AutoSwitch { get; set; }
    public string? PresetId { get; set; }
    public bool FullscreenOnly { get; set; }
    public DateTime? LastMatchedAt { get; set; }

    public bool Matches(string processName)
    {
        return ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 内置游戏配置
    /// </summary>
    public static class BuiltIn
    {
        public static IEnumerable<GameProfile> GetAll()
        {
            return new[]
            {
                new GameProfile
                {
                    Id = "builtin-cs2",
                    DisplayName = "Counter-Strike 2",
                    ProcessName = "cs2",
                    Priority = 100
                },
                new GameProfile
                {
                    Id = "builtin-valorant",
                    DisplayName = "Valorant",
                    ProcessName = "VALORANT-Win64-Shipping",
                    Priority = 100,
                    FullscreenOnly = true
                },
                new GameProfile
                {
                    Id = "builtin-apex",
                    DisplayName = "Apex Legends",
                    ProcessName = "r5apex",
                    Priority = 100
                },
                new GameProfile
                {
                    Id = "builtin-overwatch2",
                    DisplayName = "Overwatch 2",
                    ProcessName = "Overwatch",
                    Priority = 100
                },
                new GameProfile
                {
                    Id = "builtin-pubg",
                    DisplayName = "PUBG",
                    ProcessName = "TslGame",
                    Priority = 90
                }
            };
        }
    }
}
```

---

## 4. 非功能性设计

### 4.1 性能指标

| 指标 | 目标值 | 实现策略 |
|------|--------|----------|
| 启动时间 | < 2秒 | 延迟加载、AOT编译 |
| 内存占用 | < 50MB | 对象池、及时释放 |
| CPU占用 | < 1% | 高效渲染、事件节流 |
| 渲染延迟 | < 16ms | 硬件加速、Geometry缓存 |
| 热键响应 | < 100ms | Win32原生API |

### 4.2 性能优化策略

#### 4.2.1 内存优化

```csharp
// 1. 对象池复用
public class BrushPool
{
    private readonly ConcurrentDictionary<string, Brush> _cache = new();

    public Brush GetOrCreate(string key, Func<Brush> factory)
    {
        return _cache.GetOrAdd(key, k =>
        {
            var brush = factory();
            brush.Freeze(); // 冻结对象，使其线程安全且不可变
            return brush;
        });
    }
}

// 2. 及时释放资源
public class OverlayWindow : Window, IDisposable
{
    private readonly Timer _renderTimer;
    private bool _disposed;

    protected override void OnClosed(EventArgs e)
    {
        Dispose();
        base.OnClosed(e);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _renderTimer?.Dispose();
        _disposed = true;
    }
}
```

#### 4.2.2 渲染优化

```csharp
// 使用OnRender而不是定时器重绘
protected override void OnRender(DrawingContext drawingContext)
{
    base.OnRender(drawingContext);

    // 只在配置变化时重新渲染
    if (!_configChanged) return;

    _renderer.Render(drawingContext, _config, RenderSize);
    _configChanged = false;
}

// 配置变化时触发重绘
private void OnConfigChanged()
{
    _configChanged = true;
    InvalidateVisual(); // 标记需要重绘
}
```

### 4.3 安全设计

#### 4.3.1 反作弊兼容性

| 反作弊系统 | 兼容性 | 说明 |
|------------|--------|------|
| VAC (Steam) | ✅ 通常安全 | 外部覆盖不读取游戏内存 |
| BattlEye | ⚠️ 需谨慎 | 部分游戏禁止任何覆盖 |
| Easy Anti-Cheat | ⚠️ 需谨慎 | 因游戏而异 |
| Riot Vanguard | ❌ 禁止 | 不允许任何第三方覆盖 |

**安全措施：**
1. 仅使用外部覆盖层技术
2. 不读取游戏内存
3. 不注入游戏进程
4. 明确告知用户风险

#### 4.3.2 配置文件安全

```csharp
// 配置文件校验
public class ConfigValidator
{
    public bool Validate(CrosshairConfig config)
    {
        // 参数范围校验
        if (config.Size < 1 || config.Size > 100) return false;
        if (config.Thickness < 1 || config.Thickness > 10) return false;
        if (config.Opacity < 0 || config.Opacity > 100) return false;

        // 颜色格式校验
        if (!Regex.IsMatch(config.Color, @"^#[0-9A-Fa-f]{6}$")) return false;

        return true;
    }
}
```

---

## 5. 部署架构

### 5.1 项目结构

```
CrosshairPro/
├── src/
│   ├── CrosshairPro.Core/              # 核心业务库
│   │   ├── Models/                     # 领域模型
│   │   ├── Enums/                      # 枚举定义
│   │   ├── Events/                     # 事件定义
│   │   └── Interfaces/                 # 接口定义
│   │
│   ├── CrosshairPro.Services/          # 业务服务库
│   │   ├── Crosshair/                  # 准心渲染
│   │   ├── Configuration/              # 配置管理
│   │   ├── Hotkey/                     # 热键管理
│   │   ├── GameDetection/              # 游戏检测
│   │   └── SystemTray/                 # 系统托盘
│   │
│   ├── CrosshairPro.Infrastructure/    # 基础设施库
│   │   ├── Win32/                      # Win32 API封装
│   │   ├── Hotkey/                     # 热键底层实现
│   │   ├── IO/                         # 文件操作
│   │   └── Logging/                    # 日志实现
│   │
│   └── CrosshairPro.App/               # WPF应用程序
│       ├── Views/                      # 视图
│       ├── ViewModels/                 # 视图模型
│       ├── Converters/                 # 值转换器
│       ├── Controls/                   # 自定义控件
│       ├── Themes/                     # 主题样式
│       └── App.xaml                    # 应用入口
│
├── tests/                              # 测试项目
│   ├── CrosshairPro.Core.Tests/
│   ├── CrosshairPro.Services.Tests/
│   └── CrosshairPro.UI.Tests/
│
├── docs/                               # 文档
│   ├── PRD.md
│   ├── prototype-design.md
│   └── technical-design.md
│
├── CrosshairPro.sln                    # 解决方案文件
└── README.md                           # 项目说明
```

### 5.2 依赖关系

```
┌──────────────────────────────────────────────────────────────┐
│                      CrosshairPro.App                         │
│                    (WPF应用程序入口)                           │
└────────────────────────────┬─────────────────────────────────┘
                             │
            ┌────────────────┴────────────────┐
            │                                 │
            ▼                                 ▼
┌───────────────────────┐     ┌───────────────────────────────┐
│  CrosshairPro.Services │     │  CrosshairPro.Infrastructure  │
│    (业务服务层)        │────▶│     (基础设施层)              │
└───────────┬───────────┘     └───────────────────────────────┘
            │
            ▼
┌───────────────────────┐
│   CrosshairPro.Core   │
│    (核心业务模型)      │
└───────────────────────┘
```

---

## 6. 开发计划

### 6.1 开发阶段划分

| 阶段 | 内容 | 时间 |
|------|------|------|
| 第一阶段 | 核心功能（渲染引擎、基础样式、配置管理） | 2周 |
| 第二阶段 | 重要功能（自定义图片、热键、游戏检测） | 2周 |
| 第三阶段 | 增强功能（动画效果、预设分享、主题） | 1周 |
| 第四阶段 | 测试和优化 | 1周 |

### 6.2 技术风险

| 风险 | 级别 | 缓解措施 |
|------|------|----------|
| 全屏游戏覆盖问题 | 中 | 提供详细设置指南，建议无边框模式 |
| 反作弊兼容性 | 中 | 明确告知用户风险，避免Valorant等严格游戏 |
| 性能不达标 | 低 | 使用Geometry缓存、Brush缓存优化 |
| 热键冲突 | 低 | 提供热键冲突检测和修改提示 |

---

## 附录

### A. 参考资料

- [WPF官方文档](https://docs.microsoft.com/zh-cn/dotnet/desktop/wpf/)
- [.NET 8官方文档](https://docs.microsoft.com/zh-cn/dotnet/)
- [Win32 API文档](https://docs.microsoft.com/zh-cn/windows/win32/)

### B. 变更历史

| 版本 | 日期 | 变更内容 | 变更人 |
|-----|------|---------|--------|
| v1.0 | 2026-06-07 | 初始版本 | Claude |

---

**技术负责人：** Claude
**最后更新：** 2026-06-07
