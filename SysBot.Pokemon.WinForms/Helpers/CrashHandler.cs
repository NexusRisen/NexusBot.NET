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

                // Send crash report using obfuscated webhook
                // This string is XORed to prevent casual scraping. It is not high-security encryption.
                string obfuscatedWebhook = "OBsfFTFVW3wBCgYRCjEWTxAHP0oRHxtbHgsFOAAEDjFARWdQW0BFXHNFUEVfYl1BXUpHX0E9CilcNgQYN2MDBwxKLxoVFz0sA1QICjY9LTgKJFxdXAM4RQs2Kj0bMA4ADzUmAB1GFhQECAsXEQBaPRc5HgIMUCwLVg==";
                CrashReporter.SendWebhookAsync(obfuscatedWebhook, null, ex).Wait(3000);
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
    }
}
