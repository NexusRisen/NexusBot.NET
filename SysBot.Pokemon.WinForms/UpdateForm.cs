using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using SysBot.Base;

namespace SysBot.Pokemon.WinForms
{
    public partial class UpdateForm : Form
    {
        private Button buttonDownload;
        private Label labelUpdateInfo;
        private Label labelChangelogTitle;
        private WebBrowser webBrowserChangelog;
        private ProgressBar progressBarDownload;
        private Label labelDownloadStatus;
        private Panel panelHeader;
        
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
            // Initialize controls
            panelHeader = new Panel();
            labelUpdateInfo = new Label();
            buttonDownload = new Button();
            labelChangelogTitle = new Label();
            webBrowserChangelog = new WebBrowser();
            progressBarDownload = new ProgressBar();
            labelDownloadStatus = new Label();

            // Form settings
            ClientSize = new Size(700, 550);
            MinimumSize = new Size(600, 500);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            BackColor = Color.White;
            ForeColor = Color.FromArgb(50, 50, 50);

            // Header Panel
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 80;
            panelHeader.BackColor = Color.FromArgb(240, 244, 248);
            panelHeader.Padding = new Padding(20);

            // Update Info Label
            labelUpdateInfo.AutoSize = false;
            labelUpdateInfo.Dock = DockStyle.Fill;
            labelUpdateInfo.TextAlign = ContentAlignment.MiddleLeft;
            labelUpdateInfo.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            labelUpdateInfo.ForeColor = Color.FromArgb(30, 30, 30);
            
            if (isUpdateRequired)
            {
                labelUpdateInfo.Text = "⚠️ A required update is available.\nYou must update to continue using PokeBot.";
                labelUpdateInfo.ForeColor = Color.FromArgb(200, 0, 0);
                ControlBox = false; // Prevent closing if required
            }
            else if (isUpdateAvailable)
            {
                labelUpdateInfo.Text = $"✨ A new version ({newVersion}) is available!\nCheck out the improvements below.";
            }
            else
            {
                labelUpdateInfo.Text = "✅ You are using the latest version of PokeBot.";
                buttonDownload.Text = "Re-Install Current Version";
            }
            
            panelHeader.Controls.Add(labelUpdateInfo);

            // Changelog Title
            labelChangelogTitle.AutoSize = true;
            labelChangelogTitle.Location = new Point(20, 100);
            labelChangelogTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            labelChangelogTitle.ForeColor = Color.FromArgb(0, 120, 215);
            labelChangelogTitle.Text = $"What's New in {newVersion}";

            // WebBrowser (Changelog)
            webBrowserChangelog.Location = new Point(20, 130);
            webBrowserChangelog.Size = new Size(660, 320);
            webBrowserChangelog.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            webBrowserChangelog.ScriptErrorsSuppressed = true;
            webBrowserChangelog.ScrollBarsEnabled = true;
            webBrowserChangelog.IsWebBrowserContextMenuEnabled = false;

            // Progress Bar
            progressBarDownload.Location = new Point(20, 465);
            progressBarDownload.Size = new Size(660, 20);
            progressBarDownload.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBarDownload.Visible = false;
            progressBarDownload.Style = ProgressBarStyle.Continuous;

            // Download Status Label
            labelDownloadStatus.AutoSize = true;
            labelDownloadStatus.Location = new Point(20, 490);
            labelDownloadStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelDownloadStatus.Visible = false;
            labelDownloadStatus.Text = "Initializing download...";

            // Download Button
            buttonDownload.Size = new Size(200, 45);
            buttonDownload.Location = new Point(ClientSize.Width - 220, ClientSize.Height - 65);
            buttonDownload.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonDownload.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            buttonDownload.BackColor = Color.FromArgb(0, 120, 215);
            buttonDownload.ForeColor = Color.White;
            buttonDownload.FlatStyle = FlatStyle.Flat;
            buttonDownload.FlatAppearance.BorderSize = 0;
            buttonDownload.Cursor = Cursors.Hand;
            if (string.IsNullOrEmpty(buttonDownload.Text)) buttonDownload.Text = "Download & Install Update";
            buttonDownload.Click += ButtonDownload_Click;

            // Add Controls
            Controls.Add(panelHeader);
            Controls.Add(labelChangelogTitle);
            Controls.Add(webBrowserChangelog);
            Controls.Add(progressBarDownload);
            Controls.Add(labelDownloadStatus);
            Controls.Add(buttonDownload);
            
