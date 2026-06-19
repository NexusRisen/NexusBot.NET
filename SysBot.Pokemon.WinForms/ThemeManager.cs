using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace SysBot.Pokemon.WinForms;

public static class ThemeManager
{
    public static readonly ThemePalette DarkTheme = new()
    {
        Name = "Dark Theme",
        BackColor = Color.FromArgb(30, 33, 39),
        ForeColor = Color.FromArgb(220, 223, 228),
        AccentColor = Color.FromArgb(0, 162, 255),
        TabBackColor = Color.FromArgb(24, 26, 31),
        TabPageBackColor = Color.FromArgb(30, 33, 39),
        TabForeColor = Color.FromArgb(220, 223, 228),
        InputBackColor = Color.FromArgb(33, 37, 43),
        InputForeColor = Color.White,
        InputBorderColor = Color.FromArgb(24, 26, 31),
        ButtonBackColor = Color.FromArgb(33, 37, 43),
        ButtonForeColor = Color.White,
        ButtonBorderColor = Color.FromArgb(24, 26, 31),
        StartButtonBackColor = Color.FromArgb(46, 160, 67),
        StartButtonForeColor = Color.White,
        StopButtonBackColor = Color.FromArgb(218, 54, 51),
        StopButtonForeColor = Color.White,
        RestartButtonBackColor = Color.FromArgb(0, 162, 255),
        RestartButtonForeColor = Color.White,
        PropertyGridBackColor = Color.FromArgb(30, 33, 39),
        PropertyGridLineColor = Color.FromArgb(24, 26, 31),
        PropertyGridCategoryForeColor = Color.FromArgb(0, 162, 255),
        PropertyGridHelpBackColor = Color.FromArgb(24, 26, 31),
        PropertyGridHelpForeColor = Color.FromArgb(220, 223, 228),
        LogBackColor = Color.FromArgb(24, 26, 31),
        LogForeColor = Color.FromArgb(171, 178, 191),
        SidebarBackColor = Color.FromArgb(24, 26, 31),
        SidebarButtonForeColor = Color.FromArgb(171, 178, 191),
        SidebarButtonActiveColor = Color.FromArgb(30, 33, 39),
        HeaderBackColor = Color.FromArgb(30, 33, 39),
        HeaderForeColor = Color.White,
        BotControllerHoverColor = Color.FromArgb(33, 37, 43)
    };

    public static readonly ThemePalette ModernTheme = new()
    {
        Name = "Modern Theme",
        BackColor = Color.FromArgb(18, 18, 18),
        ForeColor = Color.FromArgb(238, 238, 238),
        AccentColor = Color.FromArgb(0, 204, 153),
        TabBackColor = Color.FromArgb(24, 24, 24),
        TabPageBackColor = Color.FromArgb(18, 18, 18),
        TabForeColor = Color.FromArgb(238, 238, 238),
        InputBackColor = Color.FromArgb(28, 28, 28),
        InputForeColor = Color.White,
        InputBorderColor = Color.FromArgb(45, 45, 45),
        ButtonBackColor = Color.FromArgb(28, 28, 28),
        ButtonForeColor = Color.White,
        ButtonBorderColor = Color.FromArgb(45, 45, 45),
        StartButtonBackColor = Color.FromArgb(0, 204, 153),
        StartButtonForeColor = Color.Black,
        StopButtonBackColor = Color.FromArgb(239, 83, 80),
        StopButtonForeColor = Color.White,
        RestartButtonBackColor = Color.FromArgb(41, 182, 246),
        RestartButtonForeColor = Color.Black,
        PropertyGridBackColor = Color.FromArgb(18, 18, 18),
        PropertyGridLineColor = Color.FromArgb(28, 28, 28),
        PropertyGridCategoryForeColor = Color.FromArgb(0, 204, 153),
        PropertyGridHelpBackColor = Color.FromArgb(24, 24, 24),
        PropertyGridHelpForeColor = Color.FromArgb(238, 238, 238),
        LogBackColor = Color.Black,
        LogForeColor = Color.FromArgb(204, 204, 204),
        SidebarBackColor = Color.FromArgb(24, 24, 24),
        SidebarButtonForeColor = Color.Gainsboro,
        SidebarButtonActiveColor = Color.FromArgb(0, 204, 153),
        HeaderBackColor = Color.FromArgb(24, 24, 24),
        HeaderForeColor = Color.White,
        BotControllerHoverColor = Color.FromArgb(28, 28, 28)
    };

