using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ClientUtils.Core.Contracts;
using ClientUtils.Core.Models;
using iText.Forms;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using Microsoft.VisualBasic.FileIO;

namespace FileRenamer
{
    public partial class FileRenamerForm : Form
    {
        #region Properties
        private readonly string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.json");
        readonly string formattedDate = DateTime.Now.Date.ToString("yyyyMMdd");
        [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        private static extern int StrCmpLogicalW(string psz1, string psz2);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time 
        private ProgressForm loadingScreen;
        private readonly Func<ProgressForm> _progressFormFactory;
        private readonly IPdfProcessor pdfProcessor;
        #endregion

        #region Constructor
        public FileRenamerForm(Func<ProgressForm> progressFormFactory, IPdfProcessor pdfProcessor)
        {
            InitializeComponent();
            _progressFormFactory = progressFormFactory;
            this.pdfProcessor = pdfProcessor;
        }
        #endregion

        #region UI Control Events
        private void FileRenamerForm_Load(object sender, EventArgs e)
        {
            CmbCompany.SelectedIndex = 0;
            SetupGrid();
            LoadSettings();
        }

        private void BtnClean_Click(object sender, EventArgs e)
        {
            DgvPayments.Rows.Clear();
            DgvPayments.Rows.Add(formattedDate);
            LblFolder.Text = "...";
            CmbCompany.SelectedIndex = 0;
        }

        private void DgvPayments_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e) => e.Row.Cells[0].Value = formattedDate;

        private void DgvPayments_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (DgvPayments.Columns[e.ColumnIndex].Name == "dgvPaymentsColAmount")
            {
                var cell = DgvPayments.Rows[e.RowIndex].Cells[e.ColumnIndex];

                // 3. If they typed text, try to convert it to a real number (double)
                if (cell.Value != null && double.TryParse(cell.Value.ToString(), out double numericValue))
                {
                    // By saving it back as a double instead of text, 
                    // the "N2" format rule is instantly triggered!
                    cell.Value = numericValue;
                    cell.Style.Format = "N2";
                }
            }
        }

