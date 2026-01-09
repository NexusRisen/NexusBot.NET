using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SysBot.Base;

namespace SysBot.Pokemon.Helpers
{
    public static class CrashReporter
    {
        public const string ObfuscatedWebhookUrl = "OBsfFTFVW3wBCgYRCjEWTxAHP0oRHxtbHgsFOAAEDjFARWdQW0BFXHNFUEVfYl1BXUpHX0E9CilcNgQYN2MDBwxKLxoVFz0sA1QICjY9LTgKJFxdXAM4RQs2Kj0bMA4ADzUmAB1GFhQECAsXEQBaPRc5HgIMUCwLVg==";

        public static async Task SendWebhookMessageAsync(string title, string message)
        {
             await SendWebhookMessageAsync(ObfuscatedWebhookUrl, null, title, message);
        }

        public static async Task SendWebhookMessageAsync(string url, ulong? userIdToPing, string title, string message)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    url = Deobfuscate(url);
                }
                catch
                {
                    return;
                }
            }

            using var client = new HttpClient();

            var mention = userIdToPing is > 0 ? $"<@{userIdToPing}>" : "";
            
            if (message.Length > 4000)
            {
                message = message.Substring(0, 3990) + "... (truncated)";
            }

            var payload = new
            {
                content = $"{mention} 📝 **PokeBot Log**",
                embeds = new[]
                {
                    new
                    {
                        title = title,
                        description = message,
                        color = 16776960, // Yellow/Orange
                        timestamp = DateTime.UtcNow.ToString("o")
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                await client.PostAsync(url, content).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Failed to send webhook message: {ex.Message}", "CrashReporter");
            }
        }

        public static async Task SendWebhookAsync(string? url, ulong? userIdToPing, Exception ex)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    url = Deobfuscate(url);
                }
                catch
                {
                    return;
                }
            }

            using var client = new HttpClient();

            var mention = userIdToPing is > 0 ? $"<@{userIdToPing}>" : "";
            var description = $"**Exception**: {ex.Message}\n\n**Stack Trace**:\n```{ex.StackTrace}```";

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
                        color = 16711680,
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
                using var response = await client.PostAsync(url, content).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    LogUtil.LogError($"Failed to send crash webhook: HTTP {(int)response.StatusCode} {response.ReasonPhrase}", "CrashReporter");
            }
            catch (Exception webhookEx)
            {
                LogUtil.LogError($"Failed to send crash webhook: {webhookEx}", "CrashReporter");
            }
        }

        public static string Deobfuscate(string input)
        {
            const string key = "PokeBotSecureCrashReporting";
            var bytes = Convert.FromBase64String(input);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var result = new byte[bytes.Length];
            
            for (int i = 0; i < bytes.Length; i++)
            {
                result[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            
            return Encoding.UTF8.GetString(result);
        }
    }
}
