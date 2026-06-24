using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

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

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "NexusBot");

                // Check latest commit
                var response = await client.GetAsync(RepoApiUrl);
                if (!response.IsSuccessStatusCode)
                    return;

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var latestCommit = doc.RootElement.GetProperty("sha").GetString();

                string versionFile = Path.Combine(mgdbPath, VersionFileName);
                if (File.Exists(versionFile))
                {
                    var currentCommit = await File.ReadAllTextAsync(versionFile);
                    if (currentCommit == latestCommit)
                        return; // Up to date
                }

                // Download zip
                var zipResponse = await client.GetAsync(RepoZipUrl);
                if (!zipResponse.IsSuccessStatusCode)
                    return;

                var zipPath = Path.Combine(mgdbPath, "mgdb.zip");
                using (var fs = new FileStream(zipPath, FileMode.Create))
                {
                    await zipResponse.Content.CopyToAsync(fs);
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

                await File.WriteAllTextAsync(versionFile, latestCommit ?? string.Empty);
                
                Console.WriteLine("MGDB has been updated to the latest version.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating MGDB: {ex.Message}");
            }
        }
    }
}
