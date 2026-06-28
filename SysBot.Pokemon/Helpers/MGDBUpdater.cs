using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SysBot.Base;

namespace SysBot.Pokemon.Helpers
{
    public static class MGDBUpdater
    {
        private const string RepoApiUrl = "https://api.github.com/repos/projectpokemon/EventsGallery/commits/master";
        private const string RepoZipUrl = "https://github.com/projectpokemon/EventsGallery/archive/refs/heads/master.zip";
        private const string VersionFileName = "mgdb_version.txt";

        public static async Task UpdateMGDBAsync(string mgdbPath)
        {
            if (string.IsNullOrWhiteSpace(mgdbPath))
                return;

            if (!Directory.Exists(mgdbPath))
                Directory.CreateDirectory(mgdbPath);

            LogUtil.LogInfo($"Directory located at: {Path.GetFullPath(mgdbPath)}", "MGDB");

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "NexusBot");

                // Check latest commit
                var response = await client.GetAsync(RepoApiUrl).ConfigureAwait(false);
                string? latestCommit = null;
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var doc = JsonDocument.Parse(json!);
                    latestCommit = doc.RootElement.GetProperty("sha").GetString();
                }
                else
                {
                    LogUtil.LogError($"Could not check for MGDB updates (GitHub API returned {response.StatusCode}).", "MGDB");
                }

                string versionFile = Path.Combine(mgdbPath, VersionFileName);
                bool hasSubdirs = Directory.GetDirectories(mgdbPath).Length > 0;

                if (latestCommit != null && File.Exists(versionFile) && hasSubdirs)
                {
                    var currentCommit = await File.ReadAllTextAsync(versionFile).ConfigureAwait(false);
                    if (currentCommit == latestCommit)
                    {
                        LogUtil.LogInfo("Up to date.", "MGDB");
                        return;
                    }
                }
                else if (latestCommit == null && hasSubdirs)
                {
                    // Rate limited but we have files, assume we're good to avoid re-downloading constantly
                    LogUtil.LogInfo("Local files exist. Skipping download due to rate limit.", "MGDB");
                    return;
                }

                LogUtil.LogInfo("Downloading update from GitHub...", "MGDB");
                // Download zip
                var zipResponse = await client.GetAsync(RepoZipUrl).ConfigureAwait(false);
                if (!zipResponse.IsSuccessStatusCode)
                    return;

                var zipPath = Path.Combine(mgdbPath, "mgdb.zip");
                using (var fs = new FileStream(zipPath, FileMode.Create))
                {
                    await zipResponse.Content.CopyToAsync(fs).ConfigureAwait(false);
                }

                // Delete old files except version file and zip
                foreach (var dir in Directory.GetDirectories(mgdbPath))
                    Directory.Delete(dir, true);
                foreach (var file in Directory.GetFiles(mgdbPath))
                {
                    if (file != zipPath && file != versionFile)
                        File.Delete(file);
                }

                ZipFile.ExtractToDirectory(zipPath, mgdbPath, true);
                File.Delete(zipPath);

                await File.WriteAllTextAsync(versionFile, latestCommit ?? string.Empty).ConfigureAwait(false);
                
                LogUtil.LogInfo("Update completed to the latest version.", "MGDB");
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Error updating: {ex.Message}", "MGDB");
            }
        }
    }
}
