using Kook;
using Kook.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public static class SysKookSettings
{
    public static PokeTradeHubConfig HubConfig { get; internal set; } = default!;

    public static KookManager Manager { get; internal set; } = default!;

    public static KookSettings Settings => Manager.Config;
}

public sealed class SysKook<T> : IDisposable where T : PKM, new()
{
    public readonly PokeTradeHub<T> Hub;
    private readonly ProgramConfig _config;
    private readonly KookSocketClient _client;
    // Kook.Net doesn't have a direct CommandService like Discord.Net yet in some versions, 
    // but we can implement a simple one or use Kook.Net.Commands if available.
    // For now, let's keep it simple.

    private readonly HashSet<string> _validCommands = new HashSet<string>
    {
        "trade", "t", "clone", "fixOT", "fix", "f", "dittoTrade", "ditto", "dt", "itemTrade", "item", "it",
        "egg", "Egg", "hidetrade", "ht", "batchTrade", "bt", "listevents", "le",
        "eventrequest", "er", "battlereadylist", "brl", "battlereadyrequest", "brr", "pokepaste", "pp",
        "PokePaste", "PP", "randomteam", "rt", "RandomTeam", "Rt", "specialrequestpokemon", "srp",
        "queueStatus", "qs", "queueClear", "qc", "ts", "tc", "deleteTradeCode", "dtc", "mysteryegg", "me"
    };

    private readonly KookManager Manager;
    private readonly CancellationTokenSource _cts = new();

    public SysKook(PokeBotRunner<T> runner, ProgramConfig config)
    {
        Runner = runner;
        Hub = runner.Hub;
        Manager = new KookManager(Hub.Config.Kook);
        _config = config;

        foreach (var bot in runner.Hub.Bots.ToArray())
        {
            if (bot is ITradeBot tradeBot)
            {
                tradeBot.ConnectionError += OnBotConnectionError;
                tradeBot.ConnectionSuccess += OnBotConnectionSuccess;
            }
        }
        SysKookSettings.Manager = Manager;
        SysKookSettings.HubConfig = Hub.Config;

        _client = new KookSocketClient(new KookSocketConfig
        {
            LogLevel = LogSeverity.Info,
        });

        _client.Log += Log;
        
        _client.Disconnected += (exception) =>
        {
            LogUtil.LogText($"Kook connection lost. Reason: {exception?.Message ?? "Unknown"}");
            Task.Run(() => ReconnectAsync());
            return Task.CompletedTask;
        };

        _client.MessageReceived += (msg, author, channel) => HandleMessageAsync(msg);
    }

    private void OnBotConnectionError(object? sender, Exception ex) => Task.Run(HandleBotStop);
    private void OnBotConnectionSuccess(object? sender, EventArgs e) => Task.Run(HandleBotStart);

    private Task HandleBotStop() => Task.CompletedTask; // TODO: Implement
    private Task HandleBotStart() => Task.CompletedTask; // TODO: Implement

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

        _client.Log -= Log;
        _client.MessageReceived -= (msg, author, channel) => HandleMessageAsync(msg);

        foreach (var bot in Runner.Hub.Bots.ToArray())
        {
            if (bot is ITradeBot tradeBot)
            {
                tradeBot.ConnectionError -= OnBotConnectionError;
                tradeBot.ConnectionSuccess -= OnBotConnectionSuccess;
            }
        }

        try
        {
            _client.StopAsync().GetAwaiter().GetResult();
            _client.Dispose();
        }
        catch { }
    }

    public static PokeBotRunner<T> Runner { get; private set; } = default!;

    private async Task ReconnectAsync()
    {
        const int maxRetries = 5;
        const int delayBetweenRetries = 5000;

        await Task.Delay(10000).ConfigureAwait(false);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (_client.ConnectionState == ConnectionState.Connected) return;

                await _client.LoginAsync(TokenType.Bot, Hub.Config.Kook.Token).ConfigureAwait(false);
                await _client.StartAsync().ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                LogUtil.LogText($"Kook reconnection attempt {i + 1} failed: {ex.Message}");
                if (i < maxRetries - 1) await Task.Delay(delayBetweenRetries).ConfigureAwait(false);
            }
        }
    }

    public async Task MainAsync(string token, CancellationToken ct)
    {
        try
        {
            await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            await Task.Delay(-1, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            LogUtil.LogError(ex.Message, "SysKook");
        }
    }

    private Task Log(LogMessage msg)
    {
        LogUtil.LogText(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage message)
    {
        if (message.Author.IsBot ?? false) return;
        
        // 1. Channel Whitelist Check
        if (!Manager.CanUseCommandChannel(message.Channel.Id)) return;

        var content = message.Content;
        var prefix = Hub.Config.Kook.CommandPrefix;
        
        if (!content.StartsWith(prefix)) return;
        
        var parts = content.Substring(prefix.Length).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;
        
        var cmd = parts[0].ToLower();
        if (!_validCommands.Contains(cmd)) return;

        // 2. Global Sudo Check (Example)
        bool isSudo = Hub.Config.Kook.GlobalSudoList.Contains(message.Author.Id);
        
        LogUtil.LogText($"Kook Command received: {cmd} from {message.Author.Username} (Sudo: {isSudo})");
        
        // Example response with optional deletion
        if (cmd == "ts")
        {
            var response = await message.Channel.SendTextAsync($"Hello {message.Author.Username}, I am online!");
            
            // 3. Message Deletion Logic
            if (Hub.Config.Kook.MessageDeletionEnabled)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(Hub.Config.Kook.ErrorMessageDeleteDelaySeconds * 1000);
                    try 
                    { 
                        var msg = await response.DownloadAsync();
                        if (msg != null) await msg.DeleteAsync(); 
                    } 
                    catch { }
                });
            }
        }
    }
}

public class KookManager(KookSettings Config)
{
    public readonly KookSettings Config = Config;
    public bool CanUseCommandChannel(ulong channelId) 
    {
        if (Config.ChannelWhitelist.List.Count == 0)
            return Config.ChannelWhitelist.AllowIfEmpty;
        return Config.ChannelWhitelist.Contains(channelId);
    }
}
