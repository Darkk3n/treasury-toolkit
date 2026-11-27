using CommunityToolkit.Mvvm.ComponentModel;
using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.Wpf.App.ViewModels
{
    /// <summary>One row of the PDF tool's grid: a scanned receipt, or a row the user added by hand.</summary>
    public partial class PaymentRowViewModel : ObservableObject
    {
        private string amount;

        [ObservableProperty]
        private string date;

        [ObservableProperty]
        private string fileName;

        [ObservableProperty]
        private string vendor;

        [ObservableProperty]
        private string concept;

        [ObservableProperty]
        private string currency;

        [ObservableProperty]
        private string comment;

        /// <summary>
        /// Used by the DataGrid's new-row line and by "Limpiar": a hand-added row covers one page and is dated
        /// today, like v1's DefaultValuesNeeded handler.
        /// </summary>
        public PaymentRowViewModel()
        {
#pragma warning disable MVVMTK0034
            date = DateTime.Now.ToString("yyyyMMdd");
#pragma warning restore MVVMTK0034
        }

        public PaymentRowViewModel(ScannedPaymentData scanned)
        {
#pragma warning disable MVVMTK0034
            date = scanned.Date;
            fileName = scanned.FileName;
            vendor = scanned.Vendor;
            concept = scanned.Concept;
            currency = scanned.Currency;
            comment = scanned.Comment;
#pragma warning restore MVVMTK0034

            // Kept exactly as scanned: when a page can't be read the scanner puts the error message here
            amount = scanned.Amount;
            PageCount = scanned.PageCount;
            ClientName = scanned.ClientName;
        }

        /// <summary>
        /// Amounts the user types are stored formatted as N2 (e.g. "1,234.50"), matching v1, where an edited cell
        /// became a double displayed with the "N2" format and that formatted text went into the file name.
        /// Non-numeric input never gets here: AmountValidationRule rejects it while the cell is being edited.
        /// </summary>
        public string Amount
        {
            get => amount;
            set => SetProperty(ref amount, double.TryParse(value, out double number) ? number.ToString("N2") : value);
        }

        /// <summary>Consecutive source pages this row covers (continuation pages stay with their receipt).</summary>
        public int PageCount { get; } = 1;

        /// <summary>"Nombre del Cliente" from the source file's first page; null for hand-added rows or when absent.</summary>
        public string ClientName { get; }

        public PaymentRowData ToPaymentRowData() => new()
        {
            Date = Date,
            Vendor = Vendor,
            Concept = Concept,
            Amount = Amount,
            Currency = Currency,
            Comment = Comment ?? string.Empty,
            PageCount = PageCount
        };
    }
}
