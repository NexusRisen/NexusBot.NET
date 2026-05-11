using SysBot.Pokemon.WinForms.Properties;

namespace SysBot.Pokemon.WinForms
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            P_Sidebar = new Panel();
            B_HideTray = new Button();
            B_Credits = new Button();
            B_NavLogs = new Button();
            B_NavHub = new Button();
            B_NavBots = new Button();
            P_LangArea = new Panel();
            CB_Language = new ComboBox();
            L_Language = new Label();
            P_LogoArea = new Panel();
            PB_LogoSidebar = new PictureBox();
            P_Header = new Panel();
            L_Title = new Label();
            B_Update = new Button();
            B_Restart = new Button();
            B_Stop = new Button();
            B_Start = new Button();
            P_Bottom = new Panel();
            L_Version = new Label();
            PB_Logo = new PictureBox();
            TC_Main = new TabControl();
            Tab_Bots = new TabPage();
            FLP_Bots = new FlowLayoutPanel();
            P_AddBot = new Panel();
            B_New = new Button();
            TB_IP = new TextBox();
            NUD_Port = new NumericUpDown();
            CB_Protocol = new ComboBox();
            CB_Routine = new ComboBox();
            CB_Mode = new ComboBox();
            CB_Theme = new ComboBox();
            Tab_Hub = new TabPage();
            PG_Hub = new PropertyGrid();
            Tab_Logs = new TabPage();
            RTB_Logs = new RichTextBox();
            Tab_Credits = new TabPage();
            P_CreditsContainer = new Panel();
            L_CreditsSpecialNames = new Label();
            L_CreditsSpecialTitle = new Label();
            L_CreditsContribNames = new Label();
            L_CreditsContribTitle = new Label();
            L_CreditsDevName = new Label();
            L_CreditsDevTitle = new Label();
            L_CreditsMainTitle = new Label();
            L_CreditsOwnersTitle = new Label();
            L_CreditsOwnersNames = new Label();
            PB_CreditsLogo = new PictureBox();
            P_Sidebar.SuspendLayout();
            P_LangArea.SuspendLayout();
            P_LogoArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PB_LogoSidebar).BeginInit();
            P_Header.SuspendLayout();
            P_Bottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PB_Logo).BeginInit();
            TC_Main.SuspendLayout();
            Tab_Bots.SuspendLayout();
            P_AddBot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NUD_Port).BeginInit();
            Tab_Hub.SuspendLayout();
            Tab_Logs.SuspendLayout();
            Tab_Credits.SuspendLayout();
            P_CreditsContainer.SuspendLayout();
            SuspendLayout();
            // 
            // trayIcon
            // 
            trayIcon = new NotifyIcon(components);
            trayMenu = new ContextMenuStrip(components);
            trayRestore = new ToolStripMenuItem();
            trayExit = new ToolStripMenuItem();
            trayMenu.SuspendLayout();
            // 
            // trayIcon
            // 
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Icon = Resources.icon;
            trayIcon.Text = "DudeBot.NET";
            trayIcon.MouseDoubleClick += trayIcon_MouseDoubleClick;
            // 
            // trayMenu
            // 
            trayMenu.Items.AddRange(new ToolStripItem[] { trayRestore, trayExit });
            trayMenu.Name = "trayMenu";
            trayMenu.Size = new Size(114, 48);
            // 
            // trayRestore
            // 
            trayRestore.Name = "trayRestore";
            trayRestore.Size = new Size(113, 22);
            trayRestore.Text = "Restore";
            trayRestore.Click += trayRestore_Click;
            // 
            // trayExit
            // 
            trayExit.Name = "trayExit";
            trayExit.Size = new Size(113, 22);
            trayExit.Text = "Exit";
            trayExit.Click += trayExit_Click;
            // 
            // P_Sidebar
            // 
            P_Sidebar.Controls.Add(B_HideTray);
            P_Sidebar.Controls.Add(B_Credits);
            P_Sidebar.Controls.Add(B_NavLogs);
            P_Sidebar.Controls.Add(B_NavHub);
            P_Sidebar.Controls.Add(B_NavBots);
            P_Sidebar.Controls.Add(P_LangArea);
            P_Sidebar.Controls.Add(P_LogoArea);
            P_Sidebar.Dock = DockStyle.Left;
            P_Sidebar.Location = new Point(0, 0);
            P_Sidebar.Name = "P_Sidebar";
            P_Sidebar.Size = new Size(180, 533);
            P_Sidebar.TabIndex = 0;
            // 
            // B_HideTray
            // 
            B_HideTray.Dock = DockStyle.Top;
            B_HideTray.FlatAppearance.BorderSize = 0;
            B_HideTray.FlatStyle = FlatStyle.Flat;
            B_HideTray.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            B_HideTray.ForeColor = Color.Gainsboro;
            B_HideTray.ImageAlign = ContentAlignment.MiddleLeft;
            B_HideTray.Location = new Point(0, 335);
            B_HideTray.Name = "B_HideTray";
            B_HideTray.Padding = new Padding(15, 0, 0, 0);
            B_HideTray.Size = new Size(180, 50);
            B_HideTray.TabIndex = 4;
            B_HideTray.Text = "  Hide to Tray";
            B_HideTray.TextAlign = ContentAlignment.MiddleLeft;
            B_HideTray.TextImageRelation = TextImageRelation.ImageBeforeText;
            B_HideTray.UseVisualStyleBackColor = true;
            B_HideTray.Click += B_HideTray_Click;
            // 
            // B_Credits
            // 
            B_Credits.Dock = DockStyle.Top;
            B_Credits.FlatAppearance.BorderSize = 0;
            B_Credits.FlatStyle = FlatStyle.Flat;
            B_Credits.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            B_Credits.ForeColor = Color.Gainsboro;
            B_Credits.ImageAlign = ContentAlignment.MiddleLeft;
            B_Credits.Location = new Point(0, 285);
            B_Credits.Name = "B_Credits";
            B_Credits.Padding = new Padding(15, 0, 0, 0);
            B_Credits.Size = new Size(180, 50);
            B_Credits.TabIndex = 5;
            B_Credits.Text = "  Credits";
            B_Credits.TextAlign = ContentAlignment.MiddleLeft;
            B_Credits.TextImageRelation = TextImageRelation.ImageBeforeText;
            B_Credits.UseVisualStyleBackColor = true;
            B_Credits.Click += B_Credits_Click;
            // 
            // B_NavLogs
            // 
            B_NavLogs.Dock = DockStyle.Top;
            B_NavLogs.FlatAppearance.BorderSize = 0;
            B_NavLogs.FlatStyle = FlatStyle.Flat;
            B_NavLogs.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            B_NavLogs.ForeColor = Color.Gainsboro;
            B_NavLogs.Image = Resources.stop;
            B_NavLogs.ImageAlign = ContentAlignment.MiddleLeft;
            B_NavLogs.Location = new Point(0, 235);
            B_NavLogs.Name = "B_NavLogs";
            B_NavLogs.Padding = new Padding(15, 0, 0, 0);
            B_NavLogs.Size = new Size(180, 50);
            B_NavLogs.TabIndex = 3;
            B_NavLogs.Text = "  Logs";
            B_NavLogs.TextAlign = ContentAlignment.MiddleLeft;
            B_NavLogs.TextImageRelation = TextImageRelation.ImageBeforeText;
            B_NavLogs.UseVisualStyleBackColor = true;
            B_NavLogs.Click += B_NavLogs_Click;
            // 
            // B_NavHub
            // 
            B_NavHub.Dock = DockStyle.Top;
            B_NavHub.FlatAppearance.BorderSize = 0;
            B_NavHub.FlatStyle = FlatStyle.Flat;
            B_NavHub.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            B_NavHub.ForeColor = Color.Gainsboro;
            B_NavHub.Image = Resources.refresh;
            B_NavHub.ImageAlign = ContentAlignment.MiddleLeft;
            B_NavHub.Location = new Point(0, 185);
            B_NavHub.Name = "B_NavHub";
            B_NavHub.Padding = new Padding(15, 0, 0, 0);
            B_NavHub.Size = new Size(180, 50);
            B_NavHub.TabIndex = 2;
            B_NavHub.Text = "  Settings";
            B_NavHub.TextAlign = ContentAlignment.MiddleLeft;
            B_NavHub.TextImageRelation = TextImageRelation.ImageBeforeText;
            B_NavHub.UseVisualStyleBackColor = true;
            B_NavHub.Click += B_NavHub_Click;
            // 
            // B_NavBots
            // 
            B_NavBots.Dock = DockStyle.Top;
            B_NavBots.FlatAppearance.BorderSize = 0;
            B_NavBots.FlatStyle = FlatStyle.Flat;
            B_NavBots.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            B_NavBots.ForeColor = Color.Gainsboro;
            B_NavBots.Image = Resources.start;
            B_NavBots.ImageAlign = ContentAlignment.MiddleLeft;
            B_NavBots.Location = new Point(0, 135);
            B_NavBots.Name = "B_NavBots";
            B_NavBots.Padding = new Padding(15, 0, 0, 0);
            B_NavBots.Size = new Size(180, 50);
            B_NavBots.TabIndex = 1;
            B_NavBots.Text = "  Bots";
            B_NavBots.TextAlign = ContentAlignment.MiddleLeft;
            B_NavBots.TextImageRelation = TextImageRelation.ImageBeforeText;
            B_NavBots.UseVisualStyleBackColor = true;
            B_NavBots.Click += B_NavBots_Click;
            // 
            // P_LangArea
            // 
            P_LangArea.Controls.Add(CB_Language);
            P_LangArea.Controls.Add(L_Language);
            P_LangArea.Dock = DockStyle.Top;
            P_LangArea.Location = new Point(0, 70);
            P_LangArea.Name = "P_LangArea";
            P_LangArea.Size = new Size(180, 65);
            P_LangArea.TabIndex = 6;
            // 
            // CB_Language
            // 
            CB_Language.DropDownStyle = ComboBoxStyle.DropDownList;
            CB_Language.FlatStyle = FlatStyle.Flat;
            CB_Language.FormattingEnabled = true;
            CB_Language.Location = new Point(15, 30);
            CB_Language.Name = "CB_Language";
            CB_Language.Size = new Size(150, 23);
            CB_Language.TabIndex = 1;
            CB_Language.SelectedIndexChanged += CB_Language_SelectedIndexChanged;
            // 
            // L_Language
            // 
            L_Language.AutoSize = true;
            L_Language.ForeColor = Color.Gainsboro;
            L_Language.Location = new Point(15, 10);
            L_Language.Name = "L_Language";
            L_Language.Size = new Size(62, 15);
            L_Language.TabIndex = 0;
            L_Language.Text = "Language:";
            // 
            // P_LogoArea
            // 
            P_LogoArea.Controls.Add(PB_LogoSidebar);
            P_LogoArea.Dock = DockStyle.Top;
            P_LogoArea.Location = new Point(0, 0);
            P_LogoArea.Name = "P_LogoArea";
            P_LogoArea.Size = new Size(180, 70);
            P_LogoArea.TabIndex = 0;
            // 
            // PB_LogoSidebar
            // 
            PB_LogoSidebar.Image = Resources.icon.ToBitmap();
            PB_LogoSidebar.Location = new Point(65, 10);
            PB_LogoSidebar.Name = "PB_LogoSidebar";
            PB_LogoSidebar.Size = new Size(50, 50);
            PB_LogoSidebar.SizeMode = PictureBoxSizeMode.Zoom;
            PB_LogoSidebar.TabIndex = 0;
            PB_LogoSidebar.TabStop = false;
            // 
            // P_Header
            // 
            P_Header.Controls.Add(L_Title);
            P_Header.Controls.Add(B_Update);
            P_Header.Controls.Add(B_Restart);
            P_Header.Controls.Add(B_Stop);
            P_Header.Controls.Add(B_Start);
            P_Header.Dock = DockStyle.Top;
            P_Header.Location = new Point(180, 0);
            P_Header.Name = "P_Header";
            P_Header.Size = new Size(670, 65);
            P_Header.TabIndex = 1;
            // 
            // L_Title
            // 
            L_Title.AutoSize = true;
            L_Title.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            L_Title.ForeColor = Color.White;
            L_Title.Location = new Point(25, 17);
            L_Title.Name = "L_Title";
            L_Title.Size = new Size(66, 30);
            L_Title.TabIndex = 0;
            L_Title.Text = "BOTS";
            // 
            // B_Update
            // 
            B_Update.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            B_Update.FlatStyle = FlatStyle.Flat;
            B_Update.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            B_Update.Image = Resources.update;
            B_Update.ImageAlign = ContentAlignment.MiddleLeft;
            B_Update.Location = new Point(555, 18);
            B_Update.Name = "B_Update";
            B_Update.Size = new Size(105, 30);
            B_Update.TabIndex = 4;
            B_Update.Text = "Update";
            B_Update.TextAlign = ContentAlignment.MiddleRight;
            B_Update.UseVisualStyleBackColor = true;
            B_Update.Click += B_Update_Click;
            // 
            // B_Restart
            // 
            B_Restart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            B_Restart.FlatStyle = FlatStyle.Flat;
            B_Restart.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            B_Restart.Image = Resources.refresh;
            B_Restart.ImageAlign = ContentAlignment.MiddleLeft;
            B_Restart.Location = new Point(445, 18);
            B_Restart.Name = "B_Restart";
            B_Restart.Size = new Size(105, 30);
            B_Restart.TabIndex = 3;
            B_Restart.Text = "Restart All";
            B_Restart.TextAlign = ContentAlignment.MiddleRight;
            B_Restart.UseVisualStyleBackColor = true;
            B_Restart.Click += B_Restart_Click;
            // 
            // B_Stop
            // 
            B_Stop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            B_Stop.FlatStyle = FlatStyle.Flat;
            B_Stop.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            B_Stop.Image = Resources.stop;
            B_Stop.ImageAlign = ContentAlignment.MiddleLeft;
            B_Stop.Location = new Point(335, 18);
            B_Stop.Name = "B_Stop";
            B_Stop.Size = new Size(105, 30);
            B_Stop.TabIndex = 2;
            B_Stop.Text = "Stop All";
            B_Stop.TextAlign = ContentAlignment.MiddleRight;
            B_Stop.UseVisualStyleBackColor = true;
            B_Stop.Click += B_Stop_Click;
            // 
            // B_Start
            // 
            B_Start.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            B_Start.FlatStyle = FlatStyle.Flat;
            B_Start.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            B_Start.Image = Resources.start;
            B_Start.ImageAlign = ContentAlignment.MiddleLeft;
            B_Start.Location = new Point(225, 18);
            B_Start.Name = "B_Start";
            B_Start.Size = new Size(105, 30);
            B_Start.TabIndex = 1;
            B_Start.Text = "Start All";
            B_Start.TextAlign = ContentAlignment.MiddleRight;
            B_Start.UseVisualStyleBackColor = true;
            B_Start.Click += B_Start_Click;
            // 
            // P_Bottom
            // 
            P_Bottom.Controls.Add(L_Version);
            P_Bottom.Controls.Add(PB_Logo);
            P_Bottom.Dock = DockStyle.Bottom;
            P_Bottom.Location = new Point(0, 533);
            P_Bottom.Name = "P_Bottom";
            P_Bottom.Size = new Size(850, 28);
            P_Bottom.TabIndex = 2;
            // 
            // L_Version
            // 
            L_Version.AutoSize = true;
            L_Version.Location = new Point(2, 6);
            L_Version.Name = "L_Version";
            L_Version.Size = new Size(82, 15);
            L_Version.TabIndex = 0;
            L_Version.Text = "DudeBot.NET";
            // 
            // PB_Logo
            // 
            PB_Logo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            PB_Logo.Image = Resources.icon.ToBitmap();
            PB_Logo.Location = new Point(820, 2);
            PB_Logo.Name = "PB_Logo";
            PB_Logo.Size = new Size(24, 24);
            PB_Logo.SizeMode = PictureBoxSizeMode.Zoom;
            PB_Logo.TabIndex = 1;
            PB_Logo.TabStop = false;
            // 
            // TC_Main
            // 
            TC_Main.Appearance = TabAppearance.FlatButtons;
            TC_Main.Controls.Add(Tab_Bots);
            TC_Main.Controls.Add(Tab_Hub);
            TC_Main.Controls.Add(Tab_Logs);
            TC_Main.Controls.Add(Tab_Credits);
            TC_Main.Dock = DockStyle.Fill;
            TC_Main.ItemSize = new Size(0, 1);
            TC_Main.Location = new Point(180, 65);
            TC_Main.Name = "TC_Main";
            TC_Main.SelectedIndex = 0;
            TC_Main.Size = new Size(670, 468);
            TC_Main.SizeMode = TabSizeMode.Fixed;
            TC_Main.TabIndex = 3;
            // 
            // Tab_Bots
            // 
            Tab_Bots.Controls.Add(FLP_Bots);
            Tab_Bots.Controls.Add(P_AddBot);
            Tab_Bots.Location = new Point(4, 5);
            Tab_Bots.Name = "Tab_Bots";
            Tab_Bots.Size = new Size(662, 459);
            Tab_Bots.TabIndex = 0;
            Tab_Bots.Text = "Bots";
            Tab_Bots.UseVisualStyleBackColor = true;
            // 
            // FLP_Bots
            // 
            FLP_Bots.AutoScroll = true;
            FLP_Bots.BorderStyle = BorderStyle.None;
            FLP_Bots.Dock = DockStyle.Fill;
            FLP_Bots.Location = new Point(0, 50);
            FLP_Bots.Name = "FLP_Bots";
            FLP_Bots.Size = new Size(662, 409);
            FLP_Bots.TabIndex = 1;
            FLP_Bots.Resize += FLP_Bots_Resize;
            // 
            // P_AddBot
            // 
            P_AddBot.Controls.Add(B_New);
            P_AddBot.Controls.Add(TB_IP);
            P_AddBot.Controls.Add(NUD_Port);
            P_AddBot.Controls.Add(CB_Protocol);
            P_AddBot.Controls.Add(CB_Routine);
            P_AddBot.Controls.Add(CB_Mode);
            P_AddBot.Controls.Add(CB_Theme);
            P_AddBot.Dock = DockStyle.Top;
            P_AddBot.Location = new Point(0, 0);
            P_AddBot.Name = "P_AddBot";
            P_AddBot.Size = new Size(662, 50);
            P_AddBot.TabIndex = 0;
            // 
            // B_New
            // 
            B_New.FlatStyle = FlatStyle.Flat;
            B_New.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            B_New.Image = Resources.add;
            B_New.ImageAlign = ContentAlignment.MiddleLeft;
            B_New.Location = new Point(6, 10);
            B_New.Name = "B_New";
            B_New.Size = new Size(70, 30);
            B_New.TabIndex = 0;
            B_New.Text = "Add";
            B_New.TextAlign = ContentAlignment.MiddleRight;
            B_New.UseVisualStyleBackColor = true;
            B_New.Click += B_New_Click;
            // 
            // TB_IP
            // 
            TB_IP.BorderStyle = BorderStyle.FixedSingle;
            TB_IP.Font = new Font("Segoe UI", 10F);
            TB_IP.Location = new Point(82, 13);
            TB_IP.Name = "TB_IP";
            TB_IP.Size = new Size(110, 25);
            TB_IP.TabIndex = 1;
            TB_IP.Text = "192.168.0.1";
            // 
            // NUD_Port
            // 
            NUD_Port.BorderStyle = BorderStyle.FixedSingle;
            NUD_Port.Font = new Font("Segoe UI", 10F);
            NUD_Port.Location = new Point(197, 13);
            NUD_Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            NUD_Port.Name = "NUD_Port";
            NUD_Port.Size = new Size(60, 25);
            NUD_Port.TabIndex = 2;
            NUD_Port.Value = new decimal(new int[] { 6000, 0, 0, 0 });
            // 
            // CB_Protocol
            // 
            CB_Protocol.DropDownStyle = ComboBoxStyle.DropDownList;
            CB_Protocol.FlatStyle = FlatStyle.Flat;
            CB_Protocol.Font = new Font("Segoe UI", 10F);
            CB_Protocol.Location = new Point(262, 12);
            CB_Protocol.Name = "CB_Protocol";
            CB_Protocol.Size = new Size(70, 26);
            CB_Protocol.TabIndex = 3;
            CB_Protocol.SelectedIndexChanged += CB_Protocol_SelectedIndexChanged;
            // 
            // CB_Routine
            // 
            CB_Routine.DropDownStyle = ComboBoxStyle.DropDownList;
            CB_Routine.FlatStyle = FlatStyle.Flat;
            CB_Routine.Font = new Font("Segoe UI", 10F);
            CB_Routine.Location = new Point(337, 12);
            CB_Routine.Name = "CB_Routine";
            CB_Routine.Size = new Size(115, 26);
            CB_Routine.TabIndex = 4;
            // 
            // CB_Mode
            // 
            CB_Mode.DropDownStyle = ComboBoxStyle.DropDownList;
            CB_Mode.FlatStyle = FlatStyle.Flat;
            CB_Mode.Font = new Font("Segoe UI", 10F);
            CB_Mode.Location = new Point(457, 12);
            CB_Mode.Name = "CB_Mode";
            CB_Mode.Size = new Size(80, 26);
            CB_Mode.TabIndex = 5;
            CB_Mode.SelectedIndexChanged += CB_Mode_SelectedIndexChanged;
            // 
            // CB_Theme
            // 
            CB_Theme.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CB_Theme.DropDownStyle = ComboBoxStyle.DropDownList;
            CB_Theme.FlatStyle = FlatStyle.Flat;
            CB_Theme.Font = new Font("Segoe UI", 10F);
            CB_Theme.Location = new Point(542, 12);
            CB_Theme.Name = "CB_Theme";
            CB_Theme.Size = new Size(115, 26);
            CB_Theme.TabIndex = 6;
            CB_Theme.SelectedIndexChanged += CB_Theme_SelectedIndexChanged;
            // 
            // Tab_Hub
            // 
            Tab_Hub.Controls.Add(PG_Hub);
            Tab_Hub.Location = new Point(4, 5);
            Tab_Hub.Name = "Tab_Hub";
            Tab_Hub.Size = new Size(662, 457);
            Tab_Hub.TabIndex = 2;
            Tab_Hub.Text = "Hub";
            Tab_Hub.UseVisualStyleBackColor = true;
            // 
            // PG_Hub
            // 
            PG_Hub.Dock = DockStyle.Fill;
            PG_Hub.Location = new Point(0, 0);
            PG_Hub.Name = "PG_Hub";
            PG_Hub.PropertySort = PropertySort.Categorized;
            PG_Hub.Size = new Size(662, 457);
            PG_Hub.TabIndex = 0;
            // 
            // Tab_Logs
            // 
            Tab_Logs.Controls.Add(RTB_Logs);
            Tab_Logs.Location = new Point(4, 5);
            Tab_Logs.Name = "Tab_Logs";
            Tab_Logs.Size = new Size(662, 457);
            Tab_Logs.TabIndex = 1;
            Tab_Logs.Text = "Logs";
            Tab_Logs.UseVisualStyleBackColor = true;
            // 
            // RTB_Logs
            // 
            RTB_Logs.BorderStyle = BorderStyle.None;
            RTB_Logs.Dock = DockStyle.Fill;
            RTB_Logs.Location = new Point(0, 0);
            RTB_Logs.Name = "RTB_Logs";
            RTB_Logs.ReadOnly = true;
            RTB_Logs.Size = new Size(662, 457);
            RTB_Logs.TabIndex = 0;
            RTB_Logs.Text = "";
            // 
            // Tab_Credits
            // 
            Tab_Credits.Controls.Add(P_CreditsContainer);
            Tab_Credits.Location = new Point(4, 5);
            Tab_Credits.Name = "Tab_Credits";
            Tab_Credits.Size = new Size(662, 457);
            Tab_Credits.TabIndex = 3;
            Tab_Credits.Text = "Credits";
            Tab_Credits.UseVisualStyleBackColor = true;
            // 
            // P_CreditsContainer
            // 
            P_CreditsContainer.AutoScroll = true;
            P_CreditsContainer.Controls.Add(L_CreditsSpecialNames);
            P_CreditsContainer.Controls.Add(L_CreditsSpecialTitle);
            P_CreditsContainer.Controls.Add(L_CreditsContribNames);
            P_CreditsContainer.Controls.Add(L_CreditsContribTitle);
            P_CreditsContainer.Controls.Add(L_CreditsDevName);
            P_CreditsContainer.Controls.Add(L_CreditsDevTitle);
            P_CreditsContainer.Controls.Add(L_CreditsOwnersNames);
            P_CreditsContainer.Controls.Add(L_CreditsOwnersTitle);
            P_CreditsContainer.Controls.Add(L_CreditsMainTitle);
            P_CreditsContainer.Controls.Add(PB_CreditsLogo);
            P_CreditsContainer.Dock = DockStyle.Fill;
            P_CreditsContainer.Location = new Point(0, 0);
            P_CreditsContainer.Name = "P_CreditsContainer";
            P_CreditsContainer.Size = new Size(662, 457);
            P_CreditsContainer.TabIndex = 0;
            // 
            // L_CreditsSpecialNames
            // 
            L_CreditsSpecialNames.Dock = DockStyle.Top;
            L_CreditsSpecialNames.Font = new Font("Segoe UI", 12F);
            L_CreditsSpecialNames.Location = new Point(0, 420);
            L_CreditsSpecialNames.Name = "L_CreditsSpecialNames";
            L_CreditsSpecialNames.Size = new Size(662, 60);
            L_CreditsSpecialNames.TabIndex = 7;
            L_CreditsSpecialNames.Text = "kwsch, Creator of SysBot.NET";
            L_CreditsSpecialNames.TextAlign = ContentAlignment.TopCenter;
            // 
            // L_CreditsSpecialTitle
            // 
            L_CreditsSpecialTitle.Dock = DockStyle.Top;
            L_CreditsSpecialTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            L_CreditsSpecialTitle.Location = new Point(0, 385);
            L_CreditsSpecialTitle.Name = "L_CreditsSpecialTitle";
            L_CreditsSpecialTitle.Size = new Size(662, 35);
            L_CreditsSpecialTitle.TabIndex = 6;
            L_CreditsSpecialTitle.Text = "Special Thanks To";
            L_CreditsSpecialTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // L_CreditsContribNames
            // 
            L_CreditsContribNames.Dock = DockStyle.Top;
            L_CreditsContribNames.Font = new Font("Segoe UI", 12F);
            L_CreditsContribNames.Location = new Point(0, 480);
            L_CreditsContribNames.Name = "L_CreditsContribNames";
            L_CreditsContribNames.Size = new Size(662, 100);
            L_CreditsContribNames.TabIndex = 5;
            L_CreditsContribNames.Text = "Secludedly, Medals, Refactoring & Feature Enhancements\r\nLusamine, Research & Data Analysis\r\nHexbyt3, Core Engine Enhancements\r\nSantaCrab2, Auto-Legality Mod (ALM)";
            L_CreditsContribNames.TextAlign = ContentAlignment.TopCenter;
            // 
            // L_CreditsContribTitle
            // 
            L_CreditsContribTitle.Dock = DockStyle.Top;
            L_CreditsContribTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            L_CreditsContribTitle.Location = new Point(0, 445);
            L_CreditsContribTitle.Name = "L_CreditsContribTitle";
            L_CreditsContribTitle.Size = new Size(662, 35);
            L_CreditsContribTitle.TabIndex = 4;
            L_CreditsContribTitle.Text = "Project Contributors";
            L_CreditsContribTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // L_CreditsDevName
            // 
            L_CreditsDevName.Dock = DockStyle.Top;
            L_CreditsDevName.Font = new Font("Segoe UI", 12F);
            L_CreditsDevName.Location = new Point(0, 405);
            L_CreditsDevName.Name = "L_CreditsDevName";
            L_CreditsDevName.Size = new Size(662, 40);
            L_CreditsDevName.TabIndex = 3;
            L_CreditsDevName.Text = "Nexus Risen, Developer of DudeBot.NET";
            L_CreditsDevName.TextAlign = ContentAlignment.TopCenter;
            // 
            // L_CreditsDevTitle
            // 
            L_CreditsDevTitle.Dock = DockStyle.Top;
            L_CreditsDevTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            L_CreditsDevTitle.Location = new Point(0, 370);
            L_CreditsDevTitle.Name = "L_CreditsDevTitle";
            L_CreditsDevTitle.Size = new Size(662, 35);
            L_CreditsDevTitle.TabIndex = 2;
            L_CreditsDevTitle.Text = "Main Developer";
            L_CreditsDevTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // L_CreditsOwnersNames
            // 
            L_CreditsOwnersNames.Dock = DockStyle.Top;
            L_CreditsOwnersNames.Font = new Font("Segoe UI", 12F);
            L_CreditsOwnersNames.Location = new Point(0, 255);
            L_CreditsOwnersNames.Name = "L_CreditsOwnersNames";
            L_CreditsOwnersNames.Size = new Size(662, 50);
            L_CreditsOwnersNames.TabIndex = 9;
            L_CreditsOwnersNames.Text = "Havok, Logo & Asset Creation\r\nLink, Logo & Asset Creation";
            L_CreditsOwnersNames.TextAlign = ContentAlignment.TopCenter;
            // 
            // L_CreditsOwnersTitle
            // 
            L_CreditsOwnersTitle.Dock = DockStyle.Top;
            L_CreditsOwnersTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            L_CreditsOwnersTitle.Location = new Point(0, 220);
            L_CreditsOwnersTitle.Name = "L_CreditsOwnersTitle";
            L_CreditsOwnersTitle.Size = new Size(662, 35);
            L_CreditsOwnersTitle.TabIndex = 8;
            L_CreditsOwnersTitle.Text = "Project Owners";
            L_CreditsOwnersTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // L_CreditsMainTitle
            // 
            L_CreditsMainTitle.Dock = DockStyle.Top;
            L_CreditsMainTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            L_CreditsMainTitle.Location = new Point(0, 160);
            L_CreditsMainTitle.Name = "L_CreditsMainTitle";
            L_CreditsMainTitle.Size = new Size(662, 60);
            L_CreditsMainTitle.TabIndex = 1;
            L_CreditsMainTitle.Text = "DudeBot.NET";
            L_CreditsMainTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // PB_CreditsLogo
            // 
            PB_CreditsLogo.Dock = DockStyle.Top;
            PB_CreditsLogo.Location = new Point(0, 0);
            PB_CreditsLogo.Name = "PB_CreditsLogo";
            PB_CreditsLogo.Padding = new Padding(0, 20, 0, 0);
            PB_CreditsLogo.Size = new Size(662, 160);
            PB_CreditsLogo.SizeMode = PictureBoxSizeMode.Zoom;
            PB_CreditsLogo.TabIndex = 0;
            PB_CreditsLogo.TabStop = false;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(850, 561);
            Controls.Add(TC_Main);
            Controls.Add(P_Header);
            Controls.Add(P_Sidebar);
            Controls.Add(P_Bottom);
            Font = new Font("Segoe UI", 9F);
            Icon = Resources.icon;
            MinimumSize = new Size(866, 600);
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SysBot: Pokémon";
            FormClosing += Main_FormClosing;
            P_Sidebar.ResumeLayout(false);
            P_LangArea.ResumeLayout(false);
            P_LangArea.PerformLayout();
            P_LogoArea.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PB_LogoSidebar).EndInit();
            P_Header.ResumeLayout(false);
            P_Header.PerformLayout();
            P_Bottom.ResumeLayout(false);
            P_Bottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PB_Logo).EndInit();
            TC_Main.ResumeLayout(false);
            Tab_Bots.ResumeLayout(false);
            P_AddBot.ResumeLayout(false);
            P_AddBot.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NUD_Port).EndInit();
            Tab_Hub.ResumeLayout(false);
            Tab_Logs.ResumeLayout(false);
            Tab_Credits.ResumeLayout(false);
            P_CreditsContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel P_Sidebar;
        private System.Windows.Forms.Panel P_LogoArea;
        private System.Windows.Forms.PictureBox PB_LogoSidebar;
        private System.Windows.Forms.Button B_NavBots;
        private System.Windows.Forms.Button B_NavHub;
        private System.Windows.Forms.Button B_NavLogs;
        private System.Windows.Forms.Panel P_Header;
        private System.Windows.Forms.Label L_Title;
        private System.Windows.Forms.Panel P_Bottom;
        private System.Windows.Forms.Panel P_AddBot;
        private System.Windows.Forms.Label L_Version;
        private System.Windows.Forms.TabControl TC_Main;
        private System.Windows.Forms.TabPage Tab_Bots;
        private System.Windows.Forms.TabPage Tab_Logs;
        private System.Windows.Forms.RichTextBox RTB_Logs;
        private System.Windows.Forms.TabPage Tab_Credits;
        private System.Windows.Forms.Panel P_CreditsContainer;
        private System.Windows.Forms.PictureBox PB_CreditsLogo;
        private System.Windows.Forms.Label L_CreditsMainTitle;
        private System.Windows.Forms.Label L_CreditsDevTitle;
        private System.Windows.Forms.Label L_CreditsDevName;
        private System.Windows.Forms.Label L_CreditsContribTitle;
        private System.Windows.Forms.Label L_CreditsContribNames;
        private System.Windows.Forms.Label L_CreditsSpecialTitle;
        private System.Windows.Forms.Label L_CreditsSpecialNames;
        private System.Windows.Forms.Label L_CreditsOwnersTitle;
        private System.Windows.Forms.Label L_CreditsOwnersNames;
        private System.Windows.Forms.TabPage Tab_Hub;
        private System.Windows.Forms.PropertyGrid PG_Hub;
        private System.Windows.Forms.Button B_Stop;
        private System.Windows.Forms.Button B_Start;
        private System.Windows.Forms.TextBox TB_IP;
        private System.Windows.Forms.ComboBox CB_Routine;
        private System.Windows.Forms.NumericUpDown NUD_Port;
        private System.Windows.Forms.Button B_New;
        private System.Windows.Forms.FlowLayoutPanel FLP_Bots;
        private System.Windows.Forms.ComboBox CB_Protocol;
        private System.Windows.Forms.Button B_Restart;
        private System.Windows.Forms.Button B_Update;
        private System.Windows.Forms.ComboBox CB_Theme;
        private System.Windows.Forms.ComboBox CB_Mode;
        private System.Windows.Forms.PictureBox PB_Logo;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem trayRestore;
        private System.Windows.Forms.ToolStripMenuItem trayExit;
        private System.Windows.Forms.Button B_HideTray;
        private System.Windows.Forms.Button B_Credits;
        private System.Windows.Forms.Panel P_LangArea;
        private System.Windows.Forms.Label L_Language;
        private System.Windows.Forms.ComboBox CB_Language;
    }
}
