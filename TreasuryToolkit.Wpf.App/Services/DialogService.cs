using System.Windows;
using Microsoft.Win32;
using TreasuryToolkit.Wpf.App.Views;

namespace TreasuryToolkit.Wpf.App.Services
{
    public class DialogService(Func<ProgressWindow> progressWindowFactory, ErrorReportBuilder errorReportBuilder) : IDialogService
    {
        public void ShowInfo(string message, string title) => Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public void ShowWarning(string message, string title) => Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void ShowError(string message, string title) => Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public bool Confirm(string message, string title) =>
            Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        // WPF's MessageBox has no Retry/Cancel buttons, so OK stands in for "Reintentar"
        public bool AskRetry(string message, string title) =>
            Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK;

        public string PickFolder(string title)
        {
            var dialog = new OpenFolderDialog { Title = title, Multiselect = false };
            return dialog.ShowDialog(GetOwner()) == true ? dialog.FolderName : null;
        }

        public IProgressDialog ShowProgress(string initialStatus)
        {
            var window = progressWindowFactory();
            window.Status = initialStatus;

            var owner = GetOwner();
            if (owner != null)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            window.Show();
            return new ProgressHandle(window);
        }

        public void ShowError(Exception exception)
        {
            var window = new ErrorWindow(errorReportBuilder.Build(exception));
            var owner = GetOwner();
            if (owner != null)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            window.ShowDialog();
        }

        private static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            var owner = GetOwner();
            return owner != null
                ? MessageBox.Show(owner, message, title, buttons, image)
                : MessageBox.Show(message, title, buttons, image);
        }

        // The active window if there is one (so a prompt over the progress window stays on top of it),
        // otherwise the main window once it's on screen. Null during startup, before any window is shown.
        private static Window GetOwner()
        {
            var app = Application.Current;
            if (app == null)
            {
                return null;
            }

            var active = app.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (active != null)
            {
                return active;
            }

            return app.MainWindow is { IsLoaded: true, IsVisible: true } main ? main : null;
        }

        private sealed class ProgressHandle(ProgressWindow window) : IProgressDialog
        {
            public string Status
            {
                get => window.Status;
                set => window.Status = value;
            }

            public void Dispose() => window.Close();
        }
    }
}
