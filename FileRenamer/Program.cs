using ClientUtils.Core.Contracts;
using ClienUtils.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileRenamer
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
            var mainForm = serviceProvider.GetRequiredService<FileRenamerForm>();

            Application.Run(mainForm);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<FileRenamerForm>();
            services.AddTransient<ProgressForm>();
            services.AddSingleton<Func<ProgressForm>>(x => () => x.GetRequiredService<ProgressForm>());
            services.AddTransient<IPdfProcessor, TextPdfProcessor>();
        }
    }
}