using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>
    /// Builds the text shown (and copied) in the error window: message, stack trace, inner exception, the saved
    /// configuration and the environment. Same sections and format as v1's ExceptionHandlerForm.
    /// </summary>
    public class ErrorReportBuilder(SettingsService settingsService)
    {
        public string Build(Exception ex)
        {
            string nl = Environment.NewLine;
            var details = new StringBuilder();

            details.Append(ex.Message);
            details.Append(nl).Append(nl).Append("Stack Trace:").Append(nl).Append(nl);
            details.Append(string.IsNullOrWhiteSpace(ex.StackTrace) ? "(no disponible)" : ex.StackTrace);

            if (ex.InnerException != null)
            {
                details.Append(nl).Append(nl).Append("Inner Exception:").Append(nl).Append(nl);
                details.Append(ex.InnerException.ToString());
            }

            details.Append(nl).Append(nl).Append("Configuration (appsettings.local.json):").Append(nl).Append(nl);
            details.Append(SafeDescribe(DescribeSettings));

            details.Append(nl).Append(nl).Append("Environment:").Append(nl).Append(nl);
            details.Append(SafeDescribe(DescribeEnvironment));

            return details.ToString();
        }

        // The error window must never throw while describing the error, so any failure becomes a line of text
        private static string SafeDescribe(Func<IEnumerable<(string Name, object Value)>> describe)
        {
            try
            {
                var entries = describe().ToList();
                int width = entries.Max(e => e.Name.Length) + 1;
                return string.Join(Environment.NewLine, entries.Select(e => $"   {(e.Name + ":").PadRight(width)} {e.Value}"));
            }
            catch (Exception ex)
            {
                return $"   (no disponible: {ex.Message})";
            }
        }

        // What's saved on disk, i.e. what the next launch will restore
        private IEnumerable<(string Name, object Value)> DescribeSettings()
        {
            if (!File.Exists(settingsService.FilePath))
            {
                return [("Status", "file not found, defaults in use")];
            }

            if (!settingsService.TryReadFromDisk(out var settings, out string error) || settings == null)
            {
                return [("Status", $"could not be read ({error ?? "empty file"})")];
            }

            // Reflection keeps new settings showing up here without touching this class
            return typeof(LocalAppSettings).GetProperties()
                .Select(p => (p.Name, p.GetValue(settings) is bool b ? (object)(b ? "true" : "false") : p.GetValue(settings)));
        }

        private static IEnumerable<(string Name, object Value)> DescribeEnvironment()
        {
            // WPF works in device-independent units (1/96 inch), so these are the sizes the layout sees
            var workArea = SystemParameters.WorkArea;

            return
            [
                ("App version", AppInfo.Version),
                ("OS", Environment.OSVersion.VersionString),
                (".NET", Environment.Version),
                ("Culture", CultureInfo.CurrentUICulture.Name),
                ("Screen", $"{SystemParameters.PrimaryScreenWidth:0}x{SystemParameters.PrimaryScreenHeight:0} (working area {workArea.Width:0}x{workArea.Height:0})"),
                ("Scaling", DescribeScaling()),
                ("Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            ];
        }

        private static string DescribeScaling()
        {
            var window = Application.Current?.MainWindow;
            if (window == null || !window.IsLoaded)
            {
                return "(no disponible)";
            }

            var dpi = VisualTreeHelper.GetDpi(window);
            return $"{dpi.DpiScaleX:P0} ({dpi.PixelsPerInchX:0} DPI)";
        }
    }
}
