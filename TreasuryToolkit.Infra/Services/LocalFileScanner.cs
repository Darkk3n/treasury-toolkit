using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Core.Models;
using TreasuryToolkit.Core.Text;

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

                    // Read from page 1 only and copied onto every row of the file
                    string clientName = null;

                    for (int pageNum = 1; pageNum <= totalPages; pageNum++)
                    {
                        if (totalPages > 1)
                        {
                            onProgress?.Invoke($"Procesando {fileNameOnly} (Página {pageNum}/{totalPages})...");
                        }

                        string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum));

                        if (pageNum == 1)
                        {
                            clientName = ExtractClientName(pageText);
                        }

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
                            Currency = currency,
                            ClientName = clientName
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
        internal static string StripCompanySuffix(string rawVendorName) =>
            Regex.Split(rawVendorName,
                @"(?:RFC|Banco|Monto|Importe|Motivo|Concepto|Dato\s*no\s*verificado" +
                @"|\bMEXICOS\s*DERL\b" +                 // Explicitly targets a known problem string
                @"|\bS\s*DE?\s*R?L?\s*DE?\s*C?V?\b" +    // Dynamic catcher for S DE RL DE CV fragments
                @"|\bSA\s*DE\s*C?V?\b" +                 // Dynamic catcher for SA DE CV fragments
                @"|\bSAB\b(?=\s*(?:DE\b|$))" +           // SAB DE CV (sociedad anónima bursátil)
                @"|\bS\s*A\b(?=\s*(?:DE?\b|$))" +        // "SA"/"S A" cut off by a line wrap ("... SA", "... S A DE", "... SA D")
                @"|\s[SA]\s*$" +                         // lone "S"/"A" left by a cut-off "S.A."/"S.C."/"A.C."
                @"|\bSAPI\s*DE\s*C?V?\b" +               // Dynamic catcher for SAPI DE CV fragments
                @"|\bS\s*DE?R?L?\b" +                    // Catches "S DERL" or "S DE RL"
                @"|\bSC\b)",                             // Isolated SC boundary
                RegexOptions.IgnoreCase)[0];

        /// <summary>
        /// The account holder the receipt was issued to, with whitespace collapsed, or null when none is found:
        /// <list type="number">
        /// <item>"Nombre del Cliente:" (top of the page), value on the label's line.</item>
        /// <item>"Nombre del Ordenante:" (middle of the page, under the charge details), value on the label's
        /// line, or else on the line right above it: in some layouts the name is vertically centered next to
        /// the label and the text extractor emits it first, e.g.
        /// <c>EMPRESA DEMO SA DE CV</c> / <c>Nombre del Ordenante:</c>.</item>
        /// <item>"Contrato:" at the start of a line (top of the page), value on the label's line with the
        /// trailing contract number dropped, e.g. <c>Contrato:  EMPRESA DEMO SA DE CV 000012345678</c>.</item>
        /// </list>
        /// </summary>
        internal static string ExtractClientName(string pageText)
        {
            if (string.IsNullOrWhiteSpace(pageText)) return null;

            Match clientMatch = Regex.Match(pageText, @"Nombre\s+del\s+Cliente:[ \t]*([^\r\n]+)", RegexOptions.IgnoreCase);
            if (clientMatch.Success)
            {
                return CollapseWhitespace(clientMatch.Groups[1].Value);
            }

            Match ordererInlineMatch = Regex.Match(pageText, @"Nombre\s+del\s+Ordenante:[ \t]*([^\r\n]+)", RegexOptions.IgnoreCase);
            if (ordererInlineMatch.Success)
            {
                return CollapseWhitespace(ordererInlineMatch.Groups[1].Value);
            }

            // Label alone on its line: the name is the line above, unless that line is another "Label: value" field
            Match ordererAboveMatch = Regex.Match(pageText, @"([^\r\n]+)\r?\n[ \t]*Nombre\s+del\s+Ordenante:[ \t]*(?:\r?\n|$)", RegexOptions.IgnoreCase);
            if (ordererAboveMatch.Success && !ordererAboveMatch.Groups[1].Value.Contains(':'))
            {
                return CollapseWhitespace(ordererAboveMatch.Groups[1].Value);
            }

            // Only at the start of a line: other layouts carry a bare "... Contrato: 00000001" mid-line
            Match contractMatch = Regex.Match(pageText, @"^[ \t]*Contrato:[ \t]*([^\r\n]+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (contractMatch.Success)
            {
                // The contract number follows the name
                return CollapseWhitespace(Regex.Replace(contractMatch.Groups[1].Value, @"(?:^|\s+)\d{4,}\s*$", ""));
            }

            return null;
        }

        private static string CollapseWhitespace(string text)
        {
            string collapsed = Regex.Replace(text, @"\s+", " ").Trim();
            return collapsed.Length == 0 ? null : collapsed;
        }

        private static bool RowHasData(PaymentRowData row) =>
            !string.IsNullOrWhiteSpace(row.Amount)
            || !string.IsNullOrWhiteSpace(row.Vendor)
            || !string.IsNullOrWhiteSpace(row.Concept)
            || !string.IsNullOrWhiteSpace(row.Currency)
            || !string.IsNullOrWhiteSpace(row.Date);

        internal static string CleanseAndHealText(string rawText)
        {
            if (string.IsNullOrEmpty(rawText)) return string.Empty;

            // 1. Replace any literal tabs, carriage returns, or newlines with a single space
            string cleaned = Regex.Replace(rawText, @"[\r\n\t]+", " ");

            // 2. Fix inner-word splitting issues (e.g., " SERVICI OS" or "QUERETA RO")
            // This looks for an isolated trailing part of a word and welds it back to its core.
            // Short Spanish words ("REEMBOLSO DE GASTOS", "DISENO EN", "PROVEEDOR Y ASOCIADOS", "COMERCIAL E
            // INDUSTRIAL") are not word fragments, so they are left alone: neither welded onto the word before
            // them nor taking the next fragment ("... MEXICO Y C" stays as is).
            const string shortWords = "(?:DE|EN|LA|EL|AL|Y|E)";

            // 2a. A fragment followed by a single letter is one word split in two ("NE W" → "NEW",
            // "D E" → "DE"), so they are joined to each other rather than the fragment to the word before it
            cleaned = Regex.Replace(cleaned, $@"\b(?!{shortWords}\s)([A-Za-z]{{1,2}})\s(?!Y\b)([A-Za-z])\b", "$1$2", RegexOptions.IgnoreCase);

            cleaned = Regex.Replace(cleaned, $@"\b(?!{shortWords}\s)([A-Za-z]+)\s(?!{shortWords}\b)([A-Za-z]{{1,2}}\b)", "$1$2", RegexOptions.IgnoreCase);

            // 3. Fix words split by massive gaps (e.g., "LOGISTI   CO")
            cleaned = Regex.Replace(cleaned, @"([A-Za-z]+)\s{2,}([A-Za-z]+)", "$1$2");

            // 4. Collapse any remaining multi-spaces down to a single standard space and trim edges
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            return cleaned;
        }

        internal static void ExtractPdfDataPoints(string rawPdfText, out string amount, out string vendorName, out string reason, out string currency, out string date)
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
            // "Importe Divisa:" is the amount in foreign-currency transfers (next to "Importe USD:" / "Importe Cargo:")
            string amountPattern = @"(?:Importe(?:\s+a\s+enviar|\s+de\s+la\s+operaci[ó]n|\s+Divisa)?|Monto):\s*(?:\$\s*)?([0-9]{1,3}(?:,[0-9]{3})*\.[0-9]{2}|[0-9]+\.[0-9]{2})";

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

                // A long reason wraps onto the next line(s) ("INSCRIPCION CUARTO DE" / "PRIMARIA"). Skipped when a
                // "Referencia:" value shares the line, since the wrapped text could then be either column's.
                if (!Regex.IsMatch(rawReason, @"Referencia[^:]*:\s*\S", RegexOptions.IgnoreCase))
                {
                    int reasonLineEnd = reasonMatch.Groups[1].Index + reasonMatch.Groups[1].Length;
                    // Trimmed first: a wide gap left before "Referencia" would be read as a word split by the healer
                    cleanReason = cleanReason.TrimEnd() + " " + string.Join(" ", WrappedReasonLinesAfter(rawPdfText, reasonLineEnd + 1));
                }

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
            //     EMPRESA DEMO MEXICO I                                           <- ordering, line 1
            //     PROVEEDOR EJEMPLO SA                                            <- beneficiary, line 1
            //     Titular de la cuenta: NDUSTRIAL SERVICIO S Titular de la cuenta:
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
                string rawVendorName = structuralMatch.Groups[1].Value + WrappedPayeeNameRemainder(rawPdfText, structuralMatch);

                vendorName = CleanTwoColumnPayeeName(rawVendorName);
            }
            else
            {
                // "Cuenta Abono:" or, in foreign-currency transfers, "Cuenta de" wrapped onto the line above "Abono:"
                string cuentaAbonoPattern = @"Cuenta\s+(?:de\s+)?Abono:\s*(?:[\d\s-]+\s+)?(?:-\s+)?([A-ZÁÉÍÓÚÑ\s]+)";
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
            // e.g. "Asunto beneficiario: 000000000000000000 Nombre del beneficiario: ..."). Only kicks
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

            // Foreign-currency transfers: "Divisa Abono: USD" is the payment's currency. Checked first because
            // the only other "Divisa:" on that page is "Importe Divisa:", which carries the amount.
            Match creditCurrencyMatch = Regex.Match(rawPdfText, @"Divisa\s+Abono:[ \t]*([A-Za-z]{3})\b", RegexOptions.IgnoreCase);

            if (creditCurrencyMatch.Success)
            {
                currency = creditCurrencyMatch.Groups[1].Value.ToUpperInvariant();
                if (currency == "MXP") { currency = "MXN"; }
            }
            else if (currencyMatch.Success)
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

        private static readonly HashSet<string> LegalSuffixWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "S", "SA", "DE", "RL", "C", "V", "CV", "SAPI", "SC"
        };

        /// <summary>
        /// TWO-COLUMN LAYOUT: both "Titular de la cuenta:" labels share one line and each name wraps onto the
        /// lines below, where the text extractor emits the ordering party's remainder first and the payee's
        /// after it on the same line, e.g.
        /// <c>Titular de la cuenta: EMPRESA DEMO S DE RL DE  Titular de la cuenta: ARRENDADORA EJEMPLO DEL</c> /
        /// <c>CV CENTRO</c>. The ordering party is the client, so the leading words that continue the client's
        /// name ("Nombre del Cliente:", wrapped lines included), plus the legal suffix up to "CV" that the
        /// header may leave out, belong to the left column; the rest of each line is the payee's (" CENTRO").
        /// Continuation lines start at the left margin and hold no label; the next field line is indented.
        /// Returns an empty string when there is nothing to add.
        /// </summary>
        internal static string WrappedPayeeNameRemainder(string pageText, Match twoLabelMatch)
        {
            // Labels on separate lines are a different layout
            if (twoLabelMatch.Value.Contains('\n')) return string.Empty;

            int lineStart = pageText.LastIndexOf('\n', twoLabelMatch.Index) + 1;
            int lineEnd = pageText.IndexOf('\n', twoLabelMatch.Index);
            if (lineEnd < 0) return string.Empty;

            Match columnsMatch = Regex.Match(pageText[lineStart..lineEnd],
                @"(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s+beneficiario):(.*?)(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s+beneficiario):",
                RegexOptions.IgnoreCase);
            if (!columnsMatch.Success) return string.Empty;

            string client = CompanyNameMatcher.Normalize(ExtractWrappedClientName(pageText));
            string left = CompanyNameMatcher.Normalize(columnsMatch.Groups[1].Value);
            var payee = new System.Text.StringBuilder();

            foreach (string line in WrappedLinesAfter(pageText, lineEnd + 1))
            {
                string[] tokens = Regex.Replace(line, @"Dato\s*no\s*verificado.*$", "", RegexOptions.IgnoreCase)
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                int taken = 0;
                while (taken < tokens.Length)
                {
                    string candidate = left + CompanyNameMatcher.Normalize(tokens[taken]);
                    bool continuesClient = client.Length > 0 && client.StartsWith(candidate, StringComparison.Ordinal);
                    bool trailingSuffix = (client.Length == 0 || left.StartsWith(client, StringComparison.Ordinal))
                        && !left.EndsWith("CV", StringComparison.Ordinal)
                        && LegalSuffixWords.Contains(tokens[taken]);

                    if (!continuesClient && !trailingSuffix) break;

                    left = candidate;
                    taken++;
                }

                foreach (string token in tokens.Skip(taken))
                {
                    payee.Append(' ').Append(token);
                }
            }

            return payee.ToString();
        }

        /// <summary>The bank cuts payee names in the two-column layout at this many characters.</summary>
        private const int PayeeNameMaxLength = 30;

        /// <summary>
        /// <see cref="StripCompanySuffix"/> + <see cref="CleanseAndHealText"/>, except that a 1–2 letter word
        /// ending a name the bank cut at <see cref="PayeeNameMaxLength"/> characters is what is left of the
        /// last word ("SEGUROS EJEMPLO NE W YORK LI" → "... NEW YORK LI"), not a fragment to weld onto the word
        /// before it. Only when no suffix was cut off: "... QUERETA RO SA" still heals to "QUERETARO".
        /// </summary>
        internal static string CleanTwoColumnPayeeName(string rawName)
        {
            string stripped = StripCompanySuffix(rawName);
            Match cutOffWord = Regex.Match(rawName, @"\s([A-Za-z]{1,2})\s*$");

            bool cutAtMaxLength = Regex.Replace(rawName, @"\s+", " ").Trim().Length == PayeeNameMaxLength;
            if (cutAtMaxLength && cutOffWord.Success && stripped.Trim() == rawName.Trim())
            {
                return CleanseAndHealText(rawName[..cutOffWord.Index]) + " " + cutOffWord.Groups[1].Value;
            }

            // A suffix cut right after a comma ("... ONAL,SA D") leaves the comma behind
            return CleanseAndHealText(stripped).TrimEnd(',', ' ');
        }

        /// <summary>The full "Nombre del Cliente:" value, wrapped lines included, or an empty string.</summary>
        private static string ExtractWrappedClientName(string pageText)
        {
            Match clientMatch = Regex.Match(pageText, @"Nombre\s+del\s+Cliente:[ \t]*([^\r\n]*)", RegexOptions.IgnoreCase);
            if (!clientMatch.Success) return string.Empty;

            int nextLine = pageText.IndexOf('\n', clientMatch.Index);
            return nextLine < 0
                ? clientMatch.Groups[1].Value
                : string.Join(" ", WrappedLinesAfter(pageText, nextLine + 1).Prepend(clientMatch.Groups[1].Value));
        }

        /// <summary>
        /// The lines from <paramref name="start"/> on that continue a wrapped value: non-blank, starting at the
        /// left margin and holding no "Label:" (a trailing "Dato no verificado..." aside). Stops at the first other.
        /// </summary>
        private static IEnumerable<string> WrappedLinesAfter(string pageText, int start)
        {
            if (start >= pageText.Length) yield break;

            foreach (string rawLine in pageText[start..].Split('\n'))
            {
                string line = rawLine.TrimEnd('\r');
                bool isWrapped = line.Length > 0
                    && !char.IsWhiteSpace(line[0])
                    && !Regex.Replace(line, @"Dato\s*no\s*verificado.*$", "", RegexOptions.IgnoreCase).Contains(':');

                if (!isWrapped) yield break;
                yield return line;
            }
        }

        /// <summary>
        /// The wrapped lines of a reason. Besides <see cref="WrappedLinesAfter"/>'s rules they must be upper case:
        /// reasons are printed in capitals, while unindented lines that are not part of them are section headers
        /// ("Datos de confirmación de la transferencia") or the start of a label wrapped before its colon
        /// ("Fecha y hora de" / "Alta: ...").
        /// </summary>
        private static IEnumerable<string> WrappedReasonLinesAfter(string pageText, int start) =>
            WrappedLinesAfter(pageText, start)
                .TakeWhile(line => !line.Any(char.IsLower) && !line.StartsWith("DETALLES", StringComparison.Ordinal));

        internal static string CleanVendorName(string rawVendor)
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
