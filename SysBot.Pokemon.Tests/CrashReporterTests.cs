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
        private TestLogForwarder _logForwarder;

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
            // Arrange
            string url = null;
            var ex = new Exception("Test Exception");

            // Act
            await CrashReporter.SendWebhookAsync(url, null, ex);

            // Assert
            Assert.That(_logForwarder.Logs, Is.Empty, "Should not log anything if URL is null");
        }

        [Test]
        public async Task SendWebhookAsync_InvalidUrl_LogsError()
        {
            // Arrange
            string url = "http://invalid-url-that-does-not-exist.com/webhook";
            var ex = new Exception("Test Exception");

            // Act
            await CrashReporter.SendWebhookAsync(url, 12345, ex);

            // Assert
            // It might take some time to fail, or fail immediately.
            // Since we catch exceptions inside SendWebhookAsync, we expect a log entry.
            // Note: HttpClient might throw immediately for invalid format, or timeout.
            // For this test, we assume it eventually logs an error.
            
            // Wait a bit if it's async background work? No, we await it.
            // But HttpClient might just fail.
            
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
