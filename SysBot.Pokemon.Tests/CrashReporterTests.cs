using NUnit.Framework;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Tests
{
    public class CrashReporterTests
    {
        private TestLogForwarder _logForwarder = null!;

        [SetUp]
        public void Setup()
        {
            _logForwarder = new TestLogForwarder();
            LogUtil.Forwarders.Add(_logForwarder);
        }

        [TearDown]
        public void TearDown()
        {
            LogUtil.Forwarders.Remove(_logForwarder);
        }

        [Test]
        public async Task SendWebhookAsync_NullUrl_ReturnsImmediately()
        {
            string? url = null;
            var ex = new Exception("Test Exception");

            await CrashReporter.SendWebhookAsync(url, null, ex);

            Assert.That(_logForwarder.Logs, Is.Empty, "Should not log anything if URL is null");
        }

        [Test]
        public async Task SendWebhookAsync_InvalidUrl_LogsError()
        {
            string url = "http://";
            var ex = new Exception("Test Exception");

            await CrashReporter.SendWebhookAsync(url, 12345, ex);

            Assert.That(_logForwarder.Logs.Count, Is.GreaterThan(0), "Should log an error when webhook fails");
            Assert.That(_logForwarder.Logs[0], Does.Contain("Failed to send crash webhook"), "Log message should indicate failure");
        }

        private class TestLogForwarder : ILogForwarder
        {
            public List<string> Logs { get; } = new List<string>();

            public void Forward(string message, string identity)
            {
                Logs.Add($"{identity}: {message}");
            }
        }
    }
}