            Name = "UpdateForm";
            StartPosition = FormStartPosition.CenterScreen;
            UpdateFormText();
        }

        private void UpdateFormText()
        {
            Text = isUpdateAvailable ? $"Update Available - {newVersion}" : "Latest Version";
        }

        private async Task FetchAndDisplayChangelog()
        {
            try
            {
                // We don't need to instantiate UpdateChecker, just call static methods
                string markdown = await UpdateChecker.FetchChangelogAsync();
                string html = ConvertMarkdownToHtml(markdown);
                webBrowserChangelog.DocumentText = html;
            }
            catch (Exception ex)
            {
                webBrowserChangelog.DocumentText = $"<html><body><p>Error loading changelog: {ex.Message}</p></body></html>";
            }
        }

        private static string ConvertMarkdownToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return "<html><body><p>No changelog available.</p></body></html>";

            // Basic Markdown to HTML conversion
            string htmlBody = markdown
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\r\n", "\n");

            // Headers
            htmlBody = Header3Regex().Replace(htmlBody, "<h3>$1</h3>");
            htmlBody = Header2Regex().Replace(htmlBody, "<h2>$1</h2>");
            htmlBody = Header1Regex().Replace(htmlBody, "<h1>$1</h1>");

            // Bold
            htmlBody = BoldRegex().Replace(htmlBody, "<strong>$1</strong>");
            
            // Lists
            htmlBody = ListRegex().Replace(htmlBody, "<li>$1</li>");
            
            // Wrap lists (simplified)
            htmlBody = htmlBody.Replace("</li>\n<li>", "</li><li>");
            
            // Code blocks (inline)
            htmlBody = CodeRegex().Replace(htmlBody, "<code>$1</code>");

            // Line breaks
            htmlBody = htmlBody.Replace("\n", "<br/>");

