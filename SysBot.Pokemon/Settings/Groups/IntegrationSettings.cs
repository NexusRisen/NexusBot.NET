using System.ComponentModel;

namespace SysBot.Pokemon;

public class IntegrationSettings
{
    private const string Integration = nameof(Integration);

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public DiscordSettings Discord { get; set; } = new();

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TwitchSettings Twitch { get; set; } = new();

    [Category(Integration)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public YouTubeSettings YouTube { get; set; } = new();

    [Category(Integration), Description("Allows favored users to join the queue with a more favorable position than unfavored users.")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FavoredPrioritySettings Favoritism { get; set; } = new();

    [Category(Integration), Description("Configure generation of assets for streaming.")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public StreamSettings Stream { get; set; } = new();

    [Category(Integration), Description("Settings for the Web Control Panel server.")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public WebServerSettings WebServer { get; set; } = new();

    public override string ToString() => "Integration Settings";
}
