using TreasuryToolkit.Core.Text;
using TreasuryToolkit.Infra.Services;

namespace TreasuryToolkit.Infra.Tests
{
    /// <summary>
    /// "Nombre del Cliente" extraction and its comparison against the selected company. All names are made up.
    /// </summary>
    public class ClientNameTests
    {
        [Theory]
        [InlineData("Comprobante\nNombre del Cliente:      EMPRESA DEMO SA DE CV\nCuenta: 1234", "EMPRESA DEMO SA DE CV")]
        [InlineData("NOMBRE DEL CLIENTE: ACME   INSTRUMENTOS\r\n", "ACME INSTRUMENTOS")]
        [InlineData("Nombre del Cliente:\nEMPRESA DEMO", null)]   // nothing after the label on its line
        [InlineData("Titular de la cuenta: EMPRESA DEMO", null)]
        // Ordering-party label mid-page, name emitted on the line above the label (vertically centered layout)
        [InlineData("DETALLES DEL CARGO\nCuenta Ordenante: 1234567890\nEMPRESA DEMO MEXICO SA DE CV\nNombre del Ordenante:\nFP0001\nMoneda: MXN", "EMPRESA DEMO MEXICO SA DE CV")]
        [InlineData("Cuenta Ordenante: 1234567890\r\nEMPRESA DEMO\r\nNombre del Ordenante:\r\n", "EMPRESA DEMO")]
        [InlineData("Nombre del Ordenante: ACME INSTRUMENTOS SA\nMoneda: MXN", "ACME INSTRUMENTOS SA")]
        [InlineData("Cuenta Ordenante: 1234567890\nNombre del Ordenante:\nFP0001", null)]          // line above is another field
        [InlineData("Nombre del Cliente: EMPRESA DEMO\nOTRA EMPRESA\nNombre del Ordenante:", "EMPRESA DEMO")] // Cliente wins
        // Contract label at the top, name followed by the contract number
        [InlineData("Comprobante de Operación\nTipo de\nOperación:  TRANSFERENCIA INTERBANCARIA\nContrato:  EMPRESA DEMO DE MEXICO SA DE CV 000012345678\nUsuario:  00000001 - USUARIO DEMO", "EMPRESA DEMO DE MEXICO SA DE CV")]
        [InlineData("Contrato:  ACME INSTRUMENTOS SA\r\nUsuario:  1", "ACME INSTRUMENTOS SA")]
        [InlineData("Contrato:  000012345678\nUsuario:  1", null)]                                // number only
        [InlineData("Fecha y hora de consulta:  15/01/2026 10:30:00  PM Contrato: 00000001\nCuenta: 1", null)] // mid-line, not a name
        [InlineData("Fecha y hora de consulta:  15/01/2026  PM Contrato: 00000001\nNombre del Cliente: EMPRESA DEMO", "EMPRESA DEMO")]
        [InlineData("", null)]
        public void ExtractClientName_ReadsTheLabelValue(string pageText, string expected)
        {
            Assert.Equal(expected, LocalFileScanner.ExtractClientName(pageText));
        }

        [Theory]
        [InlineData("EMPRESA DEMO SA DE CV", "EMPRESA DEMO", true)]
        [InlineData("ACME SISTEMAS PACK S.A. DE C.V.", "ACME SISTEMASPACK", true)]  // spacing and dots ignored
        [InlineData("acuídad méxico", "ACUIDAD", true)]                             // case and accents ignored
        [InlineData("EMPRESA DEMO SA DE CV", "OTRA EMPRESA", false)]
        [InlineData("EMPRESA DEMO", "", false)]
        public void CompanyNameMatcher_ChecksTheCompanyIsContained(string found, string company, bool expected)
        {
            Assert.Equal(expected, CompanyNameMatcher.Contains(found, company));
        }
    }
}
