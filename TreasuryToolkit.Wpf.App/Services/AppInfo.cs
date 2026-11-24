using System.Reflection;

namespace TreasuryToolkit.Wpf.App.Services
{
    public static class AppInfo
    {
        /// <summary>
        /// Stamped from the release tag by the CI workflow; local builds report the csproj's 0.0.0-dev.
        /// Anything after "+" (build metadata such as a commit sha) is dropped.
        /// </summary>
        public static string Version { get; } =
            (Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0")
            .Split('+')[0];
    }
}
