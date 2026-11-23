using System.Text.Json;
using System.Windows.Forms;

namespace TreasuryToolkit.App
{
    public partial class ExceptionHandlerForm : Form
    {
        private readonly Exception exception;
        private readonly string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.json");
        private bool isDarkMode = false;
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

        private static string BuildExceptionDetails(Exception ex)
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

            return details.ToString();
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
                    var settings = JsonSerializer.Deserialize<LocalAppSettings>(json);

                    if (settings != null)
                    {
                        isDarkMode = settings.IsDarkMode;
                    }
                }
                catch
                {
                    isDarkMode = false;
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
