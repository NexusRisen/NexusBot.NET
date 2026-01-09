using System;
using System.Threading;
using System.Windows.Forms;
using SysBot.Base;

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
                LogUtil.LogError($"CRASH: {ex}", "CrashHandler");
            }
            catch
            {
            }
            finally
            {
                string msg = $"An unexpected error occurred.\n\n{ex.Message}\n\nPlease check the log output for details.";
                MessageBox.Show(msg, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }
    }
}
