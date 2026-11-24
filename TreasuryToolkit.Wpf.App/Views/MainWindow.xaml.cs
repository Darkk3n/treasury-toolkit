using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using TreasuryToolkit.Wpf.App.Services;
using TreasuryToolkit.Wpf.App.ViewModels;

namespace TreasuryToolkit.Wpf.App.Views
{
    public partial class MainWindow : Window
    {
        private const double SidebarExpandedWidth = 236;
        private const double SidebarCollapsedWidth = 56;
        private static readonly Duration SidebarAnimationDuration = new(TimeSpan.FromMilliseconds(200));

        private readonly MainViewModel viewModel;
        private readonly SettingsService settings;

        public MainWindow(MainViewModel viewModel, SettingsService settings)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.settings = settings;
            DataContext = viewModel;

            // Restore the saved sidebar state instantly; only user toggles animate
            Sidebar.Width = viewModel.IsSidebarCollapsed ? SidebarCollapsedWidth : SidebarExpandedWidth;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            RestoreWindowSize(settings.Current);
            Closing += OnClosing;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsSidebarCollapsed))
            {
                var animation = new DoubleAnimation(
                    viewModel.IsSidebarCollapsed ? SidebarCollapsedWidth : SidebarExpandedWidth,
                    SidebarAnimationDuration)
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };
                Sidebar.BeginAnimation(WidthProperty, animation);
            }
        }

        // Same rules as v1's MainForm.RestoreWindowSize: clamp to the working area in case the size came from a
        // larger monitor; MinWidth/MinHeight enforce the lower bound
        private void RestoreWindowSize(LocalAppSettings saved)
        {
            if (saved.WindowWidth > 0 && saved.WindowHeight > 0)
            {
                var workArea = SystemParameters.WorkArea;
                Width = Math.Min(saved.WindowWidth, workArea.Width);
                Height = Math.Min(saved.WindowHeight, workArea.Height);
            }

            if (saved.IsWindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            // While maximized (or minimized), the size is the screen's, so persist the restored size instead
            var normalSize = WindowState == WindowState.Normal ? new Size(ActualWidth, ActualHeight) : RestoreBounds.Size;

            settings.Current.WindowWidth = (int)Math.Round(normalSize.Width);
            settings.Current.WindowHeight = (int)Math.Round(normalSize.Height);
            settings.Current.IsWindowMaximized = WindowState == WindowState.Maximized;
            settings.Save();
        }
    }
}
