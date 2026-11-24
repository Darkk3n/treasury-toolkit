using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TreasuryToolkit.Wpf.App.Services;

namespace TreasuryToolkit.Wpf.App.ViewModels
{
    public enum AppSection
    {
        PdfTool,
        ExcelTool,
        About
    }

    /// <summary>
    /// Shell state: which section is shown, the theme and the sidebar. Replaces v1's MainForm logic
    /// (ShowView, ChkGlobalTheme_CheckedChanged, BtnToggleSidebar_Click). Window size is a view concern and
    /// stays in MainWindow.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly SettingsService settings;
        private readonly ThemeService theme;
        private readonly FileRenamerViewModel fileRenamer;
        private readonly ExcelWorkflowViewModel excelWorkflow;
        private readonly AboutViewModel about;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentViewModel))]
        private AppSection selectedSection = AppSection.PdfTool;

        [ObservableProperty]
        private bool isDarkMode;

        [ObservableProperty]
        private bool isSidebarCollapsed;

        public MainViewModel(SettingsService settings, ThemeService theme,
            FileRenamerViewModel fileRenamer, ExcelWorkflowViewModel excelWorkflow, AboutViewModel about)
        {
            this.settings = settings;
            this.theme = theme;
            this.fileRenamer = fileRenamer;
            this.excelWorkflow = excelWorkflow;
            this.about = about;

            // Set the fields directly so restoring saved state doesn't re-apply the theme or rewrite the file
#pragma warning disable MVVMTK0034
            isDarkMode = settings.Current.IsDarkMode;
            isSidebarCollapsed = settings.Current.IsSidebarCollapsed;
#pragma warning restore MVVMTK0034
        }

        public object CurrentViewModel => SelectedSection switch
        {
            AppSection.ExcelTool => excelWorkflow,
            AppSection.About => about,
            _ => fileRenamer
        };

        partial void OnIsDarkModeChanged(bool value)
        {
            theme.Apply(value);
            settings.Current.IsDarkMode = value;
            settings.Save();
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarCollapsed = !IsSidebarCollapsed;
            settings.Current.IsSidebarCollapsed = IsSidebarCollapsed;
            settings.Save();
        }
    }
}
