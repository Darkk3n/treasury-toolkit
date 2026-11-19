namespace TreasuryToolkit.Core.Models
{
    public class PaymentRowData
    {
        public string Date { get; set; }
        public string Vendor { get; set; }
        public string Concept { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Comment { get; set; }

        /// <summary>
        /// Number of consecutive source-PDF pages this row covers. Usually 1; greater than 1 when the
        /// scanner detects continuation pages (e.g. a receipt whose signature table spills onto a 2nd
        /// page) that must stay attached to the row's page instead of being split into their own file.
        /// </summary>
        public int PageCount { get; set; } = 1;
    }
}