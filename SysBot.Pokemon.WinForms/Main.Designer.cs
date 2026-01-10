using SysBot.Pokemon.WinForms.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using SysBot.Base;

#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8669

namespace SysBot.Pokemon.WinForms
{
    partial class Main
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing && trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));

            trayIcon = new NotifyIcon(this.components);
            trayContextMenu = new ContextMenuStrip(this.components);
            trayMenuShow = new ToolStripMenuItem();
            trayMenuExit = new ToolStripMenuItem();

            mainLayoutPanel = new TableLayoutPanel();
            sidebarPanel = new Panel();
            contentPanel = new Panel();
            headerPanel = new Panel();

            logoPanel = new Panel();
            navButtonsPanel = new FlowLayoutPanel();
            btnNavBots = new Button();
            btnNavHub = new Button();
            btnNavLogs = new Button();
            btnNavDev = new Button();

            devPanel = new Panel();
            lblDevTitle = new Label();
            txtPattern = new TextBox();
            lblPattern = new Label();
            cbRegion = new ComboBox();
            lblRegion = new Label();
            txtStart = new TextBox();
            lblStart = new Label();
            txtLength = new TextBox();
            lblLength = new Label();
            btnScan = new Button();
            rtbResults = new RichTextBox();
            lblScanStatus = new Label();
            sidebarBottomPanel = new Panel();
            btnUpdate = new Button();
            statusIndicator = new Panel();

            titleLabel = new Label();
            controlButtonsPanel = new FlowLayoutPanel();
            btnStart = new Button();
            btnStop = new Button();
            btnReboot = new Button();

            botsPanel = new Panel();
            botsPanel.BackColor = Color.FromArgb(8, 8, 8); // Match server rack background
            botsPanel.Padding = new Padding(25, 0, 25, 0); // Reserve space for rails
            CreateChamferedRegion(botsPanel, 20);
            hubPanel = new Panel();
            logsPanel = new Panel();

            botHeaderPanel = new Panel();
            addBotPanel = new Panel();
            TB_IP = new TextBox();
            NUD_Port = new NumericUpDown();
            CB_Protocol = new ComboBox();
            CB_Routine = new ComboBox();
            B_New = new Button();
            FLP_Bots = new FlowLayoutPanel();

            PG_Hub = new PropertyGrid();

            RTB_Logs = new RichTextBox();
            logsHeaderPanel = new Panel();
            searchPanel = new Panel();
            logSearchBox = new TextBox();
            searchOptionsPanel = new FlowLayoutPanel();
            btnCaseSensitive = new CheckBox();
            btnRegex = new CheckBox();
            btnWholeWord = new CheckBox();
            btnClearLogs = new Button();
            searchStatusLabel = new Label();

            comboBox1 = new ComboBox();

            mainLayoutPanel.SuspendLayout();
            sidebarPanel.SuspendLayout();
            navButtonsPanel.SuspendLayout();
            sidebarBottomPanel.SuspendLayout();
            headerPanel.SuspendLayout();
            controlButtonsPanel.SuspendLayout();
            contentPanel.SuspendLayout();
            botsPanel.SuspendLayout();
            botHeaderPanel.SuspendLayout();
            addBotPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NUD_Port).BeginInit();
            hubPanel.SuspendLayout();
            logsPanel.SuspendLayout();
            logsHeaderPanel.SuspendLayout();
            devPanel.SuspendLayout();
            searchPanel.SuspendLayout();
            searchOptionsPanel.SuspendLayout();
            SuspendLayout();

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // Main Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 600); // Increased default size
            MinimumSize = new Size(1000, 500); // Increased minimum size
            BackColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.BackColor;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            Icon = Resources.icon;
            Name = "Main";
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PokéBot Control Center";
            FormClosing += Main_FormClosing;
            DoubleBuffered = true;
            Resize += Main_Resize;

            // Main Layout Panel
            mainLayoutPanel.ColumnCount = 2;
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayoutPanel.Controls.Add(sidebarPanel, 0, 0);
            mainLayoutPanel.Controls.Add(contentPanel, 1, 0);
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.Location = new Point(0, 0);
            mainLayoutPanel.Margin = new Padding(0);
            mainLayoutPanel.Name = "mainLayoutPanel";
            mainLayoutPanel.RowCount = 1;
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayoutPanel.TabIndex = 0;
            mainLayoutPanel.BackColor = Color.Transparent;
            EnableDoubleBuffering(mainLayoutPanel);

            // Sidebar Panel - Cuztom style
            sidebarPanel.BackColor = Color.Transparent;
            sidebarPanel.Controls.Add(navButtonsPanel);
            sidebarPanel.Controls.Add(sidebarBottomPanel);
            sidebarPanel.Controls.Add(logoPanel);
            sidebarPanel.Dock = DockStyle.Fill;
            sidebarPanel.Location = new Point(0, 0);
            sidebarPanel.Margin = new Padding(0);
            sidebarPanel.Name = "sidebarPanel";
            sidebarPanel.Size = new Size(240, 600);
            sidebarPanel.TabIndex = 0;
            EnableDoubleBuffering(sidebarPanel);

            // Logo Panel - Cuztom gradient
            logoPanel.BackColor = Color.Transparent;
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 60;
            logoPanel.Location = new Point(0, 0);
            logoPanel.Name = "logoPanel";
            logoPanel.Size = new Size(200, 60);
            logoPanel.TabIndex = 0;
            logoPanel.Paint += LogoPanel_Paint;
            CreateChamferedRegion(logoPanel, 15);
            EnableDoubleBuffering(logoPanel);

            // Navigation Buttons Panel
            navButtonsPanel.AutoSize = false;
            navButtonsPanel.Controls.Add(btnNavBots);
            navButtonsPanel.Controls.Add(btnNavHub);
            navButtonsPanel.Controls.Add(btnNavLogs);
            navButtonsPanel.Controls.Add(btnNavDev);
            navButtonsPanel.Dock = DockStyle.Fill;
            navButtonsPanel.FlowDirection = FlowDirection.TopDown;
            navButtonsPanel.Location = new Point(0, 60);
            navButtonsPanel.Margin = new Padding(0);
            navButtonsPanel.Name = "navButtonsPanel";
            navButtonsPanel.Padding = new Padding(0, 10, 0, 0);
            navButtonsPanel.Size = new Size(240, 460);
            navButtonsPanel.TabIndex = 1;
            navButtonsPanel.BackColor = Color.Transparent;
            EnableDoubleBuffering(navButtonsPanel);

            // Configure Cuztom-style nav buttons with neon accents
            ConfigureNavButton(btnNavBots, "BOTS", 0, "Manage bot connections", SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan); // Cyan
            CreateChamferedRegion(btnNavBots, 8);
            ConfigureNavButton(btnNavHub, "CONFIGURATION", 1, "System settings", SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan); // Cyan
            CreateChamferedRegion(btnNavHub, 8);
            ConfigureNavButton(btnNavLogs, "SYSTEM LOGS", 2, "View activity logs", SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan); // Cyan
            CreateChamferedRegion(btnNavLogs, 8);

            var separator = new Panel();
            separator.BackColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.BorderColor;
            separator.Size = new Size(200, 1);
            separator.Margin = new Padding(20, 20, 20, 20);
            navButtonsPanel.Controls.Add(separator);

            var btnTray = new Button();
            ConfigureNavButton(btnTray, "SEND TO TRAY", 3, "Minimize to system tray", SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan);
            CreateChamferedRegion(btnTray, 8);
            btnTray.Click += BtnTray_Click;
            navButtonsPanel.Controls.Add(btnTray);

            // Sidebar Bottom Panel
            var spacerPanel = new Panel();
            spacerPanel.Dock = DockStyle.Top;
            spacerPanel.Height = 8;  // Gap between combo and button
            sidebarBottomPanel.Controls.Add(btnUpdate);
            sidebarBottomPanel.Controls.Add(spacerPanel);
            sidebarBottomPanel.Controls.Add(comboBox1);
            sidebarBottomPanel.Dock = DockStyle.Bottom;
            sidebarBottomPanel.Height = 90;  // Increased height for better spacing
            sidebarBottomPanel.Location = new Point(0, 510);
            sidebarBottomPanel.Name = "sidebarBottomPanel";
            sidebarBottomPanel.Padding = new Padding(10, 5, 10, 10);
            sidebarBottomPanel.TabIndex = 0;
            sidebarBottomPanel.BackColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.SurfaceColor;
            sidebarBottomPanel.MaximumSize = new Size(240, 90);
            CreateChamferedRegion(sidebarBottomPanel, 15);
            EnableDoubleBuffering(sidebarBottomPanel);

            // Mode Selector ComboBox - Enhanced style
            comboBox1.Dock = DockStyle.Top;
            comboBox1.BackColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.SurfaceColor;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FlatStyle = FlatStyle.Flat;
            comboBox1.Font = new Font("Segoe UI", 9F);
            comboBox1.ForeColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan;
            comboBox1.Name = "comboBox1";
            comboBox1.TabIndex = 10;
            comboBox1.Cursor = Cursors.Hand;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            CreateChamferedRegion(comboBox1, 5);

            // Update Button - Modern style with proper spacing
            var dpiScale = this.DeviceDpi / 96f;
            btnUpdate.Dock = DockStyle.Bottom;
            btnUpdate.BackColor = Color.FromArgb(0, 100, 100); // Keep distinct for now
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 150, 150);
            btnUpdate.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 80, 80);
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.Font = ScaleFont(new Font("Segoe UI", 9F, FontStyle.Bold));
            btnUpdate.ForeColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan;
            btnUpdate.Height = 32;
            btnUpdate.Margin = new Padding(0, 8, 0, 0);  // Add gap between combo and button
            btnUpdate.Name = "btnUpdate";
            btnUpdate.TabIndex = 1;
            btnUpdate.Text = "";  // Text will be set by ConfigureUpdateButton
            btnUpdate.UseVisualStyleBackColor = false;
            btnUpdate.Click += Updater_Click;
            btnUpdate.Cursor = Cursors.Hand;
            btnUpdate.Tag = new ButtonAnimationState();
            ConfigureHoverAnimation(btnUpdate);
            ConfigureUpdateButton();
            CreateChamferedRegion(btnUpdate, 8);

            // Content Panel
            contentPanel.BackColor = Color.Transparent;
            contentPanel.Controls.Add(botsPanel);
            contentPanel.Controls.Add(hubPanel);
            contentPanel.Controls.Add(logsPanel);
            contentPanel.Controls.Add(devPanel);
            contentPanel.Controls.Add(headerPanel);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(240, 0);
            contentPanel.Margin = new Padding(0);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(860, 600);
            contentPanel.TabIndex = 1;
            EnableDoubleBuffering(contentPanel);

            // Header Panel - Cuztom style
            headerPanel.BackColor = Color.Transparent;
            headerPanel.Controls.Add(controlButtonsPanel);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            headerPanel.Location = new Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(860, 60);
            headerPanel.TabIndex = 3;
            headerPanel.Paint += HeaderPanel_Paint;
            headerPanel.Resize += HeaderPanel_Resize;
            EnableDoubleBuffering(headerPanel);

            // Title Label
            titleLabel.AutoSize = true;
            titleLabel.Font = ScaleFont(new Font("Segoe UI", 16F, FontStyle.Bold));
            titleLabel.ForeColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.TextColor;
            titleLabel.Location = new Point(20, 18);
            titleLabel.Name = "titleLabel";
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Bot Management";
            titleLabel.MaximumSize = new Size(350, 35);
            titleLabel.AutoEllipsis = true;

            // Control Buttons Panel
            controlButtonsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            controlButtonsPanel.AutoSize = true;
            controlButtonsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            controlButtonsPanel.Controls.Add(btnStart);
            controlButtonsPanel.Controls.Add(btnStop);
            controlButtonsPanel.Controls.Add(btnReboot);
            controlButtonsPanel.FlowDirection = FlowDirection.LeftToRight;
            controlButtonsPanel.Location = new Point(contentPanel.Width - 300, 18);
            controlButtonsPanel.Name = "controlButtonsPanel";
            controlButtonsPanel.TabIndex = 1;
            controlButtonsPanel.BackColor = Color.Transparent;
            controlButtonsPanel.WrapContents = false;

            // Modern control buttons with clean design
            ConfigureEnhancedControlButton(btnStart, "START", SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentGreen, "▶");
            // CreateChamferedRegion(btnStart, 12);
            ConfigureEnhancedControlButton(btnStop, "STOP", SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentRed, "■");
            // CreateChamferedRegion(btnStop, 12);
            ConfigureEnhancedControlButton(btnReboot, "RESTART", SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentPurple, "↻");
            // CreateChamferedRegion(btnReboot, 12);

            btnStart.Click += B_Start_Click;
            btnStop.Click += B_Stop_Click;
            btnReboot.Click += B_RebootStop_Click;

            // Bots Panel
            botsPanel.BackColor = Color.Transparent;
            botsPanel.Controls.Add(FLP_Bots);
            botsPanel.Controls.Add(botHeaderPanel);
            botsPanel.Dock = DockStyle.Fill;
            botsPanel.Location = new Point(0, 60);
            botsPanel.Name = "botsPanel";
            botsPanel.Padding = new Padding(25, 0, 25, 0);
            botsPanel.Size = new Size(860, 540);
            botsPanel.TabIndex = 0;
            botsPanel.Visible = true;
            EnableDoubleBuffering(botsPanel);

            // Bot Header Panel - Cuztom style
            botHeaderPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            botHeaderPanel.BackColor = Color.Transparent;
            botHeaderPanel.Controls.Add(addBotPanel);
            botHeaderPanel.Height = 80;
            botHeaderPanel.Location = new Point(30, 10);
            botHeaderPanel.Name = "botHeaderPanel";
            botHeaderPanel.Size = new Size(800, 80);
            botHeaderPanel.TabIndex = 1;
            CreateChamferedRegion(botHeaderPanel, 20);
            EnableDoubleBuffering(botHeaderPanel);

            // Add Bot Panel
            addBotPanel.Dock = DockStyle.Fill;
            addBotPanel.Location = new Point(0, 0);
            addBotPanel.Name = "addBotPanel";
            addBotPanel.Size = new Size(840, 80);
            addBotPanel.TabIndex = 0;
            addBotPanel.BackColor = Color.Transparent;
            // addBotPanel.Layout += AddBotPanel_Layout; // Removed logic

            // IP Box Container - Alien Style
            var pnlIP = new Panel();
            pnlIP.Location = new Point(10, 8);
            pnlIP.Size = new Size(140, 64);
            pnlIP.BackColor = Color.Transparent;
            pnlIP.Name = "pnlIP";
            pnlIP.Paint += PaintAlienInputPanel;
            // CreateChamferedRegion(pnlIP, 15); // Removed for smooth edges
            
            var lblIP = new Label();
            lblIP.Text = "IP ADDRESS";
            lblIP.ForeColor = Color.LightGray; // Neutral
            lblIP.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            lblIP.Location = new Point(15, 5);
            lblIP.AutoSize = true;
            lblIP.BackColor = Color.Transparent;
            pnlIP.Controls.Add(lblIP);

            // TB_IP
            TB_IP.BackColor = Color.FromArgb(20, 20, 20); // Dark neutral
            TB_IP.BorderStyle = BorderStyle.None;
            TB_IP.Font = ScaleFont(new Font("Segoe UI", 9F));
            TB_IP.ForeColor = Color.White;
            TB_IP.Location = new Point(15, 20);
            TB_IP.Name = "TB_IP";
            TB_IP.PlaceholderText = "192.168.0.1";
            TB_IP.Size = new Size(110, 23);
            TB_IP.TabIndex = 0;
            TB_IP.Text = "192.168.0.1";
            TB_IP.TextAlign = HorizontalAlignment.Center;
            // CreateChamferedRegion(TB_IP, 5); // Removed for smooth edges
            pnlIP.Controls.Add(TB_IP);
            addBotPanel.Controls.Add(pnlIP);

            // Port Box Container - Alien Style
            var pnlPort = new Panel();
            pnlPort.Location = new Point(160, 8);
            pnlPort.Size = new Size(80, 64);
            pnlPort.BackColor = Color.Transparent;
            pnlPort.Name = "pnlPort";
            pnlPort.Paint += PaintAlienInputPanel;
            // CreateChamferedRegion(pnlPort, 15); // Removed for smooth edges
            
            var lblPort = new Label();
            lblPort.Text = "PORT";
            lblPort.ForeColor = Color.LightGray; // Neutral
            lblPort.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            lblPort.Location = new Point(15, 5);
            lblPort.AutoSize = true;
            lblPort.BackColor = Color.Transparent;
            pnlPort.Controls.Add(lblPort);

            // NUD_Port
            ConfigureNumericUpDown(NUD_Port, 15, 20, 50);
            NUD_Port.BackColor = Color.FromArgb(20, 20, 20); // Dark neutral
            NUD_Port.BorderStyle = BorderStyle.None;
            NUD_Port.ForeColor = Color.White;
            NUD_Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NUD_Port.Value = new decimal(new int[] { 6000, 0, 0, 0 });
            NUD_Port.TextAlign = HorizontalAlignment.Center;
            // CreateChamferedRegion(NUD_Port, 5); // Removed for smooth edges
            pnlPort.Controls.Add(NUD_Port);
            addBotPanel.Controls.Add(pnlPort);

            // Protocol Box Container - Alien Style
            var pnlProtocol = new Panel();
            pnlProtocol.Location = new Point(250, 8);
            pnlProtocol.Size = new Size(160, 64);
            pnlProtocol.BackColor = Color.Transparent;
            pnlProtocol.Name = "pnlProtocol";
            pnlProtocol.Paint += PaintAlienInputPanel;
            
            var lblProtocol = new Label();
            lblProtocol.Text = "PROTOCOL";
            lblProtocol.ForeColor = Color.LightGray; // Neutral
            lblProtocol.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            lblProtocol.Location = new Point(15, 5);
            lblProtocol.AutoSize = true;
            lblProtocol.BackColor = Color.Transparent;
            pnlProtocol.Controls.Add(lblProtocol);

            // CB_Protocol
            CB_Protocol.SuspendLayout();
            ConfigureComboBox(CB_Protocol, 15, 20, 130);
            CB_Protocol.BackColor = Color.FromArgb(20, 20, 20); // Dark neutral
            CB_Protocol.SelectedIndexChanged += CB_Protocol_SelectedIndexChanged;
            CB_Protocol.ResumeLayout();
            // CreateChamferedRegion(CB_Protocol, 5); // Removed for smooth edges
            pnlProtocol.Controls.Add(CB_Protocol);
            addBotPanel.Controls.Add(pnlProtocol);

            // Routine Box Container - Alien Style
            var pnlRoutine = new Panel();
            pnlRoutine.Location = new Point(420, 8);
            pnlRoutine.Size = new Size(160, 64);
            pnlRoutine.BackColor = Color.Transparent;
            pnlRoutine.Name = "pnlRoutine";
            pnlRoutine.Paint += PaintAlienInputPanel;
            // CreateChamferedRegion(pnlRoutine, 15); // Removed for smooth edges
            
            var lblRoutine = new Label();
            lblRoutine.Text = "ROUTINE";
            lblRoutine.ForeColor = Color.LightGray; // Neutral
            lblRoutine.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            lblRoutine.Location = new Point(15, 5);
            lblRoutine.AutoSize = true;
            lblRoutine.BackColor = Color.Transparent;
            pnlRoutine.Controls.Add(lblRoutine);

            // CB_Routine
            ConfigureComboBox(CB_Routine, 15, 20, 130);
            CB_Routine.BackColor = Color.FromArgb(20, 20, 20); // Dark neutral
            // CreateChamferedRegion(CB_Routine, 5); // Removed for smooth edges
            pnlRoutine.Controls.Add(CB_Routine);
            addBotPanel.Controls.Add(pnlRoutine);

            // Add Bot Button - Alien Style
            B_New.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            B_New.BackColor = Color.Transparent;
            B_New.FlatAppearance.BorderSize = 0;
            B_New.FlatAppearance.MouseDownBackColor = Color.Transparent;
            B_New.FlatAppearance.MouseOverBackColor = Color.Transparent;
            B_New.FlatStyle = FlatStyle.Flat;
            B_New.Font = ScaleFont(new Font("Segoe UI", 8.5F, FontStyle.Bold));
            B_New.ForeColor = Color.White;
            B_New.Location = new Point(590, 20);
            B_New.Name = "B_New";
            B_New.Size = new Size(120, 40);
            B_New.TabIndex = 4;
            B_New.Text = "+ ADD BOT";
            B_New.UseVisualStyleBackColor = false;
            B_New.Click += B_New_Click;
            B_New.Cursor = Cursors.Hand;
            B_New.Paint += PaintAlienAddButton;
            CreateChamferedRegion(B_New, 15);
            addBotPanel.Controls.Add(B_New);

            // Bots Flow Layout Panel
            FLP_Bots.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            FLP_Bots.AutoScroll = true;
            FLP_Bots.BackColor = Color.Transparent;
            FLP_Bots.FlowDirection = FlowDirection.TopDown;
            FLP_Bots.Location = new Point(30, 95);
            FLP_Bots.Margin = new Padding(0, 5, 0, 0);
            FLP_Bots.Name = "FLP_Bots";
            FLP_Bots.Padding = new Padding(0);
            FLP_Bots.Size = new Size(800, 435);
            FLP_Bots.TabIndex = 0;
            FLP_Bots.WrapContents = false;
            FLP_Bots.Resize += FLP_Bots_Resize;
            FLP_Bots.Paint += FLP_Bots_Paint;
            FLP_Bots.Scroll += FLP_Bots_Scroll;
            FLP_Bots.ControlAdded += FLP_Bots_ControlAdded;
            FLP_Bots.ControlRemoved += FLP_Bots_ControlRemoved;
            EnableDoubleBuffering(FLP_Bots);

            // Hub Panel
            hubPanel.BackColor = Color.Transparent;
            // hubPanel.Controls.Add(PG_Hub); // Moved inside container
            hubPanel.Dock = DockStyle.Fill;
            hubPanel.Location = new Point(0, 60);
            hubPanel.Name = "hubPanel";
            hubPanel.Padding = new Padding(10);
            hubPanel.Size = new Size(860, 540);
            hubPanel.TabIndex = 1;
            hubPanel.Visible = false;
            EnableDoubleBuffering(hubPanel);

            // Hub Header Panel
            var hubHeaderPanel = new Panel();
            hubHeaderPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            hubHeaderPanel.BackColor = Color.Transparent;
            hubHeaderPanel.Height = 40;
            hubHeaderPanel.Location = new Point(10, 10);
            hubHeaderPanel.Name = "hubHeaderPanel";
            hubHeaderPanel.Size = new Size(840, 40);
            hubHeaderPanel.TabIndex = 0;
            hubHeaderPanel.Paint += PaintAlienInputPanel; // Consistent Glass Style
            hubPanel.Controls.Add(hubHeaderPanel);

            // Hub Title Label
            var lblHubTitle = new Label();
            lblHubTitle.Text = "CORE CONFIGURATION";
            lblHubTitle.Font = ScaleFont(new Font("Segoe UI", 12F, FontStyle.Bold));
            lblHubTitle.ForeColor = Color.White;
            lblHubTitle.AutoSize = true;
            lblHubTitle.Location = new Point(15, 8);
            lblHubTitle.BackColor = Color.Transparent;
            hubHeaderPanel.Controls.Add(lblHubTitle);

            // Hub Subtitle/Status
            var lblHubStatus = new Label();
            lblHubStatus.Text = "SYSTEM ONLINE";
            lblHubStatus.Font = ScaleFont(new Font("Segoe UI", 8F));
            lblHubStatus.ForeColor = Color.FromArgb(0, 200, 255); // Cyan Accent
            lblHubStatus.AutoSize = true;
            lblHubStatus.Location = new Point(200, 14);
            lblHubStatus.BackColor = Color.Transparent;
            hubHeaderPanel.Controls.Add(lblHubStatus);

            // Property Grid Container - Alien Tech Style
            var pgContainer = new Panel();
            pgContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pgContainer.BackColor = Color.Transparent;
            pgContainer.Location = new Point(10, 60); // Moved down for header
            pgContainer.Name = "pgContainer";
            pgContainer.Padding = new Padding(15); // Increased padding for tech border
            pgContainer.Size = new Size(840, 470);
            pgContainer.Paint += PaintAlienTechPanel; // Enhanced Alienware Style
            EnableDoubleBuffering(pgContainer);
            hubPanel.Controls.Add(pgContainer);

            // Property Grid - Cuztom colors
            PG_Hub.BackColor = Color.FromArgb(12, 12, 12); // Keep dark background
            PG_Hub.CategoryForeColor = Color.White; // Neutral White
            PG_Hub.CategorySplitterColor = Color.FromArgb(20, 20, 20);
            PG_Hub.CommandsBackColor = Color.FromArgb(18, 18, 18);
            PG_Hub.CommandsForeColor = Color.White;
            PG_Hub.Dock = DockStyle.Fill;
            PG_Hub.Font = ScaleFont(new Font("Segoe UI", 9F));
            PG_Hub.HelpBackColor = Color.FromArgb(15, 15, 15);
            PG_Hub.HelpForeColor = Color.FromArgb(200, 200, 200); // Light Grey
            PG_Hub.LineColor = Color.FromArgb(40, 40, 40); // Darker Grey
            PG_Hub.Location = new Point(2, 2);
            PG_Hub.Name = "PG_Hub";
            PG_Hub.PropertySort = PropertySort.Categorized;
            PG_Hub.Size = new Size(836, 516);
            PG_Hub.TabIndex = 0;
            PG_Hub.ToolbarVisible = false;
            PG_Hub.ViewBackColor = Color.FromArgb(12, 12, 12);
            PG_Hub.ViewForeColor = Color.White;
            pgContainer.Controls.Add(PG_Hub);
            PG_Hub.CreateControl();

            // Logs Panel
            logsPanel.BackColor = Color.Transparent;
            logsPanel.Dock = DockStyle.Fill;
            logsPanel.Location = new Point(0, 60);
            logsPanel.Name = "logsPanel";
            logsPanel.Padding = new Padding(10);
            logsPanel.Size = new Size(860, 540);
            logsPanel.TabIndex = 2;
            logsPanel.Visible = false;
            EnableDoubleBuffering(logsPanel);

            // Logs Container - Cuztom style
            var logsContainer = new Panel();
            logsContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logsContainer.BackColor = Color.Transparent;
            logsContainer.Location = new Point(10, 60);
            logsContainer.Margin = new Padding(0, 5, 0, 0);
            logsContainer.Name = "logsContainer";
            logsContainer.Padding = new Padding(2);
            logsContainer.Size = new Size(840, 470);
            // ConfigureAlienContainer(logsContainer); // Removed
            logsContainer.Paint += PaintAlienInputPanel; // Glass Style
            EnableDoubleBuffering(logsContainer);
            logsPanel.Controls.Add(logsContainer);
            logsPanel.Controls.Add(logsHeaderPanel);

            // Logs Header Panel - Cuztom style
            logsHeaderPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            logsHeaderPanel.BackColor = Color.Transparent;
            logsHeaderPanel.Height = 45;
            logsHeaderPanel.Location = new Point(10, 10);
            logsHeaderPanel.Name = "logsHeaderPanel";
            logsHeaderPanel.Padding = new Padding(15, 8, 15, 8);
            logsHeaderPanel.Size = new Size(840, 45);
            logsHeaderPanel.TabIndex = 1;
            // CreateChamferedRegion(logsHeaderPanel, 15); // Removed
            logsHeaderPanel.Paint += PaintAlienInputPanel; // Glass Style
            EnableDoubleBuffering(logsHeaderPanel);

            // Search Panel
            searchPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            searchPanel.Controls.Add(logSearchBox);
            searchPanel.Height = 23;
            searchPanel.Location = new Point(15, 11);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(380, 23);
            searchPanel.TabIndex = 0;
            searchPanel.BackColor = Color.Transparent;
            searchPanel.Paint += PaintAlienInputPanelSmall; // Small Glass Style

            // Log Search Box - Cuztom style
            logSearchBox.BackColor = Color.FromArgb(20, 20, 20); // Dark Neutral
            logSearchBox.BorderStyle = BorderStyle.None;
            logSearchBox.Dock = DockStyle.Fill;
            logSearchBox.Font = ScaleFont(new Font("Segoe UI", 8.5F));
            logSearchBox.ForeColor = Color.White;
            logSearchBox.Location = new Point(0, 0);
            logSearchBox.Name = "logSearchBox";
            logSearchBox.PlaceholderText = "Search logs (Enter = next, Shift+Enter = previous, Esc = clear)...";
            logSearchBox.Size = new Size(380, 23);
            logSearchBox.TabIndex = 0;
            logSearchBox.TextChanged += LogSearchBox_TextChanged;
            logSearchBox.KeyDown += LogSearchBox_KeyDown;

            // Search Options Panel
            searchOptionsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            searchOptionsPanel.AutoSize = true;
            searchOptionsPanel.Controls.Add(btnCaseSensitive);
            searchOptionsPanel.Controls.Add(btnRegex);
            searchOptionsPanel.Controls.Add(btnWholeWord);
            searchOptionsPanel.FlowDirection = FlowDirection.LeftToRight;
            searchOptionsPanel.Height = 18;
            searchOptionsPanel.Location = new Point(400, 8);
            searchOptionsPanel.Name = "searchOptionsPanel";
            searchOptionsPanel.Size = new Size(100, 28);
            searchOptionsPanel.TabIndex = 1;
            searchOptionsPanel.BackColor = Color.FromArgb(18, 18, 18);
            searchOptionsPanel.WrapContents = false;

            ConfigureSearchOption(btnCaseSensitive, "Aa", "Case sensitive search");
            ConfigureSearchOption(btnRegex, ".*", "Regular expression search");
            ConfigureSearchOption(btnWholeWord, "W", "Whole word search");

            // Search Status Label
            searchStatusLabel.AutoSize = true;
            searchStatusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchStatusLabel.Font = ScaleFont(new Font("Segoe UI", 7.5F));
            searchStatusLabel.ForeColor = Color.FromArgb(139, 179, 217);
            searchStatusLabel.Location = new Point(500, 14); // Moved left
            searchStatusLabel.Name = "searchStatusLabel";
            searchStatusLabel.Size = new Size(80, 12);
            searchStatusLabel.TabIndex = 2;
            searchStatusLabel.Text = "";
            searchStatusLabel.TextAlign = ContentAlignment.MiddleRight;

            // Auto-Scroll Button
            btnAutoScroll = new Button();
            btnAutoScroll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAutoScroll.BackColor = Color.FromArgb(0, 100, 100);
            btnAutoScroll.FlatAppearance.BorderSize = 0;
            btnAutoScroll.FlatStyle = FlatStyle.Flat;
            btnAutoScroll.Font = ScaleFont(new Font("Segoe UI", 7.5F, FontStyle.Bold));
            btnAutoScroll.ForeColor = Color.Cyan;
            btnAutoScroll.Location = new Point(590, 10);
            btnAutoScroll.Name = "btnAutoScroll";
            btnAutoScroll.Size = new Size(75, 23);
            btnAutoScroll.TabIndex = 4;
            btnAutoScroll.Text = "SCROLL: ON";
            btnAutoScroll.UseVisualStyleBackColor = false;
            btnAutoScroll.Cursor = Cursors.Hand;
            btnAutoScroll.Click += BtnAutoScroll_Click;
            ConfigureGlowButton(btnAutoScroll);
            CreateChamferedRegion(btnAutoScroll, 8);

            // Export Logs Button
            btnExportLogs = new Button();
            btnExportLogs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportLogs.BackColor = Color.FromArgb(0, 204, 255);
            btnExportLogs.FlatAppearance.BorderSize = 0;
            btnExportLogs.FlatStyle = FlatStyle.Flat;
            btnExportLogs.Font = ScaleFont(new Font("Segoe UI", 7.5F, FontStyle.Bold));
            btnExportLogs.ForeColor = Color.White;
            btnExportLogs.Location = new Point(670, 10);
            btnExportLogs.Name = "btnExportLogs";
            btnExportLogs.Size = new Size(75, 23);
            btnExportLogs.TabIndex = 5;
            btnExportLogs.Text = "EXPORT";
            btnExportLogs.UseVisualStyleBackColor = false;
            btnExportLogs.Cursor = Cursors.Hand;
            btnExportLogs.Click += BtnExportLogs_Click;
            ConfigureGlowButton(btnExportLogs);
            CreateChamferedRegion(btnExportLogs, 8);

            // Clear Logs Button - Cuztom style
            btnClearLogs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearLogs.BackColor = Color.FromArgb(236, 98, 95);
            btnClearLogs.FlatAppearance.BorderSize = 0;
            btnClearLogs.FlatStyle = FlatStyle.Flat;
            btnClearLogs.Font = ScaleFont(new Font("Segoe UI", 7.5F, FontStyle.Bold));
            btnClearLogs.ForeColor = Color.White;
            btnClearLogs.Location = new Point(750, 10);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(75, 23);
            btnClearLogs.TabIndex = 3;
            btnClearLogs.Text = "CLEAR";
            btnClearLogs.UseVisualStyleBackColor = false;
            btnClearLogs.Cursor = Cursors.Hand;
            btnClearLogs.Click += BtnClearLogs_Click;
            ConfigureGlowButton(btnClearLogs);
            CreateChamferedRegion(btnClearLogs, 8);


            // Rich Text Box - Cuztom style
            RTB_Logs.BackColor = Color.FromArgb(12, 12, 12);
            RTB_Logs.BorderStyle = BorderStyle.None;
            RTB_Logs.Dock = DockStyle.Fill;
            RTB_Logs.Font = ScaleFont(new Font("Consolas", 9F));
            RTB_Logs.ForeColor = Color.White;
            RTB_Logs.Location = new Point(2, 2);
            RTB_Logs.Name = "RTB_Logs";
            RTB_Logs.ReadOnly = true;
            RTB_Logs.Size = new Size(836, 466);
            RTB_Logs.TabIndex = 0;
            RTB_Logs.Text = "";
            RTB_Logs.HideSelection = false;
            RTB_Logs.KeyDown += RTB_Logs_KeyDown;
            // RTB_Logs.Region = new Region(logsContainer.ClientRectangle); // Ensure no clipping
            logsContainer.Controls.Add(RTB_Logs);

            // Add controls to logsHeaderPanel
            logsHeaderPanel.Controls.Add(searchPanel);
            logsHeaderPanel.Controls.Add(searchOptionsPanel);
            logsHeaderPanel.Controls.Add(searchStatusLabel);
            logsHeaderPanel.Controls.Add(btnAutoScroll);
            logsHeaderPanel.Controls.Add(btnExportLogs);
            logsHeaderPanel.Controls.Add(btnClearLogs);

            // Dev Nav Button
            ConfigureNavButton(btnNavDev, "DEVELOPER", 3, "Memory Scanner & Tools", Color.FromArgb(0, 204, 255)); // Cyan

            // Dev Panel
            devPanel.Dock = DockStyle.Fill;
            devPanel.BackColor = Color.Transparent;
            devPanel.Name = "devPanel";
            devPanel.Visible = false; // Hidden by default
            
            // Dev Title
            lblDevTitle.AutoSize = true;
            lblDevTitle.Font = ScaleFont(new Font("Segoe UI", 16F, FontStyle.Bold));
            lblDevTitle.ForeColor = Color.White;
            lblDevTitle.Location = new Point(20, 20);
            lblDevTitle.Name = "lblDevTitle";
            lblDevTitle.Text = "Developer Tools";
            devPanel.Controls.Add(lblDevTitle);

            // Dev Flow Layout Panel (Container for scrollable content)
            var devFlowPanel = new FlowLayoutPanel();
            devFlowPanel.Location = new Point(0, 60);
            devFlowPanel.Size = new Size(860, 540); // Initial size
            devFlowPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            devFlowPanel.AutoScroll = true;
            devFlowPanel.FlowDirection = FlowDirection.TopDown;
            devFlowPanel.WrapContents = false;
            devFlowPanel.Padding = new Padding(20, 0, 0, 20); // Left padding alignment
            devPanel.Controls.Add(devFlowPanel);

            // Connection Panel (Alien Tech Style)
            pnlDevConnection = new Panel();
            pnlDevConnection.Name = "pnlDevConnection";
            pnlDevConnection.Size = new Size(800, 140);
            pnlDevConnection.Margin = new Padding(0, 0, 0, 20);
            pnlDevConnection.Paint += PaintAlienTechPanel;
            pnlDevConnection.BackColor = Color.Transparent;
            devFlowPanel.Controls.Add(pnlDevConnection);

            // Title
            var lblConnTitle = new Label();
            lblConnTitle.Text = "MANUAL CONNECTION";
            lblConnTitle.Font = ScaleFont(new Font("Segoe UI", 10F, FontStyle.Bold));
            lblConnTitle.ForeColor = Color.Cyan;
            lblConnTitle.Location = new Point(20, 15);
            lblConnTitle.AutoSize = true;
            pnlDevConnection.Controls.Add(lblConnTitle);

            // IP Address
            lblIP = new Label();
            lblIP.Text = "IP ADDRESS";
            lblIP.Location = new Point(20, 50);
            lblIP.AutoSize = true;
            lblIP.ForeColor = Color.Gray;
            lblIP.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevConnection.Controls.Add(lblIP);

            txtIP = new TextBox();
            txtIP.Location = new Point(20, 65);
            txtIP.Size = new Size(180, 25);
            txtIP.BackColor = Color.FromArgb(20, 20, 20);
            txtIP.ForeColor = Color.White;
            txtIP.Text = "192.168.0.1";
            txtIP.BorderStyle = BorderStyle.FixedSingle;
            pnlDevConnection.Controls.Add(txtIP);

            // Port
            lblPort = new Label();
            lblPort.Text = "PORT";
            lblPort.Location = new Point(220, 50);
            lblPort.AutoSize = true;
            lblPort.ForeColor = Color.Gray;
            lblPort.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevConnection.Controls.Add(lblPort);

            txtPort = new TextBox();
            txtPort.Location = new Point(220, 65);
            txtPort.Size = new Size(80, 25);
            txtPort.BackColor = Color.FromArgb(20, 20, 20);
            txtPort.ForeColor = Color.White;
            txtPort.Text = "6000";
            txtPort.BorderStyle = BorderStyle.FixedSingle;
            pnlDevConnection.Controls.Add(txtPort);

            // Connect Button
            btnDevConnect = new Button();
            btnDevConnect.Text = "CONNECT";
            btnDevConnect.Location = new Point(320, 62);
            btnDevConnect.Size = new Size(140, 30);
            btnDevConnect.BackColor = Color.Transparent;
            btnDevConnect.ForeColor = Color.Cyan;
            btnDevConnect.FlatStyle = FlatStyle.Flat;
            btnDevConnect.FlatAppearance.BorderSize = 0;
            btnDevConnect.Paint += PaintAlienTechButton;
            pnlDevConnection.Controls.Add(btnDevConnect);
            
            // Connection Status
            lblConnStatus = new Label();
            lblConnStatus.Text = "DISCONNECTED";
            lblConnStatus.Location = new Point(480, 68);
            lblConnStatus.AutoSize = true;
            lblConnStatus.ForeColor = Color.Red;
            lblConnStatus.Font = ScaleFont(new Font("Segoe UI", 9F, FontStyle.Bold));
            pnlDevConnection.Controls.Add(lblConnStatus);

            // Game Selector
            var lblGame = new Label();
            lblGame.Text = "GAME VERSION";
            lblGame.Location = new Point(20, 100);
            lblGame.AutoSize = true;
            lblGame.ForeColor = Color.Gray;
            lblGame.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevConnection.Controls.Add(lblGame);

            cbGameVersion = new ComboBox();
            cbGameVersion.Items.AddRange(new object[] { "PLZA", "SV", "LA", "SWSH", "BDSP" });
            cbGameVersion.SelectedIndex = 0;
            cbGameVersion.Location = new Point(120, 97);
            cbGameVersion.Size = new Size(100, 25);
            cbGameVersion.BackColor = Color.FromArgb(20, 20, 20);
            cbGameVersion.ForeColor = Color.Cyan;
            cbGameVersion.Name = "cbGameVersion";
            cbGameVersion.FlatStyle = FlatStyle.Flat;
            pnlDevConnection.Controls.Add(cbGameVersion);

            // Scanner Panel (Alien Tech Style)
            pnlDevScanner = new Panel();
            pnlDevScanner.Name = "pnlDevScanner";
            pnlDevScanner.Size = new Size(800, 420);
            pnlDevScanner.Margin = new Padding(0, 0, 0, 20);
            pnlDevScanner.Paint += PaintAlienTechPanel;
            pnlDevScanner.BackColor = Color.Transparent;
            devFlowPanel.Controls.Add(pnlDevScanner);

            // Title
            var lblScannerTitle = new Label();
            lblScannerTitle.Text = "MEMORY SCANNER";
            lblScannerTitle.Font = ScaleFont(new Font("Segoe UI", 10F, FontStyle.Bold));
            lblScannerTitle.ForeColor = Color.Cyan;
            lblScannerTitle.Location = new Point(20, 15);
            lblScannerTitle.AutoSize = true;
            pnlDevScanner.Controls.Add(lblScannerTitle);

            // Row 1: Pattern
            lblPattern.Text = "PATTERN (HEX)";
            lblPattern.Location = new Point(20, 50);
            lblPattern.AutoSize = true;
            lblPattern.ForeColor = Color.Gray;
            lblPattern.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevScanner.Controls.Add(lblPattern);

            txtPattern.Location = new Point(20, 65);
            txtPattern.Size = new Size(300, 25);
            txtPattern.BackColor = Color.FromArgb(20, 20, 20);
            txtPattern.ForeColor = Color.White;
            txtPattern.BorderStyle = BorderStyle.FixedSingle;
            pnlDevScanner.Controls.Add(txtPattern);

            lblRegion.Text = "REGION";
            lblRegion.Location = new Point(340, 50);
            lblRegion.AutoSize = true;
            lblRegion.ForeColor = Color.Gray;
            lblRegion.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevScanner.Controls.Add(lblRegion);

            cbRegion.Items.Clear();
            cbRegion.Items.AddRange(new object[] { "Heap", "Main" });
            cbRegion.SelectedIndex = 0;
            cbRegion.Location = new Point(340, 65);
            cbRegion.Size = new Size(100, 25);
            cbRegion.BackColor = Color.FromArgb(20, 20, 20);
            cbRegion.ForeColor = Color.Cyan;
            cbRegion.FlatStyle = FlatStyle.Flat;
            pnlDevScanner.Controls.Add(cbRegion);

            btnScan.Text = "SCAN";
            btnScan.Location = new Point(460, 62);
            btnScan.Size = new Size(100, 30);
            btnScan.BackColor = Color.Transparent;
            btnScan.ForeColor = Color.Cyan;
            btnScan.FlatStyle = FlatStyle.Flat;
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Paint += PaintAlienTechButton;
            pnlDevScanner.Controls.Add(btnScan);

            // Row 2: Start & Length
            lblStart.Text = "START OFFSET";
            lblStart.Location = new Point(20, 100);
            lblStart.AutoSize = true;
            lblStart.ForeColor = Color.Gray;
            lblStart.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevScanner.Controls.Add(lblStart);

            txtStart.Location = new Point(20, 115);
            txtStart.Size = new Size(140, 25);
            txtStart.BackColor = Color.FromArgb(20, 20, 20);
            txtStart.ForeColor = Color.White;
            txtStart.Text = "0x0";
            txtStart.BorderStyle = BorderStyle.FixedSingle;
            pnlDevScanner.Controls.Add(txtStart);

            lblLength.Text = "LENGTH";
            lblLength.Location = new Point(180, 100);
            lblLength.AutoSize = true;
            lblLength.ForeColor = Color.Gray;
            lblLength.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevScanner.Controls.Add(lblLength);

            txtLength.Location = new Point(180, 115);
            txtLength.Size = new Size(140, 25);
            txtLength.BackColor = Color.FromArgb(20, 20, 20);
            txtLength.ForeColor = Color.White;
            txtLength.Text = "0x4000000";
            txtLength.BorderStyle = BorderStyle.FixedSingle;
            pnlDevScanner.Controls.Add(txtLength);

            // Row 3: Signature
            var lblSigOffset = new Label();
            lblSigOffset.Text = "OFFSET (HEX)";
            lblSigOffset.Location = new Point(20, 150);
            lblSigOffset.AutoSize = true;
            lblSigOffset.ForeColor = Color.Gray;
            lblSigOffset.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevScanner.Controls.Add(lblSigOffset);

            txtSigOffset = new TextBox();
            txtSigOffset.Location = new Point(20, 165);
            txtSigOffset.Size = new Size(140, 25);
            txtSigOffset.BackColor = Color.FromArgb(20, 20, 20);
            txtSigOffset.ForeColor = Color.White;
            txtSigOffset.Name = "txtSigOffset";
            txtSigOffset.BorderStyle = BorderStyle.FixedSingle;
            pnlDevScanner.Controls.Add(txtSigOffset);

            btnFindSig = new Button();
            btnFindSig.Text = "FIND SIG";
            btnFindSig.Location = new Point(180, 162);
            btnFindSig.Size = new Size(100, 30);
            btnFindSig.BackColor = Color.Transparent;
            btnFindSig.ForeColor = Color.Cyan;
            btnFindSig.FlatStyle = FlatStyle.Flat;
            btnFindSig.FlatAppearance.BorderSize = 0;
            btnFindSig.Name = "btnFindSig";
            btnFindSig.Paint += PaintAlienTechButton;
            pnlDevScanner.Controls.Add(btnFindSig);

            btnDumpMain = new Button();
            btnDumpMain.Text = "DUMP NSO";
            btnDumpMain.Location = new Point(300, 162);
            btnDumpMain.Size = new Size(100, 30);
            btnDumpMain.BackColor = Color.Transparent;
            btnDumpMain.ForeColor = Color.OrangeRed; // Warning
            btnDumpMain.FlatStyle = FlatStyle.Flat;
            btnDumpMain.FlatAppearance.BorderSize = 0;
            btnDumpMain.Name = "btnDumpMain";
            btnDumpMain.Paint += PaintAlienTechButton;
            pnlDevScanner.Controls.Add(btnDumpMain);

            // Row 4: Advanced
            btnAutoUpdate = new Button();
            btnAutoUpdate.Text = "AUTO-UPDATE";
            btnAutoUpdate.Location = new Point(20, 210);
            btnAutoUpdate.Size = new Size(140, 30);
            btnAutoUpdate.BackColor = Color.Transparent;
            btnAutoUpdate.ForeColor = Color.Magenta;
            btnAutoUpdate.FlatStyle = FlatStyle.Flat;
            btnAutoUpdate.FlatAppearance.BorderSize = 0;
            btnAutoUpdate.Name = "btnAutoUpdate";
            btnAutoUpdate.Paint += PaintAlienTechButton;
            pnlDevScanner.Controls.Add(btnAutoUpdate);

            btnFindChain = new Button();
            btnFindChain.Text = "FIND POINTER CHAIN";
            btnFindChain.Location = new Point(180, 210);
            btnFindChain.Size = new Size(160, 30);
            btnFindChain.BackColor = Color.Transparent;
            btnFindChain.ForeColor = Color.Lime;
            btnFindChain.FlatStyle = FlatStyle.Flat;
            btnFindChain.FlatAppearance.BorderSize = 0;
            btnFindChain.Name = "btnFindChain";
            btnFindChain.Paint += PaintAlienTechButton;
            pnlDevScanner.Controls.Add(btnFindChain);

            btnAutoScan = new Button();
            btnAutoScan.Text = "AUTO-FIND";
            btnAutoScan.Location = new Point(360, 210);
            btnAutoScan.Size = new Size(100, 30);
            btnAutoScan.BackColor = Color.Transparent;
            btnAutoScan.ForeColor = Color.Cyan;
            btnAutoScan.FlatStyle = FlatStyle.Flat;
            btnAutoScan.FlatAppearance.BorderSize = 0;
            btnAutoScan.Name = "btnAutoScan";
            btnAutoScan.Paint += PaintAlienTechButton;
            pnlDevScanner.Controls.Add(btnAutoScan);

            btnVerify = new Button();
            btnVerify.Text = "VERIFY LIVE";
            btnVerify.Location = new Point(480, 210);
            btnVerify.Size = new Size(100, 30);
            btnVerify.BackColor = Color.Transparent;
            btnVerify.ForeColor = Color.White;
            btnVerify.FlatStyle = FlatStyle.Flat;
            btnVerify.FlatAppearance.BorderSize = 0;
            btnVerify.Name = "btnVerify";
            btnVerify.Paint += PaintAlienTechButton;
            pnlDevScanner.Controls.Add(btnVerify);

            // Row 5: Status
            lblScanStatus.Text = "READY";
            lblScanStatus.Location = new Point(20, 250);
            lblScanStatus.AutoSize = true;
            lblScanStatus.ForeColor = Color.Gray;
            lblScanStatus.Font = ScaleFont(new Font("Segoe UI", 8F, FontStyle.Bold));
            pnlDevScanner.Controls.Add(lblScanStatus);

            // Row 6: Results
            rtbResults.Location = new Point(20, 270);
            rtbResults.Size = new Size(760, 130);
            rtbResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbResults.BackColor = Color.FromArgb(12, 12, 12);
            rtbResults.ForeColor = Color.White;
            rtbResults.ReadOnly = true;
            rtbResults.BorderStyle = BorderStyle.None;
            pnlDevScanner.Controls.Add(rtbResults);

            // Monitor Panel (Alien Tech Style)
            pnlDevMonitor = new Panel();
            pnlDevMonitor.Name = "pnlDevMonitor";
            pnlDevMonitor.Size = new Size(800, 300);
            pnlDevMonitor.Margin = new Padding(0, 0, 0, 20);
            pnlDevMonitor.Paint += PaintAlienTechPanel;
            pnlDevMonitor.BackColor = Color.Transparent;
            devFlowPanel.Controls.Add(pnlDevMonitor);

            // Title
            var lblMonitorTitle = new Label();
            lblMonitorTitle.Text = "MEMORY MONITOR & POINTER TOOLS";
            lblMonitorTitle.Font = ScaleFont(new Font("Segoe UI", 10F, FontStyle.Bold));
            lblMonitorTitle.ForeColor = Color.Cyan;
            lblMonitorTitle.Location = new Point(20, 15);
            lblMonitorTitle.AutoSize = true;
            pnlDevMonitor.Controls.Add(lblMonitorTitle);

            // Address Input (Row 1)
            lblMonitorAddr = new Label();
            lblMonitorAddr.Text = "ADDRESS (HEX)";
            lblMonitorAddr.Location = new Point(20, 50);
            lblMonitorAddr.AutoSize = true;
            lblMonitorAddr.ForeColor = Color.Gray;
            lblMonitorAddr.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevMonitor.Controls.Add(lblMonitorAddr);

            txtMonitorAddr = new TextBox();
            txtMonitorAddr.Location = new Point(20, 65);
            txtMonitorAddr.Size = new Size(180, 25);
            txtMonitorAddr.BackColor = Color.FromArgb(20, 20, 20);
            txtMonitorAddr.ForeColor = Color.White;
            txtMonitorAddr.BorderStyle = BorderStyle.FixedSingle;
            pnlDevMonitor.Controls.Add(txtMonitorAddr);

            // Length
            lblLengthVal = new Label();
            lblLengthVal.Text = "LENGTH";
            lblLengthVal.Location = new Point(220, 50);
            lblLengthVal.AutoSize = true;
            lblLengthVal.ForeColor = Color.Gray;
            lblLengthVal.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevMonitor.Controls.Add(lblLengthVal);

            numLength = new NumericUpDown();
            numLength.Location = new Point(220, 65);
            numLength.Size = new Size(80, 25);
            numLength.BackColor = Color.FromArgb(20, 20, 20);
            numLength.ForeColor = Color.White;
            numLength.Minimum = 1;
            numLength.Maximum = 1024;
            numLength.Value = 4;
            numLength.BorderStyle = BorderStyle.FixedSingle;
            pnlDevMonitor.Controls.Add(numLength);

            // Buttons
            btnMonitorToggle = new Button();
            btnMonitorToggle.Text = "START MONITOR";
            btnMonitorToggle.Location = new Point(320, 62);
            btnMonitorToggle.Size = new Size(140, 30);
            btnMonitorToggle.BackColor = Color.Transparent;
            btnMonitorToggle.ForeColor = Color.Cyan;
            btnMonitorToggle.FlatStyle = FlatStyle.Flat;
            btnMonitorToggle.FlatAppearance.BorderSize = 0;
            btnMonitorToggle.Paint += PaintAlienTechButton;
            pnlDevMonitor.Controls.Add(btnMonitorToggle);

            btnCopyAddress = new Button();
            btnCopyAddress.Text = "COPY ADDR";
            btnCopyAddress.Location = new Point(480, 62);
            btnCopyAddress.Size = new Size(100, 30);
            btnCopyAddress.BackColor = Color.Transparent;
            btnCopyAddress.ForeColor = Color.White;
            btnCopyAddress.FlatStyle = FlatStyle.Flat;
            btnCopyAddress.FlatAppearance.BorderSize = 0;
            btnCopyAddress.Paint += PaintAlienTechButton;
            pnlDevMonitor.Controls.Add(btnCopyAddress);

            // Checkbox
            chkCachePointer = new CheckBox();
            chkCachePointer.Text = "CACHE PTR";
            chkCachePointer.Location = new Point(600, 65);
            chkCachePointer.AutoSize = true;
            chkCachePointer.ForeColor = Color.White;
            pnlDevMonitor.Controls.Add(chkCachePointer);

            // Value Output (Row 2)
            lblMonitorValue = new Label();
            lblMonitorValue.Text = "VALUE (HEX)";
            lblMonitorValue.Location = new Point(20, 100);
            lblMonitorValue.AutoSize = true;
            lblMonitorValue.ForeColor = Color.Gray;
            lblMonitorValue.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevMonitor.Controls.Add(lblMonitorValue);
            
            // Resize handler update
            devFlowPanel.Resize += (s, e) => {
                int newWidth = devFlowPanel.ClientSize.Width - devFlowPanel.Padding.Left - devFlowPanel.Padding.Right - 20;
                if (pnlDevConnection != null) pnlDevConnection.Width = newWidth;
                if (pnlDevScanner != null) pnlDevScanner.Width = newWidth;
                if (pnlDevMonitor != null) pnlDevMonitor.Width = newWidth;
            };

            txtMonitorValue = new TextBox();
            txtMonitorValue.Location = new Point(20, 115);
            txtMonitorValue.Size = new Size(760, 100);
            txtMonitorValue.Multiline = true;
            txtMonitorValue.ScrollBars = ScrollBars.Vertical;
            txtMonitorValue.BackColor = Color.FromArgb(12, 12, 12);
            txtMonitorValue.ForeColor = Color.White;
            txtMonitorValue.Font = new Font("Consolas", 10F);
            txtMonitorValue.ReadOnly = false;
            txtMonitorValue.BorderStyle = BorderStyle.None;
            txtMonitorValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlDevMonitor.Controls.Add(txtMonitorValue);

            // Pointer Info (Row 3)
            lblPointerInfo = new Label();
            lblPointerInfo.Text = "POINTER INFO";
            lblPointerInfo.Location = new Point(20, 230);
            lblPointerInfo.AutoSize = true;
            lblPointerInfo.ForeColor = Color.Gray;
            lblPointerInfo.Font = ScaleFont(new Font("Segoe UI", 7F, FontStyle.Bold));
            pnlDevMonitor.Controls.Add(lblPointerInfo);

            txtPointerInfo = new TextBox();
            txtPointerInfo.Location = new Point(20, 245);
            txtPointerInfo.Size = new Size(760, 25);
            txtPointerInfo.BackColor = Color.FromArgb(12, 12, 12);
            txtPointerInfo.ForeColor = Color.White;
            txtPointerInfo.ReadOnly = false;
            txtPointerInfo.BorderStyle = BorderStyle.FixedSingle;
            txtPointerInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlDevMonitor.Controls.Add(txtPointerInfo);

            // Spacer for scrolling
            var spacer = new Panel();
            spacer.Size = new Size(1, 50); // 50px bottom padding
            devFlowPanel.Controls.Add(spacer);

            monitorTimer = new System.Windows.Forms.Timer(this.components);
            monitorTimer.Interval = 500; // 500ms update rate

            // Hidden tab control for compatibility
            TC_Main = new TabControl { Visible = false };
            Tab_Bots = new TabPage();
            Tab_Hub = new TabPage();
            Tab_Logs = new TabPage();
            TC_Main.TabPages.Add(Tab_Bots);
            TC_Main.TabPages.Add(Tab_Hub);
            TC_Main.TabPages.Add(Tab_Logs);
            TC_Main.SendToBack();

            Controls.Add(mainLayoutPanel);

            // Resume layouts
            mainLayoutPanel.ResumeLayout(false);
            sidebarPanel.ResumeLayout(false);
            navButtonsPanel.ResumeLayout(false);
            sidebarBottomPanel.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            controlButtonsPanel.ResumeLayout(false);
            contentPanel.ResumeLayout(false);
            botsPanel.ResumeLayout(false);
            botHeaderPanel.ResumeLayout(false);
            addBotPanel.ResumeLayout(false);
            addBotPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NUD_Port).EndInit();
            hubPanel.ResumeLayout(false);
            logsPanel.ResumeLayout(false);
            logsHeaderPanel.ResumeLayout(false);
            logsHeaderPanel.PerformLayout();
            searchPanel.ResumeLayout(false);
            searchOptionsPanel.ResumeLayout(false);
            ResumeLayout(false);

            ConfigureSystemTray();
        }

        #endregion

        #region Font Scaling

        private Font ScaleFont(Font baseFont)
        {
            using (Graphics g = CreateGraphics())
            {
                float dpiScale = g.DpiX / 96f;
                float scaledSize = baseFont.Size * dpiScale;

                if (ClientSize.Width < 900)
                {
                    scaledSize *= 0.85f;
                }
                else if (ClientSize.Width < 1100)
                {
                    scaledSize *= 0.92f;
                }

                scaledSize = Math.Max(7f, scaledSize);

                if (ClientSize.Width < 800)
                {
                    if (baseFont.Size >= 24)
                        scaledSize = Math.Min(scaledSize, 16f);
                    else if (baseFont.Size >= 11)
                        scaledSize = Math.Min(scaledSize, 9f);
                    else
                        scaledSize = Math.Min(scaledSize, 8f);
                }

                return new Font(baseFont.FontFamily, scaledSize, baseFont.Style);
            }
        }

        #endregion

        #region UI Helper Methods

        private void EnableDoubleBuffering(Control control)
        {
            if (control == null) return;

            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        private void HeaderPanel_Resize(object sender, EventArgs e)
        {
            if (controlButtonsPanel != null && headerPanel != null)
            {
                int rightMargin = 20;
                int minLeftPosition = titleLabel.Right + 20;

                int availableWidth = headerPanel.Width - minLeftPosition - rightMargin;

                controlButtonsPanel.MaximumSize = new Size(400, 32);
                controlButtonsPanel.WrapContents = false;

                int desiredX = headerPanel.Width - controlButtonsPanel.Width - rightMargin;
                controlButtonsPanel.Location = new Point(Math.Max(minLeftPosition, desiredX), 16);
            }
        }

        private void ConfigureSearchOption(CheckBox checkBox, string text, string tooltip)
        {
            checkBox.Appearance = Appearance.Button;
            checkBox.BackColor = Color.FromArgb(12, 12, 12);
            checkBox.FlatAppearance.BorderSize = 1;
            checkBox.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
            checkBox.FlatAppearance.CheckedBackColor = Color.FromArgb(0, 100, 100);
            checkBox.FlatStyle = FlatStyle.Flat;
            checkBox.Font = ScaleFont(new Font("Segoe UI", 6.5F, FontStyle.Bold));
            checkBox.ForeColor = Color.FromArgb(200, 200, 200);
            checkBox.Margin = new Padding(0, 0, 3, 0);
            checkBox.Size = new Size(26, 26);
            checkBox.Text = text;
            checkBox.TextAlign = ContentAlignment.MiddleCenter;
            checkBox.UseVisualStyleBackColor = false;
            checkBox.Cursor = Cursors.Hand;
            CreateChamferedRegion(checkBox, 5);
            
            // Ensure checked state changes text color for better visibility
            checkBox.CheckedChanged += (s, e) => 
            {
                if (checkBox.Checked)
                {
                    checkBox.ForeColor = Color.Black; // Dark text on light background
                }
                else
                {
                    checkBox.ForeColor = Color.White; // Light text on dark background
                }
            };

            var toolTip = new ToolTip();
            toolTip.SetToolTip(checkBox, tooltip);
        }

        private void ConfigureNavButton(Button btn, string text, int index, string tooltip, Color neonColor)
        {
            btn.BackColor = Color.Black;
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 20, 20);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = ScaleFont(new Font("Segoe UI", 10F, FontStyle.Regular));
            btn.ForeColor = Color.Gray;
            btn.Location = new Point(0, 10 + (index * 45));
            btn.Margin = new Padding(0, 0, 0, 5);
            btn.Name = $"btnNav{text.Replace(" ", "")}";
            btn.Padding = new Padding(50, 0, 0, 0);
            btn.Size = new Size(240, 40);
            btn.TabIndex = index;
            btn.Text = text;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.UseVisualStyleBackColor = false;
            btn.Tag = new NavButtonState { NeonColor = neonColor, Index = index };
            CreateChamferedRegion(btn, 12);

            btn.Paint += (s, e) => {
                var navState = btn.Tag as NavButtonState;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw background
                using (var bgBrush = new SolidBrush(btn.BackColor))
                {
                    g.FillRectangle(bgBrush, btn.ClientRectangle);
                }

                // Draw left accent bar when selected
                if (navState.IsSelected)
                {
                    using (var accentBrush = new SolidBrush(navState.NeonColor))
                    {
                        g.FillRectangle(accentBrush, 0, 0, 3, btn.Height);
                    }

                    // Draw neon glow effect
                    for (int i = 1; i <= 2; i++)
                    {
                        using (var glowBrush = new SolidBrush(Color.FromArgb(20 / i, navState.NeonColor)))
                        {
                            g.FillRectangle(glowBrush, 0, 0, 3 + i * 2, btn.Height);
                        }
                    }

                    // Update text color to match neon
                    btn.ForeColor = navState.NeonColor;
                }
                else
                {
                    btn.ForeColor = Color.Gray;
                }

                // Draw icon
                int iconSize = 18;
                var iconRect = new Rectangle(15, (btn.Height - iconSize) / 2, iconSize, iconSize);
                using var iconFont = new Font("Segoe MDL2 Assets", 13F);
                string iconText = index switch
                {
                    0 => "\uE77B", // Bots icon
                    1 => "\uE713", // Settings icon
                    2 => "\uE7C3", // Logs icon
                    3 => "\uE90F", // Developer icon (Tools)
                    _ => "\uE700"
                };

                var iconColor = navState.IsSelected ? navState.NeonColor : Color.Gray;
                using var iconBrush = new SolidBrush(iconColor);
                var textSize = g.MeasureString(iconText, iconFont);
                var textX = iconRect.X + (iconRect.Width - textSize.Width) / 2;
                var textY = iconRect.Y + (iconRect.Height - textSize.Height) / 2;
                g.DrawString(iconText, iconFont, iconBrush, textX, textY);

                // Draw text with proper font
                var textRect = new Rectangle(50, 0, btn.Width - 50, btn.Height);
                TextRenderer.DrawText(g, btn.Text, btn.Font, textRect, btn.ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };

            btn.Click += (s, e) => {
                if (index > 3) return; // Don't select tray button

                // Update all nav buttons
                foreach (Button navBtn in navButtonsPanel.Controls.OfType<Button>())
                {
                    if (navBtn.Tag is NavButtonState state)
                    {
                        state.IsSelected = false;
                        navBtn.Invalidate();
                    }
                }

                // Select this button
                var navState = btn.Tag as NavButtonState;
                navState.IsSelected = true;
                btn.Invalidate();

                TransitionPanels(index);

                titleLabel.Text = index switch
                {
                    0 => "Bot Management",
                    1 => "Configuration",
                    2 => "System Logs",
                    3 => "Developer Tools",
                    _ => "PokéBot"
                };
            };

            ConfigureHoverAnimation(btn);

            // Select first button by default
            if (index == 0)
            {
                var navState = btn.Tag as NavButtonState;
                navState.IsSelected = true;
            }
        }

        private void ConfigureEnhancedControlButton(Button btn, string text, Color baseColor, string iconText)
        {
            var dpiScale = this.DeviceDpi / 96f;
            
            // Modern glass-morphism design with responsive sizing
            btn.BackColor = Color.FromArgb(12, 12, 12);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font = ScaleFont(new Font("Segoe UI Semibold", 9F));
            btn.ForeColor = baseColor;
            btn.Margin = new Padding(5, 0, 5, 0);
            btn.Name = $"btn{text.Replace(" ", "")}";
            btn.Padding = new Padding((int)(12 * dpiScale), (int)(6 * dpiScale), (int)(12 * dpiScale), (int)(6 * dpiScale));
            btn.TabIndex = 0;
            btn.Text = $"{iconText}  {text}";
            btn.UseVisualStyleBackColor = false;
            btn.AutoSize = true;
            btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btn.MinimumSize = new Size((int)(85 * dpiScale), (int)(32 * dpiScale));
            btn.MaximumSize = new Size((int)(120 * dpiScale), (int)(36 * dpiScale));

            var animState = new EnhancedButtonAnimationState
            {
                BaseColor = baseColor,
                IconText = iconText,
                IsActive = false
            };
            btn.Tag = animState;

            // Create rounded corners with custom region
            CreateChamferedRegion(btn, 12);
            ConfigureEnhancedHoverAnimation(btn);

            // Add custom paint for modern glass effect
            btn.Paint += EnhancedControlButton_Paint;
        }

        private void ConfigureEnhancedHoverAnimation(Button btn)
        {
            var animState = btn.Tag as EnhancedButtonAnimationState;

            btn.MouseEnter += (s, e) => {
                animState.IsHovering = true;
                animState.AnimationStart = DateTime.Now;
                btn.Invalidate();
            };

            btn.MouseLeave += (s, e) => {
                animState.IsHovering = false;
                animState.AnimationStart = DateTime.Now;
                btn.Invalidate();
            };

            btn.MouseDown += (s, e) => {
                animState.IsPressed = true;
                btn.Invalidate();
            };

            btn.MouseUp += (s, e) => {
                animState.IsPressed = false;
                btn.Invalidate();
            };
        }

        private void ConfigureNumericUpDown(NumericUpDown nud, int x, int y, int width)
        {
            nud.BackColor = Color.FromArgb(12, 12, 12);
            nud.BorderStyle = BorderStyle.None;
            nud.Font = ScaleFont(new Font("Segoe UI", 9F));
            nud.ForeColor = Color.Cyan;
            nud.Location = new Point(x, y);
            nud.Name = nud.Name;
            nud.Size = new Size(width, 23);
            nud.TabIndex = 1;
        }

        private void ConfigureComboBox(ComboBox cb, int x, int y, int width)
        {
            cb.BackColor = Color.FromArgb(12, 12, 12);
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.FlatStyle = FlatStyle.Flat;
            cb.Font = ScaleFont(new Font("Segoe UI", 9F));
            cb.ForeColor = Color.Cyan;
            cb.Location = new Point(x, y);
            cb.Name = cb.Name;
            cb.Size = new Size(width, 23);
            cb.TabIndex = 2;
        }

        private void ConfigureHoverAnimation(Control control)
        {
            var animState = control.Tag as ButtonAnimationState ?? new ButtonAnimationState();
            control.Tag = animState;

            control.MouseEnter += (s, e) => {
                animState.IsHovering = true;
                animState.AnimationStart = DateTime.Now;
            };

            control.MouseLeave += (s, e) => {
                animState.IsHovering = false;
                animState.AnimationStart = DateTime.Now;
            };
        }

        private void ConfigureGlowButton(Button btn)
        {
            ConfigureHoverAnimation(btn);

            btn.Paint += (s, e) => {
                var animState = btn.Tag as ButtonAnimationState;
                if (animState != null && animState.HoverProgress > 0)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    var glowAlpha = (int)(40 * animState.HoverProgress);
                    using (var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, btn.BackColor)))
                    {
                        for (int i = 1; i <= 2; i++)
                        {
                            var rect = new Rectangle(-i * 2, -i * 2, btn.Width + i * 4, btn.Height + i * 4);
                            e.Graphics.FillRectangle(glowBrush, rect);
                        }
                    }
                }
            };
        }

        private void CreateChamferedRegion(Control control, int chamfer)
        {
            void UpdateRegion()
            {
                if (control.Region != null) control.Region.Dispose();
                using var path = new GraphicsPath();
                var rect = control.ClientRectangle;
                // Ensure we don't cross if small
                int c = Math.Min(chamfer, Math.Min(rect.Width, rect.Height) / 2);
                
                path.AddLine(rect.Left + c, rect.Top, rect.Right - c, rect.Top);
                path.AddLine(rect.Right, rect.Top + c, rect.Right, rect.Bottom - c);
                path.AddLine(rect.Right - c, rect.Bottom, rect.Left + c, rect.Bottom);
                path.AddLine(rect.Left, rect.Bottom - c, rect.Left, rect.Top + c);
                path.CloseFigure();
                control.Region = new Region(path);
            }

            control.SizeChanged += (s, e) => UpdateRegion();
            UpdateRegion();
        }

        private void ConfigureAlienContainer(Panel panel)
        {
            CreateChamferedRegion(panel, 15);

            panel.Paint += (s, e) => {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;

                var rect = panel.ClientRectangle;
                // Fix: Do not shrink rect to avoid black borders/corners
                // rect.Width -= 1; 
                // rect.Height -= 1;

                // Alien Chamfer Style
                using var path = new GraphicsPath();
                int chamfer = 15;
                path.AddLine(rect.Left + chamfer, rect.Top, rect.Right - chamfer, rect.Top);
                path.AddLine(rect.Right, rect.Top + chamfer, rect.Right, rect.Bottom - chamfer);
                path.AddLine(rect.Right - chamfer, rect.Bottom, rect.Left + chamfer, rect.Bottom);
                path.AddLine(rect.Left, rect.Bottom - chamfer, rect.Left, rect.Top + chamfer);
                path.CloseFigure();
                
                // 1. Background (Dark)
                using (var brush = new SolidBrush(Color.FromArgb(10, 10, 10)))
                {
                    g.FillPath(brush, path);
                }

                // 2. Mesh Pattern
                using (var clip = new Region(path))
                {
                    g.SetClip(clip, CombineMode.Replace);
                    using (var pen = new Pen(Color.FromArgb(15, 255, 255, 255), 1))
                    {
                        for (int i = -rect.Height; i < rect.Width + rect.Height; i += 6)
                        {
                            g.DrawLine(pen, i, rect.Top, i + rect.Height, rect.Bottom);
                        }
                    }
                    g.ResetClip();
                }

                // 3. Border (Cyan/Teal)
                using (var pen = new Pen(Color.FromArgb(0, 100, 100), 1.5f))
                {
                    g.DrawPath(pen, path);
                }
                
                // 4. Corner Accents
                using (var pen = new Pen(Color.FromArgb(0, 255, 255), 2))
                {
                    int arm = 15;
                    g.DrawLine(pen, rect.Left, rect.Top + chamfer, rect.Left, rect.Top + chamfer + arm);
                    g.DrawLine(pen, rect.Right, rect.Bottom - chamfer, rect.Right, rect.Bottom - chamfer - arm);
                }
            };
        }



        private void CreateCircularRegion(Control control)
        {
            void UpdateRegion()
            {
                if (control.Region != null) control.Region.Dispose();
                using var path = new GraphicsPath();
                path.AddEllipse(0, 0, control.Width, control.Height);
                control.Region = new Region(path);
            }

            control.SizeChanged += (s, e) => UpdateRegion();
            UpdateRegion();
        }

        private void ConfigureUpdateButton()
        {
            // Scale-aware indicator sizing
            var dpiScale = this.DeviceDpi / 96f;
            var indicatorSize = (int)(8 * dpiScale);
            var indicatorMargin = (int)(18 * dpiScale);
            var indicatorTop = (int)(13 * dpiScale);
            
            statusIndicator.BackColor = Color.FromArgb(100, 100, 100);
            statusIndicator.Size = new Size(indicatorSize, indicatorSize);
            statusIndicator.Location = new Point(btnUpdate.ClientSize.Width - indicatorMargin, indicatorTop);
            statusIndicator.Name = "statusIndicator";
            statusIndicator.Enabled = false;
            statusIndicator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CreateCircularRegion(statusIndicator);
            btnUpdate.Controls.Add(statusIndicator);
            statusIndicator.BringToFront();

            statusIndicator.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = statusIndicator.ClientRectangle;
                rect.Inflate(-1, -1);

                using var brush = new SolidBrush(statusIndicator.BackColor);
                e.Graphics.FillEllipse(brush, rect);

                var mainForm = (Main)statusIndicator.FindForm();
                if (mainForm != null && mainForm.hasUpdate)
                {
                    var highlightRect = new Rectangle(1, 1, 3, 3);
                    using var highlightBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
                    e.Graphics.FillEllipse(highlightBrush, highlightRect);
                }
            };

            var updateTooltip = new ToolTip();
            updateTooltip.SetToolTip(btnUpdate, "Check for updates");
            btnUpdate.MouseEnter += (s, e) => {
                var mainForm = (Main)btnUpdate.FindForm();
                if (mainForm != null && mainForm.hasUpdate)
                {
                    updateTooltip.SetToolTip(btnUpdate, "Update available! Click to download.");
                }
                else
                {
                    updateTooltip.SetToolTip(btnUpdate, "No updates available");
                }
            };

            btnUpdate.Resize += (s, e) => {
                if (statusIndicator != null && btnUpdate.Controls.Contains(statusIndicator))
                {
                    var dpiScale = this.DeviceDpi / 96f;
                    var indicatorMargin = (int)(18 * dpiScale);
                    var indicatorTop = (int)(13 * dpiScale);
                    statusIndicator.Location = new Point(btnUpdate.ClientSize.Width - indicatorMargin, indicatorTop);
                }
            };

            btnUpdate.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                var animState = btnUpdate.Tag as ButtonAnimationState;

                if (animState != null && animState.HoverProgress > 0 && animState.IsHovering)
                {
                    using var glowBrush = new SolidBrush(Color.FromArgb((int)(20 * animState.HoverProgress), 102, 192, 244));
                    e.Graphics.FillRectangle(glowBrush, btnUpdate.ClientRectangle);
                }

                var iconColor = btnUpdate.ForeColor;
                if (animState != null && animState.HoverProgress > 0)
                {
                    iconColor = Color.FromArgb(
                        (int)(139 + (239 - 139) * animState.HoverProgress),
                        (int)(179 + (239 - 179) * animState.HoverProgress),
                        (int)(217 + (239 - 217) * animState.HoverProgress)
                    );
                }

                using var iconFont = new Font("Segoe MDL2 Assets", 11F);
                var iconText = "\uE895";

                using var iconBrush = new SolidBrush(iconColor);
                var iconSize = e.Graphics.MeasureString(iconText, iconFont);

                var iconX = 10;
                var iconY = (btnUpdate.Height - iconSize.Height) / 2;
                e.Graphics.DrawString(iconText, iconFont, iconBrush, iconX, iconY);

                using var textFont = ScaleFont(new Font("Segoe UI", 7.5F, FontStyle.Regular));
                var text = "CHECK FOR UPDATES";

                var textSize = e.Graphics.MeasureString(text, textFont);
                var textX = iconX + iconSize.Width + 5;
                var textY = (btnUpdate.Height - textSize.Height) / 2;
                e.Graphics.DrawString(text, textFont, iconBrush, textX, textY);

                var mainForm = (Main)btnUpdate.FindForm();
                if (mainForm != null && mainForm.hasUpdate && statusIndicator != null)
                {
                    var indicatorBounds = new Rectangle(
                        statusIndicator.Left - 2,
                        statusIndicator.Top - 2,
                        statusIndicator.Width + 4,
                        statusIndicator.Height + 4
                    );

                    for (int i = 2; i > 0; i--)
                    {
                        var glowAlpha = 15 / i; // Animation removed
                        using var glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, 102, 192, 244));
                        var glowRect = new Rectangle(
                            indicatorBounds.X - i * 2,
                            indicatorBounds.Y - i * 2,
                            indicatorBounds.Width + i * 4,
                            indicatorBounds.Height + i * 4
                        );
                        e.Graphics.FillEllipse(glowBrush, glowRect);
                    }
                }
            };
        }

        #endregion

        #region Paint Event Handlers

        private void LogoPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var rect = logoPanel.ClientRectangle;

            // Draw Alienware-style background
            using (var bgPath = new GraphicsPath())
            {
                int chamfer = 15;
                bgPath.AddLine(rect.Left + chamfer, rect.Top, rect.Right - chamfer, rect.Top);
                bgPath.AddLine(rect.Right, rect.Top + chamfer, rect.Right, rect.Bottom - chamfer);
                bgPath.AddLine(rect.Right - chamfer, rect.Bottom, rect.Left + chamfer, rect.Bottom);
                bgPath.AddLine(rect.Left, rect.Bottom - chamfer, rect.Left, rect.Top + chamfer);
                bgPath.CloseFigure();

                using (var pgBrush = new PathGradientBrush(bgPath))
                {
                    pgBrush.CenterColor = SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.SurfaceColor;
                    pgBrush.SurroundColors = new[] { SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.BackColor };
                    pgBrush.FocusScales = new PointF(0.8f, 0.5f);
                    e.Graphics.FillPath(pgBrush, bgPath);
                }
            }

            // Draw cyan accent lines
            using (var pen = new Pen(Color.FromArgb(40, SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan), 1))
            {
                e.Graphics.DrawLine(pen, 0, 0, rect.Width, 0);
                e.Graphics.DrawLine(pen, 0, rect.Height - 1, rect.Width, rect.Height - 1);
            }

            // Draw main logo text with Alienware effect (static, no animation)
            DrawStaticMetallicText(e.Graphics, rect);
        }

        private void DrawStaticMetallicText(Graphics g, Rectangle rect)
        {
            using var font = ScaleFont(new Font("Segoe UI", 14F, FontStyle.Bold));
            var text = "POKÉBOT";
            var textSize = g.MeasureString(text, font);
            var x = (rect.Width - textSize.Width) / 2;
            var y = (rect.Height - textSize.Height) / 2;

            // Create Alienware gradient
            var textRect = new RectangleF(x, y, textSize.Width, textSize.Height);
            using (var metalBrush = new LinearGradientBrush(
                textRect,
                SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan,
                Color.FromArgb(255, 0, 100, 150),   // Dark Blue
                LinearGradientMode.Vertical))
            {
                metalBrush.SetBlendTriangularShape(0.5f);

                // Shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(80, SysBot.Pokemon.WinForms.Helpers.WinFormsTheme.AccentCyan)))
                {
                    g.DrawString(text, font, shadowBrush, x + 2, y + 2);
                }

                // Main text (static, no glow effects)
                g.DrawString(text, font, metalBrush, x, y);
            }
        }


        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            // Alienware-style bottom border
            using var pen = new Pen(Color.FromArgb(0, 204, 255), 1);
            e.Graphics.DrawLine(pen, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
        }

        private void EnhancedControlButton_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;
            
            var animState = btn.Tag as EnhancedButtonAnimationState;
            if (animState == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = btn.ClientRectangle;
            var drawRect = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            
            // Create chamfered rectangle path
            using var path = new GraphicsPath();
            int chamfer = 12;
            path.AddLine(drawRect.Left + chamfer, drawRect.Top, drawRect.Right - chamfer, drawRect.Top);
            path.AddLine(drawRect.Right, drawRect.Top + chamfer, drawRect.Right, drawRect.Bottom - chamfer);
            path.AddLine(drawRect.Right - chamfer, drawRect.Bottom, drawRect.Left + chamfer, drawRect.Bottom);
            path.AddLine(drawRect.Left, drawRect.Bottom - chamfer, drawRect.Left, drawRect.Top + chamfer);
            path.CloseFigure();
            
            // Clip region for click detection
            // btn.Region = new Region(path); // Fix: Remove Region clipping for smoother edges

            // 1. Background (Dark)
            using (var bgBrush = new SolidBrush(Color.FromArgb(15, 15, 15)))
            {
                g.FillPath(bgBrush, path);
            }

            // 2. Hover/Press Effect
            if (animState.IsHovering || animState.IsPressed)
            {
                int alpha = animState.IsPressed ? 50 : 30;
                using (var glowBrush = new SolidBrush(Color.FromArgb(alpha, animState.BaseColor)))
                {
                    g.FillPath(glowBrush, path);
                }
            }

            // 3. Border (Full Neon Color)
            using (var pen = new Pen(animState.BaseColor, 2))
            {
                g.DrawPath(pen, path);
            }

            // 4. Text
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            TextRenderer.DrawText(g, btn.Text, btn.Font, drawRect, animState.BaseColor, flags);
        }

        private void FLP_Bots_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // Alienware Rack Container
            var rect = FLP_Bots.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            // 1. Grid Background (Tech Mesh)
            using (var brush = new SolidBrush(Color.FromArgb(10, 200, 200, 200)))
            {
                // Draw faint horizontal lines for rack slots
                using (var pen = new Pen(Color.FromArgb(5, 200, 200, 200)))
                {
                    for (int i = 0; i < rect.Height; i += 40)
                    {
                        g.DrawLine(pen, rect.Left, i, rect.Right, i);
                    }
                }
            }

            // 2. Container Border (Tech Frame)
            using (var path = new GraphicsPath())
            {
                int cornerSize = 20;
                using (var pen = new Pen(Color.FromArgb(60, 60, 60), 2))
                {
                    // Top Left Bracket
                    g.DrawLine(pen, rect.Left, rect.Top + cornerSize, rect.Left, rect.Top);
                    g.DrawLine(pen, rect.Left, rect.Top, rect.Left + cornerSize, rect.Top);

                    // Top Right Bracket
                    g.DrawLine(pen, rect.Right, rect.Top + cornerSize, rect.Right, rect.Top);
                    g.DrawLine(pen, rect.Right, rect.Top, rect.Right - cornerSize, rect.Top);

                    // Bottom Left Bracket
                    g.DrawLine(pen, rect.Left, rect.Bottom - cornerSize, rect.Left, rect.Bottom);
                    g.DrawLine(pen, rect.Left, rect.Bottom, rect.Left + cornerSize, rect.Bottom);

                    // Bottom Right Bracket
                    g.DrawLine(pen, rect.Right, rect.Bottom - cornerSize, rect.Right, rect.Bottom);
                    g.DrawLine(pen, rect.Right, rect.Bottom, rect.Right - cornerSize, rect.Bottom);
                }
            }

            if (_currentModeImage != null && FLP_Bots.Controls.Count == 0)
            {
                var image = _currentModeImage;
                var panelWidth = FLP_Bots.ClientSize.Width;
                var panelHeight = FLP_Bots.ClientSize.Height;

                float scale = 0.35f;
                int imageWidth = (int)(image.Width * scale);
                int imageHeight = (int)(image.Height * scale);

                int x = (panelWidth - imageWidth) / 2;
                int y = 30;

                using (var attributes = new ImageAttributes())
                {
                    float[][] matrixItems = {
                        new float[] {1, 0, 0, 0, 0},
                        new float[] {0, 1, 0, 0, 0},
                        new float[] {0, 0, 1, 0, 0},
                        new float[] {0, 0, 0, 0.1f, 0},
                        new float[] {0, 0, 0, 0, 1}
                    };
                    var colorMatrix = new ColorMatrix(matrixItems);
                    attributes.SetColorMatrix(colorMatrix);

                    g.DrawImage(image,
                        new Rectangle(x, y, imageWidth, imageHeight),
                        0, 0, image.Width, image.Height,
                        GraphicsUnit.Pixel, attributes);
                }

                using var font = ScaleFont(new Font("Segoe UI", 11F, FontStyle.Regular));
                using var brush = new SolidBrush(Color.FromArgb(139, 179, 217));
                var text = "No bots configured. Add a bot using the form above.";
                var size = g.MeasureString(text, font);
                g.DrawString(text, font, brush,
                    (panelWidth - size.Width) / 2,
                    y + imageHeight + 10);
            }
        }

        private void TransitionPanels(int index)
        {
            // Ensure proper panel layout before transitioning
            contentPanel.SuspendLayout();
            
            // Determine target panel
            Panel targetPanel = index switch
            {
                0 => botsPanel,
                1 => hubPanel,
                2 => logsPanel,
                3 => devPanel,
                _ => botsPanel
            };

            // Optimization: Suspend layout for bot flow panel when switching to it
            if (targetPanel == botsPanel)
            {
                FLP_Bots.SuspendLayout();
            }

            // Fix z-order to ensure headerPanel is on top
            contentPanel.Controls.SetChildIndex(headerPanel, contentPanel.Controls.Count - 1);
            
            // Reset and reapply header docking
            headerPanel.Dock = DockStyle.None;
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            
            // Show target panel first to avoid flickering
            targetPanel.Visible = true;
            targetPanel.Dock = DockStyle.None;
            targetPanel.Dock = DockStyle.Fill;
            targetPanel.BringToFront();

            // Hide other panels
            if (botsPanel != targetPanel) botsPanel.Visible = false;
            if (hubPanel != targetPanel) hubPanel.Visible = false;
            if (logsPanel != targetPanel) logsPanel.Visible = false;
            if (devPanel != targetPanel) devPanel.Visible = false;

            // Toggle Start/Stop/Reboot buttons (Only visible in Bots section)
            if (controlButtonsPanel != null)
            {
                controlButtonsPanel.Visible = (index == 0);
            }
            
            contentPanel.ResumeLayout(true);
            contentPanel.PerformLayout();

            // Optimization: Resume layout for bot flow panel
            if (index == 0)
            {
                FLP_Bots.ResumeLayout(true);
            }
            // Fix for PropertyGrid text duplication/glitches
            else if (index == 1)
            {
                PG_Hub.Refresh();
            }
        }

        #endregion

        #region System Tray

        private void ConfigureSystemTray()
        {
            trayIcon.Icon = Icon;
            trayIcon.Text = "PokéBot Control Center";
            trayIcon.Visible = false;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;

            trayContextMenu.BackColor = Color.FromArgb(12, 12, 12);
            trayContextMenu.Font = ScaleFont(new Font("Segoe UI", 9F));
            trayContextMenu.Renderer = new CuztomMenuRenderer();

            trayMenuShow.Text = "Show Window";
            trayMenuShow.ForeColor = Color.FromArgb(239, 239, 239);
            trayMenuShow.Click += TrayMenuShow_Click;

            var separator = new ToolStripSeparator();

            var trayMenuStart = new ToolStripMenuItem("Start All Bots");
            trayMenuStart.ForeColor = Color.FromArgb(90, 186, 71);
            trayMenuStart.Click += (s, e) => {
                RunningEnvironment.InitializeStart();
                foreach (var c in FLP_Bots.Controls.OfType<BotController>())
                    c.SendCommand(BotControlCommand.Start);
                LogUtil.LogInfo("All bots started from tray", "Tray");
            };

            var trayMenuStop = new ToolStripMenuItem("Stop All Bots");
            trayMenuStop.ForeColor = Color.FromArgb(236, 98, 95);
            trayMenuStop.Click += (s, e) => {
                RunningEnvironment.StopAll();
                LogUtil.LogInfo("All bots stopped from tray", "Tray");
            };

            var separator2 = new ToolStripSeparator();

            trayMenuExit.Text = "Exit";
            trayMenuExit.ForeColor = Color.FromArgb(236, 98, 95);
            trayMenuExit.Click += TrayMenuExit_Click;

            trayContextMenu.Items.AddRange(new ToolStripItem[] {
                trayMenuShow,
                separator,
                trayMenuStart,
                trayMenuStop,
                separator2,
                trayMenuExit
            });
            trayIcon.ContextMenuStrip = trayContextMenu;
        }

        #endregion

        #region Custom Classes

        private class CuztomMenuRenderer : ToolStripProfessionalRenderer
        {
            public CuztomMenuRenderer() : base(new CuztomColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var rc = new Rectangle(Point.Empty, e.Item.Size);
                var c = e.Item.Selected ? Color.FromArgb(20, 20, 20) : Color.FromArgb(12, 12, 12);
                using (var brush = new SolidBrush(c))
                    e.Graphics.FillRectangle(brush, rc);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? e.Item.ForeColor : Color.Gray;
                base.OnRenderItemText(e);
            }
        }

        private class CuztomColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(20, 20, 20);
            public override Color MenuItemBorder => Color.FromArgb(0, 204, 255);
            public override Color MenuBorder => Color.FromArgb(64, 64, 64);
            public override Color ToolStripDropDownBackground => Color.FromArgb(12, 12, 12);
            public override Color ImageMarginGradientBegin => Color.FromArgb(12, 12, 12);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(12, 12, 12);
            public override Color ImageMarginGradientEnd => Color.FromArgb(12, 12, 12);
            public override Color SeparatorDark => Color.FromArgb(64, 64, 64);
            public override Color SeparatorLight => Color.FromArgb(32, 32, 32);
        }

        private class ButtonAnimationState
        {
            public bool IsHovering { get; set; }
            public DateTime AnimationStart { get; set; }
            public double HoverProgress { get; set; }
            public Color BaseColor { get; set; }
        }

        private class EnhancedButtonAnimationState
        {
            public bool IsHovering { get; set; }
            public bool IsPressed { get; set; }
            public bool IsActive { get; set; }
            public DateTime AnimationStart { get; set; }
            public float HoverProgress { get; set; }
            public float PulsePhase { get; set; }
            public float PulseIntensity { get; set; }
            public Color BaseColor { get; set; }
            public string IconText { get; set; } = "";
        }

        private class NavButtonState : ButtonAnimationState
        {
            public Color NeonColor { get; set; }
            public bool IsSelected { get; set; }
            public int Index { get; set; }
        }

        #endregion

        #region Controls Declaration

        private TableLayoutPanel mainLayoutPanel;
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Panel headerPanel;
        private Panel logoPanel;
        private FlowLayoutPanel navButtonsPanel;
        private Button btnNavBots;
        private Button btnNavHub;
        private Button btnNavLogs;
        private Panel sidebarBottomPanel;
        private Button btnUpdate;
        private Label titleLabel;
        private FlowLayoutPanel controlButtonsPanel;
        private Button btnStart;
        private Button btnStop;
        private Button btnReboot;
        private Panel botsPanel;
        private Panel hubPanel;
        private Panel logsPanel;
        private Panel botHeaderPanel;
        private Panel addBotPanel;
        private TextBox TB_IP;
        private NumericUpDown NUD_Port;
        private ComboBox CB_Protocol;
        private ComboBox CB_Routine;
        private Button B_New;
        private FlowLayoutPanel FLP_Bots;
        private PropertyGrid PG_Hub;
        private RichTextBox RTB_Logs;
        private Panel logsHeaderPanel;
        private Panel searchPanel;
        private TextBox logSearchBox;
        private FlowLayoutPanel searchOptionsPanel;
        private CheckBox btnCaseSensitive;
        private CheckBox btnRegex;
        private CheckBox btnWholeWord;
        private Button btnClearLogs;
        private Label searchStatusLabel;
        private Button btnExportLogs;
        private Button btnAutoScroll;
        private Panel statusIndicator;
        // Animation timer removed
        private ComboBox comboBox1;

        private System.Windows.Forms.Button btnNavDev;
        private System.Windows.Forms.Panel devPanel;
        private System.Windows.Forms.Label lblDevTitle;
        private System.Windows.Forms.Panel pnlDevConnection;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btnDevConnect;
        private System.Windows.Forms.Label lblConnStatus;
        private System.Windows.Forms.Panel pnlDevScanner;
        private System.Windows.Forms.TextBox txtPattern;
        private System.Windows.Forms.Label lblPattern;
        private System.Windows.Forms.ComboBox cbRegion;
        private System.Windows.Forms.Label lblRegion;
        private System.Windows.Forms.TextBox txtStart;
        private System.Windows.Forms.Label lblStart;
        private System.Windows.Forms.TextBox txtLength;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.RichTextBox rtbResults;
        private System.Windows.Forms.Label lblScanStatus;

        private System.Windows.Forms.ComboBox cbGameVersion;
        private System.Windows.Forms.TextBox txtSigOffset;
        private System.Windows.Forms.Button btnFindSig;
        private System.Windows.Forms.Button btnDumpMain;
        private System.Windows.Forms.Button btnAutoUpdate;
        private System.Windows.Forms.Button btnFindChain;
        private System.Windows.Forms.Button btnAutoScan;
        private System.Windows.Forms.Button btnVerify;

        private System.Windows.Forms.Panel pnlDevMonitor;
        private System.Windows.Forms.Label lblMonitorAddr;
        private System.Windows.Forms.TextBox txtMonitorAddr;
        private System.Windows.Forms.Button btnMonitorToggle;
        private System.Windows.Forms.Label lblMonitorValue;
        private System.Windows.Forms.TextBox txtMonitorValue;
        private System.Windows.Forms.Label lblPointerInfo;
        private System.Windows.Forms.TextBox txtPointerInfo;
        private System.Windows.Forms.Label lblLengthVal;
        private System.Windows.Forms.NumericUpDown numLength;
        private System.Windows.Forms.CheckBox chkCachePointer;
        private System.Windows.Forms.Button btnCopyAddress;
        private System.Windows.Forms.Timer monitorTimer;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayContextMenu;
        private ToolStripMenuItem trayMenuShow;
        private ToolStripMenuItem trayMenuExit;

        private Button updater => btnUpdate;
        private Button B_Start => btnStart;
        private Button B_Stop => btnStop;
        private Button B_RebootStop => btnReboot;
        private TabControl TC_Main;
        private TabPage Tab_Bots;
        private TabPage Tab_Hub;
        private TabPage Tab_Logs;
        private Panel ButtonPanel => controlButtonsPanel;

        #endregion
    }

    public static class GraphicsExtensions
    {
        public static void AddRoundedRectangle(this GraphicsPath path, Rectangle rect, int radius)
        {
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
        }
    }
}

#pragma warning restore CS8618
#pragma warning restore CS8625
#pragma warning restore CS8669
