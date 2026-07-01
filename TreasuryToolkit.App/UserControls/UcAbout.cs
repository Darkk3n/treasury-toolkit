using System.Diagnostics;

namespace TreasuryToolkit.App
{
    public partial class UcAbout : UserControl
    {
        #region Constructor
        public UcAbout()
        {
            InitializeComponent();
        }
        #endregion

        #region UI Controls Events
        private void LnkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Darkk3n",
                UseShellExecute = true
            });
        }

        private void LnkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var emailAddress = "gerardo.aguilar01@outlook.com";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{emailAddress}?subject=Consulta%20TreasuryToolkit",
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                Clipboard.SetText(emailAddress);

                MessageBox.Show(
                    "No se pudo abrir un cliente de correo automáticamente.\n\n" +
                    "La dirección de correo electrónico ha sido copiada al portapapeles para que pueda pegarla manualmente.",
                    "Información de Contacto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
        #endregion

        #region Helpers
        public void ApplyTheme(bool isDarkMode)
        {
            Color controlBg = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            Color textColor = isDarkMode ? Color.White : Color.FromArgb(51, 51, 51);
            Color linkColor = isDarkMode ? Color.FromArgb(0, 122, 204) : Color.FromKnownColor(KnownColor.HotTrack);
            this.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromKnownColor(KnownColor.ControlLightLight);
            ApplyThemeToAllControls(FlpContainer, controlBg, textColor, linkColor);
        }

        private static void ApplyThemeToAllControls(Control parent, Color controlBg, Color textColor, Color linkColor)
        {
            foreach (Control c in parent.Controls)
            {
                ChangeLabelAndLinkLabelColors(controlBg, textColor, linkColor, c);
                if (c.HasChildren)
                {
                    ApplyThemeToAllControls(c, controlBg, textColor, linkColor);
                }
            }
        }

        private static void ChangeLabelAndLinkLabelColors(Color controlBg, Color textColor, Color linkColor, Control c)
        {
            if (c is Label lbl)
            {
                lbl.ForeColor = textColor;
                lbl.BackColor = controlBg;
            }
            else if (c is LinkLabel lnk)
            {
                lnk.LinkColor = linkColor;
                lnk.ActiveLinkColor = linkColor;
                lnk.VisitedLinkColor = linkColor;
                lnk.BackColor = controlBg;
            }
        } 
        #endregion
    }
}