    public static readonly ThemePalette GengarTheme = new()
    {
        Name = "Gengar Theme",
        BackColor = Color.FromArgb(74, 58, 90),
        ForeColor = Color.FromArgb(230, 225, 235),
        AccentColor = Color.FromArgb(230, 74, 66),
        TabBackColor = Color.FromArgb(50, 38, 60),
        TabPageBackColor = Color.FromArgb(74, 58, 90),
        TabForeColor = Color.FromArgb(230, 225, 235),
        InputBackColor = Color.FromArgb(50, 38, 60),
        InputForeColor = Color.FromArgb(230, 225, 235),
        InputBorderColor = Color.FromArgb(74, 58, 90),
        ButtonBackColor = Color.FromArgb(50, 38, 60),
        ButtonForeColor = Color.FromArgb(230, 225, 235),
        ButtonBorderColor = Color.FromArgb(74, 58, 90),
        StartButtonBackColor = Color.FromArgb(230, 74, 66),
        StartButtonForeColor = Color.White,
        StopButtonBackColor = Color.FromArgb(230, 74, 66),
        StopButtonForeColor = Color.White,
        RestartButtonBackColor = Color.FromArgb(230, 74, 66),
        RestartButtonForeColor = Color.White,
        PropertyGridBackColor = Color.FromArgb(74, 58, 90),
        PropertyGridLineColor = Color.FromArgb(50, 38, 60),
        PropertyGridCategoryForeColor = Color.FromArgb(230, 74, 66),
        PropertyGridHelpBackColor = Color.FromArgb(50, 38, 60),
        PropertyGridHelpForeColor = Color.FromArgb(230, 225, 235),
        LogBackColor = Color.FromArgb(50, 38, 60),
        LogForeColor = Color.FromArgb(230, 225, 235),
        SidebarBackColor = Color.FromArgb(50, 38, 60),
        SidebarButtonForeColor = Color.FromArgb(230, 225, 235),
        SidebarButtonActiveColor = Color.FromArgb(230, 74, 66),
        HeaderBackColor = Color.FromArgb(50, 38, 60),
        HeaderForeColor = Color.White,
        BotControllerHoverColor = Color.FromArgb(50, 38, 60)
    };

    public static readonly ThemePalette PitchBlackTheme = new()
    {
        Name = "Pitch Black Theme",
        BackColor = Color.Black,
        ForeColor = Color.White,
        AccentColor = Color.White,
        TabBackColor = Color.Black,
        TabPageBackColor = Color.Black,
        TabForeColor = Color.White,
        InputBackColor = Color.Black,
        InputForeColor = Color.White,
        InputBorderColor = Color.FromArgb(51, 51, 51),
        ButtonBackColor = Color.Black,
        ButtonForeColor = Color.White,
        ButtonBorderColor = Color.FromArgb(51, 51, 51),
        StartButtonBackColor = Color.Black,
        StartButtonForeColor = Color.White,
        StopButtonBackColor = Color.Black,
        StopButtonForeColor = Color.White,
        RestartButtonBackColor = Color.Black,
        RestartButtonForeColor = Color.White,
        PropertyGridBackColor = Color.Black,
        PropertyGridLineColor = Color.FromArgb(51, 51, 51),
        PropertyGridCategoryForeColor = Color.White,
        PropertyGridHelpBackColor = Color.Black,
        PropertyGridHelpForeColor = Color.White,
        LogBackColor = Color.Black,
        LogForeColor = Color.White,
        SidebarBackColor = Color.Black,
        SidebarButtonForeColor = Color.Gray,
        SidebarButtonActiveColor = Color.White,
        HeaderBackColor = Color.Black,
        HeaderForeColor = Color.White,
        BotControllerHoverColor = Color.FromArgb(51, 51, 51)
    };

