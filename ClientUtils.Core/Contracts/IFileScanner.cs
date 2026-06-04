using ClientUtils.Core.Models;

namespace ClientUtils.Core.Contracts
{
    public interface IFileScanner
    {
        void ScanPdfFiles(string date, string[] files, Action<string> onProgress, Action<ScannedPaymentData> onRowExtracted);
    }
}
