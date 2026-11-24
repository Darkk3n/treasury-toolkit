namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>
    /// Persisted to appsettings.local.json next to the exe. Same property names as v1's
    /// TreasuryToolkit.App.LocalAppSettings, so a v1 settings file is picked up unchanged; settings added in v2
    /// keep their defaults until saved (and v1 ignores them if it reads a v2 file).
    /// </summary>
    public class LocalAppSettings
    {
        public bool IsDarkMode { get; set; } = false;
        public bool IsSidebarCollapsed { get; set; } = false;
        public int WindowWidth { get; set; } = 0;
        public int WindowHeight { get; set; } = 0;
        public bool IsWindowMaximized { get; set; } = false;

        /// <summary>Last folder picked in the PDF tool; the folder picker opens there next time. Added in v2.</summary>
        public string LastFolder { get; set; }
    }
}
