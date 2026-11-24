using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>
    /// Swaps the Light/Dark brush dictionary at runtime. Every control references the brushes through
    /// DynamicResource, so all open windows repaint at once with no per-view theming code (v1's ApplyTheme methods).
    /// Also darkens the native title bar of every window, which v1 never did.
    /// </summary>
    public class ThemeService
    {
        private const int DwmwaUseImmersiveDarkMode = 20; // Windows 10 20H1 and later
        private static readonly Uri LightTheme = new("Themes/Light.xaml", UriKind.Relative);
        private static readonly Uri DarkTheme = new("Themes/Dark.xaml", UriKind.Relative);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        public bool IsDarkMode { get; private set; }

        /// <summary>Call once at startup so windows opened later get the right title bar too.</summary>
        public void Initialize()
        {
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent,
                new RoutedEventHandler((sender, _) => ApplyTitleBar((Window)sender)));
        }

        public void Apply(bool isDarkMode)
        {
            IsDarkMode = isDarkMode;

            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var themeDictionary = new ResourceDictionary { Source = isDarkMode ? DarkTheme : LightTheme };

            // App.xaml keeps the theme brushes at index 0 and the control styles after it
            if (dictionaries.Count > 0)
            {
                dictionaries[0] = themeDictionary;
            }
            else
            {
                dictionaries.Add(themeDictionary);
            }

            foreach (Window window in Application.Current.Windows)
            {
                ApplyTitleBar(window);
            }
        }

        private void ApplyTitleBar(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            // Best effort: older Windows builds ignore the attribute and keep a light title bar
            int useDark = IsDarkMode ? 1 : 0;
            _ = DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref useDark, sizeof(int));
        }
    }
}
