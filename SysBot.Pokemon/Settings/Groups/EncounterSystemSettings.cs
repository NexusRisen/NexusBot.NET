using System.ComponentModel;

namespace SysBot.Pokemon;

public class EncounterSystemSettings
{
    private const string BotEncounter = nameof(BotEncounter);

    [Browsable(false)]
    [Category(BotEncounter)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public EncounterSettings EncounterSWSH { get; set; } = new();

    [Browsable(false)]
    [Category(BotEncounter)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public RaidSettings RaidSWSH { get; set; } = new();

    [Browsable(false)]
    [Category(BotEncounter)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public SeedCheckSettings SeedCheckSWSH { get; set; } = new();

    [Browsable(false)]
    [Category(BotEncounter), Description("Stop conditions for EncounterBot.")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public StopConditionSettings StopConditions { get; set; } = new();

    public override string ToString() => "Encounter System Settings";
}
