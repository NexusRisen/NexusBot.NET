using System.ComponentModel;

namespace SysBot.Pokemon;

public class GlobalSettings
{
    private const string FeatureToggle = nameof(FeatureToggle);
    private const string Operation = nameof(Operation);
    private const string BotTrade = nameof(BotTrade);
    private const string Integration = nameof(Integration);

    [Category(BotTrade), Description("Name of the Discord Bot the Program is Running. This will Title the window for easier recognition. Requires program restart.")]
    public string BotName { get; set; } = string.Empty;

    [Browsable(false)]
    [Category(Integration), Description("Users Theme Option Choice.")]
    public string ThemeOption { get; set; } = string.Empty;

    [Category(FeatureToggle), Description("When enabled, the bot will press the B button occasionally when it is not processing anything (to avoid sleep).")]
    public bool AntiIdle { get; set; }

    [Category(FeatureToggle), Description("Enables text logs. Restart to apply changes.")]
    public bool LoggingEnabled { get; set; } = true;

    [Category(FeatureToggle), Description("Maximum number of old text log files to retain. Set this to <= 0 to disable log cleanup. Restart to apply changes.")]
    public int MaxArchiveFiles { get; set; } = 14;

    [Category(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FolderSettings Folder { get; set; } = new();

    [Category(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public LegalitySettings Legality { get; set; } = new();

    [Category(Operation), Description("Settings for automatic bot recovery after crashes.")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public RecoverySettings Recovery { get; set; } = new();

    [Category(Operation), Description("Add extra time for slower Switches.")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TimingSettings Timings { get; set; } = new();

    [Browsable(false)]
    [Category("Debug"), Description("Skips creating bots when the program is started; helpful for testing integrations.")]
    public bool SkipConsoleBotCreation { get; set; }

    [Category(Operation), Description("GitHub repository owner used for in-app updates.")]
    public string UpdateRepoOwner { get; set; } = "hexbyt3";

    [Category(Operation), Description("GitHub repository name used for in-app updates.")]
    public string UpdateRepoName { get; set; } = "PokeBot";

    public override string ToString() => "Global Settings";
}
