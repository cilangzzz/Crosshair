using System;
using System.IO;

namespace CrosshairPro.Core;

/// <summary>
/// 应用数据目录与持久化文件路径的唯一权威定义（%APPDATA%\CrosshairPro）
/// 仓库实现与需要绕过异步接口直接读文件的地方（如 App 单实例校验）都必须引用此类，
/// 避免存储契约多处硬编码后漂移
/// </summary>
public static class CrosshairProPaths
{
    /// <summary>应用数据目录（%APPDATA%\CrosshairPro）</summary>
    public static readonly string AppDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrosshairPro");

    /// <summary>应用状态文件（appstate.json，记录当前预设 ID 与界面语言）</summary>
    public static readonly string AppStateFile = Path.Combine(AppDirectory, "appstate.json");
}