    public static readonly ThemePalette CyberpunkTheme = new()
    {
        Name = "Cyberpunk Theme",
        BackColor = Color.FromArgb(10, 10, 30),
        ForeColor = Color.FromArgb(220, 220, 255),
        AccentColor = Color.FromArgb(0, 255, 255),
        TabBackColor = Color.FromArgb(5, 5, 20),
        TabPageBackColor = Color.FromArgb(10, 10, 30),
        TabForeColor = Color.FromArgb(220, 220, 255),
        InputBackColor = Color.FromArgb(20, 20, 45),
        InputForeColor = Color.FromArgb(0, 255, 255),
        InputBorderColor = Color.FromArgb(255, 0, 127),
        ButtonBackColor = Color.FromArgb(20, 20, 45),
        ButtonForeColor = Color.FromArgb(0, 255, 255),
        ButtonBorderColor = Color.FromArgb(255, 0, 127),
        StartButtonBackColor = Color.FromArgb(0, 255, 255),
        StartButtonForeColor = Color.Black,
        StopButtonBackColor = Color.FromArgb(255, 0, 127),
        StopButtonForeColor = Color.White,
        RestartButtonBackColor = Color.FromArgb(255, 255, 0),
        RestartButtonForeColor = Color.Black,
        PropertyGridBackColor = Color.FromArgb(10, 10, 30),
        PropertyGridLineColor = Color.FromArgb(20, 20, 45),
        PropertyGridCategoryForeColor = Color.FromArgb(0, 255, 255),
        PropertyGridHelpBackColor = Color.FromArgb(5, 5, 20),
        PropertyGridHelpForeColor = Color.FromArgb(220, 220, 255),
        LogBackColor = Color.FromArgb(5, 5, 20),
        LogForeColor = Color.FromArgb(255, 0, 127),
        SidebarBackColor = Color.FromArgb(5, 5, 20),
        SidebarButtonForeColor = Color.FromArgb(0, 255, 255),
        SidebarButtonActiveColor = Color.FromArgb(255, 0, 127),
        HeaderBackColor = Color.FromArgb(5, 5, 20),
        HeaderForeColor = Color.White,
        BotControllerHoverColor = Color.FromArgb(20, 20, 45)
    };

    public static readonly ThemePalette DraculaTheme = new()
    {
        Name = "Dracula Theme",
        BackColor = Color.FromArgb(40, 42, 54),
        ForeColor = Color.FromArgb(248, 248, 242),
        AccentColor = Color.FromArgb(255, 121, 198),
        TabBackColor = Color.FromArgb(33, 34, 44),
        TabPageBackColor = Color.FromArgb(40, 42, 54),
        TabForeColor = Color.FromArgb(248, 248, 242),
        InputBackColor = Color.FromArgb(68, 71, 90),
        InputForeColor = Color.FromArgb(248, 248, 242),
        InputBorderColor = Color.FromArgb(33, 34, 44),
        ButtonBackColor = Color.FromArgb(68, 71, 90),
        ButtonForeColor = Color.FromArgb(248, 248, 242),
        ButtonBorderColor = Color.FromArgb(33, 34, 44),
        StartButtonBackColor = Color.FromArgb(80, 250, 123),
        StartButtonForeColor = Color.FromArgb(40, 42, 54),
        StopButtonBackColor = Color.FromArgb(255, 85, 85),
        StopButtonForeColor = Color.FromArgb(40, 42, 54),
        RestartButtonBackColor = Color.FromArgb(139, 233, 253),
        RestartButtonForeColor = Color.FromArgb(40, 42, 54),
        PropertyGridBackColor = Color.FromArgb(40, 42, 54),
        PropertyGridLineColor = Color.FromArgb(68, 71, 90),
        PropertyGridCategoryForeColor = Color.FromArgb(255, 121, 198),
        PropertyGridHelpBackColor = Color.FromArgb(33, 34, 44),
        PropertyGridHelpForeColor = Color.FromArgb(248, 248, 242),
        LogBackColor = Color.FromArgb(33, 34, 44),
        LogForeColor = Color.FromArgb(248, 248, 242),
        SidebarBackColor = Color.FromArgb(33, 34, 44),
        SidebarButtonForeColor = Color.FromArgb(139, 233, 253),
        SidebarButtonActiveColor = Color.FromArgb(255, 121, 198),
        HeaderBackColor = Color.FromArgb(33, 34, 44),
        HeaderForeColor = Color.FromArgb(248, 248, 242),
        BotControllerHoverColor = Color.FromArgb(68, 71, 90)
    };

