using System.IO;
using System.Text.Json;

namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>
    /// Loads and saves <see cref="LocalAppSettings"/>. A missing or unreadable file falls back to defaults,
    /// as in v1, so a bad settings file never stops the app from starting.
    /// </summary>
    public class SettingsService
    {
        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        public string FilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.local.json");

        public LocalAppSettings Current { get; private set; } = new();

        public void Load()
        {
            if (TryReadFromDisk(out var settings, out _) && settings != null)
            {
                Current = settings;
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(Current, WriteOptions));
            }
            catch (Exception)
            {
                // Losing a theme or window-size preference isn't worth interrupting the user over
            }
        }

        /// <summary>
        /// Reads the file as it is on disk right now (used by the error report, which shows what the next launch
        /// will restore). Returns false with <paramref name="error"/> set when the file exists but can't be read.
        /// </summary>
        public bool TryReadFromDisk(out LocalAppSettings settings, out string error)
        {
            settings = null;
            error = null;

            if (!File.Exists(FilePath))
            {
                return true;
            }

            try
            {
                settings = JsonSerializer.Deserialize<LocalAppSettings>(File.ReadAllText(FilePath));
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
