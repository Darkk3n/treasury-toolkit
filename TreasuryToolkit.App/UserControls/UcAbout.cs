using System.Diagnostics;

namespace TreasuryToolkit.App
{
    public partial class UcAbout : UserControl
    {
        public UcAbout()
        {
            InitializeComponent();
        }

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
    }
}