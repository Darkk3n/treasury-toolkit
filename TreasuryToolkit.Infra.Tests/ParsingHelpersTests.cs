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
        [InlineData("PEREZ Y LOPEZ", "PEREZ Y LOPEZ")]             // conjunction "Y" is not a fragment
        [InlineData("COMERCIAL E INDUSTRIAL", "COMERCIAL E INDUSTRIAL")] // nor is "E"
        [InlineData("HOLDINGS MEXICO Y C", "HOLDINGS MEXICO Y C")]   // ...and it doesn't take the next fragment
        [InlineData("REEMBOLSO DE GASTOS", "REEMBOLSO DE GASTOS")]   // nor are DE / EN / LA / EL / AL
        [InlineData("FELIPE DE LA SERNA", "FELIPE DE LA SERNA")]
        [InlineData("LINEA EN VIAJES", "LINEA EN VIAJES")]
        [InlineData("SERVICIOS DE QUERETA RO", "SERVICIOS DE QUERETARO")] // a real fragment after them still heals
        [InlineData("SEGUROS EJEMPLAR NE W VIDAS", "SEGUROS EJEMPLAR NEW VIDAS")] // fragment + single letter: one word
        [InlineData("COMPRESORES D E MEXICO", "COMPRESORES DE MEXICO")]
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
        [InlineData("TELECOMUNICACIONES DEMO  SAB DE CV", "TELECOMUNICACIONES DEMO")]
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

        [Theory]
        // Client's column already complete: a payee's own legal suffix on the next line stays with the payee
        [InlineData("Nombre del Cliente: EMPRESA DEMO S DE RL DE CV\n  Titular de la cuenta: EMPRESA DEMO S DE RL DE CV  Titular de la cuenta: MENSAJERIA EJEMPLO SA DE\nCV\n  Fecha de creación: 15/01/2026\n",
            "MENSAJERIA EJEMPLO")]
        // ...and so does a payee line starting with "DE"
        [InlineData("Nombre del Cliente: EMPRESA DEMO S DE RL DE CV\n  Titular de la cuenta: EMPRESA DEMO S DE RL DE CV  Titular de la cuenta: JUAN PEREZ\nDE LA ROSA\n  Fecha de creación: 15/01/2026\n",
            "JUAN PEREZ DE LA ROSA")]
        // No client name on the page: only legal-suffix words are given to the left column
        [InlineData("  Titular de la cuenta: EMPRESA DEMO S DE RL DE  Titular de la cuenta: JUAN PEREZ\nCV LOPEZ\n  Fecha de creación: 15/01/2026\n",
            "JUAN PEREZ LOPEZ")]
        // Cut-off "S.C." leaves a lone "S" that is not glued onto the name
        [InlineData("Nombre del Cliente: EMPRESA DEMO S DE RL DE CV\n  Titular de la cuenta: EMPRESA DEMO S DE RL DE  Titular de la cuenta: SERVICIOS EJEMPLO\nCV CORPORATIVOS S\n  Fecha de creación: 15/01/2026\n",
            "SERVICIOS EJEMPLO CORPORATIVOS")]
        // Nothing wrapped: the next line is an indented field
        [InlineData("Nombre del Cliente: EMPRESA DEMO\n  Titular de la cuenta: EMPRESA DEMO  Titular de la cuenta: JUAN PEREZ LOPEZ\n  Fecha de creación: 15/01/2026\n",
            "JUAN PEREZ LOPEZ")]
        public void Vendor_TwoColumnPayeeNameWrapped_TakesThePayeesPartOfTheNextLines(string pageText, string expected)
        {
            LocalFileScanner.ExtractPdfDataPoints(pageText, out _, out string vendor, out _, out _, out _);

            Assert.Equal(expected, vendor);
        }

        [Theory]
        // Cut by the bank at 30 characters: the trailing "LI" is what's left of a word, not a fragment of "VIDAS"
        [InlineData("SEGUROS EJEMPLAR NE W VIDAS LI", "SEGUROS EJEMPLAR NEW VIDAS LI")]
        // Shorter than the limit: a trailing fragment still heals
        [InlineData("PROVEEDOR EJEMPLO QUERETA RO", "PROVEEDOR EJEMPLO QUERETARO")]
        // At the limit but a suffix was cut off: heals as usual
        [InlineData("SERVICIO EJEMPLO QUERETA RO SA", "SERVICIO EJEMPLO QUERETARO")]
        // Suffix cut right after a comma
        [InlineData("SERVICIOS INTERNACI ONAL,SA D", "SERVICIOS INTERNACI ONAL")]
        public void CleanTwoColumnPayeeName_KeepsWhatTheBankCutOff(string raw, string expected)
        {
            Assert.Equal(expected, LocalFileScanner.CleanTwoColumnPayeeName(raw));
        }

        [Theory]
        // Wrapped onto the next line, a section header after it
        [InlineData("  Hora de captura en el canal: 12:00:00  Motivo de pago: REEMBOLSO JUAN PEREZ DE\nLA ROSA\nDatos de confirmación de la transferencia\n  Folio de firma: 1\n",
            "REEMBOLSO JUAN PEREZ DE LA ROSA")]
        // An empty "Referencia:" shares the line; the gap before it must not weld the words
        [InlineData("  Concepto de pago: INSCRIPCION CUARTO DE  Referencia:\nPRIMARIA\n  Hora de captura en el canal: 12:00:00\n",
            "INSCRIPCION CUARTO DE PRIMARIA")]
        // A "Referencia:" with a value shares the line: the wrapped text could be either column's, so it is left out
        [InlineData("  Concepto de pago: INSCRIPCION CUARTO DE  Referencia: 1234\nPRIMARIA\n  Hora de captura en el canal: 12:00:00\n",
            "INSCRIPCION CUARTO DE")]
        // A label wrapped before its colon is not part of the reason
        [InlineData("Concepto:  NOMINA QUINCENAL 16\nFecha y hora de\nAlta:  15/01/2026 09:00:00\n", "NOMINA QUINCENAL 16")]
        // Value on the line below an empty label, then a section header
        [InlineData("Concepto de Pago:\nFI01 FI02\nClasificación de la Operación\n", "FI01 FI02")]
        public void Reason_WrappedOntoTheNextLine_IsJoined(string pageText, string expected)
        {
            LocalFileScanner.ExtractPdfDataPoints(pageText, out _, out _, out string reason, out _, out _);

            Assert.Equal(expected, reason);
        }

        [Fact]
        public void Vendor_CuentaDeAbonoWrappedOverTwoLines_IsTheNameAfterTheAccount()
        {
            LocalFileScanner.ExtractPdfDataPoints(
                "Cuenta de\nCargo:  00000000001 - EMPRESA DEMO SA DE CV\nCuenta de\nAbono:  0000000000002 - PROVEEDOR EJEMPLO INC\nImporte USD:  $ 1,000.00\n",
                out _, out string vendor, out _, out _, out _);

            Assert.Equal("PROVEEDOR EJEMPLO INC", vendor);
        }

        [Fact]
        public void Currency_DivisaAbono_WinsOverImporteDivisa()
        {
            LocalFileScanner.ExtractPdfDataPoints("Divisa Abono:  USD\nImporte USD:  $ 1,000.00\nImporte Divisa:  $ 1,000.00\n",
                out string amount, out _, out _, out string currency, out _);

            Assert.Equal("1,000.00", amount);
            Assert.Equal("USD", currency);
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
