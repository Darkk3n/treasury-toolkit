using System.Diagnostics;
using System.Windows;

namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>Shell and clipboard access, kept behind an interface so ViewModels don't touch Process or Clipboard.</summary>
    public interface ISystemService
    {
        /// <summary>Opens a URL, mailto: link or folder with the user's default handler. Returns false if that failed.</summary>
        bool TryOpen(string target);

        void CopyToClipboard(string text);
    }

    public class SystemService : ISystemService
    {
        public bool TryOpen(string target)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void CopyToClipboard(string text) => Clipboard.SetText(text);
    }
}
