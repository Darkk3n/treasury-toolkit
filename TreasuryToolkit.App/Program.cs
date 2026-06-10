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
            ApplicationConfiguration.Initialize();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // 2. Start the app by requesting the Main Form from the container
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
        }
    }
}