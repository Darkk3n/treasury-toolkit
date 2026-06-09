using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.Core.Contracts
{
    public interface IFileScanner
    {
        void ScanPdfFiles(string date, string[] files, Action<string> onProgress, Action<ScannedPaymentData> onRowExtracted);
    }
}
