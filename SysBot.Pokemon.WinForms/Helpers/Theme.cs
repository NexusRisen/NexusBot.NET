using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms.Helpers
{
    public static class Theme
    {
        // Alienware / PokeBot Dark Theme Palette
        public static readonly Color BackColor = Color.FromArgb(5, 5, 5);          // Deepest Black
        public static readonly Color SurfaceColor = Color.FromArgb(20, 20, 20);    // Dark Grey for Panels
        public static readonly Color BorderColor = Color.FromArgb(40, 40, 40);     // Subtle Border
        public static readonly Color TextColor = Color.FromArgb(220, 220, 220);    // Off-White
        public static readonly Color MutedTextColor = Color.FromArgb(120, 120, 120);

        // Accents
        public static readonly Color AccentCyan = Color.FromArgb(0, 255, 255);     // Alienware Cyan
        public static readonly Color AccentPurple = Color.FromArgb(180, 0, 255);   // Psychic Purple
        public static readonly Color AccentRed = Color.FromArgb(255, 30, 30);      // Pokeball Red
        public static readonly Color AccentGreen = Color.FromArgb(0, 255, 0);      // Success Green

        public static void Apply(Control control)
        {
            control.BackColor = BackColor;
            control.ForeColor = TextColor;

            // Enable double buffering and resize redraw for smooth rendering
            if (control is Form form)
            {
                // We handle painting in the form itself or via helper
                // ResizeRedraw is protected, handled in Form constructor or Designer
            }

            ApplyToControls(control.Controls);
            
            // Apply Dark Mode to Title Bar
            DarkModeHelper.SetDarkMode(control.Handle);
        }

        public static void PaintBackground(Graphics g, Rectangle bounds)
        {
            // 1. Draw Deep Gradient Background
            using (var brush = new LinearGradientBrush(bounds, BackColor, Color.FromArgb(10, 10, 15), 45f))
            {
                g.FillRectangle(brush, bounds);
            }

            // 2. Draw Subtle Hex/Tech Grid
            // We'll just draw a simple diagonal grid for "tech" feel
            using (var pen = new Pen(Color.FromArgb(15, 255, 255, 255), 1))
            {
                int step = 40;
                for (int x = 0; x < bounds.Width + bounds.Height; x += step)
                {
                    // Diagonal lines /
                    g.DrawLine(pen, x, 0, x - bounds.Height, bounds.Height);
                }
            }

            // 3. Draw faint glow in corners
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(bounds.Width - 300, -150, 600, 600);
                using (var pBrush = new PathGradientBrush(path))
                {
                    pBrush.CenterColor = Color.FromArgb(20, 0, 255, 255); // Faint Cyan Glow
                    pBrush.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(pBrush, path);
                }
            }
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                ApplyToControl(c);
                if (c.HasChildren)
                {
                    ApplyToControls(c.Controls);
                }
            }
        }

        private static void ApplyToControl(Control c)
        {
            // Base styling
            if (c is Panel || c is GroupBox || c is TabPage)
            {
                // Preserve transparency if already set (fully transparent or semi-transparent)
                if (c.BackColor.A == 255 && c.BackColor != Color.Transparent)
                {
                    c.BackColor = SurfaceColor;
                }
                c.ForeColor = TextColor;
            }
            else if (c is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = AccentCyan;
                btn.BackColor = Color.FromArgb(30, 30, 30);
                btn.ForeColor = AccentCyan;
                btn.Cursor = Cursors.Hand;
                
                // Special buttons could be overridden by Tag or Name check if needed
                if (btn.Name.Contains("Stop") || btn.Name.Contains("Disconnect") || btn.Name.Contains("Remove"))
                {
                    btn.ForeColor = AccentRed;
                    btn.FlatAppearance.BorderColor = AccentRed;
                }
                else if (btn.Name.Contains("Start") || btn.Name.Contains("Connect") || btn.Name.Contains("Add"))
                {
                    btn.ForeColor = AccentGreen;
                    btn.FlatAppearance.BorderColor = AccentGreen;
                }
            }
            else if (c is TextBox || c is RichTextBox || c is NumericUpDown || c is ComboBox)
            {
                c.BackColor = Color.FromArgb(10, 10, 10);
                c.ForeColor = TextColor;
            }
            else if (c is Label lbl)
            {
                lbl.ForeColor = TextColor;
                lbl.BackColor = Color.Transparent; // Blend with container
            }
            else if (c is TabControl tc)
            {
                tc.BackColor = BackColor;
                tc.ForeColor = TextColor;
                // Hook draw item if we want custom tabs
                // We leave DrawMode setting to the form/designer to avoid overriding logic unexpectedly
            }
            else if (c is CheckBox chk)
            {
                chk.ForeColor = TextColor;
                chk.BackColor = Color.Transparent;
            }
            else if (c is ListBox lb)
            {
                lb.BackColor = Color.FromArgb(10, 10, 10);
                lb.ForeColor = TextColor;
                lb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is PropertyGrid pg)
            {
                pg.ViewBackColor = Color.FromArgb(10, 10, 10);
                pg.ViewForeColor = TextColor;
                pg.LineColor = SurfaceColor;
                pg.CategoryForeColor = AccentCyan;
                pg.CategorySplitterColor = SurfaceColor;
                pg.HelpBackColor = SurfaceColor;
                pg.HelpForeColor = TextColor;
                pg.CommandsBackColor = SurfaceColor;
                pg.CommandsForeColor = AccentCyan;
            }
        }

        public static void DrawTabControl(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tc) return;

            // Paint the background of the tab area
            // We can't easily paint the area behind tabs without subclassing, 
            // but we can paint the tabs themselves.
            
            var page = tc.TabPages[e.Index];
            var rect = e.Bounds;
            
            // Determine state
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            
            // Fill Background
            using (var brush = new SolidBrush(isSelected ? Color.FromArgb(40, 40, 40) : Color.FromArgb(20, 20, 20)))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
            
            // Draw Border/Accent
            if (isSelected)
            {
                using (var pen = new Pen(AccentCyan, 2))
                {
                    // Bottom line accent
                    e.Graphics.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                }
            }
            
            // Draw Text
            TextRenderer.DrawText(e.Graphics, page.Text, page.Font ?? tc.Font, rect, 
                isSelected ? AccentCyan : MutedTextColor, 
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
