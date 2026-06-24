using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.Infra.Services
{
    public class TextPdfProcessor : IPdfProcessor
    {
        public void ProcessPaymentBatch(string[] files, List<PaymentRowData> rows, string companyName, int startConsecutive, Action<int, string> onRowProcessed)
        {
            int currentFileIndex = 0;
            int internalPageTracker = 1;
            int consecutiveNumber = startConsecutive;
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
                if (useConsecutive && i > 0) consecutiveNumber++;

                // Clean file string strings
                var vendor = string.Join("_", (row.Vendor ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();
                var concept = string.Join("_", (row.Concept ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();
                var amount = string.Join("_", (row.Amount ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();
                var currency = string.Join("_", (row.Currency ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();

                string directory = Path.GetDirectoryName(currentFilePath);
                var consecutivePart = useConsecutive ? $"{consecutiveNumber}-" : "";
                string newFileName = $"{row.Date}-{companyName}-{consecutivePart}{vendor} {concept}-{amount} {currency}.pdf";
                string destinationPath = Path.Combine(directory, newFileName);
                string safeDestinationPath = GetUniqueFilePath(destinationPath);

                int totalPagesInFile = 0;

                ReaderProperties readerProperties = new();
                readerProperties.SetPassword(Encoding.UTF8.GetBytes(""));

                using (PdfReader reader = new(currentFilePath, readerProperties))
                using (PdfDocument sourcePdfDoc = new(reader))
                {
                    totalPagesInFile = sourcePdfDoc.GetNumberOfPages();

                    using (PdfWriter writer = new(safeDestinationPath))
                    using (PdfDocument newSinglePagePdf = new(writer))
                    {
                        if (companyName == "EMKA")
                        {
                            SliceSecuredPage(sourcePdfDoc, newSinglePagePdf, internalPageTracker);
                        }
                        else
                        {
                            // Original high-performance path for standard files
                            sourcePdfDoc.CopyPagesTo(internalPageTracker, internalPageTracker, newSinglePagePdf);
                        }
                    }

                    if (totalPagesInFile == 1)
                    {
                        if (!filesToDelete.Contains(currentFilePath))
                        {
                            filesToDelete.Add(currentFilePath);
                        }
                    }
                }

                // Step tracking
                if (internalPageTracker < totalPagesInFile)
                {
                    internalPageTracker++;
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
    }
}
