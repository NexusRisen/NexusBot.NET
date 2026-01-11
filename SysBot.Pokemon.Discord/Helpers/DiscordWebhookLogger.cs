using System;
using System.Linq;
using SysBot.Base;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.Discord.Helpers
{
    public sealed class DiscordWebhookLogger : ILogForwarder, ILogExceptionForwarder
    {
        private readonly string _webhookUrl;

        public DiscordWebhookLogger()
        {
            _webhookUrl = CrashReporter.ObfuscatedWebhookUrl;
        }

        public void Forward(string message, string identity)
        {
            if (string.IsNullOrWhiteSpace(_webhookUrl)) return;
            
            // Filter out wifi/connection related errors as requested
            if (IsWifiOrConnectionError(message)) return;

            _ = CrashReporter.SendWebhookMessageAsync(_webhookUrl, null, $"Log: {identity}", message);
        }

        public void Forward(Exception exception, string identity)
        {
            if (string.IsNullOrWhiteSpace(_webhookUrl)) return;

            // Filter out wifi/connection related errors as requested
            if (IsWifiOrConnectionError(exception.Message)) return;

            _ = CrashReporter.SendWebhookAsync(_webhookUrl, null, exception);
        }

        private static bool IsWifiOrConnectionError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;
            
            // Common connection/wifi error terms
            var terms = new[] 
            { 
                "wifi", "wi-fi", "internet", "connect", "network", 
                "socket", "timeout", "cancel", "task was canceled", 
                "host", "unreachable", "refused"
            };

            return terms.Any(term => message.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
    }
}
