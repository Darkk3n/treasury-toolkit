using TreasuryToolkit.Infra.Services;

namespace TreasuryToolkit.Infra.Tests
{
    /// <summary>
    /// The text-cleanup helpers behind the field extraction, using the cases described in their comments.
    /// All names are made up.
    /// </summary>
    public class ParsingHelpersTests
    {
        [Theory]
        [InlineData("SERVICI OS", "SERVICIOS")]                     // trailing fragment split off a word
        [InlineData("LOGISTI   CO", "LOGISTICO")]                   // word split by a wide gap
        [InlineData("ACME\r\n\tINSTRUMENTOS", "ACME INSTRUMENTOS")]  // line breaks and tabs become one space
        [InlineData("  ACME INSTRUMENTOS  ", "ACME INSTRUMENTOS")]   // edges trimmed
        [InlineData("", "")]
        public void CleanseAndHealText_RepairsLayoutArtifacts(string raw, string expected)
        {
            Assert.Equal(expected, LocalFileScanner.CleanseAndHealText(raw));
        }

        [Theory]
        [InlineData("ACME INSTRUMENTOS SA DE CV", "ACME INSTRUMENTOS")]
        [InlineData("EMPRESA DEMO S DE RL DE CV", "EMPRESA DEMO")]
        [InlineData("MENSAJERIA RAPIDA MEXICO S A DE DE CV CV", "MENSAJERIA RAPIDA MEXICO")]
        [InlineData("PROVEEDOR EJEMPLO SAPI DE CV", "PROVEEDOR EJEMPLO")]
        [InlineData("PROVEEDOR EJEMPLO RFC PEJ010101AAA", "PROVEEDOR EJEMPLO")]
        public void StripCompanySuffix_KeepsOnlyTheCoreName(string raw, string expected)
        {
            Assert.Equal(expected, LocalFileScanner.StripCompanySuffix(raw).Trim());
        }

        [Theory]
        [InlineData("PROVEEDOR EJEMPLO SPEI", "PROVEEDOR EJEMPLO")]          // payment-type noise at the end
        [InlineData("JUAN PEREZ LOPEZNOMINA", "JUAN PEREZ LOPEZ")]           // noise glued onto the last word
        [InlineData("ACME INSTRUMENTOS Importe: 1,000.00", "ACME INSTRUMENTOS")] // next field merged onto the line
        [InlineData("   ", "")]
        public void CleanVendorName_RemovesTrailingNoise(string raw, string expected)
        {
            Assert.Equal(expected, LocalFileScanner.CleanVendorName(raw));
        }

        [Fact]
        public void Currency_SpidServiceWithoutCurrencyField_IsUsd()
        {
            LocalFileScanner.ExtractPdfDataPoints("Importe: 1,000.00\nDescripción del servicio: SPID\n",
                out _, out _, out _, out string currency, out _);

            Assert.Equal("USD", currency);
        }

        [Fact]
        public void Currency_SingleMxpLabel_IsReportedAsMxn()
        {
            LocalFileScanner.ExtractPdfDataPoints("Importe: 1,000.00\nDivisa: MXP\n",
                out _, out _, out _, out string currency, out _);

            Assert.Equal("MXN", currency);
        }
    }
}