    public static readonly ThemePalette PikachuTheme = new()
    {
        Name = "Pikachu Theme",
        BackColor = Color.FromArgb(246, 208, 60),
        ForeColor = Color.Black,
        AccentColor = Color.FromArgb(230, 74, 66),
        TabBackColor = Color.FromArgb(130, 69, 18),
        TabPageBackColor = Color.FromArgb(246, 208, 60),
        TabForeColor = Color.Black,
        InputBackColor = Color.FromArgb(130, 69, 18),
        InputForeColor = Color.White,
        InputBorderColor = Color.FromArgb(130, 69, 18),
        ButtonBackColor = Color.FromArgb(130, 69, 18),
        ButtonForeColor = Color.White,
        ButtonBorderColor = Color.FromArgb(130, 69, 18),
        StartButtonBackColor = Color.FromArgb(230, 74, 66),
        StartButtonForeColor = Color.White,
        StopButtonBackColor = Color.FromArgb(230, 74, 66),
        StopButtonForeColor = Color.White,
        RestartButtonBackColor = Color.FromArgb(230, 74, 66),
        RestartButtonForeColor = Color.White,
        PropertyGridBackColor = Color.FromArgb(246, 208, 60),
        PropertyGridLineColor = Color.FromArgb(130, 69, 18),
        PropertyGridCategoryForeColor = Color.FromArgb(230, 74, 66),
        PropertyGridHelpBackColor = Color.FromArgb(130, 69, 18),
        PropertyGridHelpForeColor = Color.White,
        LogBackColor = Color.FromArgb(130, 69, 18),
        LogForeColor = Color.White,
        SidebarBackColor = Color.FromArgb(130, 69, 18),
        SidebarButtonForeColor = Color.White,
        SidebarButtonActiveColor = Color.FromArgb(230, 74, 66),
        HeaderBackColor = Color.FromArgb(130, 69, 18),
        HeaderForeColor = Color.White,
        BotControllerHoverColor = Color.FromArgb(130, 69, 18)
    };

    public static IEnumerable<ThemePalette> AllThemes { get; } = new ThemePalette[]
    {
        DarkTheme,
        ModernTheme,
        GengarTheme,
        PitchBlackTheme,
        CyberpunkTheme,
        DraculaTheme,
        PikachuTheme
    };

    private static ThemePalette _currentTheme = DarkTheme;
    public static ThemePalette CurrentTheme 
    { 
        get => _currentTheme ?? DarkTheme; 
        private set => _currentTheme = value; 
    }

    public static void ApplyTheme(Form form, string themeName)
    {
        var theme = AllThemes.FirstOrDefault(t => t.Name == themeName) ?? DarkTheme;
        CurrentTheme = theme;
        ApplyTheme(form, CurrentTheme);
    }

    public static void ApplyTheme(Control control, ThemePalette palette)
    {
        if (control is Form form)
        {
            form.BackColor = palette.BackColor;
            form.ForeColor = palette.ForeColor;
            form.Font = new Font("Segoe UI", 9F);
        }

        foreach (Control c in control.Controls)
        {
            ApplyToControl(c, palette);
        }
    }

