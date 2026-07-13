using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{
    public class UpdateForm : Form
    {
        private Button buttonDownload = null!;
        private Label labelUpdateInfo = null!;
        private readonly Label labelChangelogTitle = new();
        private RichTextBox textBoxChangelog = null!;
        private ProgressBar progressBarDownload = null!;
        private Label labelProgress = null!;
        private Button buttonCopy = null!;
        private readonly bool isUpdateRequired;
        private readonly bool isUpdateAvailable;
        private readonly string newVersion;

        // Colors based on the app's Dark Theme
        private static readonly Color DarkGrey = Color.FromArgb(30, 30, 30);
        private static readonly Color LightGrey = Color.FromArgb(60, 60, 60);
        private static readonly Color SoftWhite = Color.FromArgb(245, 245, 245);
        private static readonly Color AccentBlue = Color.FromArgb(0, 120, 215);

        public UpdateForm(bool updateRequired, string newVersion, bool updateAvailable)
        {
            isUpdateRequired = updateRequired;
            this.newVersion = newVersion;
            isUpdateAvailable = updateAvailable;
            
            InitializeComponent();
            ThemeManager.ApplyTheme(this, ThemeManager.CurrentTheme);
            if (isUpdateRequired)
            {
                labelUpdateInfo.ForeColor = Color.FromArgb(255, 100, 100); // Re-apply alert color after theme
            }
            Load += async (sender, e) => {
                await FetchAndDisplayChangelog();
                labelChangelogTitle.Focus(); // Move focus away from the textbox to prevent auto-selection
                
                bool available = await UpdateChecker.IsDownloadAvailableAsync();
                if (!available)
                {
                    buttonDownload.Enabled = false;
                    buttonDownload.Text = "Not Available Yet";
                    labelUpdateInfo.Text = "An update is listed, but the program files are not yet available for download. Please try again in a few minutes.";
                }
            };
            UpdateFormText();
        }

        private void InitializeComponent()
        {
            labelUpdateInfo = new Label();
            buttonDownload = new Button { Name = "ButtonUpdate" };
            progressBarDownload = new ProgressBar();
            labelProgress = new Label();
            textBoxChangelog = new RichTextBox();
            buttonCopy = new Button();

            SuspendLayout();

            // Form Settings
            ClientSize = new Size(520, 450);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = isUpdateAvailable ? $"Update Available ({newVersion})" : "Re-Download Latest Version";

            // labelUpdateInfo
            labelUpdateInfo.AutoSize = false;
            labelUpdateInfo.Location = new Point(15, 15);
            labelUpdateInfo.Size = new Size(490, 50);
            labelUpdateInfo.Font = new Font("Segoe UI", 9.5F);
            labelUpdateInfo.TextAlign = ContentAlignment.MiddleLeft;
            if (isUpdateRequired)
            {
                labelUpdateInfo.Text = "A required update is available. You must update to continue using this application.";
                labelUpdateInfo.ForeColor = Color.FromArgb(255, 100, 100); // Light red for alert
                ControlBox = false;
            }
            else if (isUpdateAvailable)
            {
                labelUpdateInfo.Text = "A new version is available. Please download the latest version.";
            }
            else
            {
                labelUpdateInfo.Text = "You are on the latest version. You can re-download if needed.";
                buttonDownload.Text = "Re-Download Latest Version";
            }

            // labelChangelogTitle
            labelChangelogTitle.AutoSize = true;
            labelChangelogTitle.Location = new Point(15, 75);
            labelChangelogTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            labelChangelogTitle.Text = $"Release Notes ({newVersion}):";

            // textBoxChangelog
            textBoxChangelog.ReadOnly = true;
            textBoxChangelog.ScrollBars = RichTextBoxScrollBars.Vertical;
            textBoxChangelog.Location = new Point(15, 100);
            textBoxChangelog.Size = new Size(490, 220);
            textBoxChangelog.BackColor = Color.FromArgb(20, 20, 20);
            textBoxChangelog.ForeColor = Color.Gainsboro;
            textBoxChangelog.BorderStyle = BorderStyle.None;
            textBoxChangelog.Font = new Font("Consolas", 9F);
            // Prevent selection highlighting on click by capturing the Enter event
            textBoxChangelog.Enter += (s, e) => { labelChangelogTitle.Focus(); };

            // buttonCopy
            buttonCopy.Size = new Size(60, 25);
            buttonCopy.Location = new Point(445, 72);
            buttonCopy.FlatStyle = FlatStyle.Flat;
            buttonCopy.FlatAppearance.BorderSize = 0;
            buttonCopy.BackColor = Color.FromArgb(45, 45, 45);
            buttonCopy.ForeColor = Color.Silver;
            buttonCopy.Font = new Font("Segoe UI", 8F);
            buttonCopy.Text = "Copy";
            buttonCopy.Click += (s, e) => {
                if (!string.IsNullOrEmpty(textBoxChangelog.Text))
                {
                    Clipboard.SetText(textBoxChangelog.Text);
                    buttonCopy.Text = "Copied!";
                    Task.Delay(2000).ContinueWith(_ => Invoke(() => buttonCopy.Text = "Copy"));
                }
            };

            // progressBarDownload
            progressBarDownload.Location = new Point(15, 335);
            progressBarDownload.Size = new Size(410, 23);
            progressBarDownload.Visible = false;

            // labelProgress
            labelProgress.AutoSize = true;
            labelProgress.Location = new Point(435, 338);
            labelProgress.Size = new Size(50, 20);
            labelProgress.Text = "0%";
            labelProgress.Visible = false;

            // buttonDownload
            buttonDownload.Size = new Size(180, 40);
            buttonDownload.Location = new Point(170, 385);
            buttonDownload.FlatStyle = FlatStyle.Flat;
            buttonDownload.FlatAppearance.BorderSize = 1;
            buttonDownload.FlatAppearance.BorderColor = LightGrey;
            buttonDownload.BackColor = Color.FromArgb(50, 50, 50);
            buttonDownload.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            if (string.IsNullOrEmpty(buttonDownload.Text))
            {
                buttonDownload.Text = "Download Update";
            }
            buttonDownload.Click += ButtonDownload_Click;

            Controls.Add(labelUpdateInfo);
            Controls.Add(labelChangelogTitle);
            Controls.Add(textBoxChangelog);
            Controls.Add(buttonCopy);
            Controls.Add(progressBarDownload);
            Controls.Add(labelProgress);
            Controls.Add(buttonDownload);

            ResumeLayout(false);
            PerformLayout();
        }

        private void UpdateFormText()
        {
            Text = isUpdateAvailable ? $"Update Available ({newVersion})" : "Re-Download Latest Version";
        }

        private async Task FetchAndDisplayChangelog()
        {
            textBoxChangelog.Text = "Fetching changelog...";
            string changelog = await UpdateChecker.FetchChangelogAsync();
            RenderMarkdown(changelog);
        }

        private void RenderMarkdown(string markdown)
        {
            textBoxChangelog.Clear();
            if (string.IsNullOrWhiteSpace(markdown)) return;

            string[] lines = markdown.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                ProcessLine(line);
                textBoxChangelog.AppendText(Environment.NewLine);
            }
        }

        private void ProcessLine(string line)
        {
            line = line.TrimEnd();
            if (string.IsNullOrWhiteSpace(line)) return;

            // Headers
            if (line.StartsWith("### "))
            {
                AppendFormattedText(line[4..], new Font(textBoxChangelog.Font.FontFamily, 11, FontStyle.Bold), Color.FromArgb(0, 150, 255));
                return;
            }
            if (line.StartsWith("## "))
            {
                AppendFormattedText(line[3..], new Font(textBoxChangelog.Font.FontFamily, 12, FontStyle.Bold), Color.FromArgb(0, 180, 255));
                return;
            }
            if (line.StartsWith("# "))
            {
                AppendFormattedText(line[2..], new Font(textBoxChangelog.Font.FontFamily, 14, FontStyle.Bold), Color.FromArgb(0, 200, 255));
                return;
            }

            // Lists
            if (line.TrimStart().StartsWith("* ") || line.TrimStart().StartsWith("- "))
            {
                textBoxChangelog.SelectionBullet = true;
                textBoxChangelog.SelectionIndent = 15;
                string content = line.TrimStart()[2..];
                ProcessInlineFormatting(content);
                textBoxChangelog.SelectionBullet = false;
                textBoxChangelog.SelectionIndent = 0;
                return;
            }

            // Normal line with inline formatting
            ProcessInlineFormatting(line);
        }

        private void ProcessInlineFormatting(string text)
        {
            int currentPos = 0;
            while (currentPos < text.Length)
            {
                // Bold (**text**)
                if (text.IndexOf("**", currentPos) == currentPos)
                {
                    int endPos = text.IndexOf("**", currentPos + 2);
                    if (endPos != -1)
                    {
                        string boldText = text.Substring(currentPos + 2, endPos - (currentPos + 2));
                        AppendFormattedText(boldText, new Font(textBoxChangelog.Font, FontStyle.Bold), textBoxChangelog.ForeColor);
                        currentPos = endPos + 2;
                        continue;
                    }
                }

                // Append a single character and move forward
                textBoxChangelog.AppendText(text[currentPos].ToString());
                currentPos++;
            }
        }

        private void AppendFormattedText(string text, Font font, Color color)
        {
            int start = textBoxChangelog.TextLength;
            textBoxChangelog.AppendText(text);
            int end = textBoxChangelog.TextLength;

            textBoxChangelog.Select(start, end - start);
            textBoxChangelog.SelectionFont = font;
            textBoxChangelog.SelectionColor = color;
            textBoxChangelog.SelectionLength = 0; // Deselect
        }

        private async void ButtonDownload_Click(object? sender, EventArgs? e)
        {
            buttonDownload.Enabled = false;
            buttonDownload.Text = "Initializing...";
            
            // Final check before initiating
            bool available = await UpdateChecker.IsDownloadAvailableAsync();
            if (!available)
            {
                MessageBox.Show("The program files for this update are not yet available on the server. Please check back later.", "Update Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                buttonDownload.Text = "Not Available Yet";
                return;
            }

            progressBarDownload.Visible = true;
            labelProgress.Visible = true;
            progressBarDownload.Value = 0;

            try
            {
                string? downloadUrl = await UpdateChecker.FetchDownloadUrlAsync();
                if (!string.IsNullOrWhiteSpace(downloadUrl))
                {
                    IProgress<int> progress = new Progress<int>(v => {
                        progressBarDownload.Value = v;
                        labelProgress.Text = $"{v}%";
                    });

                    string downloadedFilePath = await StartDownloadProcessAsync(downloadUrl, progress);
                    if (!string.IsNullOrEmpty(downloadedFilePath))
                    {
                        buttonDownload.Text = "Installing...";
                        InstallUpdate(downloadedFilePath);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to fetch the download URL.", "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            buttonDownload.Text = isUpdateAvailable ? "Download Update" : "Re-Download Latest Version";
            progressBarDownload.Visible = false;
            labelProgress.Visible = false;
        }

        private static async Task<string> StartDownloadProcessAsync(string downloadUrl, IProgress<int> progress)
        {
            Main.IsUpdating = true;
            string tempPath = Path.Combine(Path.GetTempPath(), $"nexusbot_{Guid.NewGuid()}.exe");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "NexusBot");
                
                using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var buffer = new byte[8192];
                    var totalRead = 0L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        int bytesRead;
                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            if (totalBytes != -1)
                            {
                                int percentage = (int)((totalRead * 100) / totalBytes);
                                progress.Report(percentage);
                            }
                        }
                    }
                }
            }

            return tempPath;
        }

        private void InstallUpdate(string downloadedFilePath)
        {
            try
            {
                string currentExePath = Application.ExecutablePath;
                string applicationDirectory = Path.GetDirectoryName(currentExePath) ?? "";
                string targetExePath = Path.Combine(applicationDirectory, "nexusbot.exe");
                string backupPath = Path.Combine(applicationDirectory, "nexusbot.exe.backup");

                string batchPath = Path.Combine(Path.GetTempPath(), "UpdateNexusBot.bat");
                
                string cleanupCommand = !currentExePath.Equals(targetExePath, StringComparison.OrdinalIgnoreCase) 
                    ? $"del \"{currentExePath}\"" 
                    : "";

                string batchContent = @$"
@echo off
echo Updating NexusBot...

:retry
timeout /t 1 /nobreak >nul

if exist ""{targetExePath}"" (
    if exist ""{backupPath}"" (
        del ""{backupPath}""
    )
    move /y ""{targetExePath}"" ""{backupPath}"" >nul 2>&1
    if errorlevel 1 goto retry
)

move /y ""{downloadedFilePath}"" ""{targetExePath}"" >nul 2>&1
if errorlevel 1 goto retry

{cleanupCommand}

start """" ""{targetExePath}""
del ""%~f0""
";

                File.WriteAllText(batchPath, batchContent);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = batchPath,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(startInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to install update: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetUI();
            }
        }
    }
}
