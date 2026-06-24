using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.WinForms.Properties;
using SysBot.Pokemon.Z3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

using SysBot.Pokemon;

namespace SysBot.Pokemon.WinForms;

public sealed partial class Main : Form
{
    private readonly List<PokeBotState> Bots = [];

    private IPokeBotRunner RunningEnvironment { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal ProgramConfig Config { get; set; } = null!;

    public static bool IsUpdating { get; set; } = false;

    private bool _isFormLoading = true;
    private bool _isUpdatingUI;

#pragma warning disable CS8618

    public Main()

#pragma warning restore CS8618
    {
        InitializeComponent();
        CB_Mode.SelectedIndexChanged += new EventHandler(CB_Mode_SelectedIndexChanged);
        Load += async (sender, e) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (IsUpdating)
            return;
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        LogUtil.LogInfo("Starting UI Initialization...", "Form");

        PokeTradeBotSWSH.SeedChecker = new Z3SeedSearchHandler<PK8>();

        // Run update check in background to not block UI loading
        _ = Task.Run(async () =>
        {
            try
            {
                var (updateAvailable, updateRequired, newVersion) = await UpdateChecker.CheckForUpdatesAsync();
                if (updateAvailable)
                {
                    BeginInvoke(() =>
                    {
                        UpdateForm updateForm = new UpdateForm(updateRequired, newVersion, updateAvailable: true);
                        updateForm.ShowDialog();
                    });
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Update check failed: {ex.Message}", "Update");
            }
        });

        if (File.Exists(Program.ConfigPath))
        {
            var lines = File.ReadAllText(Program.ConfigPath);
            Config = JsonSerializer.Deserialize(lines, ProgramConfigContext.Default.ProgramConfig) ?? new ProgramConfig();
            
            LogLocalizer.CurrentLanguage = Config.Language;
            LogUtil.MessageTranslator = LogLocalizer.Translate;

            LogConfig.MaxArchiveFiles = Config.Hub.MaxArchiveFiles;
            LogConfig.LoggingEnabled = Config.Hub.LoggingEnabled;
            CB_Mode.SelectedValue = (int)Config.Mode;
            RunningEnvironment = GetRunner(Config);
            Config.Hub.Legality.CreateDefaults(Program.WorkingDirectory);
            foreach (var bot in Config.Bots)
            {
                bot.Initialize();
                AddBot(bot);
            }
        }
        else
        {
            Config = new ProgramConfig();

            LogLocalizer.CurrentLanguage = Config.Language;
            LogUtil.MessageTranslator = LogLocalizer.Translate;

            RunningEnvironment = GetRunner(Config);
            Config.Hub.Folder.CreateDefaults(Program.WorkingDirectory);
            Config.Hub.Legality.CreateDefaults(Program.WorkingDirectory);
        }
        LogUtil.LogInfo($"Config loaded and bots added: {sw.ElapsedMilliseconds}ms", "Form");

        // Create default folders if they do not exist even if a config file is present
        var dump = Config.Hub.Folder.DumpFolder;
        var distri = Config.Hub.Folder.DistributeFolder;
        var home = Config.Hub.Folder.HOMEReadyPKMFolder;
        var events = Config.Hub.Folder.EventsFolder;
        var trainer = Config.Hub.Legality.GeneratePathTrainerInfo;

        if ((!Directory.Exists(dump)) || (!Directory.Exists(distri)) || (!Directory.Exists(home)) || (!Directory.Exists(events)) || (!Directory.Exists(trainer)))
        {
            Config.Hub.Folder.CreateDefaults(Program.WorkingDirectory);
            Config.Hub.Legality.CreateDefaults(Program.WorkingDirectory);
            LogUtil.LogInfo("Required folders created.", "Form");
        }

        RTB_Logs.MaxLength = 32_767; // character length
        LoadControls();
        LogUtil.LogInfo($"Controls loaded: {sw.ElapsedMilliseconds}ms", "Form");

        Text = $"{(string.IsNullOrEmpty(Config.Hub.BotName) ? "NexusBot.NET" : Config.Hub.BotName)} {NexusBot.Version} ({Config.Mode})";
        L_Version.Text = $"{(string.IsNullOrEmpty(Config.Hub.BotName) ? "NexusBot.NET" : Config.Hub.BotName)} {NexusBot.Version}";
        _ = Task.Run(BotMonitor);
        InitUtil.InitializeStubs(Config.Mode);
        
        UILocalizer.ApplyLocalization(this, Config.Language);
        UpdateDropdownTranslations(Config.Language);
        UpdateBackgroundImage(Config.Mode);
        
        _isFormLoading = false;
        LogUtil.LogInfo($"UI Initialization complete: {sw.ElapsedMilliseconds}ms", "Form");
    }

    private static IPokeBotRunner GetRunner(ProgramConfig cfg) => cfg.Mode switch
    {
        ProgramMode.LGPE => new PokeBotRunnerImpl<PB7>(new PokeTradeHub<PB7>(cfg.Hub), new BotFactory7LGPE(), cfg),
        ProgramMode.SWSH => new PokeBotRunnerImpl<PK8>(new PokeTradeHub<PK8>(cfg.Hub), new BotFactory8SWSH(), cfg),
        ProgramMode.BDSP => new PokeBotRunnerImpl<PB8>(new PokeTradeHub<PB8>(cfg.Hub), new BotFactory8BS(), cfg),
        ProgramMode.LA => new PokeBotRunnerImpl<PA8>(new PokeTradeHub<PA8>(cfg.Hub), new BotFactory8LA(), cfg),
        ProgramMode.SV => new PokeBotRunnerImpl<PK9>(new PokeTradeHub<PK9>(cfg.Hub), new BotFactory9SV(), cfg),
        ProgramMode.PLZA => new PokeBotRunnerImpl<PA9>(new PokeTradeHub<PA9>(cfg.Hub), new BotFactory9PLZA(), cfg),
        _ => throw new IndexOutOfRangeException("Unsupported mode."),
    };

    private async Task BotMonitor()
    {
        while (!Disposing)
        {
            try
            {
                foreach (var c in FLP_Bots.Controls.OfType<BotController>())
                    c.ReadState();
            }
            catch
            {
                // Updating the collection by adding/removing bots will change the iterator
                // Can try a for-loop or ToArray, but those still don't prevent concurrent mutations of the array.
                // Just try, and if failed, ignore. Next loop will be fine. Locks on the collection are kinda overkill, since this task is not critical.
            }
            await Task.Delay(2_000).ConfigureAwait(false);
        }
    }

    private void LoadControls()
    {
        MinimumSize = Size;
        PG_Hub.SelectedObject = RunningEnvironment.Config;

        var routines = ((PokeRoutineType[])Enum.GetValues(typeof(PokeRoutineType))).Where(z => RunningEnvironment.SupportsRoutine(z));
        var list = routines.Select(z => new ComboItem(z.ToString(), (int)z)).ToArray();
        CB_Routine.DisplayMember = nameof(ComboItem.Text);
        CB_Routine.ValueMember = nameof(ComboItem.Value);
        CB_Routine.DataSource = list;
        CB_Routine.SelectedValue = (int)PokeRoutineType.FlexTrade; // default option

        var protocols = (SwitchProtocol[])Enum.GetValues(typeof(SwitchProtocol));
        var listP = protocols.Select(z => new ComboItem(z.ToString(), (int)z)).ToArray();
        CB_Protocol.DisplayMember = nameof(ComboItem.Text);
        CB_Protocol.ValueMember = nameof(ComboItem.Value);
        CB_Protocol.DataSource = listP;
        CB_Protocol.SelectedIndex = (int)SwitchProtocol.WiFi; // default option

        // Populate the game mode dropdown
        var gameModes = Enum.GetValues(typeof(ProgramMode))
            .Cast<ProgramMode>()
            .Where(m => m != ProgramMode.None) // Exclude the 'None' value
            .Select(mode => new { Text = mode.ToString(), Value = (int)mode })
        .ToList();

        CB_Mode.DisplayMember = "Text";
        CB_Mode.ValueMember = "Value";
        CB_Mode.DataSource = gameModes;

        // Set the current mode as selected in the dropdown
        CB_Mode.SelectedValue = (int)Config.Mode;

        // Populate themes from ThemeManager
        CB_Theme.Items.Clear();
        foreach (var t in ThemeManager.AllThemes)
        {
            CB_Theme.Items.Add(t.Name);
        }

        // Load the current theme from configuration and set it in the CB_Theme
        string theme = Config.Hub.ThemeOption;
        if (string.IsNullOrEmpty(theme) || !CB_Theme.Items.Contains(theme))
        {
            CB_Theme.SelectedIndex = 0;  // Set default selection if ThemeOption is empty or invalid
        }
        else
        {
            CB_Theme.SelectedItem = theme;  // Set the selected item in the combo box based on ThemeOption
        }

        // Populate languages
        var languages = new[]
        {
            new { Text = "English", Value = "en" },
            new { Text = "中文 (简体)", Value = "zh-Hans" },
            new { Text = "中文 (繁體)", Value = "zh-Hant" },
            new { Text = "Français", Value = "fr" },
            new { Text = "Deutsch", Value = "de" },
            new { Text = "Русский", Value = "ru" },
            new { Text = "Español", Value = "es" },
            new { Text = "Italiano", Value = "it" },
            new { Text = "日本語", Value = "ja" },
            new { Text = "한국어", Value = "ko" }
        };
        CB_Language.DisplayMember = "Text";
        CB_Language.ValueMember = "Value";
        CB_Language.DataSource = languages;
        CB_Language.SelectedValue = Config.Language;

        ThemeManager.ApplyTheme(this, CB_Theme.SelectedItem?.ToString() ?? "Dark Theme");

        LogUtil.Forwarders.RemoveAll(x => x is TextBoxForwarder);
        LogUtil.Forwarders.Add(new TextBoxForwarder(RTB_Logs));

        PB_CreditsLogo.Image = Resources.icon.ToBitmap();
    }

    private void UpdateDropdownTranslations(string lang)
    {
        _isUpdatingUI = true;
        try
        {
            // Translate Routines
            var routines = ((PokeRoutineType[])Enum.GetValues(typeof(PokeRoutineType))).Where(z => RunningEnvironment.SupportsRoutine(z));
            var list = routines.Select(z => new ComboItem(UILocalizer.Translate(z.ToString(), lang), (int)z)).ToArray();
            var selectedRoutine = CB_Routine.SelectedValue;
            CB_Routine.DisplayMember = nameof(ComboItem.Text);
            CB_Routine.ValueMember = nameof(ComboItem.Value);
            CB_Routine.DataSource = list;
            if (selectedRoutine != null) CB_Routine.SelectedValue = selectedRoutine;

            // Translate Modes
            var gameModes = Enum.GetValues(typeof(ProgramMode))
                .Cast<ProgramMode>()
                .Where(m => m != ProgramMode.None)
                .Select(mode => new { Text = UILocalizer.Translate(mode.ToString(), lang), Value = (int)mode })
                .ToList();
            var selectedMode = CB_Mode.SelectedValue;
            CB_Mode.DisplayMember = "Text";
            CB_Mode.ValueMember = "Value";
            CB_Mode.DataSource = gameModes;
            if (selectedMode != null) CB_Mode.SelectedValue = selectedMode;

            // Translate Themes
            var selectedTheme = CB_Theme.SelectedItem?.ToString();
            CB_Theme.Items.Clear();
            foreach (var t in ThemeManager.AllThemes)
            {
                CB_Theme.Items.Add(UILocalizer.Translate(t.Name, lang));
            }

            string currentThemeRaw = Config.Hub.ThemeOption;
            string currentThemeTranslated = UILocalizer.Translate(currentThemeRaw, lang);
            CB_Theme.SelectedItem = currentThemeTranslated;
        }
        finally
        {
            _isUpdatingUI = false;
        }
    }

    private ProgramConfig GetCurrentConfiguration()
    {

        Config.Bots = [.. Bots];
        return Config;
    }

    private void B_NavBots_Click(object sender, EventArgs e)
    {
        TC_Main.SelectedIndex = 0;
        L_Title.Text = UILocalizer.Translate("Tab_Bots", Config.Language).ToUpper();
    }

    private void B_NavHub_Click(object sender, EventArgs e)
    {
        TC_Main.SelectedIndex = 1;
        L_Title.Text = UILocalizer.Translate("Tab_Hub", Config.Language).ToUpper();
    }

    private void B_NavLogs_Click(object sender, EventArgs e)
    {
        TC_Main.SelectedIndex = 2;
        L_Title.Text = UILocalizer.Translate("Tab_Logs", Config.Language).ToUpper();
    }

    private void Main_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (IsUpdating)
        {
            return;
        }
        SaveCurrentConfig();
        var bots = RunningEnvironment;
        if (bots == null || !bots.IsRunning)
            return;

        async Task WaitUntilNotRunning()
        {
            while (bots.IsRunning)
                await Task.Delay(10).ConfigureAwait(false);
        }

        // Try to let all bots hard-stop before ending execution of the entire program.
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        bots.StopAll();
        Task.WhenAny(WaitUntilNotRunning(), Task.Delay(5_000)).ConfigureAwait(true).GetAwaiter().GetResult();
        bots.Dispose();
    }

    private void SaveCurrentConfig()
    {
        try
        {
            var cfg = GetCurrentConfiguration();
            var json = JsonSerializer.Serialize(cfg, ProgramConfigContext.Default.ProgramConfig);

            // Use atomic write operation to prevent corruption
            var tempPath = Program.ConfigPath + ".tmp";
            var backupPath = Program.ConfigPath + ".bak";

            // Write to temporary file first
            File.WriteAllText(tempPath, json);

            // Create backup of existing config if it exists
            if (File.Exists(Program.ConfigPath))
            {
                File.Copy(Program.ConfigPath, backupPath, true);
            }

            // Atomic rename operation
            File.Move(tempPath, Program.ConfigPath, true);

            // Delete backup after successful save
            if (File.Exists(backupPath))
            {
                try { File.Delete(backupPath); } catch { }
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Failed to save config: {ex.Message}", "Config");
        }
    }

    private async void CB_Mode_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isFormLoading || _isUpdatingUI) return; // Check to avoid processing during form loading or UI updates

        if (CB_Mode.SelectedValue is int selectedValue)
        {
            ProgramMode newMode = (ProgramMode)selectedValue;
            if (Config.Mode == newMode) return;

            Config.Mode = newMode;
            SaveCurrentConfig();
            
            // Immediately update UI visuals so the app doesn't appear "frozen" while the bots are stopping/starting
            UpdateBackgroundImage(newMode);
            UpdateDropdownTranslations(Config.Language);
            
            _isUpdatingUI = true;
            try
            {
                await UpdateRunnerAndUIAsync();
            }
            finally
            {
                _isUpdatingUI = false;
            }
        }
    }

