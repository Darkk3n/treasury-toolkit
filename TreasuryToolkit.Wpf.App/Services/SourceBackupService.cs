using System.IO;

namespace TreasuryToolkit.Wpf.App.Services
{
    /// <summary>
    /// Copies the source folder before processing and removes the copy once processing succeeds
    /// (v1's UcFileRenamer.BackupSourceFiles / DeleteBackUp). If processing fails, the copy stays behind so the
    /// originals can be restored by hand. Pure file I/O, safe to run off the UI thread.
    /// </summary>
    public class SourceBackupService
    {
        /// <summary>
        /// Copies <paramref name="sourceFolder"/> next to itself as "&lt;name&gt;_Backup_yyyyMMdd". Returns the backup
        /// path, or null if the copy couldn't be made. Like v1, a failed backup doesn't stop processing.
        /// </summary>
        public string CreateBackup(string sourceFolder)
        {
            if (!Directory.Exists(sourceFolder))
            {
                return null;
            }

            try
            {
                string parent = Directory.GetParent(sourceFolder).FullName;
                string baseName = Path.GetFileName(sourceFolder) + "_Backup_" + DateTime.Now.ToString("yyyyMMdd");
                string backupFolder = Path.Combine(parent, baseName);

                // Never write into an existing backup: one left behind by a failed run still holds the originals
                for (int attempt = 2; Directory.Exists(backupFolder); attempt++)
                {
                    backupFolder = Path.Combine(parent, $"{baseName}_{attempt}");
                }

                CopyDirectory(sourceFolder, backupFolder);
                return backupFolder;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Deletes the backup. Returns null on success, or the reason it couldn't be deleted.</summary>
        public string TryDelete(string backupFolder)
        {
            try
            {
                if (string.IsNullOrEmpty(backupFolder) || !Directory.Exists(backupFolder))
                {
                    return null;
                }

                // Same steps as v1: step out of the folder, release any PDF streams still waiting for finalization,
                // and clear read-only attributes before deleting
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                GC.Collect();
                GC.WaitForPendingFinalizers();

                var dirInfo = new DirectoryInfo(backupFolder);
                foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Attributes = FileAttributes.Normal;
                }
                foreach (var subDir in dirInfo.GetDirectories("*", SearchOption.AllDirectories))
                {
                    subDir.Attributes = FileAttributes.Normal;
                }
                dirInfo.Attributes = FileAttributes.Normal;

                int retries = 3;
                while (true)
                {
                    try
                    {
                        Directory.Delete(backupFolder, true);
                        return null;
                    }
                    catch (UnauthorizedAccessException) when (retries > 1)
                    {
                        retries--;
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Replaces v1's Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory, which relies on WinForms UI options
        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
            }

            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
            }
        }
    }
}
