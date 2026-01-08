using SysBot.Base;
using SysBot.Pokemon.Discord;
using SysBot.Pokemon.WinForms.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{
    public partial class BotController : UserControl
    {
        private bool _suspendPainting = false;
        private volatile bool _hasPendingStateUpdate = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PokeBotState State { get; private set; } = new();
        private IPokeBotRunner? Runner;
        public EventHandler? Remove;

        private Color currentStatusColor = Color.FromArgb(90, 186, 71);
        private DateTime LastUpdateStatus = DateTime.Now;
        private bool buttonHovering = false;
        private bool _isHovered = false;
        private float pulseScale = 1.0f;
        private bool pulseGrowing = true;
        private float shimmerPosition = -1.0f;
        private const float MIN_PULSE_SCALE = 0.6f;
        private const float MAX_PULSE_SCALE = 1.0f;
        private const float PULSE_SPEED = 0.03f;
        private const float SHIMMER_SPEED = 0.02f;

        private readonly Color CuztomBackground = Color.Black;
        private readonly Color CuztomDarkBackground = Color.FromArgb(5, 5, 5);
        private readonly Color CuztomDarkerBackground = Color.FromArgb(0, 0, 0);
        private readonly Color CuztomAccent = Color.FromArgb(0, 204, 255);
        private readonly Color CuztomText = Color.FromArgb(239, 239, 239);
        private readonly Color CuztomSubText = Color.Gray;
        private readonly Color CuztomGreen = Color.FromArgb(90, 186, 71);
        private readonly Color CuztomRed = Color.FromArgb(236, 98, 95);
        private readonly Color CuztomYellow = Color.FromArgb(245, 197, 92);
        private readonly Color CuztomOrange = Color.FromArgb(251, 176, 64);

        public BotController()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer | 
                    ControlStyles.SupportsTransparentBackColor |
                    ControlStyles.Opaque, true);
            UpdateStyles();
            
            this.BackColor = Color.Transparent;
            
            // Skip initialization in design mode
            if (DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            ConfigureContextMenu();
            ConfigureChildControls();
            ModernizeStatusIndicator();
            ConfigureButtonAppearance();
            InitializeAnimationTimer();
            
            mainPanel.Resize += (s, e) => UpdateRegion();
            UpdateRegion();
        }
        
        private void UpdateRegion()
        {
            if (mainPanel == null || mainPanel.IsDisposed) return;
            
            var rect = mainPanel.ClientRectangle;
            using var path = new GraphicsPath();
            int chamfer = 25;
            path.AddLine(rect.Left + chamfer, rect.Top, rect.Right - chamfer, rect.Top);
            path.AddLine(rect.Right, rect.Top + chamfer, rect.Right, rect.Bottom - chamfer);
            path.AddLine(rect.Right - chamfer, rect.Bottom, rect.Left + chamfer, rect.Bottom);
            path.AddLine(rect.Left, rect.Bottom - chamfer, rect.Left, rect.Top + chamfer);
            path.CloseFigure();
            
            mainPanel.Region = new Region(path);
        }
        
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            
            // Skip if in design mode or disposed
            if (DesignMode || IsDisposed)
                return;
                
            if (Visible)
            {
                // When becoming visible, ensure animations are running
                ResumeAnimations();
            }
            else
            {
                // When hidden, pause animations to save resources
                PauseAnimations();
            }
        }
        
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            
            // Skip if in design mode or disposed
            if (DesignMode || IsDisposed)
                return;
                
            // Ensure timer is running when control is added to a parent
            if (Parent != null && Visible)
            {
                ResumeAnimations();
            }
        }

        private void ModernizeStatusIndicator()
        {
            // Scale-aware sizing for larger glow circle
            var dpiScale = DeviceDpi / 96f;
            var scaledSize = (int)(40 * dpiScale); 
            statusIndicator.Size = new Size(scaledSize, scaledSize);
            statusIndicator.Location = new Point((int)(15 * dpiScale), (int)(30 * dpiScale)); // Vertically centered
            statusIndicator.BackColor = Color.Transparent;
        }

        private void ConfigureButtonAppearance()
        {
            btnActions.Text = "COMMANDS";
            btnActions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnActions.ForeColor = Color.White;
            btnActions.FlatStyle = FlatStyle.Flat;
            btnActions.FlatAppearance.BorderSize = 0;
            btnActions.Cursor = Cursors.Hand;
        }

        private void InitializeAnimationTimer()
        {
            if (animationTimer is null)
            {
                animationTimer = new System.Windows.Forms.Timer
                {
                    Interval = 30 // 30ms for smooth animation (~33 FPS)
                };
                animationTimer.Tick += AnimationTimer_Tick;
            }
            if (!animationTimer.Enabled)
            {
                animationTimer.Start();
            }
        }

        private void ConfigureContextMenu()
        {
            var opt = (BotControlCommand[])Enum.GetValues(typeof(BotControlCommand));

            contextMenu.Renderer = new CuztomMenuRenderer();

            for (int i = 1; i < opt.Length; i++)
            {
                var cmd = opt[i];
                var item = new ToolStripMenuItem(cmd.ToString())
                {
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F)
                };
                item.Click += (_, __) => SendCommand(cmd);

                switch (cmd)
                {
                    case BotControlCommand.Start:
                        item.Text = "▶  Start";
                        break;
                    case BotControlCommand.Stop:
                        item.Text = "■  Stop";
                        break;
                    case BotControlCommand.Idle:
                        item.Text = "❚❚  Idle";
                        break;
                    case BotControlCommand.Resume:
                        item.Text = "⏵  Resume";
                        break;
                    case BotControlCommand.Restart:
                        item.Text = "↻  Restart";
                        break;
                    case BotControlCommand.RebootAndStop:
                        item.Text = "⚡  Reboot & Stop";
                        break;
                    case BotControlCommand.ScreenOnAll:
                        item.Text = "☀  Screen On";
                        break;
                    case BotControlCommand.ScreenOffAll:
                        item.Text = "🌙  Screen Off";
                        break;
                }

                contextMenu.Items.Add(item);
            }

            contextMenu.Items.Add(new ToolStripSeparator());

            // Add recovery status item
            var recoveryItem = new ToolStripMenuItem("📊 Recovery Status")
            {
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F)
            };
            recoveryItem.Click += ShowRecoveryStatus;
            contextMenu.Items.Add(recoveryItem);

            var remove = new ToolStripMenuItem("╳  Remove Bot")
            {
                ForeColor = CuztomRed,
                BackColor = CuztomDarkBackground,
                Font = new Font("Segoe UI", 8.5F)
            };
            remove.Click += (_, __) => TryRemove();
            contextMenu.Items.Add(remove);
            contextMenu.Opening += RcMenuOnOpening;

            RCMenu = contextMenu;
        }

        private void ConfigureChildControls()
        {
            foreach (var c in Controls.OfType<Control>())
            {
                if (c != btnActions)
                {
                    c.MouseEnter += BotController_MouseEnter;
                    c.MouseLeave += BotController_MouseLeave;
                }
            }

            foreach (var c in mainPanel.Controls.OfType<Control>())
            {
                c.MouseEnter += BotController_MouseEnter;
                c.MouseLeave += BotController_MouseLeave;
            }
        }

        private void RcMenuOnOpening(object? sender, CancelEventArgs e)
        {
            if (Runner is null)
                return;

            var bot = Runner.GetBot(State);
            if (bot is null)
                return;

            foreach (var tsi in contextMenu.Items.OfType<ToolStripMenuItem>())
            {
                if (tsi.Text != null && tsi.Text.Length >= 3)
                {
                    var text = tsi.Text[3..].Trim();
                    tsi.Enabled = Enum.TryParse(text.Replace(" ", "").Replace("&", "And"), out BotControlCommand cmd)
                        ? cmd.IsUsable(bot.IsRunning, bot.IsPaused)
                        : !bot.IsRunning;
                }
            }
        }

        public void Initialize(IPokeBotRunner runner, PokeBotState cfg)
        {
            Runner = runner;
            State = cfg;
            ReloadStatus();
            lblConnectionInfo.Text = "Initializing...";
        }

        public void ReloadStatus()
        {
            var bot = GetBot()?.Bot;
            if (bot is null) return;
            // Bot name with TID format on second line
            lblBotName.Text = $"{bot.Connection.Name}-{bot.Connection.Label}";
            // Trade type will be updated in ReloadStatus(BotSource) with current time
            lblRoutineType.Text = $"{State.InitialRoutine}";
            lblRoutineType.Visible = true;
            L_Left.Text = $"{bot.Connection.Name}\n{State.InitialRoutine}";
        }

        public void ReloadStatus(BotSource<PokeBotState> b)
        {
            ReloadStatus();
            var bot = b.Bot;
            
            // Line 2: Bot name with TID format
            lblBotName.Text = $"{bot.Connection.Name}-{bot.Connection.Label}";
            
            // Line 3: Trade type with current time (12-hour format)
            var routineType = bot.Config.CurrentRoutineType == PokeRoutineType.Idle ? 
                State.InitialRoutine.ToString() : bot.Config.CurrentRoutineType.ToString();
            lblRoutineType.Text = $"{routineType} @ {DateTime.Now:h:mm:ss tt}";
            
            // Line 4: Current activity with arrow
            lblConnectionInfo.Text = $"\u21aa {bot.LastLogged}";

            var botState = ReadBotState();
            // Line 1: Status text next to pulsing indicator
            lblStatusValue.Text = botState.ToUpper();

            // Check for recovery status
            var recoveryState = b.GetRecoveryState();
            if (recoveryState is { ConsecutiveFailures: > 0 })
            {
                lblConnectionInfo.Text += $" [Recovery Attempts: {recoveryState.ConsecutiveFailures}]";
            }

            switch (botState)
            {
                case "STOPPED":
                    currentStatusColor = Color.FromArgb(100, 100, 100);
                    lblStatusValue.ForeColor = Color.FromArgb(100, 100, 100);
                    // Check if recovering
                    if (recoveryState is { IsRecovering: true })
                    {
                        currentStatusColor = CuztomOrange;
                        lblStatusValue.ForeColor = CuztomOrange;
                        lblStatusValue.Text = "RECOVERING";
                    }
                    break;
                case "IDLE":
                case "IDLING":
                    currentStatusColor = CuztomYellow;
                    lblStatusValue.ForeColor = CuztomYellow;
                    break;
                case "ERROR":
                    currentStatusColor = CuztomRed;
                    lblStatusValue.ForeColor = CuztomRed;
                    break;
                case "REBOOTING":
                    currentStatusColor = CuztomAccent;
                    lblStatusValue.ForeColor = CuztomAccent;
                    break;
                default:
                    currentStatusColor = CuztomGreen;
                    lblStatusValue.ForeColor = CuztomGreen;
                    break;
            }
            
            statusIndicator.Invalidate();

            var lastTime = bot.LastTime;
            if (!b.IsRunning)
            {
                currentStatusColor = Color.FromArgb(100, 100, 100);
                statusIndicator.Invalidate();
                return;
            }

            if (!b.Bot.Connection.Connected)
            {
                currentStatusColor = CuztomAccent;
                statusIndicator.Invalidate();
                return;
            }

            var cfg = bot.Config;
            if (cfg.CurrentRoutineType == PokeRoutineType.Idle && cfg.NextRoutineType == PokeRoutineType.Idle)
            {
                currentStatusColor = CuztomYellow;
                statusIndicator.Invalidate();
                return;
            }

            if (LastUpdateStatus == lastTime)
                return;

            const int threshold = 100;
            Color good = cfg.Connection.Protocol == SwitchProtocol.USB ? CuztomAccent : CuztomGreen;
            Color bad = CuztomRed;

            var delta = DateTime.Now - lastTime;
            var seconds = delta.Seconds;

            LastUpdateStatus = lastTime;
            if (seconds > 2 * threshold)
            {
                statusIndicator.Invalidate();
                return;
            }

            if (seconds > threshold)
            {
                currentStatusColor = bad;
            }
            else
            {
                var factor = seconds / (double)threshold;
                currentStatusColor = Blend(bad, good, factor * factor);
            }
            
            statusIndicator.Invalidate();
        }

        private static Color Blend(Color color, Color backColor, double amount)
        {
            byte r = (byte)((color.R * amount) + (backColor.R * (1 - amount)));
            byte g = (byte)((color.G * amount) + (backColor.G * (1 - amount)));
            byte b = (byte)((color.B * amount) + (backColor.B * (1 - amount)));
            return Color.FromArgb(r, g, b);
        }

        public void TryRemove()
        {
            var bot = GetBot();
            if (!Runner!.Config.Global.SkipConsoleBotCreation)
                bot?.Stop();

            Remove?.Invoke(this, EventArgs.Empty);
        }

        public void SendCommand(BotControlCommand cmd, bool echo = true)
        {
            if (Runner?.Config.Global.SkipConsoleBotCreation != false)
            {
                LogUtil.LogError("No bots were created because SkipConsoleBotCreation is on!", "Hub");
                return;
            }
            var bot = GetBot();
            if (bot is null)
            {
                LogUtil.LogError("Bot is null!", "BotController");
                return;
            }

            switch (cmd)
            {
                case BotControlCommand.Idle:
                    bot.Pause();
                    break;
                case BotControlCommand.Start:
                    try
                    {
                        Runner.InitializeStart();
                        bot.Start();
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogError($"Failed to start bot: {ex.Message}", "BotController");
                        WinFormsUtil.Alert($"Failed to start bot: {ex.Message}");
                    }
                    break;
                case BotControlCommand.Stop:
                    bot.Stop();
                    break;
                case BotControlCommand.Resume:
                    bot.Resume();
                    break;
                case BotControlCommand.RebootAndStop:
                    bot.RebootAndStop();
                    break;
                case BotControlCommand.Restart:
                    {
                        var prompt = WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Are you sure you want to restart the connection?");
                        if (prompt != DialogResult.Yes)
                            return;

                        // Stop the bot first to ensure proper cleanup
                        bot.Stop();
                        
                        // Use async delay instead of blocking the UI thread
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(500); // Give it time to stop properly
                            
                            // Use BeginInvoke to update UI from background thread
                            if (!IsDisposed)
                            {
                                BeginInvoke((MethodInvoker)(() =>
                                {
                                    try
                                    {
                                        Runner.InitializeStart();
                                        bot.Bot.Connection.Reset();
                                        bot.Start();
                                    }
                                    catch (Exception ex)
                                    {
                                        LogUtil.LogError($"Failed to restart bot: {ex.Message}", "BotController");
                                        WinFormsUtil.Alert($"Failed to restart bot: {ex.Message}");
                                    }
                                }));
                            }
                        });
                        break;
                    }
                case BotControlCommand.ScreenOnAll:
                    ExecuteScreenCommand(true);
                    break;
                case BotControlCommand.ScreenOffAll:
                    ExecuteScreenCommand(false);
                    break;
                default:
                    WinFormsUtil.Alert($"{cmd} is not a command that can be sent to the Bot.");
                    return;
            }
        }

        private void ExecuteScreenCommand(bool screenOn)
        {
            if (Runner is null)
            {
                LogUtil.LogError("Runner is null - cannot execute screen command", "BotController");
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var bots = Runner.Bots;
                    if (bots is null or { Count: 0 })
                    {
                        LogUtil.LogError("No bots available to execute screen command", "BotController");
                        return;
                    }

                    int successCount = 0;
                    int totalCount = bots.Count;

                    foreach (var botSource in bots)
                    {
                        try
                        {
                            var bot = botSource.Bot;
                            if (bot?.Connection != null && bot.Connection.Connected)
                            {
                                var crlf = bot is SwitchRoutineExecutor<PokeBotState> { UseCRLF: true };
                                await bot.Connection.SendAsync(SwitchCommand.SetScreen(screenOn ? ScreenState.On : ScreenState.Off, crlf), CancellationToken.None);
                                successCount++;
                                LogUtil.LogInfo($"Screen turned {(screenOn ? "ON" : "OFF")} for {bot.Connection.Name}", "BotController");
                            }
                            else
                            {
                                LogUtil.LogError($"Cannot send screen command - bot {bot?.Connection?.Name ?? "unknown"} is not connected", "BotController");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogUtil.LogError($"Failed to send screen command to bot: {ex.Message}", "BotController");
                        }
                    }

                    LogUtil.LogInfo($"Screen command sent to {successCount} of {totalCount} bots", "BotController");
                }
                catch (Exception ex)
                {
                    LogUtil.LogError($"Failed to execute screen command for all bots: {ex.Message}", "BotController");
                }
            });
        }

        private void ShowRecoveryStatus(object? sender, EventArgs e)
        {
            var bot = GetBot();
            if (bot is null)
            {
                MessageBox.Show("Bot not found.", "Recovery Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var recoveryState = bot.GetRecoveryState();
            if (recoveryState is null)
            {
                MessageBox.Show("Recovery service is not enabled for this bot.", "Recovery Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var status = $"Bot: {bot.Bot.Connection.Name}\n" +
                        $"Status: {(bot.IsRunning ? "Running" : "Stopped")}\n" +
                        $"Recovery Attempts: {recoveryState.ConsecutiveFailures}\n" +
                        $"Total Crashes: {recoveryState.CrashHistory.Count}\n" +
                        $"Is Recovering: {(recoveryState.IsRecovering ? "Yes" : "No")}\n";

            if (recoveryState.LastRecoveryAttempt is not null)
            {
                status += $"Last Recovery: {recoveryState.LastRecoveryAttempt.Value:yyyy-MM-dd HH:mm:ss}\n";
            }

            if (recoveryState.CrashHistory.Count > 0)
            {
                var lastCrash = recoveryState.CrashHistory.OrderByDescending(c => c).FirstOrDefault();
                if (lastCrash != default)
                {
                    status += $"Last Crash: {lastCrash:yyyy-MM-dd HH:mm:ss}\n";
                }
            }

            MessageBox.Show(status, "Recovery Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public string ReadBotState()
        {
            try
            {
                var botSource = GetBot();
                if (botSource is null)
                    return "ERROR";

                var bot = botSource.Bot;
                if (bot is null)
                    return "ERROR";

                if (!botSource.IsRunning)
                    return "STOPPED";

                if (botSource.IsStopping)
                    return "STOPPING";

                if (botSource.IsPaused)
                {
                    if (bot.Config?.CurrentRoutineType != PokeRoutineType.Idle)
                        return "IDLING";
                    else
                        return "IDLE";
                }

                if (botSource.IsRunning && !bot.Connection.Connected)
                    return "REBOOTING";

                var cfg = bot.Config;
                if (cfg == null)
                    return "UNKNOWN";

                if (cfg.CurrentRoutineType == PokeRoutineType.Idle)
                    return "IDLE";

                if (botSource.IsRunning && bot.Connection.Connected)
                    return cfg.CurrentRoutineType.ToString();

                return "UNKNOWN";
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Error reading bot state: {ex.Message}", "BotController");
                return "ERROR";
            }
        }

        public BotSource<PokeBotState>? GetBot()
        {
            try
            {
                if (Runner is null)
                    return null;

                var bot = Runner.GetBot(State);
                if (bot is null)
                    return null;

                return bot;
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Error getting bot: {ex.Message}", "BotController");
                return null;
            }
        }

        private void BotController_MouseEnter(object? sender, EventArgs e)
        {
            _isHovered = true;
            mainPanel.Invalidate();
        }

        private void BotController_MouseLeave(object? sender, EventArgs e)
        {
            if (!ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                _isHovered = false;
                mainPanel.Invalidate();
            }
        }

        private void BtnActions_MouseEnter(object? sender, EventArgs e)
        {
            buttonHovering = true;
        }

        private void BtnActions_MouseLeave(object? sender, EventArgs e)
        {
            buttonHovering = false;
        }

        public void ReadState()
        {
            if (_suspendPainting || IsDisposed) return;
            
            var bot = GetBot();
            if (bot is null) return;

            if (InvokeRequired)
            {
                // Use BeginInvoke to avoid blocking the calling thread
                // Check if we already have a pending update to avoid queuing multiple updates
                if (!_hasPendingStateUpdate)
                {
                    _hasPendingStateUpdate = true;
                    BeginInvoke((System.Windows.Forms.MethodInvoker)(() => 
                    {
                        _hasPendingStateUpdate = false;
                        if (!_suspendPainting && !IsDisposed)
                        {
                            try
                            {
                                ReloadStatus(bot);
                            }
                            catch (Exception ex)
                            {
                                LogUtil.LogError($"Error updating bot status: {ex.Message}", "BotController");
                            }
                        }
                    }));
                }
            }
            else
            {
                if (!IsDisposed)
                {
                    try
                    {
                        ReloadStatus(bot);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogError($"Error updating bot status: {ex.Message}", "BotController");
                    }
                }
            }
        }

        public void PauseAnimations()
        {
            _suspendPainting = true;
            if (animationTimer is not null)
                animationTimer.Stop();
        }

        public void ResumeAnimations()
        {
            _suspendPainting = false;
            if (animationTimer is not null && !animationTimer.Enabled)
            {
                animationTimer.Start();
            }
            // Force a refresh of status when resuming
            ReadState();
            statusIndicator?.Invalidate();
        }

        private void BotController_Paint(object sender, PaintEventArgs e)
        {
            if (_suspendPainting) return;
            
            // Transparent background - do not paint
        }

        private void MainPanel_Paint(object sender, PaintEventArgs e)
        {
            if (_suspendPainting) return;
            
            var g = e.Graphics;
            g.CompositingMode = CompositingMode.SourceOver;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(0, 0, mainPanel.Width - 1, mainPanel.Height - 1);
            
            // Alien Tech Background
            using (var path = new GraphicsPath())
            {
                // Aggressive chamfered corners
                int chamfer = 25;
                path.AddLine(rect.Left + chamfer, rect.Top, rect.Right - chamfer, rect.Top);
                path.AddLine(rect.Right, rect.Top + chamfer, rect.Right, rect.Bottom - chamfer);
                path.AddLine(rect.Right - chamfer, rect.Bottom, rect.Left + chamfer, rect.Bottom);
                path.AddLine(rect.Left, rect.Bottom - chamfer, rect.Left, rect.Top + chamfer);
                path.CloseFigure();
                
                // 1. Deep Dark Base (High Opacity for readability over Rack)
                using (var brush = new SolidBrush(Color.FromArgb(240, 5, 5, 5)))
                {
                    g.FillPath(brush, path);
                }

                // 2. True Hexagon Mesh Pattern
                using (var clip = new Region(path))
                {
                    g.SetClip(clip, CombineMode.Replace);
                    using (var pen = new Pen(Color.FromArgb(10, 0, 255, 255), 1))
                    {
                        float hexRadius = 15f;
                        float hexHeight = (float)(Math.Sqrt(3) * hexRadius); // Height of flat-topped hex
                        float hexWidth = 2 * hexRadius; // Width of flat-topped hex
                        float horizDist = hexWidth * 0.75f;
                        float vertDist = hexHeight;
                        
                        // Calculate grid bounds
                        int cols = (int)(rect.Width / horizDist) + 2;
                        int rows = (int)(rect.Height / vertDist) + 2;

                        for (int r = 0; r < rows; r++)
                        {
                            for (int c = 0; c < cols; c++)
                            {
                                float x = c * horizDist;
                                float y = r * vertDist;
                                if (c % 2 == 1) y += vertDist / 2; // Stagger odd columns

                                // Draw Hexagon
                                PointF[] points = new PointF[6];
                                for (int i = 0; i < 6; i++)
                                {
                                    float angle_deg = 60 * i;
                                    float angle_rad = (float)(Math.PI / 180 * angle_deg);
                                    points[i] = new PointF(
                                        x + hexRadius * (float)Math.Cos(angle_rad),
                                        y + hexRadius * (float)Math.Sin(angle_rad)
                                    );
                                }
                                g.DrawPolygon(pen, points);
                            }
                        }
                    }
                    g.ResetClip();
                }
                
                // 3. Vertical Gradient Overlay (Vignette)
                using (var brush = new LinearGradientBrush(rect, 
                    Color.FromArgb(20, 255, 255, 255), 
                    Color.FromArgb(200, 0, 0, 0), 
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }

                // 4. Glowing Border
                var borderColor = currentStatusColor;
                if (borderColor == Color.Empty) borderColor = CuztomAccent;

                int glowIntensity = 40;
                int borderThickness = 6;
                
                if (_isHovered)
                {
                    // Intensify the glow on hover (Brighter and thicker)
                    glowIntensity = 120;
                    borderThickness = 8;
                    // Boost brightness significantly
                    borderColor = ControlPaint.Light(borderColor, 0.5f); 
                }

                // Outer soft glow
                using (var pen = new Pen(Color.FromArgb(glowIntensity, borderColor), borderThickness))
                {
                    pen.LineJoin = LineJoin.Bevel;
                    g.DrawPath(pen, path);
                }
                
                // Inner sharp border
                using (var pen = new Pen(borderColor, _isHovered ? 2.5f : 1.5f))
                {
                    pen.LineJoin = LineJoin.Bevel;
                    g.DrawPath(pen, path);
                }

                // 5. Tech Accents (Corner Brackets)
                using (var pen = new Pen(borderColor, 3))
                {
                    int arm = 25;
                    // Top Left
                    g.DrawLine(pen, rect.Left + chamfer - 5, rect.Top, rect.Left + chamfer + arm, rect.Top);
                    g.DrawLine(pen, rect.Left, rect.Top + chamfer - 5, rect.Left, rect.Top + chamfer + arm);
                    
                    // Bottom Right
                    g.DrawLine(pen, rect.Right - chamfer + 5, rect.Bottom, rect.Right - chamfer - arm, rect.Bottom);
                    g.DrawLine(pen, rect.Right, rect.Bottom - chamfer + 5, rect.Right, rect.Bottom - chamfer - arm);
                }

                // 6. Shimmer Effect (Alien Pulse)
                if (shimmerPosition > -0.5f && shimmerPosition < 1.5f)
                {
                    float shimmerX = rect.Left + (rect.Width * shimmerPosition);
                    // Draw a angled highlight moving across
                    using (var shimmerBrush = new LinearGradientBrush(
                        new PointF(shimmerX - 100, rect.Top),
                        new PointF(shimmerX + 100, rect.Bottom),
                        Color.Transparent,
                        Color.Transparent))
                    {
                        ColorBlend blend = new ColorBlend();
                        blend.Positions = new[] { 0.0f, 0.5f, 1.0f };
                        blend.Colors = new[] { Color.Transparent, Color.FromArgb(100, borderColor), Color.Transparent };
                        shimmerBrush.InterpolationColors = blend;
                        
                        g.FillPath(shimmerBrush, path);
                    }
                }
            }
        }

        private void StatusIndicator_Paint(object sender, PaintEventArgs e)
        {
            if (_suspendPainting) return;
            
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;

            if (sender is not PictureBox control) return;

            var centerX = control.Width / 2f;
            var centerY = control.Height / 2f;
            var baseRadius = Math.Min(control.Width, control.Height) * 0.4f; 
            
            // Pulsing logic
            var currentRadius = baseRadius * (0.9f + (pulseScale - 0.6f) * 0.3f);

            // Alien Power Button Look
            
            // 1. Glow Halo
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(centerX - currentRadius * 1.5f, centerY - currentRadius * 1.5f, currentRadius * 3f, currentRadius * 3f);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(150, currentStatusColor);
                    brush.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(brush, path);
                }
            }

            // 2. Hexagon Frame
            var hexPoints = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = (float)(i * 60 * Math.PI / 180);
                hexPoints[i] = new PointF(
                    centerX + currentRadius * (float)Math.Cos(angle),
                    centerY + currentRadius * (float)Math.Sin(angle));
            }

            // Fill Hex (Dark Tech)
            using (var brush = new LinearGradientBrush(
                new PointF(centerX, centerY - currentRadius),
                new PointF(centerX, centerY + currentRadius),
                Color.FromArgb(240, 30, 30, 30),
                Color.FromArgb(240, 10, 10, 10)))
            {
                g.FillPolygon(brush, hexPoints);
            }
            
            // Stroke Hex
            using (var pen = new Pen(currentStatusColor, 2f))
            {
                g.DrawPolygon(pen, hexPoints);
            }

            // 3. Power Symbol
            float iconRadius = currentRadius * 0.5f;
            using (var pen = new Pen(currentStatusColor, 2.5f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                
                // Power Circle Arc (Gap at top)
                // Start at 300 degrees, sweep 300 degrees
                g.DrawArc(pen, centerX - iconRadius, centerY - iconRadius, iconRadius * 2, iconRadius * 2, 300, 300);
                
                // Power Line (Vertical at top)
                g.DrawLine(pen, centerX, centerY - iconRadius, centerX, centerY - iconRadius * 0.2f);
            }
        }

        private void BtnActions_Paint(object sender, PaintEventArgs e)
        {
            if (_suspendPainting) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (sender is not Button btn) return;
            var rect = btn.ClientRectangle;

            btn.Region?.Dispose();

            // Chamfered button
            using var path = new GraphicsPath();
            int chamfer = 6;
            path.AddLine(rect.Left + chamfer, rect.Top, rect.Right - chamfer, rect.Top);
            path.AddLine(rect.Right, rect.Top + chamfer, rect.Right, rect.Bottom - chamfer);
            path.AddLine(rect.Right - chamfer, rect.Bottom, rect.Left + chamfer, rect.Bottom);
            path.AddLine(rect.Left, rect.Bottom - chamfer, rect.Left, rect.Top + chamfer);
            path.CloseFigure();
            
            btn.Region = new Region(path);

            // Alien Tech Button Style
            var glowColor = buttonHovering ? Color.Cyan : Color.FromArgb(0, 150, 200);

            // 1. Base (Deep Key Cap)
            using (var brush = new LinearGradientBrush(rect, 
                Color.FromArgb(20, 20, 20), 
                Color.FromArgb(5, 5, 5), 
                LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            // 2. Inner Highlight (Top Edge)
            var topRect = new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height / 2);
            using (var brush = new LinearGradientBrush(topRect, 
                Color.FromArgb(50, 255, 255, 255), 
                Color.Transparent, 
                LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            // 3. Glowing Text Shadow (Neon Effect)
            var textFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            
            // Draw glow behind text
            for (int i = 0; i < 3; i++)
            {
                using var glowBrush = new SolidBrush(Color.FromArgb(30, glowColor));
                g.DrawString(btn.Text, btn.Font, glowBrush, new RectangleF(rect.X + 1, rect.Y + 1, rect.Width, rect.Height), textFormat);
            }

            // 4. Border (Active/Inactive)
            using (var pen = new Pen(buttonHovering ? glowColor : Color.FromArgb(60, 60, 60), 1.5f))
            {
                g.DrawPath(pen, path);
            }

            // 5. Text
            using var textBrush = new SolidBrush(buttonHovering ? Color.White : Color.FromArgb(220, 220, 220));
            g.DrawString(btn.Text, btn.Font, textBrush, rect, textFormat);
        }

        private void BtnActions_Click(object sender, EventArgs e)
        {
            contextMenu.Show(btnActions, new Point(0, btnActions.Height + 1));
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (_suspendPainting || IsDisposed) return;

            // Update pulse scale
            if (pulseGrowing)
            {
                pulseScale += PULSE_SPEED;
                if (pulseScale >= MAX_PULSE_SCALE)
                {
                    pulseScale = MAX_PULSE_SCALE;
                    pulseGrowing = false;
                }
            }
            else
            {
                pulseScale -= PULSE_SPEED;
                if (pulseScale <= MIN_PULSE_SCALE)
                {
                    pulseScale = MIN_PULSE_SCALE;
                    pulseGrowing = true;
                }
            }
            
            // Update shimmer
            shimmerPosition += SHIMMER_SPEED;
            if (shimmerPosition > 1.5f)
            {
                shimmerPosition = -3.0f; // Reset and wait a bit (pause)
            }

            // Invalidate main panel for shimmer
            if (mainPanel != null && !mainPanel.IsDisposed && shimmerPosition > -0.5f && shimmerPosition < 1.5f)
            {
                mainPanel.Invalidate();
            }
            // Otherwise just invalidate indicator
            else if (statusIndicator is not null && !statusIndicator.IsDisposed)
            {
                statusIndicator.Invalidate();
            }
            
            // Update the time display periodically (every second)
            if (DateTime.Now.Subtract(LastUpdateStatus).TotalSeconds >= 1)
            {
                ReadState();
            }
        }

        private class CuztomMenuRenderer : ToolStripProfessionalRenderer
        {
            public CuztomMenuRenderer() : base(new CuztomColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var rc = new Rectangle(Point.Empty, e.Item.Size);
                var c = e.Item.Selected ? Color.FromArgb(20, 20, 20) : Color.FromArgb(12, 12, 12);
                using var brush = new SolidBrush(c);
                e.Graphics.FillRectangle(brush, rc);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                // Force white text for better visibility
                if (!e.Item.Enabled)
                    e.TextColor = Color.Gray;
                else
                    e.TextColor = Color.Cyan;
                    
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
    }

    public enum BotControlCommand
    {
        None,
        Start,
        Stop,
        Idle,
        Resume,
        Restart,
        RebootAndStop,
        ScreenOnAll,
        ScreenOffAll,
    }

    public static class BotControlCommandExtensions
    {
        public static bool IsUsable(this BotControlCommand cmd, bool running, bool paused)
        {
            return cmd switch
            {
                BotControlCommand.Start => !running,
                BotControlCommand.Stop => running,
                BotControlCommand.Idle => running && !paused,
                BotControlCommand.Resume => paused,
                BotControlCommand.Restart => true,
                BotControlCommand.ScreenOnAll => running,
                BotControlCommand.ScreenOffAll => running,
                _ => false,
            };
        }
    }
}

