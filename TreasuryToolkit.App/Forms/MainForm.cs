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
        private UcAbout _about;
        private readonly Func<ProgressForm> progressFormFactory;
        private readonly IPdfProcessor pdfProcessor;
        private readonly IFileScanner fileScanner;
        private readonly ICompanyService companyService;
        private ImageList _lightThemeIcons;
        private ImageList _darkThemeIcons;
        private bool _isSidebarCollapsed = false;
        private Dictionary<Control, string> _sidebarLabels;
        private const int SidebarExpandedWidth = 236;
        private const int SidebarCollapsedWidth = 56;
        private const int SidebarAnimationStep = 30;
        #endregion

        #region Constructor
        public MainForm(Func<ProgressForm> progressFormFactory, IPdfProcessor pdfProcessor, IFileScanner fileScanner, ICompanyService companyService)
        {
            this.progressFormFactory = progressFormFactory;
            this.pdfProcessor = pdfProcessor;
            this.fileScanner = fileScanner;
            this.companyService = companyService;
            InitializeComponent();
            _sidebarLabels = new()
            {
                [BtnPdfTool] = BtnPdfTool.Text,
                [BtnExcelTool] = BtnExcelTool.Text,
                [BtnAbout] = BtnAbout.Text,
                [ChkGlobalTheme] = ChkGlobalTheme.Text
            };
            InitViews();
            LoadSettings();
            InitializeIconContainers();
            ConfigureSidebarButtonLayouts();

            // Restore the saved sidebar state instantly, without animating
            PnlSideBar.Width = _isSidebarCollapsed ? SidebarCollapsedWidth : SidebarExpandedWidth;
            ApplySidebarLabels();
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

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            ShowView(_about);
            UpdateButtonHighlight(BtnAbout);
        }

        private void BtnToggleSidebar_Click(object sender, EventArgs e)
        {
            _isSidebarCollapsed = !_isSidebarCollapsed;

            // Hide labels before shrinking so they never show clipped; expanding restores them once at full width
            if (_isSidebarCollapsed)
            {
                ApplySidebarLabels();
            }

            TmrSidebarAnimation.Start();
            SaveSettings();
        }

        private void TmrSidebarAnimation_Tick(object sender, EventArgs e)
        {
            int target = _isSidebarCollapsed ? SidebarCollapsedWidth : SidebarExpandedWidth;
            int step = _isSidebarCollapsed ? -SidebarAnimationStep : SidebarAnimationStep;
            int nextWidth = PnlSideBar.Width + step;

            if ((_isSidebarCollapsed && nextWidth <= target) || (!_isSidebarCollapsed && nextWidth >= target))
            {
                PnlSideBar.Width = target;
                TmrSidebarAnimation.Stop();
                if (!_isSidebarCollapsed)
                {
                    ApplySidebarLabels();
                }
                return;
            }

            PnlSideBar.Width = nextWidth;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) => SaveSettings();
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
                        // Must be set before Checked, whose CheckedChanged handler calls SaveSettings
                        _isSidebarCollapsed = settings.IsSidebarCollapsed;
                        RestoreWindowSize(settings);
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
                BtnPdfTool.ImageList = _darkThemeIcons;
                BtnExcelTool.ImageList = _darkThemeIcons;
                BtnAbout.ImageList = _darkThemeIcons;
                ChkGlobalTheme.ForeColor = Color.White;
                BtnToggleSidebar.ForeColor = Color.White;
                BtnToggleSidebar.BackColor = PnlSideBar.BackColor;
            }
            else
            {
                PnlSideBar.BackColor = Color.FromArgb(230, 235, 240);
                PnlMainContent.BackColor = Color.White;

                BtnPdfTool.ForeColor = Color.Black;
                BtnExcelTool.ForeColor = Color.Black;
                ChkGlobalTheme.ForeColor = Color.Black;
                BtnAbout.ForeColor = Color.Black;
                BtnPdfTool.ImageList = _lightThemeIcons;
                BtnExcelTool.ImageList = _lightThemeIcons;
                BtnAbout.ImageList = _lightThemeIcons;
                BtnToggleSidebar.ForeColor = Color.Black;
                BtnToggleSidebar.BackColor = PnlSideBar.BackColor;
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

            else if (_currentView is UcAbout aboutView)
            {
                aboutView.ApplyTheme(_isDarkMode);
                UpdateButtonHighlight(BtnAbout);
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
            _about = new UcAbout()
            {
                Dock = DockStyle.Fill
            };
        }

        private void RestoreWindowSize(LocalAppSettings settings)
        {
            if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
            {
                // Clamp in case the saved size came from a larger monitor; MinimumSize enforces the lower bound
                var workingArea = Screen.PrimaryScreen.WorkingArea;
                Size = new Size(Math.Min(settings.WindowWidth, workingArea.Width), Math.Min(settings.WindowHeight, workingArea.Height));
            }

            if (settings.IsWindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        private void SaveSettings()
        {
            // While maximized (or minimized), Size is the screen size, so persist the restored size instead
            var normalSize = WindowState == FormWindowState.Normal ? Size : RestoreBounds.Size;
            var settings = new LocalAppSettings
            {
                IsDarkMode = ChkGlobalTheme.Checked,
                IsSidebarCollapsed = _isSidebarCollapsed,
                WindowWidth = normalSize.Width,
                WindowHeight = normalSize.Height,
                IsWindowMaximized = WindowState == FormWindowState.Maximized
            };
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

        private void InitializeIconContainers()
        {
            _lightThemeIcons = new ImageList { ImageSize = new Size(24, 24), ColorDepth = ColorDepth.Depth32Bit };
            _darkThemeIcons = new ImageList { ImageSize = new Size(24, 24), ColorDepth = ColorDepth.Depth32Bit };

            _lightThemeIcons.Images.Add(Properties.Resources.PDF_Black); // Dark gray icon for light background
            _darkThemeIcons.Images.Add(Properties.Resources.PDF_White);   // White icon for dark background

            _lightThemeIcons.Images.Add(Properties.Resources.Excel_Black);
            _darkThemeIcons.Images.Add(Properties.Resources.Excel_White);

            _lightThemeIcons.Images.Add(Properties.Resources.About_Black);
            _darkThemeIcons.Images.Add(Properties.Resources.About_White);
        }

        private void ConfigureSidebarButtonLayouts()
        {
            BtnPdfTool.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnPdfTool.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPdfTool.TextAlign = ContentAlignment.MiddleLeft;
            BtnPdfTool.Padding = new Padding(15, 0, 0, 0);
            BtnPdfTool.ImageIndex = 0;

            BtnExcelTool.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnExcelTool.ImageAlign = ContentAlignment.MiddleLeft;
            BtnExcelTool.TextAlign = ContentAlignment.MiddleLeft;
            BtnExcelTool.Padding = new Padding(15, 0, 0, 0);
            BtnExcelTool.ImageIndex = 1;

            BtnAbout.TextImageRelation = TextImageRelation.ImageBeforeText;
            BtnAbout.ImageAlign = ContentAlignment.MiddleLeft;
            BtnAbout.TextAlign = ContentAlignment.MiddleLeft;
            BtnAbout.Padding = new Padding(15, 0, 0, 0);
            BtnAbout.ImageIndex = 2;
        }

        private void ApplySidebarLabels()
        {
            Button[] navButtons = [BtnPdfTool, BtnExcelTool, BtnAbout];
            foreach (Button button in navButtons)
            {
                button.Text = _isSidebarCollapsed ? string.Empty : _sidebarLabels[button];
                button.ImageAlign = _isSidebarCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
                button.Padding = _isSidebarCollapsed ? Padding.Empty : new Padding(15, 0, 0, 0);
                TipSidebar.SetToolTip(button, _isSidebarCollapsed ? _sidebarLabels[button] : null);
            }

            // Reduced padding keeps the checkbox centered within the collapsed rail
            ChkGlobalTheme.Text = _isSidebarCollapsed ? string.Empty : _sidebarLabels[ChkGlobalTheme];
            ChkGlobalTheme.Padding = _isSidebarCollapsed ? new Padding(21, 0, 0, 0) : new Padding(40, 0, 0, 0);
            TipSidebar.SetToolTip(ChkGlobalTheme, _isSidebarCollapsed ? _sidebarLabels[ChkGlobalTheme] : null);
        }
        #endregion
    }
}
