using System.ComponentModel;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SysBot.Pokemon;

public sealed class PokeTradeHubConfig
{
    [Category("Global")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public GlobalSettings Global { get; set; } = new();

    [Category("Trade System")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TradeSystemSettings TradeSystem { get; set; } = new();

    [Category("Web & Integrations")]
    [DisplayName("Web Integrations")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public IntegrationSettings Integration { get; set; } = new();

    [Browsable(false)]
    [Category("Encounter System")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public EncounterSystemSettings EncounterSystem { get; set; } = new();

    [Browsable(false)]
    public TimingSettings Timings => Global.Timings;

    [Browsable(false)]
    public TradeSettings Trade => TradeSystem.Settings;
}
