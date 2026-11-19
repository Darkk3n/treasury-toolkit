using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using TreasuryToolkit.Core;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.Infra.Services
{
    public class TextPdfProcessor : IPdfProcessor
    {
        public void ProcessPaymentBatch(string[] files, List<PaymentRowData> rows, string companyName, Action<int, string> onRowProcessed)
        {
            int currentFileIndex = 0;
            int internalPageTracker = 1;
            bool useConsecutive = companyName == "EMKA";
            List<string> filesToDelete = [];

            if (files.Any(IsFileLocked))
            {
                throw new InvalidOperationException("Uno mas archivos se encuentran abiertos en otro programa. Por favor cierra todos los archivos abiertos e intenta de nuevo.");
            }

            for (int i = 0; i < rows.Count; i++)
            {
                if (currentFileIndex >= files.Length) break;

                string currentFilePath = files[currentFileIndex];

                onRowProcessed?.Invoke(i, Path.GetFileName(currentFilePath));

                var row = rows[i];

                // Clean file string strings
                var vendor = string.Join("_", (row.Vendor ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();
                var concept = string.Join("_", (row.Concept ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();
                var amount = string.Join("_", (row.Amount ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();
                var currency = string.Join("_", (row.Currency ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();
                var comment = string.Join("_", (row.Comment ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();

                string directory = Path.GetDirectoryName(currentFilePath);
                var cleanVendor = vendor.HasValue ? $"{vendor}-" : string.Empty;
                var cleanConcept = concept.HasValue ? $"{concept}-" : string.Empty;
                string newFileName = $"{row.Date}-{companyName}-{cleanVendor}{cleanConcept}{amount} {currency}";
                if (comment.HasValue)
                {
                    newFileName += $" ({comment})";
                }
                newFileName += $".pdf";
                string destinationPath = Path.Combine(directory, newFileName);
                string safeDestinationPath = GetUniqueFilePath(destinationPath);

                int totalPagesInFile = 0;
                int lastPageForRow = internalPageTracker;

                ReaderProperties readerProperties = new();
                readerProperties.SetPassword(Encoding.UTF8.GetBytes(""));

                using (PdfReader reader = new(currentFilePath, readerProperties))
                using (PdfDocument sourcePdfDoc = new(reader))
                {
                    totalPagesInFile = sourcePdfDoc.GetNumberOfPages();

                    // A row normally covers one page, but continuation pages detected by the scanner
                    // (PageCount > 1) are copied into the same output file. Clamp to the file's end.
                    lastPageForRow = Math.Min(internalPageTracker + Math.Max(row.PageCount, 1) - 1, totalPagesInFile);

                    using (PdfWriter writer = new(safeDestinationPath))
                    using (PdfDocument newSinglePagePdf = new(writer))
                    {
                        if (SpecialLogicCompanies.Contains(companyName))
                        {
                            for (int pageNum = internalPageTracker; pageNum <= lastPageForRow; pageNum++)
                            {
                                SliceSecuredPage(sourcePdfDoc, newSinglePagePdf, pageNum);
                            }
                        }
                        else
                        {
                            // Original high-performance path for standard files
                            sourcePdfDoc.CopyPagesTo(internalPageTracker, lastPageForRow, newSinglePagePdf);
                        }
                    }

                    // Clean up the source when this single row consumed the whole file (a one-page file,
                    // or a receipt whose extra pages were all continuation pages). Multi-page batch files
                    // split across several rows are kept, as before.
                    if (internalPageTracker == 1 && lastPageForRow == totalPagesInFile)
                    {
                        if (!filesToDelete.Contains(currentFilePath))
                        {
                            filesToDelete.Add(currentFilePath);
                        }
                    }
                }

                // Step tracking
                if (lastPageForRow < totalPagesInFile)
                {
                    internalPageTracker = lastPageForRow + 1;
                }
                else
                {
                    internalPageTracker = 1;
                    currentFileIndex++;
                }
            }

            // Handle deletions of fully consumed files safely
            foreach (string oldFile in filesToDelete)
            {
                try { if (File.Exists(oldFile)) File.Delete(oldFile); } catch { }
            }
        }

        /// <summary>
        /// Bypasses Owner Password restrictions by drawing page contents onto a fresh canvas.
        /// </summary>
        private static void SliceSecuredPage(PdfDocument sourceDoc, PdfDocument targetDoc, int pageNumber)
        {
            PdfPage sourcePage = sourceDoc.GetPage(pageNumber);

            // Match dimensions perfectly
            var pageRectangle = sourcePage.GetPageSize();
            var targetPageSize = new iText.Kernel.Geom.PageSize(pageRectangle);
            PdfPage newPage = targetDoc.AddNewPage(targetPageSize);

            // Turn the page content into a vector form object and draw it
            var pageForm = sourcePage.CopyAsFormXObject(targetDoc);
            PdfCanvas canvas = new(newPage);
            canvas.AddXObjectAt(pageForm, 0, 0);
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        private static string GetUniqueFilePath(string targetFilePath)
        {
            if (!File.Exists(targetFilePath))
            {
                return targetFilePath;
            }

            // Deconstruct the file so we don't accidentally mess up the extension
            string directory = Path.GetDirectoryName(targetFilePath) ?? string.Empty;
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(targetFilePath);
            string extension = Path.GetExtension(targetFilePath);

            int counter = 1;
            string uniqueFilePath = targetFilePath;

            // 🚀 Loop until we find a filename that does NOT exist on the disk
            while (File.Exists(uniqueFilePath))
            {
                string newFileName = $"{fileNameWithoutExt}_{counter}{extension}";
                uniqueFilePath = Path.Combine(directory, newFileName);

                counter++;
            }
            return uniqueFilePath;
        }

        private static List<string> SpecialLogicCompanies => ["EMKA", "KLEIBERIT", "DAP", "ELWEMA", "EUROPCELL", "HENN", "REINHAUSEN", "STOROPACK", "RUTRONIK"];
    }
}
