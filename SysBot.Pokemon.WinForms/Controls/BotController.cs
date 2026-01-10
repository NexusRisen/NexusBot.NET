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
        
        // Nintendo Switch Themed Colors
        private readonly Color SwitchBlue = Color.FromArgb(0, 195, 227);     // Neon Blue Joycon
        private readonly Color SwitchGreen = Color.FromArgb(0, 255, 127);    // Bright Neon Green
        private readonly Color SwitchYellow = Color.FromArgb(255, 220, 0);   // Neon Yellow
        private readonly Color SwitchGrey = Color.FromArgb(80, 80, 80);      // Console Grey
        private readonly Color SwitchRed = Color.FromArgb(255, 69, 58);      // Neon Red Joycon

        private readonly Color CuztomAccent = Color.FromArgb(0, 204, 255);
        private readonly Color CuztomText = Color.FromArgb(239, 239, 239);
        private readonly Color CuztomSubText = Color.Gray;
        private readonly Color CuztomGreen = Color.FromArgb(90, 186, 71);
        private readonly Color CuztomRed = Color.FromArgb(236, 98, 95);
        private readonly Color CuztomYellow = Color.FromArgb(245, 197, 92);
        private readonly Color CuztomOrange = Color.FromArgb(251, 176, 64);

        private string _botNameText = "192.168.1.100";
        private string _statusValueText = "STOPPED";
        private string _routineTypeText = "FlexTrade";
        private string _connectionInfoText = "→ Waiting for command...";
        
        private readonly Font _fontBotName = new("Segoe UI", 12F, FontStyle.Bold);
        private readonly Font _fontStatus = new("Segoe UI", 9F, FontStyle.Bold);
        private readonly Font _fontRoutine = new("Segoe UI", 8.5F);
        private readonly Font _fontConnection = new("Segoe UI", 8F);

        // Shared timer for all bot instances to reduce resource usage
        private static System.Windows.Forms.Timer? _sharedTimer;
        private static readonly object _timerLock = new();
        private static int _instanceCount = 0;
        
        private static void CreateChamferedRegion(Control control, int chamfer)
        {
            void UpdateRegion()
            {
                if (control.Region != null) control.Region.Dispose();
                using var path = new GraphicsPath();
                var rect = control.ClientRectangle;
                
                // Ensure valid dimensions
                if (rect.Width <= 0 || rect.Height <= 0) return;
                
                int c = Math.Min(chamfer, Math.Min(rect.Width, rect.Height) / 2);
                
                path.AddLine(rect.Left + c, rect.Top, rect.Right - c, rect.Top);
                path.AddLine(rect.Right, rect.Top + c, rect.Right, rect.Bottom - c);
                path.AddLine(rect.Right - c, rect.Bottom, rect.Left + c, rect.Bottom);
                path.AddLine(rect.Left, rect.Bottom - c, rect.Left, rect.Top + c);
                path.CloseFigure();
                control.Region = new Region(path);
            }

            control.SizeChanged += (_, _) => UpdateRegion();
            UpdateRegion();
        }

        private static void CreateCircularRegion(Control control)
        {
            void UpdateRegion()
            {
                if (control.Region != null) control.Region.Dispose();
                using var path = new GraphicsPath();
                path.AddEllipse(control.ClientRectangle);
                control.Region = new Region(path);
            }

            control.SizeChanged += (_, _) => UpdateRegion();
            UpdateRegion();
        }

        private static void InitializeAnimationTimer()
        {
            lock (_timerLock)
            {
                _instanceCount++;
                if (_sharedTimer == null)
                {
                    _sharedTimer = new System.Windows.Forms.Timer
                    {
                        Interval = 30 // 30ms for smooth animation (~33 FPS)
                    };
                    _sharedTimer.Tick += SharedTimer_Tick;
                    _sharedTimer.Start();
                }
            }
        }
        
        private static void SharedTimer_Tick(object? sender, EventArgs e)
        {
            // This static event is no longer needed as we iterate via weak references or similar, 
            // but for simplicity in WinForms, we'll let instances subscribe/unsubscribe to a static event
            // OR simpler: make the timer invoke a static event that instances subscribe to.
            OnSharedTick?.Invoke(sender, e);
        }

        private static event EventHandler? OnSharedTick;

        public BotController()
        {
            InitializeComponent();
            
            EnableDoubleBuffering(mainPanel);
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer | 
                    ControlStyles.ResizeRedraw |
                    ControlStyles.OptimizedDoubleBuffer | 
                    ControlStyles.SupportsTransparentBackColor, true);
            UpdateStyles();
            
            this.BackColor = Color.Transparent;
            
            // Skip initialization in design mode
            if (DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            // Apply Alienware Theme
            WinFormsTheme.Apply(this);
            
            // Force Transparency AFTER Theme Apply (Critical for removing black corners)
            this.BackColor = Color.Transparent;
            this.mainPanel.BackColor = Color.Transparent;
            if (this.Parent != null) this.Parent.Invalidate();

            ConfigureContextMenu();
            ConfigureChildControls();
            ModernizeStatusIndicator();
            ConfigureButtonAppearance();
            
            // Apply chamfered regions to controls
            CreateChamferedRegion(btnActions, 8);
            // CreateChamferedRegion(mainPanel, 25); // Fix: Remove Region clipping to prevent black artifacts
            CreateCircularRegion(statusIndicator);
            
            InitializeAnimationTimer();
            OnSharedTick += AnimationTimer_Tick;
            
            mainPanel.Paint += MainPanel_Paint;
            statusIndicator.Paint += StatusIndicator_Paint;
        }
        
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            
            // Skip if in design mode or disposed
            if (DesignMode || IsDisposed)
                return;
                
            if (Visible)
            {
                ResumeAnimations();
            }
            else
            {
                PauseAnimations();
            }
        }
        
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            
            // Skip if in design mode or disposed
            if (DesignMode || IsDisposed)
                return;
                
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
            btnActions.Resize += BtnActions_Resize;
            BtnActions_Resize(btnActions, EventArgs.Empty);
        }

        private static void EnableDoubleBuffering(Control control)
        {
            if (control == null) return;
            var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            property?.SetValue(control, true, null);
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
                        item.Text = "⚡  Reboot && Stop";
                        break;
                    case BotControlCommand.ScreenOnAll:
                        item.Text = "☀  Screen On All";
                        break;
                    case BotControlCommand.ScreenOffAll:
                        item.Text = "🌙  Screen Off All";
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
            
            // Hook up resize for region handling to avoid doing it on every paint
            btnActions.Resize += BtnActions_Resize;
            BtnActions_Resize(btnActions, EventArgs.Empty);
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
                    tsi.Enabled = Enum.TryParse(text.Replace(" ", "").Replace("&&", "&").Replace("&", "And"), out BotControlCommand cmd)
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
            _connectionInfoText = "Initializing...";
            mainPanel.Invalidate();
        }

        public void ReloadStatus()
        {
            var bot = GetBot()?.Bot;
            if (bot is null) return;
            // Bot name with TID format on second line (Fix repetitive IP-IP)
            _botNameText = bot.Connection.Name == bot.Connection.Label 
                ? bot.Connection.Name 
                : $"{bot.Connection.Name} - {bot.Connection.Label}";
            // Trade type will be updated in ReloadStatus(BotSource) with current time
            _routineTypeText = $"{State.InitialRoutine}";
            mainPanel.Invalidate();
        }

        public void ReloadStatus(BotSource<PokeBotState> b)
        {
            ReloadStatus();
            var bot = b.Bot;
            
            // Line 2: Bot name with TID format (Fix repetitive IP-IP)
            _botNameText = bot.Connection.Name == bot.Connection.Label 
                ? bot.Connection.Name 
                : $"{bot.Connection.Name} - {bot.Connection.Label}";
            
            // Line 3: Trade type with current time (12-hour format)
            var routineType = bot.Config.CurrentRoutineType == PokeRoutineType.Idle ? 
                State.InitialRoutine.ToString() : bot.Config.CurrentRoutineType.ToString();
            _routineTypeText = $"{routineType} @ {DateTime.Now:h:mm:ss tt}";
            
            // Line 4: Current activity with arrow
            _connectionInfoText = $"\u21aa {bot.LastLogged}";

            var botState = ReadBotState();
            // Line 1: Status text next to pulsing indicator
            _statusValueText = botState.ToUpper();

            // Check for recovery status
            var recoveryState = b.GetRecoveryState();
            if (recoveryState is { ConsecutiveFailures: > 0 })
            {
                _connectionInfoText += $" [Recovery Attempts: {recoveryState.ConsecutiveFailures}]";
            }

            switch (botState)
            {
                case "STOPPED":
                    currentStatusColor = SwitchGrey;
                    // Check if recovering
                    if (recoveryState is { IsRecovering: true })
                    {
                        currentStatusColor = CuztomOrange;
                        _statusValueText = "RECOVERING";
                    }
                    break;
                case "IDLE":
                case "IDLING":
                    currentStatusColor = SwitchYellow;
                    break;
                case "ERROR":
                    currentStatusColor = SwitchRed;
                    break;
                case "REBOOTING":
                    currentStatusColor = SwitchBlue;
                    _statusValueText = "CONNECTING...";
                    break;
                default:
                    currentStatusColor = SwitchGreen;
                    break;
            }
            
            statusIndicator.Invalidate();
            mainPanel.Invalidate(); // Trigger repaint for text

            var lastTime = bot.LastTime;
            if (!b.IsRunning)
            {
                currentStatusColor = SwitchGrey;
                statusIndicator.Invalidate();
                mainPanel.Invalidate();
                return;
            }

            if (!b.Bot.Connection.Connected)
            {
                currentStatusColor = SwitchBlue;
                _statusValueText = "CONNECTING...";
                statusIndicator.Invalidate();
                mainPanel.Invalidate();
                return;
            }

            var cfg = bot.Config;
            if (cfg.CurrentRoutineType == PokeRoutineType.Idle && cfg.NextRoutineType == PokeRoutineType.Idle)
            {
                currentStatusColor = SwitchYellow;
                statusIndicator.Invalidate();
                mainPanel.Invalidate();
                return;
            }

            if (LastUpdateStatus == lastTime)
                return;

            const int threshold = 100;
            Color good = cfg.Connection.Protocol == SwitchProtocol.USB ? SwitchBlue : SwitchGreen;
            Color bad = SwitchRed;

            var delta = DateTime.Now - lastTime;
            var seconds = delta.Seconds;

            LastUpdateStatus = lastTime;
            if (seconds > 2 * threshold)
            {
                statusIndicator.Invalidate();
                mainPanel.Invalidate();
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
            mainPanel.Invalidate();
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

        public void SendCommand(BotControlCommand cmd)
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
        }

        public void ResumeAnimations()
        {
            _suspendPainting = false;
            statusIndicator?.Invalidate();
        }

        private void BotController_Paint(object? sender, PaintEventArgs e)
        {
            if (_suspendPainting) return;
            
            // Transparent background - do not paint
        }

        private void MainPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_suspendPainting) return;
            
            var g = e.Graphics;
            g.CompositingMode = CompositingMode.SourceOver;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = mainPanel.ClientRectangle;
            // Adjust rect to avoid clipping border
            var drawRect = new RectangleF(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
            
            // Clean Tech Background
            using (var path = new GraphicsPath())
            {
                // Chamfered corners
                int chamfer = 20;
                path.AddLine(drawRect.Left + chamfer, drawRect.Top, drawRect.Right - chamfer, drawRect.Top);
                path.AddLine(drawRect.Right, drawRect.Top + chamfer, drawRect.Right, drawRect.Bottom - chamfer);
                path.AddLine(drawRect.Right - chamfer, drawRect.Bottom, drawRect.Left + chamfer, drawRect.Bottom);
                path.AddLine(drawRect.Left, drawRect.Bottom - chamfer, drawRect.Left, drawRect.Top + chamfer);
                path.CloseFigure();
                
                // 1. Background - Transparent Glass Effect
                // Fill with extremely low opacity to show background grid
                using (var brush = new LinearGradientBrush(drawRect, 
                    Color.FromArgb(10, 0, 0, 0),    // Crystal clear top
                    Color.FromArgb(40, 5, 5, 10),   // Very subtle tint bottom
                    90f))
                {
                    g.FillPath(brush, path);
                }
                
                // 2. Subtle Tech Grid Overlay (Alienware Style)
                // Draw faint diagonal lines clipped to the path
                g.SetClip(path);
                using (var pen = new Pen(Color.FromArgb(20, 200, 255, 255), 1))
                {
                    // Draw diagonal mesh
                    for (int i = -100; i < drawRect.Width + drawRect.Height; i += 15)
                    {
                         g.DrawLine(pen, drawRect.Left + i, drawRect.Top, drawRect.Left + i - drawRect.Height, drawRect.Bottom);
                    }
                }
                g.ResetClip();

                // 3. Determine Status Color
                var statusColor = currentStatusColor;
                if (statusColor == Color.Empty) statusColor = CuztomAccent;

                // 4. Draw Text
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Add a text backing plate for readability if needed (optional, keeping it clean for now)
                
                // Bot Name
                using (var b = new SolidBrush(Color.FromArgb(255, 255, 255)))
                {
                    // Slight shadow for text readability on transparent bg
                    using (var s = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                         g.DrawString(_botNameText, _fontBotName, s, 71, 16);
                         
                    g.DrawString(_botNameText, _fontBotName, b, 70, 15);
                }

                // Status Value
                using (var b = new SolidBrush(statusColor))
                {
                    // Text glow effect
                    for(int i=1; i<=2; i++)
                    {
                         using(var p = new Pen(Color.FromArgb(30/i, statusColor), i))
                             g.DrawString(_statusValueText, _fontStatus, b, 70, 42); 
                    }
                    g.DrawString(_statusValueText, _fontStatus, b, 70, 42);
                }

                // Routine Type
                using (var b = new SolidBrush(Color.FromArgb(200, 200, 200)))
                    g.DrawString(_routineTypeText, _fontRoutine, b, 70, 60);

                // Connection Info
                using (var b = new SolidBrush(Color.FromArgb(160, 160, 160)))
                {
                    TextRenderer.DrawText(g, _connectionInfoText, _fontConnection, 
                        new Rectangle(70, 78, (int)drawRect.Width - 80, 20), 
                        Color.FromArgb(160, 160, 160), 
                        TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
                }

                // 5. Border & Tech Accents
                if (_isHovered)
                {
                    // Bright neon border on hover
                    using (var pen = new Pen(statusColor, 2))
                    {
                        g.DrawPath(pen, path);
                    }
                    
                    // Outer Glow
                    using (var pen = new Pen(Color.FromArgb(80, statusColor), 6))
                    {
                        pen.LineJoin = LineJoin.Round;
                        g.DrawPath(pen, path);
                    }
                }
                else
                {
                    // Semi-transparent border normally
                    using (var pen = new Pen(Color.FromArgb(80, 80, 80, 90), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                    
                    // Corner accents (Alienware brackets)
                    using (var pen = new Pen(statusColor, 2))
                    {
                        float cornerLen = 15;
                        // Top Left Bracket
                        g.DrawLine(pen, drawRect.Left + chamfer, drawRect.Top, drawRect.Left + chamfer + cornerLen, drawRect.Top);
                        g.DrawLine(pen, drawRect.Left, drawRect.Top + chamfer, drawRect.Left, drawRect.Top + chamfer + cornerLen);
                        
                        // Bottom Right Bracket
                        g.DrawLine(pen, drawRect.Right - chamfer, drawRect.Bottom, drawRect.Right - chamfer - cornerLen, drawRect.Bottom);
                        g.DrawLine(pen, drawRect.Right, drawRect.Bottom - chamfer, drawRect.Right, drawRect.Bottom - chamfer - cornerLen);
                    }
                }
            }
        }

        private void StatusIndicator_Paint(object? sender, PaintEventArgs e)
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

        private void BtnActions_Resize(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            var rect = btn.ClientRectangle;
            
            using var path = new GraphicsPath();
            int chamfer = 6;
            path.AddLine(rect.Left + chamfer, rect.Top, rect.Right - chamfer, rect.Top);
            path.AddLine(rect.Right, rect.Top + chamfer, rect.Right, rect.Bottom - chamfer);
            path.AddLine(rect.Right - chamfer, rect.Bottom, rect.Left + chamfer, rect.Bottom);
            path.AddLine(rect.Left, rect.Bottom - chamfer, rect.Left, rect.Top + chamfer);
            path.CloseFigure();
            
            btn.Region = new Region(path);
        }

        private void BtnActions_Paint(object? sender, PaintEventArgs e)
        {
            if (_suspendPainting) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (sender is not Button btn) return;
            var rect = btn.ClientRectangle;

            // Region is now handled in BtnActions_Resize to improve performance

            // Chamfered button path for drawing
            using var path = new GraphicsPath();
            int chamfer = 6;
            path.AddLine(rect.Left + chamfer, rect.Top, rect.Right - chamfer, rect.Top);
            path.AddLine(rect.Right, rect.Top + chamfer, rect.Right, rect.Bottom - chamfer);
            path.AddLine(rect.Right - chamfer, rect.Bottom, rect.Left + chamfer, rect.Bottom);
            path.AddLine(rect.Left, rect.Bottom - chamfer, rect.Left, rect.Top + chamfer);
            path.CloseFigure();
            
            // Alien Tech Button Style
            var glowColor = buttonHovering ? Color.White : Color.Gray;

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

        private void BtnActions_Click(object? sender, EventArgs e)
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
            public CuztomMenuRenderer() : base(new CuztomColorTable()) 
            {
                this.RoundedEdges = false; 
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
            public override Color MenuItemBorder => Color.Transparent; 
            public override Color MenuBorder => Color.FromArgb(64, 64, 64);
            public override Color ToolStripDropDownBackground => Color.FromArgb(12, 12, 12);
            
            // Image Margin
            public override Color ImageMarginGradientBegin => Color.FromArgb(12, 12, 12);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(12, 12, 12);
            public override Color ImageMarginGradientEnd => Color.FromArgb(12, 12, 12);
            
            // Separators
            public override Color SeparatorDark => Color.FromArgb(64, 64, 64);
            public override Color SeparatorLight => Color.FromArgb(32, 32, 32);
            
            // Pressed State
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(20, 20, 20);
            public override Color MenuItemPressedGradientMiddle => Color.FromArgb(20, 20, 20);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(20, 20, 20);
            
            // Selected State
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(20, 20, 20);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(20, 20, 20);
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

