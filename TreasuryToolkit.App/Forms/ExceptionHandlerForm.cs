using System.Text.Json;
using System.Windows.Forms;

namespace TreasuryToolkit.App
{
    public partial class ExceptionHandlerForm : Form
    {
        private readonly Exception exception;
        private readonly string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.json");
        private bool isDarkMode = false;
        private LocalAppSettings settings;
        private string settingsReadError;
        private ToolTip copyToolTip;

        public ExceptionHandlerForm(Exception exception)
        {
            InitializeComponent();
            this.exception = exception;
            copyToolTip = new ToolTip();
        }

        override protected void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadAndApplySettings();
            TxtExceptionDetails.Text = BuildExceptionDetails(exception);
        }

        private string BuildExceptionDetails(Exception ex)
        {
            // A multiline TextBox only breaks lines on "\r\n", so build with Environment.NewLine.
            string nl = Environment.NewLine;
            var details = new System.Text.StringBuilder();

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

        // The error form must never throw while describing the error, so any failure becomes a line of text
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

        private IEnumerable<(string Name, object Value)> DescribeSettings()
        {
            if (!File.Exists(settingsFilePath))
            {
                return [("Status", "file not found, defaults in use")];
            }

            if (settings == null)
            {
                return [("Status", $"could not be read ({settingsReadError ?? "empty file"})")];
            }

            // Reflection keeps new settings showing up here without touching this form
            return typeof(LocalAppSettings).GetProperties()
                .Select(p => (p.Name, p.GetValue(settings) is bool b ? (object)(b ? "true" : "false") : p.GetValue(settings)));
        }

        private static IEnumerable<(string Name, object Value)> DescribeEnvironment()
        {
            // The app is DPI-unaware, so these are the scaled sizes the app lays out against, not physical pixels
            var screen = Screen.PrimaryScreen;

            return
            [
                ("OS", Environment.OSVersion.VersionString),
                (".NET", Environment.Version),
                ("Culture", System.Globalization.CultureInfo.CurrentUICulture.Name),
                ("Screen", $"{screen.Bounds.Width}x{screen.Bounds.Height} (working area {screen.WorkingArea.Width}x{screen.WorkingArea.Height})"),
                ("Screens", Screen.AllScreens.Length),
                ("Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            ];
        }

        private void BtnClose_Click(object sender, EventArgs e) => Close();

        private void BtnCopyDtls_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TxtExceptionDetails.Text);

            copyToolTip.Show("¡Copiado al portapapeles!", BtnCopyDtls, 0, -35);
            ToolTipTimer.Start();
        }

        private void LoadAndApplySettings()
        {
            if (File.Exists(settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(settingsFilePath);
                    settings = JsonSerializer.Deserialize<LocalAppSettings>(json);

                    if (settings != null)
                    {
                        isDarkMode = settings.IsDarkMode;
                    }
                }
                catch (Exception ex)
                {
                    isDarkMode = false;
                    settingsReadError = ex.Message;
                }
            }

            Color formBg = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromKnownColor(KnownColor.ControlLightLight);
            Color controlBg = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            Color textColor = isDarkMode ? Color.White : Color.FromArgb(51, 51, 51);
            Color buttonBg = isDarkMode ? Color.FromArgb(0, 122, 204) : Color.FromKnownColor(KnownColor.ControlLightLight);

            // 1. Paint the main form background
            this.BackColor = formBg;

            foreach (Control c in this.Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = buttonBg;
                    btn.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
                else if (c is Label)
                {
                    c.ForeColor = textColor;
                    c.BackColor = controlBg;
                }
            }
        }

        private void ToolTipTimer_Tick(object sender, EventArgs e)
        {
            copyToolTip.Hide(BtnCopyDtls);
            ToolTipTimer.Stop();
        }
    }
}
