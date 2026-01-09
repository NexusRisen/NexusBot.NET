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
        private FlowLayoutPanel flowPanelChangelog;
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            SysBot.Pokemon.WinForms.Helpers.Theme.PaintBackground(e.Graphics, this.ClientRectangle);
        }

        private void InitializeComponent()
        {
            // Initialize controls
            panelHeader = new Panel();
            labelUpdateInfo = new Label();
            buttonDownload = new Button();
            labelChangelogTitle = new Label();
            flowPanelChangelog = new FlowLayoutPanel();
            progressBarDownload = new ProgressBar();
            labelDownloadStatus = new Label();

            // Form settings
            ClientSize = new Size(700, 550);
            MinimumSize = new Size(600, 500);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            
            // Apply Theme
            SysBot.Pokemon.WinForms.Helpers.Theme.Apply(this);

            // Header Panel
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 80;
            panelHeader.BackColor = Color.Transparent;
            panelHeader.Padding = new Padding(20);

            // Update Info Label
            labelUpdateInfo.AutoSize = false;
            labelUpdateInfo.Dock = DockStyle.Fill;
            labelUpdateInfo.TextAlign = ContentAlignment.MiddleLeft;
            labelUpdateInfo.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            labelUpdateInfo.ForeColor = SysBot.Pokemon.WinForms.Helpers.Theme.TextColor;
            
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

            // FlowLayoutPanel (Changelog)
            flowPanelChangelog.Location = new Point(20, 130);
            flowPanelChangelog.Size = new Size(660, 320);
            flowPanelChangelog.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            flowPanelChangelog.BackColor = Color.Transparent;
            flowPanelChangelog.AutoScroll = true;
            flowPanelChangelog.FlowDirection = FlowDirection.TopDown;
            flowPanelChangelog.WrapContents = false;
            flowPanelChangelog.Resize += (s, e) =>
            {
                foreach (Control c in flowPanelChangelog.Controls)
                {
                    c.MaximumSize = new Size(flowPanelChangelog.Width - 30, 0);
                }
            };

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
            Controls.Add(flowPanelChangelog);
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
                RenderChangelog(markdown);
            }
            catch (Exception ex)
            {
                flowPanelChangelog.Controls.Clear();
                var lbl = new Label
                {
                    Text = $"Error loading changelog: {ex.Message}",
                    ForeColor = Color.Red,
                    AutoSize = true,
                    MaximumSize = new Size(flowPanelChangelog.Width - 30, 0)
                };
                flowPanelChangelog.Controls.Add(lbl);
            }
        }

        private void RenderChangelog(string markdown)
        {
            flowPanelChangelog.Controls.Clear();
            flowPanelChangelog.SuspendLayout();

            if (string.IsNullOrWhiteSpace(markdown))
            {
                flowPanelChangelog.ResumeLayout();
                return;
            }

            var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var lbl = new Label
                {
                    AutoSize = true,
                    MaximumSize = new Size(flowPanelChangelog.Width - 30, 0),
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 5) // Spacing
                };

                if (line.StartsWith("### "))
                {
                    lbl.Text = line.Substring(4).Trim();
                    lbl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    lbl.ForeColor = SysBot.Pokemon.WinForms.Helpers.Theme.AccentCyan;
                    lbl.Margin = new Padding(0, 10, 0, 5);
                }
                else if (line.StartsWith("## "))
                {
                    lbl.Text = line.Substring(3).Trim();
                    lbl.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                    lbl.ForeColor = SysBot.Pokemon.WinForms.Helpers.Theme.TextColor;
                    lbl.Margin = new Padding(0, 15, 0, 5);
                }
                else if (line.StartsWith("# "))
                {
                    lbl.Text = line.Substring(2).Trim();
                    lbl.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                    lbl.ForeColor = SysBot.Pokemon.WinForms.Helpers.Theme.AccentCyan;
                    lbl.Margin = new Padding(0, 10, 0, 5);
                }
                else if (line.Trim().StartsWith("- "))
                {
                    lbl.Text = "• " + line.Trim().Substring(2);
                    lbl.Font = new Font("Segoe UI", 9F);
                    lbl.ForeColor = SysBot.Pokemon.WinForms.Helpers.Theme.TextColor;
                    lbl.Padding = new Padding(10, 0, 0, 0); // Indent
                }
                else
                {
                    lbl.Text = line.Trim();
                    lbl.Font = new Font("Segoe UI", 9F);
                    lbl.ForeColor = SysBot.Pokemon.WinForms.Helpers.Theme.TextColor;
                }
                
                // Strip bold markers for now
                lbl.Text = lbl.Text.Replace("**", "");
                lbl.Text = lbl.Text.Replace("`", "");

                flowPanelChangelog.Controls.Add(lbl);
            }

            flowPanelChangelog.ResumeLayout();
        }

        public void PerformUpdate()
        {
            if (!IsHandleCreated) CreateHandle();
            ButtonDownload_Click(this, EventArgs.Empty);
        }

        private async void ButtonDownload_Click(object? sender, EventArgs? e)
        {
            // Fail-safe: Ensure we are running as PokeBot.exe before downloading
            string currentExe = Path.GetFileName(Application.ExecutablePath);
            if (!string.Equals(currentExe, "PokeBot.exe", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show($"Update/Download can only be performed when running as 'PokeBot.exe'.\nCurrent executable: {currentExe}",
                    "Update Safety Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
            buttonDownload.BackColor = SysBot.Pokemon.WinForms.Helpers.Theme.AccentCyan;
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