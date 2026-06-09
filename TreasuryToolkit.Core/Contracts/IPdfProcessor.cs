using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.Core.Contracts
{
    public interface IPdfProcessor
    {
        void ProcessPaymentBatch(string[] files, List<PaymentRowData> rows, string companyName, int startConsecutive, Action<int, string> onRowProcessed);
    }
}