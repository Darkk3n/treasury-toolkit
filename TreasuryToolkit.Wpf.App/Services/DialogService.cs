using System.Windows;
using Microsoft.Win32;
using TreasuryToolkit.Wpf.App.Views;

namespace TreasuryToolkit.Wpf.App.Services
{
    public class DialogService(Func<ProgressWindow> progressWindowFactory, ErrorReportBuilder errorReportBuilder) : IDialogService
    {
        // Every message uses the themed MessageWindow: the native MessageBox is drawn by Windows and can't follow
        // the app's theme. Button texts are Spanish regardless of the Windows language.
        public void ShowInfo(string message, string title) => ShowMessage(message, title, MessageKind.Information, "Aceptar");

        public void ShowWarning(string message, string title) => ShowMessage(message, title, MessageKind.Warning, "Aceptar");

        public void ShowError(string message, string title) => ShowMessage(message, title, MessageKind.Error, "Aceptar");

        public bool Confirm(string message, string title) => ShowMessage(message, title, MessageKind.Question, "Sí", "No");

        public bool AskRetry(string message, string title) => ShowMessage(message, title, MessageKind.Warning, "Reintentar", "Cancelar");

        public string PickFolder(string title, string initialFolder = null)
        {
            var dialog = new OpenFolderDialog { Title = title, Multiselect = false };

            string startFolder = NearestExistingFolder(initialFolder);
            if (startFolder != null)
            {
                dialog.InitialDirectory = startFolder;
            }

            return dialog.ShowDialog(GetOwner()) == true ? dialog.FolderName : null;
        }

        // A remembered folder may have been renamed or deleted since; walk up until something exists
        private static string NearestExistingFolder(string folder)
        {
            try
            {
                while (!string.IsNullOrEmpty(folder) && !System.IO.Directory.Exists(folder))
                {
                    folder = System.IO.Path.GetDirectoryName(folder);
                }

                return string.IsNullOrEmpty(folder) ? null : folder;
            }
            catch (Exception)
            {
                return null; // A malformed path in the settings file shouldn't block the picker
            }
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

        // Returns true when the primary button was chosen
        private static bool ShowMessage(string message, string title, MessageKind kind, string primaryButtonText, string secondaryButtonText = null)
        {
            var window = new MessageWindow(title, message, kind, primaryButtonText, secondaryButtonText);
            var owner = GetOwner();
            if (owner != null)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            return window.ShowDialog() == true;
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
