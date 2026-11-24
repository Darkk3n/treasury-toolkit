using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Infra.Services;
using TreasuryToolkit.Wpf.App.Services;
using TreasuryToolkit.Wpf.App.ViewModels;
using TreasuryToolkit.Wpf.App.Views;

namespace TreasuryToolkit.Wpf.App
{
    public partial class App : Application
    {
        private ServiceProvider services;
        private bool startupCompleted;
        private bool isShowingError;

        public App()
        {
            // Registered before OnStartup so errors during startup are reported too
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            services = serviceCollection.BuildServiceProvider();

            var settings = services.GetRequiredService<SettingsService>();
            settings.Load();

            var theme = services.GetRequiredService<ThemeService>();
            theme.Initialize();
            theme.Apply(settings.Current.IsDarkMode);

            var mainWindow = services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
            startupCompleted = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            services?.Dispose();
            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // App services
            services.AddSingleton<SettingsService>();
            services.AddSingleton<ThemeService>();
            services.AddSingleton<ErrorReportBuilder>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ISystemService, SystemService>();
            services.AddSingleton<SourceBackupService>();

            // Core contracts, same registrations as v1 (TreasuryToolkit.App/Program.cs)
            services.AddTransient<IPdfProcessor, TextPdfProcessor>();
            services.AddTransient<IFileScanner, LocalFileScanner>();
            services.AddSingleton<ICompanyService, JsonCompanyService>();

            // ViewModels are singletons so each section keeps its state when navigating away and back, as in v1
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<FileRenamerViewModel>();
            services.AddSingleton<ExcelWorkflowViewModel>();
            services.AddSingleton<AboutViewModel>();

            // Windows
            services.AddSingleton<MainWindow>();
            services.AddTransient<ProgressWindow>();
            services.AddSingleton<Func<ProgressWindow>>(sp => () => sp.GetRequiredService<ProgressWindow>());
        }

        #region Unhandled exceptions
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // UI-thread errors are recoverable: report them and keep the app running, as v1 did
            e.Handled = true;
            ReportError(e.Exception);
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // Raised on the finalizer thread for a faulted Task nobody awaited
            e.SetObserved();
            Dispatcher.BeginInvoke(() => ReportError(e.Exception));
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // A background thread crashed; the process ends after this handler, so show the report first
            if (e.ExceptionObject is not Exception exception)
            {
                return;
            }

            if (Dispatcher.CheckAccess())
            {
                ReportError(exception);
            }
            else if (!Dispatcher.HasShutdownStarted)
            {
                Dispatcher.Invoke(() => ReportError(exception));
            }
        }

        private void ReportError(Exception exception)
        {
            // An error inside the error window (or before DI is ready) falls back to a plain message box
            if (isShowingError || services == null)
            {
                MessageBox.Show(exception.ToString(), "Tesoreria - Ocurrio un problema", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                isShowingError = true;
                try
                {
                    services.GetRequiredService<IDialogService>().ShowError(exception);
                }
                catch (Exception reportFailure)
                {
                    MessageBox.Show($"{exception}{Environment.NewLine}{Environment.NewLine}{reportFailure}",
                        "Tesoreria - Ocurrio un problema", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    isShowingError = false;
                }
            }

            // Without a main window nothing would ever close, so a startup failure ends the app here
            if (!startupCompleted)
            {
                Shutdown(1);
            }
        }
        #endregion
    }
}
