namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>
    /// Persisted to appsettings.local.json next to the exe. Same shape and property names as v1's
    /// TreasuryToolkit.App.LocalAppSettings, so a v1 settings file is picked up unchanged.
    /// </summary>
    public class LocalAppSettings
    {
        public bool IsDarkMode { get; set; } = false;
        public bool IsSidebarCollapsed { get; set; } = false;
        public int WindowWidth { get; set; } = 0;
        public int WindowHeight { get; set; } = 0;
        public bool IsWindowMaximized { get; set; } = false;
    }
}
