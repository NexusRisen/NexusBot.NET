using Discord;
using Discord.Net;
using SysBot.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public class DiscordManager(DiscordSettings Config)
{
    public readonly DiscordSettings Config = Config;

    public RemoteControlAccessList BlacklistedServers => Config.ServerBlacklist;

    public RemoteControlAccessList BlacklistedUsers => Config.UserBlacklist;

    public RemoteControlAccessList FavoredRoles => Config.RoleFavored;

    public ulong Owner { get; internal set; }

    public RemoteControlAccessList RolesClone => Config.RoleCanClone;

    public RemoteControlAccessList RolesTrade => Config.RoleCanTrade;

    public RemoteControlAccessList RolesSeed => Config.RoleCanSeedCheckorSpecialRequest;

    public RemoteControlAccessList RolesDump => Config.RoleCanDump;

    public RemoteControlAccessList RolesFixOT => Config.RoleCanFixOT;

    public RemoteControlAccessList RolesRemoteControl => Config.RoleRemoteControl;

    public RemoteControlAccessList SudoDiscord => Config.GlobalSudoList;

    public RemoteControlAccessList SudoRoles => Config.RoleSudo;

    public RemoteControlAccessList WhitelistedChannels => Config.ChannelWhitelist;

    private static readonly SemaphoreSlim _dmRateLimiter = new(1, 1);
    private static readonly ConcurrentDictionary<ulong, IDMChannel> _dmChannels = new();
    private static DateTime _lastDmTime = DateTime.MinValue;
    private const int MinDmDelayMs = 2000;

    public async Task<IDMChannel?> GetOrCreateDMAsync(IUser user)
    {
        await _dmRateLimiter.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_dmChannels.TryGetValue(user.Id, out var channel))
                return channel;

            var timeSinceLastDm = DateTime.UtcNow - _lastDmTime;
            if (timeSinceLastDm.TotalMilliseconds < MinDmDelayMs)
            {
                var remainingDelay = MinDmDelayMs - (int)timeSinceLastDm.TotalMilliseconds;
                await Task.Delay(remainingDelay).ConfigureAwait(false);
            }

            var dm = await user.CreateDMChannelAsync().ConfigureAwait(false);
            _dmChannels[user.Id] = dm;
            _lastDmTime = DateTime.UtcNow;
            return dm;
        }
        catch (HttpException ex) when (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (DiscordErrorCode)40003)
        {
            LogUtil.LogError($"Opening DMs too fast when creating DM channel for user {user.Username} ({user.Id}). Waiting 5 seconds...", "GetOrCreateDMAsync");
            await Task.Delay(5000).ConfigureAwait(false);

            try
            {
                var dm = await user.CreateDMChannelAsync().ConfigureAwait(false);
                _dmChannels[user.Id] = dm;
                _lastDmTime = DateTime.UtcNow;
                return dm;
            }
            catch (Exception retryEx)
            {
                LogUtil.LogError($"Failed to create DM channel after retry: {retryEx.Message}", "GetOrCreateDMAsync");
                return null;
            }
        }
        catch (ObjectDisposedException)
        {
            LogUtil.LogError("Discord client is disposed. Cannot create DM channel.", "GetOrCreateDMAsync");
            return null;
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Failed to create DM channel: {ex.Message}", "GetOrCreateDMAsync");
            return null;
        }
        finally
        {
            _dmRateLimiter.Release();
        }
    }

    public void ClearDMChannelCache(ulong userId) => _dmChannels.TryRemove(userId, out _);

    public static void ClearAllCaches() => _dmChannels.Clear();

    public bool CanUseCommandChannel(ulong channel) => (WhitelistedChannels.List.Count == 0 && WhitelistedChannels.AllowIfEmpty) || WhitelistedChannels.Contains(channel);

    public bool CanUseCommandUser(ulong uid) => !BlacklistedUsers.Contains(uid);

    public bool CanUseSudo(ulong uid) => SudoDiscord.Contains(uid);

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
