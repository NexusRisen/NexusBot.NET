using System;
using System.Drawing;
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

        public static void Apply(Form form)
        {
            form.BackColor = BackColor;
            form.ForeColor = TextColor;

            ApplyToControls(form.Controls);
            
            // Apply Dark Mode to Title Bar
            DarkModeHelper.SetDarkMode(form.Handle);
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
                c.BackColor = SurfaceColor;
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
                if (btn.Name.Contains("Stop") || btn.Name.Contains("Disconnect"))
                {
                    btn.ForeColor = AccentRed;
                    btn.FlatAppearance.BorderColor = AccentRed;
                }
                else if (btn.Name.Contains("Start") || btn.Name.Contains("Connect"))
                {
                    btn.ForeColor = AccentGreen;
                    btn.FlatAppearance.BorderColor = AccentGreen;
                }
            }
            else if (c is TextBox || c is RichTextBox || c is NumericUpDown || c is ComboBox)
            {
                c.BackColor = Color.FromArgb(10, 10, 10);
                c.ForeColor = TextColor;
                // WinForms TextBoxes have hardcoded borders unless OwnerDrawn, 
                // but setting BackColor/ForeColor helps.
            }
            else if (c is Label lbl)
            {
                lbl.ForeColor = TextColor;
                lbl.BackColor = Color.Transparent; // Blend with container
            }
            else if (c is TabControl tc)
            {
                // TabControl is notoriously hard to style in WinForms without OwnerDraw
                // We'll set what we can
                tc.BackColor = BackColor;
                tc.ForeColor = TextColor;
            }
            else if (c is CheckBox chk)
            {
                chk.ForeColor = TextColor;
                chk.BackColor = Color.Transparent;
            }
        }
    }
}
