namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>
    /// Every dialog a ViewModel can show. ViewModels never create windows themselves, which keeps them
    /// independent of WPF types and lets a fake implementation stand in for tests.
    /// </summary>
    public interface IDialogService
    {
        void ShowInfo(string message, string title);
        void ShowWarning(string message, string title);
        void ShowError(string message, string title);

        /// <summary>Yes/No question. Returns true for Yes.</summary>
        bool Confirm(string message, string title);

        /// <summary>Retry/Cancel prompt (e.g. a file in use). Returns true for Retry.</summary>
        bool AskRetry(string message, string title);

        /// <summary>Returns the chosen folder, or null if the user cancelled.</summary>
        string PickFolder(string title);

        /// <summary>Shows the progress window until the returned handle is disposed.</summary>
        IProgressDialog ShowProgress(string initialStatus);

        /// <summary>Shows the unhandled-error window with the full error report.</summary>
        void ShowError(Exception exception);
    }

    public interface IProgressDialog : IDisposable
    {
        string Status { get; set; }
    }
}
