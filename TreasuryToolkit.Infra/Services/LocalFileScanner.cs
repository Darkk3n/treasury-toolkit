using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.Infra.Services
{
    public class LocalFileScanner : IFileScanner
    {
        public void ScanPdfFiles(string[] files, Action<string> onProgress, Action<ScannedPaymentData> onRowExtracted)
        {
            for (int i = 0; i < files.Length; i++)
            {
                string filePath = files[i];
                string fileNameOnly = Path.GetFileName(filePath);
                int fileHumanIndex = i + 1;

                onProgress?.Invoke($"Procesando Archivo {fileHumanIndex} de {files.Length}: {fileNameOnly}...");

                try
                {
                    using PdfReader pdfReader = new(filePath);
                    using PdfDocument pdfDoc = new(pdfReader);
                    int totalPages = pdfDoc.GetNumberOfPages();

                    // Rows are held back one page so a following continuation page can be folded into them.
                    ScannedPaymentData pendingRow = null;

                    for (int pageNum = 1; pageNum <= totalPages; pageNum++)
                    {
                        if (totalPages > 1)
                        {
                            onProgress?.Invoke($"Procesando {fileNameOnly} (Página {pageNum}/{totalPages})...");
                        }

                        string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum));

                        string amount = string.Empty, vendor = string.Empty, concept = string.Empty, currency = string.Empty, date = string.Empty;
                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            ExtractPdfDataPoints(pageText, out amount, out vendor, out concept, out currency, out date);
                        }

                        bool pageHasNoData = string.IsNullOrWhiteSpace(amount)
                            && string.IsNullOrWhiteSpace(vendor)
                            && string.IsNullOrWhiteSpace(concept)
                            && string.IsNullOrWhiteSpace(currency)
                            && string.IsNullOrWhiteSpace(date);

                        // CONTINUATION PAGE: nothing to extract, right after a page of the same file that
                        // did yield data (e.g. an interbank transfer receipt whose "Detalle de firmas" table spills
                        // onto page 2). It belongs to that receipt, so extend its row instead of creating a
                        // new one — the processor then keeps both pages together in a single output PDF.
                        // A page with no data that does NOT follow a recognized page (unknown layout) still
                        // gets its own empty row so the user can fill it in manually.
                        if (pageHasNoData && pendingRow != null && RowHasData(pendingRow))
                        {
                            pendingRow.PageCount++;
                            continue;
                        }

                        if (pendingRow != null) onRowExtracted?.Invoke(pendingRow);

                        pendingRow = new ScannedPaymentData
                        {
                            Date = date,
                            FileName = fileNameOnly,
                            Vendor = vendor,
                            Concept = concept,
                            Amount = amount,
                            Currency = currency
                        };
                    }

                    if (pendingRow != null) onRowExtracted?.Invoke(pendingRow);
                }
                catch (Exception ex)
                {
                    // 🚀 Fallback error payload to show in the UI grid
                    onRowExtracted?.Invoke(new ScannedPaymentData
                    {
                        Date = DateTime.Now.ToString("yyyyMMdd"),
                        FileName = fileNameOnly,
                        Vendor = "ERROR",
                        Concept = "Failed to parse file",
                        Amount = ex.Message,
                        Currency = ""
                    });
                }
            }
        }

        /// <summary>
        /// Cuts a raw vendor string at the first adjacent field label or legal-entity suffix
        /// (SA DE CV, S DE RL DE CV, SAPI DE CV, SC...), keeping only the company's core name.
        /// </summary>
        private static string StripCompanySuffix(string rawVendorName) =>
            Regex.Split(rawVendorName,
                @"(?:RFC|Banco|Monto|Importe|Motivo|Concepto|Dato\s*no\s*verificado" +
                @"|\bMEXICOS\s*DERL\b" +                 // Explicitly targets a known problem string
                @"|\bS\s*DE?\s*R?L?\s*DE?\s*C?V?\b" +    // Dynamic catcher for S DE RL DE CV fragments
                @"|\bSA\s*DE\s*C?V?\b" +                 // Dynamic catcher for SA DE CV fragments
                @"|\bS\s*A\b(?=\s*(?:DE\b|$))" +         // "SA"/"S A" cut off by a line wrap ("... SA", "... S A DE")
                @"|\bSAPI\s*DE\s*C?V?\b" +               // Dynamic catcher for SAPI DE CV fragments
                @"|\bS\s*DE?R?L?\b" +                    // Catches "S DERL" or "S DE RL"
                @"|\bSC\b)",                             // Isolated SC boundary
                RegexOptions.IgnoreCase)[0];

        private static bool RowHasData(PaymentRowData row) =>
            !string.IsNullOrWhiteSpace(row.Amount)
            || !string.IsNullOrWhiteSpace(row.Vendor)
            || !string.IsNullOrWhiteSpace(row.Concept)
            || !string.IsNullOrWhiteSpace(row.Currency)
            || !string.IsNullOrWhiteSpace(row.Date);

        private static string CleanseAndHealText(string rawText)
        {
            if (string.IsNullOrEmpty(rawText)) return string.Empty;

            // 1. Replace any literal tabs, carriage returns, or newlines with a single space
            string cleaned = Regex.Replace(rawText, @"[\r\n\t]+", " ");

            // 2. Fix inner-word splitting issues (e.g., " SERVICI OS" or "QUERETA RO")
            // This looks for an isolated trailing part of a word and welds it back to its core.
            cleaned = Regex.Replace(cleaned, @"([A-Za-z]+)\s([A-Za-z]{1,2}\b)", "$1$2");

            // 3. Fix words split by massive gaps (e.g., "LOGISTI   CO")
            cleaned = Regex.Replace(cleaned, @"([A-Za-z]+)\s{2,}([A-Za-z]+)", "$1$2");

            // 4. Collapse any remaining multi-spaces down to a single standard space and trim edges
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            return cleaned;
        }

        private static void ExtractPdfDataPoints(string rawPdfText, out string amount, out string vendorName, out string reason, out string currency, out string date)
        {
            amount = string.Empty;
            vendorName = string.Empty;
            reason = string.Empty;
            currency = string.Empty;
            date = string.Empty;

            // --- 1. REAL AMOUNT EXTRACTION (Importe) ---
            // This pattern captures "Importe:", any spaces, and numbers formatted like 12,345.00
            // The \. ensures it looks for a literal period right before the cents
            #region Amount
            string amountPattern = @"(?:Importe(?:\s+a\s+enviar|\s+de\s+la\s+operaci[ó]n)?|Monto):\s*(?:\$\s*)?([0-9]{1,3}(?:,[0-9]{3})*\.[0-9]{2}|[0-9]+\.[0-9]{2})";

            Match amountMatch = Regex.Match(rawPdfText, amountPattern, RegexOptions.IgnoreCase);
            if (amountMatch.Success)
            {
                amount = amountMatch.Groups[1].Value; // Captures "12,345.00"
            }
            #endregion

            // 2. REASON EXTRACTION (The Confident Greedy Fix)
            // Changing +? to + forces it to grab the whole phrase on that line.
            // The lookahead ensures that if "Referencia" is present, it stops right before it.
            #region Reason
            Match reasonMatch = Regex.Match(
                rawPdfText,
                @"(?<!\bdel\s)(?:Motivo(?:\s+de\s+pago)?|Concepto(?:\s+de\s+pago)?|Concepto\s+CIE|Detalle\s+de\s+pago|Referencia\s+del\s+Beneficiario):\s*([^\r\n]+)",
                RegexOptions.IgnoreCase);

            if (reasonMatch.Success)
            {
                string rawReason = reasonMatch.Groups[1].Value;

                // Step 2: Drop the blade if the word "Referencia" exists on that line, 
                // throwing away the reference numbers and keeping only the reason text.
                string cleanReason = Regex.Split(rawReason, @"Referencia", RegexOptions.IgnoreCase)[0];

                // Step 3: Run it through the healer to fix spacing or layout bugs
                reason = CleanseAndHealText(cleanReason);
            }
            #endregion

            // 3. VENDOR NAME EXTRACTION (Tuned with STOP boundary and fallback)
            // Captures everything after the label but STOPS looking if it hits numbers, colons, or "Dato no verificado"
            #region Vendor
            string structuralPattern = @"(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s+beneficiario):.*?(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s:beneficiario):\s*([^\r\n:]+)";

            // TWO-COLUMN, VERTICALLY-CENTERED LAYOUT (interbank transfer receipts)
            // Both "Titular de la cuenta:" labels share one line and each name wraps around it, so the
            // text extractor interleaves the two columns line by line, e.g.:
            //     CARL BECHEM MEXICO L                                            <- ordering, line 1
            //     EUROPARTNERS MEXICO SA                                          <- beneficiary, line 1
            //     Titular de la cuenta: UBRICATION TECHNOLOG Y S Titular de la cuenta:
            //     DE CV                                                           <- beneficiary, line 2
            //     DE RL DE CV                                                     <- ordering, line 3
            // The 2nd label ends its line, so the generic pattern would grab "DE CV" from the next
            // line. Here the beneficiary is the line right above the label line plus the line right below.
            Match centeredColumnsMatch = Regex.Match(
                rawPdfText,
                @"([^\r\n]+)\r?\n[^\r\n]*(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s+beneficiario):[^\r\n]*(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s+beneficiario):[ \t]*\r?\n([^\r\n]+)",
                RegexOptions.IgnoreCase);

            Match structuralMatch = Regex.Match(rawPdfText, structuralPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (centeredColumnsMatch.Success)
            {
                string rawVendorName = $"{centeredColumnsMatch.Groups[1].Value} {centeredColumnsMatch.Groups[2].Value}";
                vendorName = CleanseAndHealText(StripCompanySuffix(rawVendorName));
            }
            else if (structuralMatch.Success)
            {
                string rawVendorName = structuralMatch.Groups[1].Value;

                vendorName = CleanseAndHealText(StripCompanySuffix(rawVendorName));
            }
            else
            {
                string cuentaAbonoPattern = @"Cuenta\s+Abono:\s*(?:[\d\s-]+\s+)?(?:-\s+)?([A-ZÁÉÍÓÚÑ\s]+)";
                Match cuentaAbonoMatch = Regex.Match(rawPdfText, cuentaAbonoPattern, RegexOptions.IgnoreCase);

                if (cuentaAbonoMatch.Success)
                {
                    vendorName = CleanVendorName(cuentaAbonoMatch.Groups[1].Value.Trim());
                }
                else
                {
                    // Fallback: Single match structural processing
                    string fallbackPattern = @"(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s+beneficiario):\s*([^\r\n:]+)";
                    Match fallbackMatch = Regex.Match(rawPdfText, fallbackPattern, RegexOptions.IgnoreCase);

                    if (fallbackMatch.Success)
                    {
                        vendorName = CleanseAndHealText(StripCompanySuffix(fallbackMatch.Groups[1].Value));
                    }
                }
            }

            // 3b. UNVERIFIED-LABEL FALLBACK
            // Some receipts print "Nombre del beneficiario: Dato no verificado por esta institución"
            // with no name after the label at all — the real payee name instead sits on its own line
            // above the label's line (the label itself is often preceded on that line by other text,
            // e.g. "Asunto beneficiario: 021180070037622911 Nombre del beneficiario: ..."). Only kicks
            // in when nothing above found a name.
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                Match precedingLineMatch = Regex.Match(
                    rawPdfText,
                    @"([^\r\n]+)\r?\n[^\r\n]*(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s+beneficiario):\s*Dato\s*no\s*verificado",
                    RegexOptions.IgnoreCase);

                if (precedingLineMatch.Success)
                {
                    vendorName = CleanseAndHealText(precedingLineMatch.Groups[1].Value);
                }
            }
            #endregion

            // --- 4. CURRENCY EXTRACTION (The Grand Finale) ---
            // Structural Sweep: Skip the 1st "Divisa", capture everything on the line of the 2nd "Divisa"
            #region Currency
            string currencyPattern = @"(?:Divisa(?:\s+de\s+la\s+cuenta)?|Moneda):.*?(?:Divisa(?:\s+de\s+la\s+cuenta)?|Moneda):\s*([^\r\n:]+)";
            Match currencyMatch = Regex.Match(rawPdfText, currencyPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (currencyMatch.Success)
            {
                string rawCurrency = currencyMatch.Groups[1].Value;

                // Chop off any adjacent headers if the layout merges text lines
                string cleanCurrency = Regex.Split(rawCurrency, @"(?:Importe|Titular|RFC|Banco|Motivo|$)", RegexOptions.IgnoreCase)[0];

                currency = CleanseAndHealText(cleanCurrency); // Will return "MXN", "USD", etc.
                if (currency == "MXP") { currency = "MXN"; }
            }
            else
            {
                Match specialCurrencyMatch = Regex.Match(rawPdfText, @"Importe\s+a\s+enviar:\s*[0-9.,]+\s+([A-Za-z]+)", RegexOptions.IgnoreCase);
                if (specialCurrencyMatch.Success)
                {
                    currency = CleanseAndHealText(specialCurrencyMatch.Groups[1].Value);
                    if (currency == "MXP") { currency = "MXN"; }
                }
                // Fallback: If a PDF layout only has one single Currency label on the whole page
                else
                {
                    Match singleCurrencyMatch = Regex.Match(rawPdfText, @"(?:Divisa(?:\s+de\s+la\s+cuenta)?|Moneda):\s*([^\r\n:]+)", RegexOptions.IgnoreCase);
                    if (singleCurrencyMatch.Success)
                    {
                        string singleCurrency = Regex.Split(singleCurrencyMatch.Groups[1].Value, @"(?:Importe|Titular|RFC|Banco|Motivo|$)", RegexOptions.IgnoreCase)[0];
                        currency = CleanseAndHealText(singleCurrency);
                        if (currency == "MXP") { currency = "MXN"; }
                    }
                }
            }

            // 4b. SPID-SERVICE USD FALLBACK
            // Some receipts carry no Divisa/Moneda field at all, but a "Descripción del servicio: SPID"
            // tag marks the transfer as a USD-denominated SPID (US Dollar clearing) payment.
            // Last-resort check — only runs when every strategy above found nothing.
            if (string.IsNullOrWhiteSpace(currency))
            {
                Match spidServiceMatch = Regex.Match(
                    rawPdfText,
                    @"Descripci[oó]n\s+del\s+servicio:\s*SPID\b",
                    RegexOptions.IgnoreCase);

                if (spidServiceMatch.Success)
                {
                    currency = "USD";
                }
            }
            #endregion

            // --- 4. DATE EXTRACTION ---
            #region Date
            string lineWrappedDatePattern = @"(?:Fecha\s+y\s+hora\s+de\s+Liquidac(?:ió|i)n|Fecha\s+aplicac(?:ió|i)n):\s*([0-9]{1,2}[\/\-][0-9]{1,2}[\/\-][0-9]{2,4}|[0-9]{1,2}\s+de\s+[A-Za-záéíóúÁÉÍÓÚ]+\s+de\s+[0-9]{4}|[0-9]{1,2}[\/\-][A-Za-z]{3}[\/\-][0-9]{2,4})";

            Match lineWrappedMatch = Regex.Match(rawPdfText, lineWrappedDatePattern, RegexOptions.IgnoreCase);
            if (lineWrappedMatch.Success)
            {
                string extractedDate = lineWrappedMatch.Groups[1].Value;             
                DateTime parsedDate = DateTime.ParseExact(extractedDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                date = parsedDate.ToString("yyyyMMdd");
            }
            else
            {
                var datePattern = @"(?:Fecha\s+de\s+(?:aplicación|liquidación|operación):?)\s*(?<date>\d{2}/\d{2}/\d{4})"
                    + @"|(?:Fecha\s+de)\s+(?<date>\d{2}/\d{2}/\d{4})[^\r\n]*[\r\n]+\s*liquidación:";

                Match dateMatch = Regex.Match(rawPdfText, datePattern, RegexOptions.IgnoreCase);
                if (dateMatch.Success)
                {
                    var rawDate = dateMatch.Groups[1].Value;
                    DateTime parsedDate = DateTime.ParseExact(rawDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    date = parsedDate.ToString("yyyyMMdd");
                }
            }
            #endregion
        }

        private static string CleanVendorName(string rawVendor)
        {
            if (string.IsNullOrWhiteSpace(rawVendor)) return string.Empty;

            string cleaned = CleanseAndHealText(rawVendor).Trim();

            // 1. If 'Importe' or other fields were merged onto the same line without spaces, strip them first
            cleaned = Regex.Split(cleaned, @"(?:Importe|Monto|Fecha|Referencia|$)", RegexOptions.IgnoreCase)[0].Trim();

            // 2. Known banking/payroll suffixes to remove from the end of the name
            string[] trailingNoiseKeywords =
            [
                "NOMINA",
                "NOM",
                "SPEI",
                "TEF",
                "PAGO",
                "TERCEROS",
                "PROVEEDOR",
                "SAPIDECV"
            ];

            foreach (var keyword in trailingNoiseKeywords)
            {
                // Matches the keyword at the end of the string ($), 
                // whether separated by space (\b) or directly glued onto the last word (?<=[A-ZÁÉÍÓÚÑ])
                string pattern = $@"(?:\b|(?<=[A-ZÁÉÍÓÚÑ]))" + Regex.Escape(keyword) + @"\s*$";
                cleaned = Regex.Replace(cleaned, pattern, "", RegexOptions.IgnoreCase).Trim();
            }

            return cleaned;
        }
    }
}
