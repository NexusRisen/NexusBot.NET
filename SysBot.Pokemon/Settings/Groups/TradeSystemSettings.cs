using System.ComponentModel;

namespace SysBot.Pokemon;

public class TradeSystemSettings
{
    private const string BotTrade = nameof(BotTrade);
    private const string Integration = nameof(Integration);
    private const string Operation = nameof(Operation);

    [Category(BotTrade), Description("Settings for idle distribution trades.")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public DistributionSettings Distribution { get; set; } = new();

    [Category(Operation)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QueueSettings Queues { get; set; } = new();

    [Category(Integration), Description("Allows favored users to join the queue with a more favorable position than unfavored users.")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FavoredPrioritySettings Favoritism { get; set; } = new();

    [Category(BotTrade)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TradeSettings Settings { get; set; } = new();

    [Category(BotTrade)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TradeAbuseSettings Abuse { get; set; } = new();

    public override string ToString() => "Trade System Settings";
}
