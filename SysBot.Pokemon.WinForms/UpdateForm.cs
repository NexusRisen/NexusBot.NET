using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{
    public class UpdateForm : Form
    {
        private Button buttonDownload;
        private Label labelUpdateInfo;
        private readonly Label labelChangelogTitle = new();
        private WebBrowser webBrowserChangelog;
        private readonly bool isUpdateRequired;
        private readonly bool isUpdateAvailable;
        private readonly string newVersion;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public UpdateForm(bool updateRequired, string newVersion, bool updateAvailable)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            isUpdateRequired = updateRequired;
            this.newVersion = newVersion;
            isUpdateAvailable = updateAvailable;
            InitializeComponent();
            Load += async (sender, e) => await FetchAndDisplayChangelog();
            UpdateFormText();
        }

        private void InitializeComponent()
        {
            labelUpdateInfo = new Label();
            buttonDownload = new Button();

            ClientSize = new Size(600, 450); // Increased size for better readability

            labelUpdateInfo.AutoSize = true;
            labelUpdateInfo.Location = new Point(12, 20);
            labelUpdateInfo.Size = new Size(560, 60);
            labelUpdateInfo.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            if (isUpdateRequired)
            {
                labelUpdateInfo.Text = "A required update is available. You must update to continue using this application.";
                ControlBox = false;
            }
            else if (isUpdateAvailable)
            {
                labelUpdateInfo.Text = "A new version is available! Check out what's new below.";
            }
            else
            {
                labelUpdateInfo.Text = "You are on the latest version.";
                buttonDownload.Text = "Re-Download Latest Version";
            }

            buttonDownload.Size = new Size(160, 35);
            buttonDownload.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            buttonDownload.BackColor = Color.FromArgb(0, 120, 215);
            buttonDownload.ForeColor = Color.White;
            buttonDownload.FlatStyle = FlatStyle.Flat;
            buttonDownload.FlatAppearance.BorderSize = 0;
            
            int buttonX = (ClientSize.Width - buttonDownload.Size.Width) / 2;
            int buttonY = ClientSize.Height - buttonDownload.Size.Height - 20;
            buttonDownload.Location = new Point(buttonX, buttonY);
            if (string.IsNullOrEmpty(buttonDownload.Text))
            {
                buttonDownload.Text = "Download Update";
            }
            buttonDownload.Click += ButtonDownload_Click;

            labelChangelogTitle.AutoSize = true;
            labelChangelogTitle.Location = new Point(12, 60);
            labelChangelogTitle.Size = new Size(70, 15);
            labelChangelogTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelChangelogTitle.Text = $"What's New in {newVersion}";

            webBrowserChangelog = new WebBrowser
            {
                Location = new Point(12, 90),
                Size = new Size(576, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
                ScriptErrorsSuppressed = true,
                ScrollBarsEnabled = true,
                IsWebBrowserContextMenuEnabled = false
            };

            Controls.Add(labelUpdateInfo);
            Controls.Add(buttonDownload);
            Controls.Add(labelChangelogTitle);
            Controls.Add(webBrowserChangelog);
            
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateForm";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White; // Clean background
            UpdateFormText();
        }

        private void UpdateFormText()
        {
            if (isUpdateAvailable)
            {
                Text = $"Update Available - {newVersion}";
            }
            else
            {
                Text = "Latest Version";
            }
        }

        public async void PerformUpdate()
        {
            buttonDownload.Enabled = false;
            buttonDownload.Text = "Downloading...";
            buttonDownload.BackColor = Color.Gray;

            try
            {
                string? downloadUrl = await UpdateChecker.FetchDownloadUrlAsync();
                if (!string.IsNullOrWhiteSpace(downloadUrl))
                {
                    string downloadedFilePath = await StartDownloadProcessAsync(downloadUrl);
                    if (!string.IsNullOrEmpty(downloadedFilePath))
                    {
                        InstallUpdate(downloadedFilePath);
                    }
                }
            }
            catch { }
        }

        private async Task FetchAndDisplayChangelog()
        {
            _ = new UpdateChecker();
            string markdown = await UpdateChecker.FetchChangelogAsync();
            string html = ConvertMarkdownToHtml(markdown);
            webBrowserChangelog.DocumentText = html;
        }

        private string ConvertMarkdownToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return "<html><body><p>No changelog available.</p></body></html>";

            // Basic Markdown to HTML conversion
            string htmlBody = markdown
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\r\n", "\n");

            // Headers
            htmlBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, @"^### (.*)$", "<h3>$1</h3>", System.Text.RegularExpressions.RegexOptions.Multiline);
            htmlBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, @"^## (.*)$", "<h2>$1</h2>", System.Text.RegularExpressions.RegexOptions.Multiline);
            htmlBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, @"^# (.*)$", "<h1>$1</h1>", System.Text.RegularExpressions.RegexOptions.Multiline);

            // Bold
            htmlBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, @"\*\*(.*?)\*\*", "<strong>$1</strong>");
            
            // Lists
            htmlBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, @"^\s*-\s+(.*)$", "<li>$1</li>", System.Text.RegularExpressions.RegexOptions.Multiline);
            
            // Wrap lists (simplified)
            htmlBody = htmlBody.Replace("</li>\n<li>", "</li><li>");
            // Note: This simple regex doesn't handle nested lists perfectly or wrap <ul> properly without more complex logic, 
            // but for release notes it's usually sufficient to just style li. 
            // Better approach: wrap contiguous li lines in ul.
            
            // Code blocks (inline)
            htmlBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, @"`(.*?)`", "<code>$1</code>");

            // Line breaks
            htmlBody = htmlBody.Replace("\n", "<br/>");

            // Cleanup <ul> wrapping (hacky but works for simple lists)
            // A better way is to style the <li> to not need a parent <ul> strictly if we just want bullet points, 
            // or do a proper pass. Let's just use CSS to style <li> if they aren't in <ul> (browsers handle this okay-ish) 
            // or just rely on <br> for newlines.
            // Actually, let's just make it a list if it starts with -
            
            string css = @"
                <style>
                    body { font-family: 'Segoe UI', sans-serif; font-size: 14px; line-height: 1.6; color: #333; padding: 15px; background-color: #ffffff; }
                    h1 { font-size: 20px; color: #0078d7; margin-bottom: 10px; border-bottom: 1px solid #eee; padding-bottom: 5px; }
                    h2 { font-size: 18px; color: #0078d7; margin-top: 15px; margin-bottom: 8px; }
                    h3 { font-size: 16px; font-weight: bold; margin-top: 12px; margin-bottom: 5px; }
                    li { margin-left: 20px; margin-bottom: 4px; list-style-type: disc; display: list-item; }
                    code { background-color: #f0f0f0; padding: 2px 4px; border-radius: 4px; font-family: Consolas, monospace; color: #c7254e; }
                    strong { font-weight: 600; }
                    p { margin-bottom: 10px; }
                </style>";

            return $"<!DOCTYPE html><html><head>{css}</head><body>{htmlBody}</body></html>";
        }

        private async void ButtonDownload_Click(object? sender, EventArgs? e)
        {
            buttonDownload.Enabled = false;
            buttonDownload.Text = "Downloading...";

            try
            {
                string? downloadUrl = await UpdateChecker.FetchDownloadUrlAsync();
                if (!string.IsNullOrWhiteSpace(downloadUrl))
                {
                    string downloadedFilePath = await StartDownloadProcessAsync(downloadUrl);
                    if (!string.IsNullOrEmpty(downloadedFilePath))
                    {
                        InstallUpdate(downloadedFilePath);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to fetch the download URL. Please check your internet connection and try again.",
                        "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonDownload.Enabled = true;
                buttonDownload.Text = isUpdateAvailable ? "Download Update" : "Re-Download Latest Version";
            }
        }

        private static async Task<string> StartDownloadProcessAsync(string downloadUrl)
        {
            Main.IsUpdating = true;
            string tempPath = Path.Combine(Path.GetTempPath(), $"SysBot.Pokemon.WinForms_{Guid.NewGuid()}.exe");
            
            const int maxRetries = 3;
            Exception? lastException = null;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                if (retry > 0)
                {
                    // Wait before retry (exponential backoff)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)));
                    Console.WriteLine($"Retrying download attempt {retry + 1}/{maxRetries}...");
                }

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10); // 10 minute timeout for downloads on slow connections
                    client.DefaultRequestHeaders.Add("User-Agent", "PokeBot");
                    // No auth token needed for public repo
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                    try
                    {
                        var response = await client.GetAsync(downloadUrl);
                        response.EnsureSuccessStatusCode();
                        
                        // Download with progress tracking for large files
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var totalBytes = response.Content.Headers.ContentLength ?? 0;
                            var bytesRead = 0;
                            var buffer = new byte[8192];
                            
                            using (var ms = new MemoryStream())
                            {
                                int read;
                                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await ms.WriteAsync(buffer, 0, read);
                                    bytesRead += read;
                                    
                                    if (totalBytes > 0)
                                    {
                                        var progress = (int)((bytesRead * 100L) / totalBytes);
                                        Console.WriteLine($"Download progress: {progress}%");
                                    }
                                }
                                
                                var fileBytes = ms.ToArray();
                                await File.WriteAllBytesAsync(tempPath, fileBytes);
                            }
                        }
                        Console.WriteLine($"Successfully downloaded update to {tempPath}");
                        return tempPath;
                    }
                    catch (TaskCanceledException ex)
                    {
                        Console.WriteLine($"Download timed out on attempt {retry + 1}: {ex.Message}");
                        lastException = ex;
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"Download failed on attempt {retry + 1}: {ex.Message}");
                        lastException = ex;
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during download on attempt {retry + 1}: {ex.Message}");
                        lastException = ex;
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                    }
                }
            }

            // All retries failed
            Console.WriteLine($"Failed to download update after {maxRetries} attempts");
            throw lastException ?? new Exception("Download failed after all retry attempts");
        }

        private static void InstallUpdate(string downloadedFilePath)
        {
            try
            {
                string currentExePath = Application.ExecutablePath;
                string applicationDirectory = Path.GetDirectoryName(currentExePath) ?? "";
                string executableName = Path.GetFileName(currentExePath);
                string backupPath = Path.Combine(applicationDirectory, $"{executableName}.backup");

                // Create batch file for update process
                string batchPath = Path.Combine(Path.GetTempPath(), "UpdateSysBot.bat");
                string batchContent = @$"
                                            @echo off
                                            timeout /t 2 /nobreak >nul
                                            echo Updating SysBot...

                                            rem Backup current version
                                            if exist ""{currentExePath}"" (
                                                if exist ""{backupPath}"" (
                                                    del ""{backupPath}""
                                                )
                                                move ""{currentExePath}"" ""{backupPath}""
                                            )

                                            rem Install new version
                                            move ""{downloadedFilePath}"" ""{currentExePath}""

                                            rem Start new version
                                            start """" ""{currentExePath}""

                                            rem Clean up
                                            del ""%~f0""
                                            ";

                File.WriteAllText(batchPath, batchContent);

                // Start the update batch file
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = batchPath,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(startInfo);

                // Exit the current instance
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to install update: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}