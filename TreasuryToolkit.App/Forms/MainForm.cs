using System.Text.Json;
using TreasuryToolkit.Core.Contracts;

namespace TreasuryToolkit.App
{
    public partial class MainForm : Form
    {
        #region Properties
        private readonly string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.json");
        private bool _isDarkMode = false;
        private UserControl _currentView;
        private UcFileRenamer _fileRenamer;
        private UcExcelWorkflowAutomator _excelWorkflowAutomator;
        private readonly Func<ProgressForm> progressFormFactory;
        private readonly IPdfProcessor pdfProcessor;
        private readonly IFileScanner fileScanner;
        private readonly ICompanyService companyService;
        #endregion

        #region Constructor
        public MainForm(Func<ProgressForm> progressFormFactory, IPdfProcessor pdfProcessor, IFileScanner fileScanner, ICompanyService companyService)
        {
            this.progressFormFactory = progressFormFactory;
            this.pdfProcessor = pdfProcessor;
            this.fileScanner = fileScanner;
            this.companyService = companyService;
            InitializeComponent();
            InitViews();
            LoadSettings();
            ShowView(_fileRenamer);
        }
        #endregion

        #region UI Control Events
        private void ChkGlobalTheme_CheckedChanged(object sender, EventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            UpdateApplicationTheme();
            SaveSettings();
        }

        private void BtnPdfTool_Click(object sender, EventArgs e)
        {
            ShowView(_fileRenamer);
            UpdateButtonHighlight(BtnPdfTool);
        }

        private void BtnExcelTool_Click(object sender, EventArgs e)
        {
            ShowView(_excelWorkflowAutomator);
            UpdateButtonHighlight(BtnExcelTool);
        }
        #endregion

        #region Helpers
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
                        ChkGlobalTheme.Checked = settings.IsDarkMode;
                        _isDarkMode = settings.IsDarkMode;
                    }
                }
                catch
                {
                    ChkGlobalTheme.Checked = false;
                    _isDarkMode = false;
                }
            }
        }

        private void UpdateApplicationTheme()
        {
            if (_isDarkMode)
            {
                PnlSideBar.BackColor = Color.FromArgb(30, 30, 30);
                PnlMainContent.BackColor = Color.FromArgb(32, 32, 32);

                BtnPdfTool.ForeColor = Color.White;
                BtnExcelTool.ForeColor = Color.White;
                BtnAbout.ForeColor = Color.White;
                ChkGlobalTheme.ForeColor = Color.White;
            }
            else
            {
                PnlSideBar.BackColor = Color.FromArgb(230, 235, 240);
                PnlMainContent.BackColor = Color.White;

                BtnPdfTool.ForeColor = Color.Black;
                BtnExcelTool.ForeColor = Color.Black;
                ChkGlobalTheme.ForeColor = Color.Black;
                BtnAbout.ForeColor = Color.Black;
            }

            if (_currentView is UcFileRenamer pdfView)
            {
                pdfView.ApplyTheme(_isDarkMode);
                UpdateButtonHighlight(BtnPdfTool);
            }
            else if (_currentView is UcExcelWorkflowAutomator excelView)
            {
                excelView.ApplyTheme(_isDarkMode);
                UpdateButtonHighlight(BtnExcelTool);
            }
        }

        private void ShowView(UserControl view)
        {
            _currentView = view;
            PnlMainContent.SuspendLayout();
            PnlMainContent.Controls.Clear();
            PnlMainContent.Controls.Add(view);
            UpdateApplicationTheme();
            PnlMainContent.ResumeLayout();
        }

        private void InitViews()
        {
            _fileRenamer = new UcFileRenamer(progressFormFactory, pdfProcessor, fileScanner, companyService)
            {
                Dock = DockStyle.Fill
            };
            _excelWorkflowAutomator = new UcExcelWorkflowAutomator()
            {
                Dock = DockStyle.Fill
            };
        }

        private void SaveSettings()
        {
            var settings = new LocalAppSettings { IsDarkMode = ChkGlobalTheme.Checked };
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }

        private void UpdateButtonHighlight(Button activeButton)
        {
            Button[] navButtons = [BtnPdfTool, BtnExcelTool, BtnAbout];
            foreach (Button button in navButtons)
            {
                if (button == activeButton)
                {
                    button.BackColor = _isDarkMode ? Color.FromArgb(45, 45, 48) : Color.FromArgb(0, 120, 215);
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = _isDarkMode ? Color.FromArgb(24, 24, 24) : Color.FromArgb(230, 235, 240);
                    button.ForeColor = _isDarkMode ? Color.DarkGray : Color.FromArgb(64, 64, 64);
                }
            }
        } 
        #endregion
    }
}
