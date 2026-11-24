using TreasuryToolkit.Infra.Services;

namespace TreasuryToolkit.Infra.Tests
{
    /// <summary>
    /// Runs the field extraction over page text captured from real receipts. Every name, amount, account,
    /// folio and date in the fixtures is made up; only the labels, spacing and line wraps are kept as the
    /// PDF text extractor produced them, since those are what the regexes depend on.
    /// To cover a new receipt layout: dump its page text, replace every real value, add it under
    /// Fixtures/ReceiptText and add a row below.
    /// </summary>
    public class ReceiptTextParsingTests
    {
        public static TheoryData<string, string, string, string, string, string> Receipts => new()
        {
            // fixture file,                               amount,      vendor,                     concept,      currency, date
            { "SameBankTransfer.txt",                      "12,345.67", "JUAN PEREZ LOPEZ",         "8842",       "MXN",    "20260115" },
            // Beneficiary name wraps onto the next line and "Dato no verificado" sits on a line of its own
            { "InterbankTransfer_UnverifiedOnOwnLine.txt", "23,456.78", "ACME INSTRUMENTOS",        "5531",       "MXN",    "20260115" },
            // "Dato no verificado" follows the wrapped "DE CV" on the same line
            { "InterbankTransfer_UnverifiedInline.txt",    "7,654.32",  "REDESFICTICIAS",           "FACT00123",  "MXN",    "20260115" },
            // "S A DE" / "CV" split across lines; "Referencia CIE" precedes the "Concepto CIE" line
            { "CiePayment.txt",                            "456.78",    "MENSAJERIA RAPIDA MEXICO", "1234567890", "MXN",    "20260115" },
        };

        [Theory]
        [MemberData(nameof(Receipts))]
        public void ExtractsPaymentFields(string fixture, string amount, string vendor, string concept, string currency, string date)
        {
            string text = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "ReceiptText", fixture));

            LocalFileScanner.ExtractPdfDataPoints(text, out string actualAmount, out string actualVendor,
                out string actualConcept, out string actualCurrency, out string actualDate);

            Assert.Equal(amount, actualAmount);
            Assert.Equal(vendor, actualVendor);
            Assert.Equal(concept, actualConcept);
            Assert.Equal(currency, actualCurrency);
            Assert.Equal(date, actualDate);
        }

        [Fact]
        public void PageWithoutKnownLabels_ExtractsNothing()
        {
            LocalFileScanner.ExtractPdfDataPoints("Estado de cuenta\nPágina 2 de 2\nDetalle de firmas",
                out string amount, out string vendor, out string concept, out string currency, out string date);

            Assert.Equal(string.Empty, amount);
            Assert.Equal(string.Empty, vendor);
            Assert.Equal(string.Empty, concept);
            Assert.Equal(string.Empty, currency);
            Assert.Equal(string.Empty, date);
        }
    }
}
