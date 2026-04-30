using System;
using System.ComponentModel;
using static SysBot.Pokemon.TradeSettings;

namespace SysBot.Pokemon;

public class KookSettings
{
    private const string Channels = nameof(Channels);

    private const string Operation = nameof(Operation);

    private const string Startup = nameof(Startup);

    private const string Users = nameof(Users);

    [Category(Startup), Description("Bot login token."), DisplayName("Kook Bot Token")]
    public string Token { get; set; } = string.Empty;

    [Category(Channels), Description("Channels with these IDs are the only channels where the bot acknowledges commands."), DisplayName("Channel Whitelist")]
    public RemoteControlAccessList ChannelWhitelist { get; set; } = new();

    [Category(Startup), Description("Bot command prefix."), DisplayName("Default Command Prefix")]
    public string CommandPrefix { get; set; } = "$";

    [Category(Users), Description("Comma separated Kook user IDs that will have sudo access to the Bot Hub."), DisplayName("Global Sudo List")]
    public RemoteControlAccessList GlobalSudoList { get; set; } = new();

    [Category(Operation), Description("When enabled, the bot will automatically delete error messages and user commands after a delay."), DisplayName("Message Deletion")]
    public bool MessageDeletionEnabled { get; set; } = true;

    [Category(Operation), Description("Number of seconds to wait before deleting bot error/response messages."), DisplayName("Delete Message Delay")]
    public int ErrorMessageDeleteDelaySeconds { get; set; } = 10;

    public override string ToString() => "Kook Integration Settings";
}