    private void CB_Language_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isFormLoading || _isUpdatingUI) return;

        if (CB_Language.SelectedValue is string lang)
        {
            Config.Language = lang;
            LogLocalizer.CurrentLanguage = lang;
            SaveCurrentConfig();
            UILocalizer.ApplyLocalization(this, lang);
            UpdateDropdownTranslations(lang);

            // Refresh bot controls
            foreach (var c in FLP_Bots.Controls.OfType<BotController>())
            {
                c.ReloadStatus();
                c.TranslateMenu();
            }

            // Update title to match current tab language
            L_Title.Text = TC_Main.SelectedIndex switch
            {
                0 => UILocalizer.Translate("Tab_Bots", lang).ToUpper(),
                1 => UILocalizer.Translate("Tab_Hub", lang).ToUpper(),
                2 => UILocalizer.Translate("Tab_Logs", lang).ToUpper(),
                3 => UILocalizer.Translate("Tab_Credits", lang).ToUpper(),
                _ => L_Title.Text
            };
        }
    }

    private async Task UpdateRunnerAndUIAsync()
    {
        if (RunningEnvironment != null)
        {
            var oldEnv = RunningEnvironment;
            // Fire and forget the disposal so we don't block the UI update with long timeout waits
            _ = Task.Run(() =>
            {
                try { oldEnv.StopAll(); } catch { }
                try { oldEnv.Dispose(); } catch { }
            });
        }
        
        // Prepare list of bot controllers and their configs on the UI thread
        var controllers = FLP_Bots.Controls.OfType<BotController>().ToList();
        var states = controllers.Select(c => c.State).ToList();

        // Create bots and runner in background since runner initialization connects to the database synchronously
        IPokeBotRunner runner = null!;
        var newBots = new List<PokeRoutineExecutorBase>();
        await Task.Run(() =>
        {
            runner = GetRunner(Config);
            foreach (var state in states)
            {
                try
                {
                    var newBot = runner.CreateBotFromConfig(state);
                    newBots.Add(newBot);
                }
                catch (Exception ex)
                {
                    LogUtil.LogError($"Failed to re-add bot {state.Connection} to new environment: {ex.Message}", "Form");
                }
            }
        });

        RunningEnvironment = runner;

        // Initialize UI and add bots on the UI thread
        for (int i = 0; i < controllers.Count; i++)
        {
            controllers[i].Initialize(RunningEnvironment, states[i]);
        }
        
        foreach (var bot in newBots)
        {
            RunningEnvironment.Add(bot);
        }

        Text = $"{(string.IsNullOrEmpty(Config.Hub.BotName) ? "NexusBot.NET" : Config.Hub.BotName)} {NexusBot.Version} ({Config.Mode})";
    }

    private void B_Start_Click(object sender, EventArgs e)
    {
        SaveCurrentConfig();

        LogUtil.LogInfo("Starting all bots...", "Form");
        RunningEnvironment.InitializeStart();
        SendAll(BotControlCommand.Start);
        Tab_Logs.Select();

        if (Bots.Count == 0)
            WinFormsUtil.Alert("No bots configured, but all supporting services have been started.");
    }

    private void B_Restart_Click(object sender, EventArgs e)
    {
        B_Stop_Click(sender, e);
        Task.Run(async () =>
        {
            await Task.Delay(3_500).ConfigureAwait(false);
            SaveCurrentConfig();
            LogUtil.LogInfo("Restarting all the consoles...", "Form");
            RunningEnvironment.InitializeStart();
            SendAll(BotControlCommand.RebootAndStop);
            await Task.Delay(5_000).ConfigureAwait(false); // Add a delay before restarting the bot
            SendAll(BotControlCommand.Start); // Start the bot after the delay
            Tab_Logs.Select();
            if (Bots.Count == 0)
                WinFormsUtil.Alert("No bots configured, but all supporting services have been issued the reboot command.");
        });
    }

    private void UpdateBackgroundImage(ProgramMode mode)
    {
        FLP_Bots.BackgroundImage = mode switch
        {
            ProgramMode.PLZA => Resources.plza_mode_image,
            ProgramMode.SV => Resources.sv_mode_image,
            ProgramMode.SWSH => Resources.swsh_mode_image,
            ProgramMode.BDSP => Resources.bdsp_mode_image,
            ProgramMode.LA => Resources.pla_mode_image,
            ProgramMode.LGPE => Resources.lgpe_mode_image,
            _ => null,
        };
        FLP_Bots.BackgroundImageLayout = ImageLayout.Zoom;
    }

    private void SendAll(BotControlCommand cmd)
    {
        foreach (var c in FLP_Bots.Controls.OfType<BotController>())
            c.SendCommand(cmd);
    }

    private void B_Stop_Click(object sender, EventArgs e)
    {
        var env = RunningEnvironment;
        if (!env.IsRunning && (ModifierKeys & Keys.Alt) == 0)
        {
            WinFormsUtil.Alert("Nothing is currently running.");
            return;
        }

        var cmd = BotControlCommand.Stop;

        if ((ModifierKeys & Keys.Control) != 0 || (ModifierKeys & Keys.Shift) != 0) // either, because remembering which can be hard
        {
            if (env.IsRunning)
            {
                WinFormsUtil.Alert("Commanding all bots to Idle.", "Press Stop (without a modifier key) to hard-stop and unlock control, or press Stop with the modifier key again to resume.");
                cmd = BotControlCommand.Idle;
            }
            else
            {
                WinFormsUtil.Alert("Commanding all bots to resume their original task.", "Press Stop (without a modifier key) to hard-stop and unlock control.");
                cmd = BotControlCommand.Resume;
            }
        }
        else
        {
            env.StopAll();
        }
        SendAll(cmd);
    }

    private void B_New_Click(object sender, EventArgs e)
    {
        var cfg = CreateNewBotConfig();
        if (!AddBot(cfg))
        {
            WinFormsUtil.Alert("Unable to add bot; ensure details are valid and not duplicate with an already existing bot.");
            return;
        }
        System.Media.SystemSounds.Asterisk.Play();
    }

    private async void B_Update_Click(object sender, EventArgs e)
    {
        var (updateAvailable, updateRequired, newVersion) = await UpdateChecker.CheckForUpdatesAsync();
        if (!updateAvailable)
        {
            var result = MessageBox.Show(
                "You are on the latest version. Would you like to re-download the current version?",
                "Update Check",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UpdateForm updateForm = new UpdateForm(updateRequired, newVersion, updateAvailable: false);
                updateForm.ShowDialog();
            }
        }
        else
        {
            UpdateForm updateForm = new UpdateForm(updateRequired, newVersion, updateAvailable: true);
            updateForm.ShowDialog();
        }
    }

    private bool AddBot(PokeBotState cfg)
    {
        if (!cfg.IsValid())
            return false;

        if (Bots.Any(z => z.Connection.Equals(cfg.Connection)))
            return false;

        PokeRoutineExecutorBase newBot;
        try
        {
            newBot = RunningEnvironment.CreateBotFromConfig(cfg);
        }
        catch
        {
            Console.WriteLine($"Current Mode ({Config.Mode}) does not support this type of bot ({cfg.CurrentRoutineType}).");
            return false;
        }

        try
        {
            RunningEnvironment.Add(newBot);
        }
        catch (ArgumentException ex)
        {
            WinFormsUtil.Error(ex.Message);
            return false;
        }

        AddBotControl(cfg);
        Bots.Add(cfg);
        return true;
    }

    private void AddBotControl(PokeBotState cfg)
    {
        var row = new BotController { Width = FLP_Bots.Width };
        row.Initialize(RunningEnvironment, cfg);
        ThemeManager.ApplyTheme(row, ThemeManager.CurrentTheme);
        FLP_Bots.Controls.Add(row);
        FLP_Bots.SetFlowBreak(row, true);
        row.Click += (s, e) =>
        {
            var details = cfg.Connection;
            TB_IP.Text = details.IP;
            NUD_Port.Value = details.Port;
            CB_Protocol.SelectedIndex = (int)details.Protocol;
            CB_Routine.SelectedValue = (int)cfg.InitialRoutine;
        };

        row.Remove += (s, e) =>
        {
            Bots.Remove(row.State);
            RunningEnvironment.Remove(row.State, !RunningEnvironment.Config.SkipConsoleBotCreation);
            FLP_Bots.Controls.Remove(row);
        };
    }

    private PokeBotState CreateNewBotConfig()
    {
        var ip = TB_IP.Text;
        var port = (int)NUD_Port.Value;
        var cfg = BotConfigUtil.GetConfig<SwitchConnectionConfig>(ip, port);
        cfg.Protocol = (SwitchProtocol)WinFormsUtil.GetIndex(CB_Protocol);

        var pk = new PokeBotState { Connection = cfg };
        var type = (PokeRoutineType)WinFormsUtil.GetIndex(CB_Routine);
        pk.Initialize(type);
        return pk;
    }

    private void FLP_Bots_Resize(object sender, EventArgs e)
    {
        foreach (var c in FLP_Bots.Controls.OfType<BotController>())
            c.Width = FLP_Bots.Width;
    }

    private void CB_Protocol_SelectedIndexChanged(object sender, EventArgs e)
    {
        TB_IP.Visible = CB_Protocol.SelectedIndex == 0;
    }

    private void CB_Theme_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            string? selectedThemeTranslated = comboBox.SelectedItem?.ToString();
            if (selectedThemeTranslated == null) return;

            // Find the raw theme name back from the translated one
            string rawTheme = "Dark Theme";
            foreach (var t in ThemeManager.AllThemes)
            {
                if (UILocalizer.Translate(t.Name, Config.Language) == selectedThemeTranslated)
                {
                    rawTheme = t.Name;
                    break;
                }
            }

            Config.Hub.ThemeOption = rawTheme;
            SaveCurrentConfig();

            ThemeManager.ApplyTheme(this, rawTheme);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized && Config.Hub.MinimizeToTray)
        {
            Hide();
            trayIcon.Visible = true;
        }
    }

    private void trayRestore_Click(object sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        trayIcon.Visible = false;
    }

    private void trayExit_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        trayRestore_Click(sender, e);
    }

    private void B_HideTray_Click(object sender, EventArgs e)
    {
        Hide();
        trayIcon.Visible = true;
    }

    private void B_Credits_Click(object sender, EventArgs e)
    {
        TC_Main.SelectedIndex = 3;
        L_Title.Text = UILocalizer.Translate("Tab_Credits", Config.Language).ToUpper();
    }

    private void B_ReportIssue_Click(object sender, EventArgs e)
    {
        using var form = new ReportIssueForm();
        form.ShowDialog(this);
    }
}
