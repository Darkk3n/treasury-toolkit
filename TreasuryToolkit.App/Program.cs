using Microsoft.Extensions.DependencyInjection;
using TreasuryToolkit.Core.Contracts;
using TreasuryToolkit.Infra.Services;

namespace TreasuryToolkit.App
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // iText discovers itext.font_asian.dll (CJK CMaps, e.g. UniGB-UTF16-H) by scanning
            // loose *.dll files next to the exe (see ResourceUtil.LoadITextResourceAssemblies).
            // In the single-file publish used for releases there's no loose file to find, so CJK
            // PDFs fail to parse in Prod only. Force-loading it here puts it in the AppDomain
            // before any PDF is touched, which iText's resource lookup also checks.
            System.Reflection.Assembly.Load("itext.font_asian");
            ApplicationConfiguration.Initialize();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // 2. Start the app by requesting the Main Form from the container

            Application.ThreadException += new ThreadExceptionEventHandler(UIThreadException);
            // Set the unhandled exception mode to force all Windows Forms errors to go through
            // our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<UcFileRenamer>();
            services.AddTransient<UcExcelWorkflowAutomator>();
            services.AddSingleton<MainForm>();
            services.AddTransient<ProgressForm>();
            services.AddSingleton<Func<ProgressForm>>(x => () => x.GetRequiredService<ProgressForm>());
            services.AddTransient<IPdfProcessor, TextPdfProcessor>();
            services.AddTransient<IFileScanner, LocalFileScanner>();
            services.AddSingleton<ICompanyService, JsonCompanyService>();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {

            using var handler = new ExceptionHandlerForm(t.Exception);
            handler.ShowDialog();
        }
    }
}