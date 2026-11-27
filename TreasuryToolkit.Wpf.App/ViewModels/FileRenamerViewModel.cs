using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Core.Models;
using TreasuryToolkit.Core.Text;
using TreasuryToolkit.Wpf.App.Services;

namespace TreasuryToolkit.Wpf.App.ViewModels
{
    /// <summary>
    /// PDF tool: scan a folder of receipts into the grid, let the user review and edit the rows, then split and
    /// rename the files. Port of v1's UcFileRenamer. Scanning and processing run on a background thread, reporting
    /// back through Progress&lt;T&gt;, instead of on the UI thread with Application.DoEvents().
    /// </summary>
    public partial class FileRenamerViewModel : ObservableObject
    {
        private const string ProgressTitle = "Procesando PDFs...";

        private readonly IFileScanner fileScanner;
        private readonly IPdfProcessor pdfProcessor;
        private readonly IDialogService dialogs;
        private readonly ISystemService system;
        private readonly SourceBackupService backupService;
        private readonly SettingsService settings;

        [ObservableProperty]
        private CompanyModel selectedCompany;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FolderDisplay))]
        private string folderPath;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsIdle))]
        [NotifyCanExecuteChangedFor(nameof(CleanCommand), nameof(ScanFolderCommand), nameof(StartCommand))]
        private bool isBusy;

        public FileRenamerViewModel(IFileScanner fileScanner, IPdfProcessor pdfProcessor, ICompanyService companyService,
            IDialogService dialogs, ISystemService system, SourceBackupService backupService, SettingsService settings)
        {
            this.fileScanner = fileScanner;
            this.pdfProcessor = pdfProcessor;
            this.dialogs = dialogs;
            this.system = system;
            this.backupService = backupService;
            this.settings = settings;

            Companies = [PlaceholderCompany, .. companyService.GetCompanyNames()];
            Clean();
        }

        /// <summary>First entry of the company list, shown until a company is picked (v1's "-- SELECCIONE --").</summary>
        public CompanyModel PlaceholderCompany { get; } = new() { Id = string.Empty, Name = "-- SELECCIONE --" };

        public IReadOnlyList<CompanyModel> Companies { get; }

        public ObservableCollection<PaymentRowViewModel> Rows { get; } = [];

        public string FolderDisplay => FolderPath ?? "...";

        public bool IsIdle => !IsBusy;

        /// <summary>Called by the view when "Renombrar" is clicked while an amount cell still holds invalid text.</summary>
        public void ReportInvalidEdit() =>
            dialogs.ShowError("Corrija el monto marcado en rojo (solo se permiten numeros) o presione Esc para descartarlo.", "Error");

        [RelayCommand(CanExecute = nameof(IsIdle))]
        private void Clean()
        {
            Rows.Clear();
            Rows.Add(new PaymentRowViewModel());
            FolderPath = null;
            SelectedCompany = PlaceholderCompany;
        }

        [RelayCommand(CanExecute = nameof(IsIdle))]
        private async Task ScanFolderAsync()
        {
            string folder = dialogs.PickFolder("Seleccione una carpeta", settings.Current.LastFolder);
            if (folder == null)
            {
                return;
            }

            // Remembered even if the folder turns out to have no PDFs: it's where the user navigated to
            settings.Current.LastFolder = folder;
            settings.Save();

            IsBusy = true;
            try
            {
                Rows.Clear();
                var files = Directory.GetFiles(folder, "*.pdf", SearchOption.TopDirectoryOnly).ToList();

                if (files.Count == 0)
                {
                    Rows.Add(new PaymentRowViewModel());
                    dialogs.ShowWarning("No se encontraron archivos para escanear.", "Aviso");
                    return;
                }

                files.Sort(NaturalStringComparer.Instance);

                using (var progressDialog = dialogs.ShowProgress(ProgressTitle))
                {
                    // Created here, on the UI thread, so the callbacks run on it no matter where Report is called
                    IProgress<string> status = new Progress<string>(message => progressDialog.Status = message);
                    IProgress<ScannedPaymentData> rows = new Progress<ScannedPaymentData>(row => Rows.Add(new PaymentRowViewModel(row)));

                    await Task.Run(() => fileScanner.ScanPdfFiles([.. files],
                        onProgress: message =>
                        {
                            status.Report(message);
                            Thread.Sleep(50); // Same short pause as v1, so each status line can be read
                        },
                        onRowExtracted: rows.Report));
                }

                dialogs.ShowInfo($"Se escanearon y cargaron {files.Count} archivo(s) en la tabla.", "Escaneo Completado");
                FolderPath = folder;
            }
            finally
            {
                // Also runs when the scanner throws, which v1 didn't handle: it left the controls disabled
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsIdle))]
        private async Task StartAsync()
        {
            if (!ValidateStart() || !ConfirmClientNameMismatches())
            {
                return;
            }

            if (!dialogs.Confirm("¿Desea renombrar y segmentar los archivos? Se conservarán los multipágina y se limpiarán los pre-segmentados.", "Confirmación"))
            {
                return;
            }

            IsBusy = true;
            try
            {
                string folder = FolderPath;
                string companyName = SelectedCompany.Name;
                var paymentRows = Rows.Select(r => r.ToPaymentRowData()).ToList();

                string backupFolder = await Task.Run(() => backupService.CreateBackup(folder));

                var files = Directory.GetFiles(folder, "*.pdf");
                Array.Sort(files, NaturalStringComparer.Instance);

                bool completed;
                using (var progressDialog = dialogs.ShowProgress(ProgressTitle))
                {
                    int totalRows = paymentRows.Count;
                    IProgress<(int RowIndex, string FileName)> progress = new Progress<(int RowIndex, string FileName)>(p =>
                    {
                        int humanRowIndex = p.RowIndex == totalRows ? totalRows : p.RowIndex + 1;
                        progressDialog.Status = $"Procesando fila {humanRowIndex} de {totalRows}: {p.FileName}...";
                    });

                    completed = await ProcessWithRetryAsync(files, paymentRows, companyName, progress);
                    if (completed)
                    {
                        await Task.Delay(300); // v1 kept the last status on screen briefly before closing
                    }
                }

                // Nothing was changed if the user cancelled the "file in use" prompt, so the backup isn't needed either way
                string deleteError = await Task.Run(() => backupService.TryDelete(backupFolder));
                if (deleteError != null)
                {
                    dialogs.ShowWarning($"Nota: El proceso terminó con éxito, pero no se pudo eliminar la carpeta temporal de respaldo: {deleteError}", "Aviso");
                }

                // v1 reported success even after the user cancelled; only report it when the batch really ran
                if (!completed)
                {
                    return;
                }

                dialogs.ShowInfo("Proceso Completado con Éxito!", "Éxito!");
                IsBusy = false;

                if (!system.TryOpen(folder, out string openError))
                {
                    dialogs.ShowError($"No se pudo abrir la carpeta: {openError}", "Error");
                }
            }
            finally
            {
                // On an unexpected error the backup is kept, so the originals can be restored by hand
                IsBusy = false;
            }
        }

        /// <summary>
        /// Runs the batch, asking to retry while a file is open in another program (the processor checks this
        /// before touching anything). Returns false if the user cancelled.
        /// </summary>
        private async Task<bool> ProcessWithRetryAsync(string[] files, List<PaymentRowData> rows, string companyName,
            IProgress<(int RowIndex, string FileName)> progress)
        {
            while (true)
            {
                try
                {
                    await Task.Run(() => pdfProcessor.ProcessPaymentBatch(files, rows, companyName, (rowIndex, fileName) =>
                    {
                        progress.Report((rowIndex, fileName));
                        Thread.Sleep(150); // Same pacing as v1, so the status line can be followed
                    }));
                    return true;
                }
                catch (InvalidOperationException ex)
                {
                    if (!dialogs.AskRetry(ex.Message, "Archivo en uso"))
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>Client names on receipts that are never checked against the selected company.</summary>
        private static readonly string[] ClientNamesExemptFromCheck = ["CARL BECHEM"];

        /// <summary>
        /// Companies whose receipts carry a client name that doesn't contain their company-list name
        /// (e.g. "EMPRESA DEMO DE MEXICO SA DE CV" for "EMPRESA DEMO MX"); any of these names is accepted instead.
        /// </summary>
        private static readonly Dictionary<string, string[]> ReceiptClientNamesByCompany = new(StringComparer.OrdinalIgnoreCase)
        {
            ["IMMOPRETT MX"] = ["IMMOPRETT DE MEXICO"],
            ["IMMOPRETT QRO"] = ["IMMOPRETT QUERETARO"],   // as reported; no sample receipt yet
        };

        private static bool ClientNameMatchesCompany(string clientName, string companyName) =>
            CompanyNameMatcher.Contains(clientName, companyName)
            || (ReceiptClientNamesByCompany.TryGetValue(companyName, out string[] receiptNames)
                && receiptNames.Any(name => CompanyNameMatcher.Contains(clientName, name)));

        /// <summary>
        /// Warns when a file's client name ("Nombre del Cliente" / "Nombre del Ordenante" / "Contrato", first page)
        /// doesn't contain the selected company's name (or one of its <see cref="ReceiptClientNamesByCompany"/>)
        /// and asks whether to go on anyway. Files without any of those labels, files whose client is in
        /// <see cref="ClientNamesExemptFromCheck"/>, and hand-added rows are not checked. Returns true to continue.
        /// </summary>
        private bool ConfirmClientNameMismatches()
        {
            const int MaxListed = 10;

            var mismatches = Rows
                .Where(r => !string.IsNullOrEmpty(r.FileName) && !string.IsNullOrEmpty(r.ClientName))
                .DistinctBy(r => r.FileName)
                .Where(r => !ClientNamesExemptFromCheck.Any(exempt => CompanyNameMatcher.Contains(r.ClientName, exempt)))
                .Where(r => !ClientNameMatchesCompany(r.ClientName, SelectedCompany.Name))
                .Select(r => $"• {r.FileName}: {r.ClientName}")
                .ToList();

            if (mismatches.Count == 0)
            {
                return true;
            }

            string list = string.Join(Environment.NewLine, mismatches.Take(MaxListed));
            if (mismatches.Count > MaxListed)
            {
                list += $"{Environment.NewLine}... y {mismatches.Count - MaxListed} más";
            }

            return dialogs.ConfirmWarning(
                $"El nombre del cliente no coincide con la empresa seleccionada ({SelectedCompany.Name}) en {mismatches.Count} archivo(s):"
                + $"{Environment.NewLine}{Environment.NewLine}{list}{Environment.NewLine}{Environment.NewLine}¿Desea continuar de todos modos?",
                "Advertencia");
        }

        private bool ValidateStart()
        {
            if (SelectedCompany == null || SelectedCompany == PlaceholderCompany)
            {
                dialogs.ShowError("Seleccione una empresa para continuar", "Error");
                return false;
            }

            if (FolderPath == null || !Directory.Exists(FolderPath))
            {
                dialogs.ShowError("Seleccione una carpeta para continuar", "Error");
                return false;
            }

            return true;
        }
    }
}