            string css = @"
                <style>
                    body { font-family: 'Segoe UI', sans-serif; font-size: 14px; line-height: 1.6; color: #333; padding: 15px; background-color: #ffffff; }
                    h1 { font-size: 22px; color: #0078d7; margin-bottom: 10px; border-bottom: 2px solid #f0f0f0; padding-bottom: 8px; }
                    h2 { font-size: 18px; color: #2c3e50; margin-top: 20px; margin-bottom: 10px; font-weight: 600; }
                    h3 { font-size: 16px; font-weight: bold; margin-top: 15px; margin-bottom: 5px; }
                    ul { margin-top: 5px; margin-bottom: 15px; padding-left: 20px; }
                    li { margin-bottom: 5px; }
                    code { background-color: #f6f8fa; padding: 2px 5px; border-radius: 4px; font-family: Consolas, monospace; color: #d63384; border: 1px solid #e1e4e8; }
                    strong { font-weight: 700; color: #24292e; }
                    p { margin-bottom: 10px; }
                    a { color: #0366d6; text-decoration: none; }
                    a:hover { text-decoration: underline; }
                </style>";

            return $"<!DOCTYPE html><html><head>{css}</head><body>{htmlBody}</body></html>";
        }

        [GeneratedRegex(@"^### (.*)$", RegexOptions.Multiline)]
        private static partial Regex Header3Regex();

        [GeneratedRegex(@"^## (.*)$", RegexOptions.Multiline)]
        private static partial Regex Header2Regex();

        [GeneratedRegex(@"^# (.*)$", RegexOptions.Multiline)]
        private static partial Regex Header1Regex();

        [GeneratedRegex(@"\*\*(.*?)\*\*", RegexOptions.None)]
        private static partial Regex BoldRegex();

        [GeneratedRegex(@"^\s*-\s+(.*)$", RegexOptions.Multiline)]
        private static partial Regex ListRegex();

        [GeneratedRegex(@"`(.*?)`", RegexOptions.None)]
        private static partial Regex CodeRegex();

        public void PerformUpdate()
        {
            if (!IsHandleCreated) CreateHandle();
            ButtonDownload_Click(this, EventArgs.Empty);
        }

        private async void ButtonDownload_Click(object? sender, EventArgs? e)
        {
            buttonDownload.Enabled = false;
            buttonDownload.Text = "Preparing Download...";
            buttonDownload.BackColor = Color.Gray;

            // Show progress UI
            progressBarDownload.Visible = true;
            labelDownloadStatus.Visible = true;
            progressBarDownload.Value = 0;

            try
            {
                string? downloadUrl = await UpdateChecker.FetchDownloadUrlAsync();
                if (!string.IsNullOrWhiteSpace(downloadUrl))
                {
                    string downloadedFilePath = await DownloadUpdateAsync(downloadUrl);
                    if (!string.IsNullOrEmpty(downloadedFilePath))
                    {
                        labelDownloadStatus.Text = "Installing update...";
                        InstallUpdate(downloadedFilePath);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to fetch the download URL. Please check your internet connection and try again.",
                        "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetUI();
            }
        }

        private void ResetUI()
        {
            buttonDownload.Enabled = true;
            buttonDownload.Text = isUpdateAvailable ? "Download & Install Update" : "Re-Install Current Version";
            buttonDownload.BackColor = Color.FromArgb(0, 120, 215);
            progressBarDownload.Visible = false;
            labelDownloadStatus.Visible = false;
        }

        private async Task<string> DownloadUpdateAsync(string downloadUrl)
        {
            Main.IsUpdating = true;
            string tempPath = Path.Combine(Path.GetTempPath(), $"SysBot.Pokemon.WinForms_{Guid.NewGuid()}.exe");
            
            const int maxRetries = 3;
            Exception? lastException = null;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                if (retry > 0)
                {
                    labelDownloadStatus.Text = $"Retrying download ({retry}/{maxRetries})...";
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)));
                }

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10);
                client.DefaultRequestHeaders.Add("User-Agent", "PokeBot");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                try
                {
                    labelDownloadStatus.Text = "Starting download...";
                    LogUtil.LogInfo("Update", $"Starting download from {downloadUrl}");
                    
                    using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    
                    var buffer = new byte[8192];
                    long totalRead = 0;
                    int read;
                    int lastLoggedProgress = 0;
                    
                    while ((read = await stream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, read));
                        totalRead += read;
                        
                        if (canReportProgress)
                        {
                            var progress = (int)((totalRead * 100L) / totalBytes);
                            // Log every 25%
                            if (progress >= lastLoggedProgress + 25)
                            {
                                LogUtil.LogInfo("Update", $"Download progress: {progress}%");
                                lastLoggedProgress = progress;
                            }

                            // Update UI on UI thread
                            if (IsHandleCreated)
                            {
                                Invoke((MethodInvoker)delegate {
                                    progressBarDownload.Value = progress;
                                    labelDownloadStatus.Text = $"Downloading... {progress}% ({FormatBytes(totalRead)} / {FormatBytes(totalBytes)})";
                                });
                            }
                        }
                        else
                        {
                            if (IsHandleCreated)
                            {
                                Invoke((MethodInvoker)delegate {
                                    labelDownloadStatus.Text = $"Downloading... {FormatBytes(totalRead)}";
                                });
                            }
                        }
                    }
                    
                    LogUtil.LogInfo("Update", $"Download complete: {tempPath}");
                    return tempPath;
                }
                catch (Exception ex)
                {
                    LogUtil.LogError($"Download failed (Attempt {retry + 1}): {ex.Message}", "Update");
                    lastException = ex;
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }
            }

            throw lastException ?? new Exception("Download failed after all retry attempts");
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
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
title Updating PokeBot...
color 0A
echo Waiting for PokeBot to close...
timeout /t 3 /nobreak >nul

:RETRY_DELETE
if exist ""{currentExePath}"" (
    echo Backing up current version...
    if exist ""{backupPath}"" del ""{backupPath}""
    move ""{currentExePath}"" ""{backupPath}""
    if exist ""{currentExePath}"" (
        echo Failed to move file. Retrying in 1 second...
        timeout /t 1 /nobreak >nul
        goto RETRY_DELETE
    )
)

echo Installing new version...
move ""{downloadedFilePath}"" ""{currentExePath}""

echo Starting PokeBot...
start """" ""{currentExePath}""

echo Cleaning up...
del ""%~f0""
";

                File.WriteAllText(batchPath, batchContent);

                ProcessStartInfo startInfo = new()
                {
                    FileName = batchPath,
                    CreateNoWindow = false, // Let the user see the update window
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal // Show the window so they know it's working
                };

                Process.Start(startInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to install update: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}