namespace TreasuryToolkit.Core.Models
{
    public class ScannedPaymentData : PaymentRowData
    {
        public string FileName { get; set; }

        /// <summary>
        /// The "Nombre del Cliente" printed on the first page of the row's source file, or null when that page
        /// has no such label. Compared against the selected company before processing.
        /// </summary>
        public string ClientName { get; set; }
    }
}
