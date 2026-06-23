using System.Diagnostics;
using System.Text.Json;
using iText.Forms;
using Microsoft.VisualBasic.FileIO;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Core.Models;

namespace TreasuryToolkit.App
{
    public partial class UcFileRenamer : UserControl
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
        private readonly IFileScanner fileScanner;
        private readonly ICompanyService companyService;
        #endregion

        #region Constructor
        public UcFileRenamer(Func<ProgressForm> progressFormFactory, IPdfProcessor pdfProcessor, IFileScanner fileScanner, ICompanyService companyService)
        {
            InitializeComponent();
            _progressFormFactory = progressFormFactory;
            this.pdfProcessor = pdfProcessor;
            this.fileScanner = fileScanner;
            this.companyService = companyService;
            SetupCombobox();
        }

        private void SetupCombobox()
        {
            var companyList = companyService.GetCompanyNames();
            CmbCompany.Items.Clear();
            CmbCompany.DropDownStyle = ComboBoxStyle.DropDown;
            CmbCompany.Items.AddRange([.. companyList]);
            CmbCompany.ValueMember = nameof(CompanyModel.Id);
            CmbCompany.DisplayMember = nameof(CompanyModel.Name);
            CmbCompany.AutoCompleteCustomSource.AddRange([.. companyList.Select(r => r.Name)]);
            CmbCompany.AutoCompleteSource = AutoCompleteSource.CustomSource;
            CmbCompany.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            CmbCompany.Items.Insert(0, "-- SELECCIONE --");
            CmbCompany.SelectedIndex = 0;
        }
        #endregion

        #region UI Control Events
        private void FileRenamerForm_Load(object sender, EventArgs e)
        {
            CmbCompany.SelectedIndex = 0;
            SetupGrid();
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
            bool isValid = ValidateStartProcess();
            if (!isValid)
            {
                return;
            }

            var dialogResult = MessageBox.Show("¿Desea renombrar y segmentar los archivos? Se conservarán los multipágina y se limpiarán los pre-segmentados.", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No)
            {
                return;
            }
            EnableControls(false);
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
            var company = (CompanyModel) CmbCompany.SelectedItem;
            var retry = true;
            while (retry)
            {
                try
                {
                    pdfProcessor.ProcessPaymentBatch(files, paymentRows, company.Name, startConsecutive,
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
                    retry = false;
                }
                catch (InvalidOperationException ex)
                {
                    var result = MessageBox.Show(ex.Message, "Archivo en uso", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                    if (result == DialogResult.Cancel)
                    {
                        retry = false;
                    }
                }
            }

            Thread.Sleep(300);
            loadingScreen.Close();
            EnableControls(true);
            DeleteBackUp(backupFolder);
            MessageBox.Show("Proceso Completado con Éxito!", "Éxito!", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            var sourceDirectory = @"C:\Renombrar";
            var files = Directory.GetFiles(sourceDirectory, "*.pdf", System.IO.SearchOption.TopDirectoryOnly).ToList();

            if (files.Count == 0)
            {
                DgvPayments.Rows.Add(DateTime.Now.ToString("yyyyMMdd"), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                MessageBox.Show("No se encontraron archivos para escanear.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            files.Sort((x, y) => StrCmpLogicalW(x, y));

            ShowProgress();

            fileScanner.ScanPdfFiles(extractedDate, [.. files],
                onProgress: (statusMessage) =>
                {
                    if (loadingScreen != null && loadingScreen.Controls["lblStatus"] != null)
                    {
                        loadingScreen.Controls["lblStatus"].Text = statusMessage;
                        loadingScreen.Refresh();
                    }
                    Application.DoEvents();
                    Thread.Sleep(50); // Small visual breather so they can read layout changes
                },
                 onRowExtracted: (dataPayload) =>
                 {
                     DgvPayments.Rows.Add(dataPayload.Date, dataPayload.FileName, dataPayload.Vendor, dataPayload.Concept, dataPayload.Amount, dataPayload.Currency);
                     Application.DoEvents();
                 }
            );

            DgvPayments.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            loadingScreen.Close();
            MessageBox.Show($"Se escanearon y cargaron {files.Count}(s) archivo(s) en la tabla.", "Escaneo Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LblFolder.Text = sourceDirectory;
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
            if (CmbCompany.SelectedIndex == 0) return;

            var selectedCompany = (CompanyModel)CmbCompany.SelectedItem;
            LblConsecutive.Visible = CmbCompany.SelectedIndex != 0 && selectedCompany.Name == "EMKA";
            TxtConsecutive.Visible = CmbCompany.SelectedIndex != 0 && selectedCompany.Name == "EMKA";
            if (selectedCompany.Name != "EMKA")
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
            foreach (DataGridViewColumn column in DgvPayments.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
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

        public void ApplyTheme(bool isDarkMode)
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

        private void ShowProgress()
        {
            loadingScreen = _progressFormFactory();

            Form parentForm = this.FindForm();

            if (parentForm != null)
            {
                // Ensure manual positioning is allowed
                loadingScreen.StartPosition = FormStartPosition.Manual;

                // Calculate the dead-center point based on parent coordinates
                int centerX = parentForm.Left + (parentForm.Width - loadingScreen.Width) / 2;
                int centerY = parentForm.Top + (parentForm.Height - loadingScreen.Height) / 2;

                loadingScreen.Location = new Point(centerX, centerY);
            }

            loadingScreen.Show(parentForm);
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

        private bool ValidateStartProcess()
        {
            if (CmbCompany.SelectedIndex == 0)
            {
                MessageBox.Show("Seleccione una empresa para continuar", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (LblFolder.Text == "..." || !Directory.Exists(LblFolder.Text))
            {
                MessageBox.Show("Seleccione una carpeta para continuar", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (CmbCompany.SelectedItem.ToString() == "EMKA" && string.IsNullOrEmpty(TxtConsecutive.Text))
            {
                MessageBox.Show("Ingrese un número consecutivo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void EnableControls(bool value)
        {
            BtnFileDialog.Enabled = value;
            BtnStart.Enabled = value;
            BtnClean.Enabled = value;
            CmbCompany.Enabled = value;
            DgvPayments.Enabled = value;
        }
        #endregion
    }
}