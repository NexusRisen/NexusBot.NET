using System;
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
            _ = CrashReporter.SendWebhookMessageAsync(_webhookUrl, null, $"Log: {identity}", message);
        }

        public void Forward(Exception exception, string identity)
        {
            if (string.IsNullOrWhiteSpace(_webhookUrl)) return;
            _ = CrashReporter.SendWebhookAsync(_webhookUrl, null, exception);
        }
    }
}
