using System;
using System.ComponentModel;
using static SysBot.Pokemon.TradeSettings;

namespace SysBot.Pokemon;

public class SlackSettings
{
    private const string Channels = nameof(Channels);
    private const string Operation = nameof(Operation);
    private const string Roles = nameof(Roles);
    private const string Startup = nameof(Startup);
    private const string Users = nameof(Users);

    [Category(Startup), Description("Slack Bot Token (xoxb-...)."), DisplayName("Bot Token")]
    public string BotToken { get; set; } = string.Empty;

    [Category(Startup), Description("Slack App-Level Token for Socket Mode (xapp-...)."), DisplayName("App Level Token")]
    public string AppLevelToken { get; set; } = string.Empty;

    [Category(Channels), Description("Channels with these IDs are the only channels where the bot acknowledges commands."), DisplayName("Channel Whitelist")]
    public RemoteControlAccessList ChannelWhitelist { get; set; } = new();

    [Category(Startup), Description("Bot command prefix."), DisplayName("Default Command Prefix")]
    public string CommandPrefix { get; set; } = "$";

    [Category(Users), Description("Comma separated Slack user IDs that will have sudo access to the Bot Hub."), DisplayName("Global Sudo List")]
    public RemoteControlAccessList GlobalSudoList { get; set; } = new();

    [Category(Operation), Description("When enabled, the bot will automatically delete error messages and user commands after a delay (if possible in Slack)."), DisplayName("Message Deletion")]
    public bool MessageDeletionEnabled { get; set; } = true;

    [Category(Operation), Description("Number of seconds to wait before deleting bot error/response messages."), DisplayName("Delete Message Delay")]
    public int ErrorMessageDeleteDelaySeconds { get; set; } = 10;

    [Category(Operation), Description("When enabled, the bot will announce its status (Online/Offline) in the whitelisted channels."), DisplayName("Bot Status Announcement")]
    public bool BotStatusAnnouncement { get; set; } = true;

    [Category(Operation), Description("Emoji to show when the bot is online."), DisplayName("Online Emoji")]
    public string OnlineEmoji { get; set; } = "🟢";

    [Category(Operation), Description("Emoji to show when the bot is offline."), DisplayName("Offline Emoji")]
    public string OfflineEmoji { get; set; } = "🔴";

    [Category(Roles), Description("Users with this role are allowed to enter the Clone queue."), DisplayName("Role can Clone")]
    public RemoteControlAccessList RoleCanClone { get; set; } = new() { AllowIfEmpty = true };

    [Category(Roles), Description("Users with this role are allowed to enter the FixOT queue."), DisplayName("Role can FixOT")]
    public RemoteControlAccessList RoleCanFixOT { get; set; } = new() { AllowIfEmpty = true };

    [Category(Roles), Description("Users with this role are allowed to enter the Trade queue."), DisplayName("Role can Trade")]
    public RemoteControlAccessList RoleCanTrade { get; set; } = new() { AllowIfEmpty = true };

    [Category(Roles), Description("Users with this role are allowed to join the queue with a better position."), DisplayName("Favored Roles")]
    public RemoteControlAccessList RoleFavored { get; set; } = new() { AllowIfEmpty = false };

    [Category(Roles), Description("Users with this role are allowed to bypass command restrictions."), DisplayName("Allowed Sudo Roles")]
    public RemoteControlAccessList RoleSudo { get; set; } = new() { AllowIfEmpty = false };

    [Category(Users), Description("Users with these user IDs cannot use the bot."), DisplayName("User Blacklist")]
    public RemoteControlAccessList UserBlacklist { get; set; } = new();

    public override string ToString() => "Slack Integration Settings";
}
