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
            // "Contrato:" header layout; payee from "Cuenta Abono:", whose name has the conjunction "Y"
            { "InterbankTransfer_ContractHeader.txt",      "9,876.54",  "PEREZ Y LOPEZ ASOCIADOS",  "4321",       "MXN",    "20260115" },
            // Same layout, same-bank transfer: account-holder prefix and "NOM" noise around the payee; date label wrapped
            { "SameBankTransfer_ContractHeader.txt",       "1,234.56",  "JUAN PEREZ LOPEZ",         "REEMBOLSO DE GASTOS SEM 03", "MXN", "20260115" },
            // Foreign-currency transfer: "Cuenta de"/"Abono:" wrapped, amount from "Importe Divisa:", currency from "Divisa Abono:"
            { "ForeignCurrencyTransfer_ContractHeader.txt", "3,456.78", "PROVEEDOR INTERNATIONAL INC", "000000001 TRAVEL EXPENSES Q1", "USD", "20260115" },
            // Browser-printed receipt: both "Titular de la cuenta:" labels on one line, names wrapped around it
            { "InterbankTransfer_PrintedFromBrowser.txt",  "2,345.00",  "PROVEEDOR EJEMPLO MEXICO", "FCUS000001", "USD",    "20260115" },
            // Charged in USD, sent in EUR: amount and currency from "Importe a enviar:"
            { "InternationalPayment_ForeignCurrency.txt",  "1,000.00",  "PROVEEDOR EUROPA GMBH",    "INVOICE 0000000001", "EUR", "20260115" },
            // SPEI-sent list; the payee name ends in "Y C" (cut off), which must not be welded together
            { "SpeiSent_OrdererNameAboveLabel.txt",        "345.67",    "MENSAJERIA GLOBAL MEXICO Y C", "FDO0000001", "MXN", "20260115" },
            // SPID-sent list: "Fecha de" / date / "liquidación:" over three lines; concept on the line below its label
            { "SpidSent_SettlementDateWrapped.txt",        "456.78",    "PROVEEDOR LOGISTICO SOLUTIONS", "FI01",   "USD",    "20260115" },
            // Two-column layout, payee name wrapped: "CV" on the next line ends the client's name, the rest is the payee's
            { "SameBankTransfer_PayeeNameWrapped.txt",     "4,321.09",  "MARIA ELENA GOMEZ DE LA ROSA", "REEMBOLSO", "MXN",    "20260115" },
            // ...wrapped over two lines, the client's own header name wrapped too and missing the "DE CV" of the column
            { "InterbankTransfer_PayeeNameWrappedTwoLines.txt", "8,765.43", "ARRENDADORA EJEMPLO DEL CENTRO", "RQRO0001 0002 03", "MXN", "20260115" },
            // Third-party transfer: concept from "Referencia del Beneficiario:", next field merged onto the payee line
            { "ThirdPartyTransfer_Usd.txt",                "1,234.56",  "EVENTOS EJEMPLO",          "123456",     "USD",    "20260115" },
        };

        public static TheoryData<string, string> ClientNames => new()
        {
            { "SameBankTransfer.txt",                       "EMPRESA DEMO MEXICO S DE RL DE CV" },        // "Nombre del Cliente:"
            { "InterbankTransfer_ContractHeader.txt",       "EMPRESA DEMO DE MEXICO SA DE CV" },          // "Contrato:", number dropped
            { "SameBankTransfer_ContractHeader.txt",        "EMPRESA DEMO INSTALACIONES MEXICO S DE RL" },
            { "ForeignCurrencyTransfer_ContractHeader.txt", "EMPRESA DEMO DE MEXICO SA DE CV" },
            { "SpeiSent_OrdererNameAboveLabel.txt",         "EMPRESA DEMO MEXICO S DE RL DE" },           // name above "Nombre del Ordenante:"
            { "SpidSent_SettlementDateWrapped.txt",         "EMPRESA DEMO MEXICO S DE RL DE" },
            { "ThirdPartyTransfer_Usd.txt",                 "EMPRESA DEMO MEXICO S DE RL DE" },
            { "InterbankTransfer_PrintedFromBrowser.txt",   null },                                       // "Nombre del Cliente" without a colon
        };

        [Theory]
        [MemberData(nameof(ClientNames))]
        public void ExtractsClientName(string fixture, string clientName)
        {
            string text = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "ReceiptText", fixture));

            Assert.Equal(clientName, LocalFileScanner.ExtractClientName(text));
        }

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