    private static void ApplyToControl(Control c, ThemePalette palette)
    {
        if (c.Name == "P_Sidebar" || c.Name == "P_LogoArea" || c.Name == "P_Bottom")
        {
            c.BackColor = palette.SidebarBackColor;
        }
        else if (c.Name == "P_Header")
        {
            c.BackColor = palette.HeaderBackColor;
            c.ForeColor = palette.HeaderForeColor;
        }
        else if (c.Name.StartsWith("B_Nav") || c.Name == "B_Credits" || c.Name == "B_HideTray")
        {
            c.BackColor = palette.SidebarBackColor;
            c.ForeColor = palette.SidebarButtonForeColor;
            if (c is Button btn)
            {
                btn.FlatAppearance.MouseOverBackColor = palette.SidebarButtonActiveColor;
                btn.FlatAppearance.MouseDownBackColor = palette.SidebarButtonActiveColor;
            }
        }
        else if (c is not Label && c is not LinkLabel && c is not PictureBox)
        {
            c.BackColor = palette.BackColor;
        }
        else if (c is not PictureBox)
        {
            c.BackColor = Color.Transparent;
        }
        
        c.ForeColor = palette.ForeColor;

        if (c.ContextMenuStrip != null)
        {
            ApplyToContextMenu(c.ContextMenuStrip, palette);
        }

        switch (c)
        {
            case TabControl tc:
                tc.BackColor = palette.TabBackColor;
                break;
            case TabPage tp:
                tp.BackColor = palette.TabPageBackColor;
                tp.ForeColor = palette.TabForeColor;
                break;
            case TextBox tb:
                tb.BackColor = palette.InputBackColor;
                tb.ForeColor = palette.InputForeColor;
                tb.BorderStyle = BorderStyle.FixedSingle;
                break;
            case ComboBox cb:
                cb.BackColor = palette.InputBackColor;
                cb.ForeColor = palette.InputForeColor;
                cb.FlatStyle = FlatStyle.Flat;
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                break;
            case NumericUpDown nud:
                nud.BackColor = palette.InputBackColor;
                nud.ForeColor = palette.InputForeColor;
                nud.BorderStyle = BorderStyle.FixedSingle;
                break;
            case Button btn:
                if (!btn.Name.StartsWith("B_Nav"))
                    ApplyButtonTheme(btn, palette);
                break;
            case PropertyGrid pg:
                pg.BackColor = palette.PropertyGridBackColor;
                pg.ViewBackColor = palette.PropertyGridBackColor;
                pg.ViewForeColor = palette.ForeColor;
                pg.LineColor = palette.PropertyGridLineColor;
                pg.CategoryForeColor = palette.PropertyGridCategoryForeColor;
                pg.CategorySplitterColor = palette.PropertyGridLineColor;
                pg.HelpBackColor = palette.PropertyGridHelpBackColor;
                pg.HelpForeColor = palette.PropertyGridHelpForeColor;
                break;
            case RichTextBox rtb:
                rtb.BackColor = palette.LogBackColor;
                rtb.ForeColor = palette.LogForeColor;
                rtb.BorderStyle = BorderStyle.None;
                break;
            case LinkLabel ll:
                ll.LinkColor = palette.SidebarButtonForeColor;
                ll.ActiveLinkColor = palette.SidebarButtonActiveColor;
                ll.VisitedLinkColor = palette.SidebarButtonForeColor;
                break;
            case PictureBox pb:
                if (pb.Name == "PB_Logo" || pb.Name == "PB_LogoSidebar")
                {
                    pb.BackColor = Color.Transparent;
                }
                break;
        }

        foreach (Control child in c.Controls)
        {
            ApplyToControl(child, palette);
        }
    }

    private static void ApplyToContextMenu(ContextMenuStrip cms, ThemePalette palette)
    {
        cms.BackColor = palette.BackColor;
        cms.ForeColor = palette.ForeColor;
        cms.RenderMode = ToolStripRenderMode.Professional;
        foreach (ToolStripItem item in cms.Items)
        {
            ApplyToToolStripItem(item, palette);
        }
    }

    private static void ApplyToToolStripItem(ToolStripItem item, ThemePalette palette)
    {
        item.BackColor = palette.BackColor;
        item.ForeColor = palette.ForeColor;
        if (item is ToolStripDropDownItem dd)
        {
            foreach (ToolStripItem subItem in dd.DropDownItems)
            {
                ApplyToToolStripItem(subItem, palette);
            }
        }
    }

    private static void ApplyButtonTheme(Button btn, ThemePalette palette)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = palette.ButtonBorderColor;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.MouseOverBackColor = palette.AccentColor;

        if (btn.Name.Contains("Start", StringComparison.OrdinalIgnoreCase))
        {
            btn.BackColor = palette.StartButtonBackColor;
            btn.ForeColor = palette.StartButtonForeColor;
        }
        else if (btn.Name.Contains("Stop", StringComparison.OrdinalIgnoreCase))
        {
            btn.BackColor = palette.StopButtonBackColor;
            btn.ForeColor = palette.StopButtonForeColor;
        }
        else if (btn.Name.Contains("Restart", StringComparison.OrdinalIgnoreCase))
        {
            btn.BackColor = palette.RestartButtonBackColor;
            btn.ForeColor = palette.RestartButtonForeColor;
        }
        else
        {
            btn.BackColor = palette.ButtonBackColor;
            btn.ForeColor = palette.ButtonForeColor;
        }
    }
}
