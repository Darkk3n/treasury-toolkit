using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TreasuryToolkit.Wpf.App.Services;

namespace TreasuryToolkit.Wpf.App.ViewModels
{
    /// <summary>Port of v1's UcAbout logic: version line and the GitHub / email links.</summary>
    public partial class AboutViewModel(ISystemService system, IDialogService dialogs) : ObservableObject
    {
        private const string GitHubUrl = "https://github.com/Darkk3n";
        private const string EmailAddress = "gerardo.aguilar01@outlook.com";

        public string VersionText { get; } = $"Version {AppInfo.Version} - Desarrollada y mantenida por Gerardo Aguilar";

        [RelayCommand]
        private void OpenGitHub() => system.TryOpen(GitHubUrl);

        [RelayCommand]
        private void SendEmail()
        {
            if (system.TryOpen($"mailto:{EmailAddress}?subject=Consulta%20TreasuryToolkit"))
            {
                return;
            }

            // Same fallback as v1 when no mail client is registered
            system.CopyToClipboard(EmailAddress);
            dialogs.ShowInfo(
                "No se pudo abrir un cliente de correo automáticamente.\n\n" +
                "La dirección de correo electrónico ha sido copiada al portapapeles para que pueda pegarla manualmente.",
                "Información de Contacto");
        }
    }
}
