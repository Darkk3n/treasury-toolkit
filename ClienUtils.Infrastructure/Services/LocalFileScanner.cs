using System.Text.RegularExpressions;
using ClientUtils.Core.Contracts;
using ClientUtils.Core.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace ClienUtils.Infrastructure.Services
{
    public class LocalFileScanner : IFileScanner
    {
        public void ScanPdfFiles(string date, string[] files, Action<string> onProgress, Action<ScannedPaymentData> onRowExtracted)
        {
            for (int i = 0; i < files.Length; i++)
            {
                string filePath = files[i];
                string fileNameOnly = Path.GetFileName(filePath);
                int fileHumanIndex = i + 1;

                // 📢 Report file progress back to the UI
                onProgress?.Invoke($"Procesando Archivo {fileHumanIndex} de {files.Length}: {fileNameOnly}...");

                try
                {
                    using PdfReader pdfReader = new(filePath);
                    using PdfDocument pdfDoc = new(pdfReader);
                    int totalPages = pdfDoc.GetNumberOfPages();

                    for (int pageNum = 1; pageNum <= totalPages; pageNum++)
                    {
                        if (totalPages > 1)
                        {
                            onProgress?.Invoke($"Procesando {fileNameOnly} (Página {pageNum}/{totalPages})...");
                        }

                        string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum));
                        if (string.IsNullOrWhiteSpace(pageText)) continue;

                        ExtractPdfDataPoints(
                            pageText,
                            out string amount,
                            out string vendor,
                            out string concept,
                            out string currency
                        );

                        onRowExtracted?.Invoke(new ScannedPaymentData
                        {
                            Date = date,
                            FileName = fileNameOnly,
                            Vendor = vendor,
                            Concept = concept,
                            Amount = amount,
                            Currency = currency
                        });
                    }
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

        private static void ExtractPdfDataPoints(string rawPdfText, out string amount, out string vendorName, out string reason, out string currency)
        {
            amount = string.Empty;
            vendorName = string.Empty;
            reason = string.Empty;
            currency = string.Empty;

            // --- 1. REAL AMOUNT EXTRACTION (Importe) ---
            // This pattern captures "Importe:", any spaces, and numbers formatted like 12,345.00
            // The \. ensures it looks for a literal period right before the cents
            #region Amount
            string amountPattern = @"(?:Importe:|Monto:)\s*([0-9.,]+\.[0-9]{2})";

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
            Match reasonMatch = Regex.Match(rawPdfText, @"(?:Motivo|Concepto)\s+de\s+pago:\s*([^\r\n]+)", RegexOptions.IgnoreCase);

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

            Match structuralMatch = Regex.Match(rawPdfText, structuralPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (structuralMatch.Success)
            {
                string rawVendorName = structuralMatch.Groups[1].Value;

                // THE CLEANER: We use a generalized boundary pattern at the end.
                // This catches variations of SA, CV, S, DE, RL, and SC dynamically.
                string cleanVendorName = Regex.Split(rawVendorName,
                    @"(?:RFC|Banco|Monto|Importe|Motivo|Concepto|Dato\s*no\s*verificado" +
                    @"|\bMEXICOS\s*DERL\b" +                 // Explicitly targets your exact problem string
                    @"|\bS\s*DE?\s*R?L?\s*DE?\s*C?V?\b" +    // Dynamic catcher for S DE RL DE CV fragments
                    @"|\bSA\s*DE\s*C?V?\b" +                 // Dynamic catcher for SA DE CV fragments
                    @"|\bS\s*DE?R?L?\b" +                    // Catches "S DERL" or "S DE RL"
                    @"|\bSC\b)",                             // Isolated SC boundary
                    RegexOptions.IgnoreCase)[0];

                vendorName = CleanseAndHealText(cleanVendorName);
            }
            else
            {
                // Fallback: Single match structural processing
                string fallbackPattern = @"(?:Titular\s+de\s+la\s+cuenta|Nombre\s+del\s+beneficiario):\s*([^\r\n:]+)";
                Match fallbackMatch = Regex.Match(rawPdfText, fallbackPattern, RegexOptions.IgnoreCase);

                if (fallbackMatch.Success)
                {
                    string singleMatch = fallbackMatch.Groups[1].Value;
                    singleMatch = Regex.Split(singleMatch,
                        @"(?:RFC|Banco|Monto|Importe|Motivo|Concepto|Dato\s*no\s*verificado" +
                        @"|\bMEXICOS\s*DERL\b" +
                        @"|\bS\s*DE?\s*R?L?\s*DE?\s*C?V?\b" +
                        @"|\bSA\s*DE\s*C?V?\b" +
                        @"|\bS\s*DE?R?L?\b" +
                        @"|\bSC\b)",
                        RegexOptions.IgnoreCase)[0];

                    vendorName = CleanseAndHealText(singleMatch);
                }
            }
            #endregion

            // --- 4. CURRENCY EXTRACTION (The Grand Finale) ---
            // Structural Sweep: Skip the 1st "Divisa", capture everything on the line of the 2nd "Divisa"
            #region Currency
            string currencyPattern = @"(?:Divisa\s+de\s+la\s+cuenta|Moneda):.*?(?:Divisa\s+de\s+la\s+cuenta|Moneda):\s*([^\r\n:]+)";
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
                // Fallback: If a PDF layout only has one single Currency label on the whole page
                Match singleCurrencyMatch = Regex.Match(rawPdfText, @"(?:Divisa\s+de\s+la\s+cuenta|Moneda):\s*([^\r\n:]+)", RegexOptions.IgnoreCase);
                if (singleCurrencyMatch.Success)
                {
                    string singleCurrency = Regex.Split(singleCurrencyMatch.Groups[1].Value, @"(?:Importe|Titular|RFC|Banco|Motivo|$)", RegexOptions.IgnoreCase)[0];
                    currency = CleanseAndHealText(singleCurrency);
                    if (currency == "MXP") { currency = "MXN"; }
                }
            }
            #endregion
        }
    }
}