        private void DgvPayments_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var row = DgvPayments.Rows[e.RowIndex];
            if (DgvPayments.Columns[e.ColumnIndex].Name == "dgvPaymentsColAmount")
            {
                if (DgvPayments.Rows[e.RowIndex].IsNewRow || string.IsNullOrEmpty(e.FormattedValue.ToString()))
                    return;

                var input = e.FormattedValue.ToString();
                if (!double.TryParse(input, out double result))
                {
                    row.ErrorText = "Monto: Solo se permiten numeros.";
                    e.Cancel = true;
                }
                else
                {
                    row.ErrorText = string.Empty;
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (CmbCompany.SelectedIndex == 0)
            {
                MessageBox.Show("Seleccione una empresa para continuar", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (LblFolder.Text == "..." || !Directory.Exists(LblFolder.Text))
            {
                MessageBox.Show("Seleccione una carpeta para continuar", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (CmbCompany.SelectedItem.ToString() == "EMKA" && string.IsNullOrEmpty(TxtConsecutive.Text))
            {
                MessageBox.Show("Ingrese un número consecutivo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dialogResult = MessageBox.Show("¿Desea renombrar y segmentar los archivos? Se conservarán los multipágina y se limpiarán los pre-segmentados.", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No)
            {
                return;
            }

            var backupFolder = BackupSourceFiles(LblFolder.Text);
            var files = Directory.GetFiles(LblFolder.Text, "*.pdf").ToArray();
            Array.Sort(files, (x, y) => StrCmpLogicalW(x, y)); // Enforces 1, 2, 3, 10 order

            ShowProgress();

            loadingScreen?.Refresh();
            var paymentRows = DgvPayments.Rows.Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => new PaymentRowData
                {
                    Date = r.Cells[0].Value?.ToString(),
                    Vendor = r.Cells[2].Value?.ToString(),
                    Concept = r.Cells[3].Value?.ToString(),
                    Amount = r.Cells[4].FormattedValue?.ToString(),
                    Currency = r.Cells[5].Value?.ToString()
                })
                .ToList();

            int totalRows = paymentRows.Count;
            int startConsecutive = string.IsNullOrEmpty(TxtConsecutive.Text) ? 0 : int.Parse(TxtConsecutive.Text);

            pdfProcessor.ProcessPaymentBatch(files, paymentRows, CmbCompany.SelectedItem.ToString(), startConsecutive,
                (currentRowIndex, currentFileName) =>
                {
                    // This code runs INSIDE the loop of the service, but executes on the Form!
                    if (loadingScreen != null && loadingScreen.Controls["lblStatus"] != null)
                    {
                        int humanRowIndex = currentRowIndex == totalRows ? totalRows : currentRowIndex + 1;
                        loadingScreen.Controls["lblStatus"].Text = $"Procesando fila {humanRowIndex} de {totalRows}: {currentFileName}...";
                        loadingScreen.Refresh();
                    }

                    // Keep the UI thread breathing and layout moving smoothly
                    Application.DoEvents();
                    Thread.Sleep(150);
                });
            Thread.Sleep(300);
            loadingScreen.Close();
            DeleteBackUp(backupFolder);
            MessageBox.Show("Proceso Completado con Éxito!", "Exito!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            OpenResultsFolder();
        }

        private void BtnFileDialog_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Seleccione una carpeta";
            folderDialog.UseDescriptionForTitle = true;
            folderDialog.ShowNewFolderButton = false; // Prevents them making a mess

            //TODO: Uncomment this
            //if (folderDialog.ShowDialog() == DialogResult.Cancel)
            //{
            //    return;
            //}
            string extractedDate = DgvPayments.Rows[0].Cells[0].Value.ToString();

            DgvPayments.Rows.Clear();
            //TODO: Revert this hardcoded path
            //var sourceDirectory = folderDialog.SelectedPath;
            var sourceDirectory = @"C:\Users\455198\Downloads\Renombrar";
            var files = Directory.GetFiles(sourceDirectory, "*.pdf", System.IO.SearchOption.TopDirectoryOnly).ToList();
            files.Sort((x, y) => StrCmpLogicalW(x, y));

            ShowProgress();

            int fileCounter = 1;

            foreach (string filePath in files)
            {
                string fileNameOnly = Path.GetFileName(filePath);
                loadingScreen.Controls["lblStatus"].Text = $"Procesando Archivo {fileCounter} de {files.Count}: {fileNameOnly}...";
                Application.DoEvents();
                try
                {
                    using PdfReader pdfReader = new(filePath);
                    using PdfDocument pdfDoc = new(pdfReader);
                    int totalPages = pdfDoc.GetNumberOfPages();

                    for (int pageNum = 1; pageNum <= totalPages; pageNum++)
                    {
                        if (totalPages > 1)
                        {
                            loadingScreen.Controls["lblStatus"].Text = $"Procesando {fileNameOnly} (Pagina {pageNum}/{totalPages})...";
                            Application.DoEvents();
                        }

                        string pageText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum));

                        if (string.IsNullOrWhiteSpace(pageText)) continue;

                        ExtractPdfDataPoints(pageText, out string extractedAmount, out string extractedVendorName, out string extractedReason, out string extractedCurrency);

                        DgvPayments.Rows.Add(extractedDate, fileNameOnly, extractedVendorName, extractedReason, extractedAmount, extractedCurrency);
                    }
                }
                catch (Exception ex)
                {
                    extractedDate = DateTime.Now.ToString("yyyyMMdd");
                    DgvPayments.Rows.Add(extractedDate, fileNameOnly, "ERROR", "Failed to parse file", ex.Message, "");
                }
            }
            DgvPayments.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            loadingScreen.Close();
            MessageBox.Show($"Se escanearon y cargaron {files.Count} archivos en la tabla!", "Escaneo Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LblFolder.Text = sourceDirectory;
        }

        private void ChkDarkMode_CheckedChanged(object sender, EventArgs e)
        {
            ApplyTheme(ChkDarkMode.Checked);
            SaveSettings();
        }

        private void TxtConsecutive_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                // e.Handled = true means "we handled this event, ignore the keystroke"
                e.Handled = true;
            }
        }

        private void CmbCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            LblConsecutive.Visible = CmbCompany.SelectedIndex != 0 && CmbCompany.SelectedItem.ToString() == "EMKA";
            TxtConsecutive.Visible = CmbCompany.SelectedIndex != 0 && CmbCompany.SelectedItem.ToString() == "EMKA";
            if (CmbCompany.SelectedItem.ToString() != "EMKA")
            {
                TxtConsecutive.Text = string.Empty;
            }
        }
        #endregion

        #region Helpers
        private void SetupGrid()
        {
            DgvPayments.Columns[0].FillWeight = 75;
            DgvPayments.Columns[1].FillWeight = 300;
            DgvPayments.Columns[1].FillWeight = 300;
            DgvPayments.Rows.Clear();
            DgvPayments.Rows.Add(formattedDate);
            DgvPayments.Columns[2].DefaultCellStyle.Format = "N2";
        }

        private static string BackupSourceFiles(string sourceFolder)
        {
            var backupFolder = string.Empty;
            if (Directory.Exists(sourceFolder))
            {
                try
                {
                    backupFolder = Path.Combine(Directory.GetParent(sourceFolder).FullName, Path.GetFileName(sourceFolder) + "_Backup_" + DateTime.Now.ToString("yyyyMMdd"));
                    FileSystem.CopyDirectory(
                        sourceFolder, backupFolder, UIOption.OnlyErrorDialogs, UICancelOption.DoNothing);
                }
                catch (Exception)
                {
                }
            }
            return backupFolder;
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

        private void ApplyTheme(bool isDarkMode)
        {
            // Define your color palettes
            Color formBg = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromKnownColor(KnownColor.ControlLightLight);
            Color controlBg = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            Color textColor = isDarkMode ? Color.White : Color.FromArgb(51, 51, 51);
            Color buttonBg = isDarkMode ? Color.FromArgb(0, 122, 204) : Color.FromKnownColor(KnownColor.ControlLightLight);

            // 1. Paint the main form background
            this.BackColor = formBg;

            foreach (Control c in this.Controls)
            {
                if (c is TextBox || c is ComboBox || c is NumericUpDown)
                {
                    c.BackColor = controlBg;
                    c.ForeColor = textColor;
                }
                // Handle Buttons
                else if (c is Button btn)
                {
                    btn.BackColor = buttonBg;
                    btn.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
                else if (c is Label)
                {
                    c.ForeColor = textColor;
                    c.BackColor = controlBg;

                }
                else if (c is CheckBox)
                {
                    c.ForeColor = textColor;

                }
                else if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = controlBg;
                    dgv.DefaultCellStyle.BackColor = controlBg;
                    dgv.DefaultCellStyle.ForeColor = textColor;
                    dgv.GridColor = isDarkMode ? Color.FromArgb(63, 63, 70) : Color.LightGray;
                }
            }
        }

        private void SaveSettings()
        {
            var settings = new LocalAppSettings { IsDarkMode = ChkDarkMode.Checked };
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(settingsFilePath);
                    var settings = JsonSerializer.Deserialize<LocalAppSettings>(json);

                    if (settings != null)
                    {
                        ChkDarkMode.Checked = settings.IsDarkMode;
                    }
                }
                catch
                {
                    ChkDarkMode.Checked = false;
                }
            }
        }

        private void ShowProgress()
        {
            loadingScreen = _progressFormFactory();
            loadingScreen.StartPosition = FormStartPosition.Manual;

            // Center it relative to the current form position
            int centerX = this.Location.X + (this.Width - loadingScreen.Width) / 2;
            int centerY = this.Location.Y + (this.Height - loadingScreen.Height) / 2;
            loadingScreen.Location = new Point(centerX, centerY);

            loadingScreen.Show(this);
        }

        private void OpenResultsFolder()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"\"{LblFolder.Text}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir la carpeta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void DeleteBackUp(string backupFolder)
        {
            try
            {
                if (Directory.Exists(backupFolder))
                {
                    Directory.Delete(backupFolder, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nota: El proceso terminó con éxito, pero no se pudo eliminar la carpeta temporal de respaldo: {ex.Message}", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
    }
}