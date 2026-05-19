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
    private readonly PokeBotRunner<T> _runner;
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
        _runner = runner;
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

        runner.BotAdded += OnBotAdded;
        runner.BotRemoved += OnBotRemoved;

        SysKookSettings.Manager = Manager;
        SysKookSettings.HubConfig = Hub.Config;

        _client = new KookSocketClient(new KookSocketConfig
        {
            LogLevel = LogSeverity.Info,
        });

        _client.Log += Log;
        _client.Disconnected += OnDisconnected;
        _client.MessageReceived += OnMessageReceived;
    }

    private Task OnDisconnected(Exception exception)
    {
        LogUtil.LogText($"Kook connection lost. Reason: {exception?.Message ?? "Unknown"}");
        Task.Run(() => ReconnectAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    private Task OnMessageReceived(SocketMessage msg, SocketUser author, ISocketMessageChannel channel) => HandleMessageAsync(msg);

    private void OnBotAdded(object? sender, PokeRoutineExecutorBase b)
    {
        if (b is ITradeBot tradeBot)
        {
            tradeBot.ConnectionError += OnBotConnectionError;
            tradeBot.ConnectionSuccess += OnBotConnectionSuccess;
        }
    }

    private void OnBotRemoved(object? sender, PokeRoutineExecutorBase b)
    {
        if (b is ITradeBot tradeBot)
        {
            tradeBot.ConnectionError -= OnBotConnectionError;
            tradeBot.ConnectionSuccess -= OnBotConnectionSuccess;
        }
    }

    private void OnBotConnectionError(object? sender, Exception ex) => Task.Run(() => HandleBotStop(ex), _cts.Token);
    private void OnBotConnectionSuccess(object? sender, EventArgs e) => Task.Run(HandleBotStart, _cts.Token);

    private Task HandleBotStop(Exception ex)
    {
        LogUtil.LogInfo("SysKook", $"Bot connection error: {ex.Message}. Notifying Kook users (TODO).");
        return Task.CompletedTask;
    }

    private Task HandleBotStart()
    {
        LogUtil.LogInfo("SysKook", "Bot connection success. Notifying Kook users (TODO).");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

        _client.Log -= Log;
        _client.Disconnected -= OnDisconnected;
        _client.MessageReceived -= OnMessageReceived;

        if (Runner != null)
        {
            Runner.BotAdded -= OnBotAdded;
            Runner.BotRemoved -= OnBotRemoved;
            foreach (var bot in Runner.Hub.Bots.ToArray())
            {
                if (bot is ITradeBot tradeBot)
                {
                    tradeBot.ConnectionError -= OnBotConnectionError;
                    tradeBot.ConnectionSuccess -= OnBotConnectionSuccess;
                }
            }
        }
        
        // Clear static reference to prevent memory leaks when reloading environment
        if (ReferenceEquals(Runner, _runner))
            Runner = null!;

        try
        {
            _client.StopAsync().GetAwaiter().GetResult();
            _client.Dispose();
        }
        catch { }
    }

    public static PokeBotRunner<T> Runner { get; private set; } = default!;

    private async Task ReconnectAsync(CancellationToken token)
    {
        const int maxRetries = 5;
        const int delayBetweenRetries = 5000;

        try { await Task.Delay(10000, token).ConfigureAwait(false); } catch (OperationCanceledException) { return; }

        for (int i = 0; i < maxRetries && !token.IsCancellationRequested; i++)
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
                if (i < maxRetries - 1)
                {
                    try { await Task.Delay(delayBetweenRetries, token).ConfigureAwait(false); } catch (OperationCanceledException) { return; }
                }
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
        
        if (cmd == "ts")
        {
            var response = await message.Channel.SendTextAsync($"Hello {message.Author.Username}, I am online!");
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
            return;
        }

        if (cmd == "trade" || cmd == "t")
        {
            await HandleTradeCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "itemtrade" || cmd == "item" || cmd == "it")
        {
            await HandleItemTradeCommandAsync(message, parts.Skip(1).ToList());
        }
    }

    private async Task HandleItemTradeCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count == 0)
        {
            await message.Channel.SendTextAsync("Please provide at least one item name.");
            return;
        }

        var itemInput = string.Join(" ", args);
        var itemNames = itemInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        if (typeof(T) == typeof(PB7) && itemNames.Length > 1)
        {
            await message.Channel.SendTextAsync("Batch trades are not supported in Let's Go Pikachu/Eevee. You can only request one item at a time.");
            return;
        }

        var batchSettings = Hub.Config.Trade.BatchSettings;
        var maxItemBatch = batchSettings.MaxItemBatchAmount;

        if (itemNames.Length > 1 && !batchSettings.AllowBatchTrades)
        {
            await message.Channel.SendTextAsync("Batch trades are currently disabled. You can only request one item at a time.");
            return;
        }

        if (itemNames.Length > maxItemBatch)
        {
            await message.Channel.SendTextAsync($"You can only request up to {maxItemBatch} items at a time.");
            return;
        }

        var pkmList = new List<T>();
        var errors = new List<string>();

        var tradeConfig = Hub.Config.Trade.TradeConfiguration;
        Species species = tradeConfig.ItemTradeSpecies == Species.None ? Species.Pikachu : tradeConfig.ItemTradeSpecies;
        var baseSpeciesName = SpeciesName.GetSpeciesNameGeneration((ushort)species, 2, 8);

        for (int i = 0; i < itemNames.Length; i++)
        {
            var itemName = itemNames[i];
            var set = new ShowdownSet($"{baseSpeciesName} @ {itemName}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var pkm = sav.GetLegal(template, out _);

            if (pkm == null)
            {
                errors.Add($"Item '{itemName}': Failed to legalize.");
                continue;
            }

            pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;

            if (pkm.HeldItem == 0)
            {
                errors.Add($"Item '{itemName}': Unrecognized item.");
                continue;
            }

            if (TradeRestrictions.IsUntradableHeld(pkm.Context, pkm.HeldItem))
            {
                errors.Add($"Item '{itemName}': Untradable item.");
                continue;
            }

            var la = new LegalityAnalysis(pkm);
            if (pkm is not T pk || !la.Valid)
            {
                errors.Add($"Item '{itemName}': Invalid Pokémon generated.");
                continue;
            }

            pk.ResetPartyStats();
            pkmList.Add(pk);
        }

        if (errors.Count > 0)
        {
            await message.Channel.SendTextAsync("Errors occurred:\n" + string.Join("\n", errors));
            return;
        }

        if (pkmList.Count == 0) return;

        var code = Hub.Queues.Info.GetRandomTradeCode(message.Author.Id);

        if (pkmList.Count == 1)
        {
            await KookHelper<T>.AddToQueueAsync(message, code, message.Author.Username, pkmList[0], message.Author, _client);
        }
        else
        {
            await KookHelper<T>.AddBatchContainerToQueueAsync(message, code, message.Author.Username, pkmList[0], pkmList, message.Author, _client);
        }
    }

    private async Task HandleTradeCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count == 0)
        {
            await message.Channel.SendTextAsync("Please provide a Showdown set or a trade code.");
            return;
        }

        // Detect LGPE Picto Codes
        List<Pictocodes>? lgCode = null;
        int code = 0;
        string showdownSet = string.Empty;

        if (args.Count >= 3)
        {
            var potentialLgCode = PictocodeConverter.ConvertFromStrings(args.Take(3).ToList());
            if (potentialLgCode.Count == 3)
            {
                lgCode = potentialLgCode;
                showdownSet = string.Join("\n", args.Skip(3));
            }
        }

        if (lgCode == null)
        {
            if (int.TryParse(args[0], out code))
            {
                showdownSet = string.Join("\n", args.Skip(1));
            }
            else
            {
                code = Hub.Queues.Info.GetRandomTradeCode(message.Author.Id);
                showdownSet = string.Join("\n", args);
            }
        }

        if (string.IsNullOrWhiteSpace(showdownSet))
        {
            await message.Channel.SendTextAsync("Please provide a Showdown set.");
            return;
        }

        var pk = await KookHelper<T>.ProcessShowdownSetAsync(showdownSet);
        if (pk == null)
        {
            await message.Channel.SendTextAsync("Oops! I couldn't parse that Showdown set.");
            return;
        }

        await KookHelper<T>.AddToQueueAsync(message, code, message.Author.Username, pk, message.Author, _client, lgCode);
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
