using SysBot.Base;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms;

/// <summary>
/// Forward logs to a TextBox with enhanced coloring and scroll control.
/// </summary>
public sealed class TextBoxForwarder(TextBoxBase Box) : ILogForwarder
{
    private readonly object _logLock = new();
    public bool AutoScroll { get; set; } = true;
    public event EventHandler? LogCleanup;

    public void Forward(string message, string identity)
    {
        lock (_logLock)
        {
            if (Box.IsDisposed) return;

            if (Box.InvokeRequired)
                Box.BeginInvoke((MethodInvoker)(() => AppendLog(message, identity)));
            else
                AppendLog(message, identity);
        }
    }

    private void AppendLog(string message, string identity)
    {
        try
        {
            if (Box.IsDisposed) return;

            CheckCleanup();

            if (Box is RichTextBox rtb)
            {
                // Suspend layout could help performance but might flicker
                
                // Timestamp
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;
                rtb.SelectionColor = Color.Gray;
                rtb.AppendText($"[{DateTime.Now:HH:mm:ss}] ");

                // Identity
                Color idColor = Color.FromArgb(100, 180, 255); // Light Blue
                if (identity.Contains("Error", StringComparison.OrdinalIgnoreCase) || identity.Contains("Fail", StringComparison.OrdinalIgnoreCase)) 
                    idColor = Color.FromArgb(255, 100, 100); // Red
                else if (identity.Contains("Warn", StringComparison.OrdinalIgnoreCase)) 
                    idColor = Color.FromArgb(255, 200, 100); // Orange
                else if (identity.Contains("Success", StringComparison.OrdinalIgnoreCase) || identity.Contains("Trade", StringComparison.OrdinalIgnoreCase)) 
                    idColor = Color.FromArgb(100, 255, 100); // Green
                
                rtb.SelectionColor = idColor;
                rtb.AppendText($"- {identity}: ");

                // Message
                rtb.SelectionColor = Color.FromArgb(220, 220, 220); // Off-white
                rtb.AppendText($"{message}{Environment.NewLine}");
                
                if (AutoScroll)
                {
                    rtb.SelectionStart = rtb.TextLength;
                    rtb.ScrollToCaret();
                }
            }
            else
            {
                var line = $"[{DateTime.Now:HH:mm:ss}] - {identity}: {message}{Environment.NewLine}";
                Box.AppendText(line);
            }
        }
        catch { }
    }

    private void CheckCleanup()
    {
        // Simple length check
        if (Box.TextLength > Box.MaxLength * 0.9)
        {
            var lines = Box.Lines;
            Box.Lines = lines[(lines.Length / 2)..];
            LogCleanup?.Invoke(this, EventArgs.Empty);
        }
    }
}
