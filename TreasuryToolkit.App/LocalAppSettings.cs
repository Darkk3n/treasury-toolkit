using System;
using System.Collections.Generic;
using System.Text;

namespace TreasuryToolkit.App
{
    public class LocalAppSettings
    {
        public bool IsDarkMode { get; set; } = false;
        public bool IsSidebarCollapsed { get; set; } = false;
        public int WindowWidth { get; set; } = 0;
        public int WindowHeight { get; set; } = 0;
        public bool IsWindowMaximized { get; set; } = false;
    }
}
