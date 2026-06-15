using StoatSharp;
using SysBot.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Stoat.Helpers;

public class StoatManager(StoatSettings Config)
{
    public readonly StoatSettings Config = Config;

    public RemoteControlAccessList BlacklistedServers => Config.ServerBlacklist;

    public RemoteControlAccessList BlacklistedUsers => Config.UserBlacklist;

    public RemoteControlAccessList FavoredRoles => Config.RoleFavored;

    public string Owner { get; internal set; } = string.Empty;

    public RemoteControlAccessList RolesClone => Config.RoleCanClone;

    public RemoteControlAccessList RolesTrade => Config.RoleCanTrade;

    public RemoteControlAccessList RolesSeed => Config.RoleCanSeedCheckorSpecialRequest;

    public RemoteControlAccessList RolesDump => Config.RoleCanDump;

    public RemoteControlAccessList RolesFixOT => Config.RoleCanFixOT;

    public RemoteControlAccessList RolesRemoteControl => Config.RoleRemoteControl;

    public RemoteControlAccessList SudoStoat => Config.GlobalSudoList;

    public RemoteControlAccessList SudoRoles => Config.RoleSudo;

    public RemoteControlAccessList WhitelistedChannels => Config.ChannelWhitelist;

    private static readonly SemaphoreSlim _dmRateLimiter = new(1, 1);
    private static readonly ConcurrentDictionary<string, DMChannel> _dmChannels = new();
    private static DateTime _lastDmTime = DateTime.MinValue;
    private const int MinDmDelayMs = 2000;

    public bool CanUseCommandChannel(ulong channel) => (WhitelistedChannels.List.Count == 0 && WhitelistedChannels.AllowIfEmpty) || WhitelistedChannels.Contains(channel);

    public bool CanUseCommandUser(ulong uid) => !BlacklistedUsers.Contains(uid);

    public bool CanUseSudo(ulong uid) => SudoStoat.Contains(uid);

    public bool CanUseSudo(IEnumerable<string> roles) => roles.Any(SudoRoles.Contains);

    public bool GetHasRoleAccess(string type, IEnumerable<string> roles)
    {
        var set = GetSet(type);
        return set is { AllowIfEmpty: true, List.Count: 0 } || roles.Any(set.Contains);
    }

    public RequestSignificance GetSignificance(IEnumerable<string> roles)
    {
        var result = RequestSignificance.None;
        foreach (var r in roles)
        {
            if (SudoRoles.Contains(r))
                result = RequestSignificance.Favored;
            if (FavoredRoles.Contains(r))
                result = RequestSignificance.Favored;
        }
        return result;
    }

    private RemoteControlAccessList GetSet(string type) => type switch
    {
        "RolesClone" => RolesClone,
        "RolesTrade" => RolesTrade,
        "RolesSeed" => RolesSeed,
        "RolesDump" => RolesDump,
        "RolesFixOT" => RolesFixOT,
        "RolesRemoteControl" => RolesRemoteControl,
        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unexpected role access type: {type}"),
    };
}
