using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SysBot.Base;
using SysBot.Pokemon;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.WinForms.Helpers
{
    public static class CrashHandler
    {
        private static bool _isHandlingCrash = false;

        public static void Initialize()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void HandleException(Exception ex)
        {
            if (_isHandlingCrash) return;
            _isHandlingCrash = true;

            try
            {
                // Log to file first (existing behavior usually)
                LogUtil.LogError($"CRASH: {ex}", "CrashHandler");

                // Try to load config to get webhook URL
                var configPath = Program.ConfigPath;
                if (File.Exists(configPath))
                {
                    try
                    {
                        var json = File.ReadAllText(configPath);
                        var config = JsonSerializer.Deserialize(json, ProgramConfigContext.Default.ProgramConfig);

                        if (config != null && !string.IsNullOrWhiteSpace(config.Hub?.Global?.BugReportWebhookUrl))
                        {
                            SendWebhook(config.Hub.Global.BugReportWebhookUrl, config.Hub.Global.BugReportUserIdToPing, ex).Wait(3000);
                        }
                    }
                    catch (Exception configEx)
                    {
                        LogUtil.LogError($"Failed to load config for crash reporting: {configEx}", "CrashHandler");
                    }
                }
            }
            catch
            {
                // Swallow errors during crash handling to ensure the user at least sees the message box
            }
            finally
            {
                string msg = $"An unexpected error occurred.\n\n{ex.Message}\n\nIf configured, a bug report has been sent to the developer.";
                MessageBox.Show(msg, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private static async Task SendWebhook(string url, ulong? userIdToPing, Exception ex)
        {
            using var client = new HttpClient();
            
            var mention = userIdToPing.HasValue ? $"<@{userIdToPing}>" : "";
            var description = $"**Exception**: {ex.Message}\n\n**Stack Trace**:\n```{ex.StackTrace}```";
            
            // Truncate if too long for Discord (2000 limit for description, but we use embed fields or just content)
            if (description.Length > 4000)
            {
                description = description.Substring(0, 3990) + "... (truncated)";
            }

            var payload = new
            {
                content = $"{mention} 🚨 **PokeBot Crash Report** 🚨\nVersion: {PokeBot.Version}",
                embeds = new[]
                {
                    new
                    {
                        title = "Unhandled Exception",
                        description = description,
                        color = 16711680, // Red
                        footer = new
                        {
                            text = $"OS: {Environment.OSVersion} | 64Bit: {Environment.Is64BitProcess}"
                        },
                        timestamp = DateTime.UtcNow.ToString("o")
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                await client.PostAsync(url, content);
            }
            catch (Exception webhookEx)
            {
                LogUtil.LogError($"Failed to send crash webhook: {webhookEx}", "CrashHandler");
            }
        }
    }
}
