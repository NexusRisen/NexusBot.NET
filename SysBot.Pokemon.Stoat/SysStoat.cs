using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.Stoat.Commands;
using SysBot.Pokemon.Stoat.Helpers;
using StoatSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Reflection;

namespace SysBot.Pokemon.Stoat;

public static class SysStoatSettings
{
    public static PokeTradeHubConfig HubConfig { get; internal set; } = default!;
    public static StoatSettings Settings => HubConfig.Stoat;
}

public sealed partial class SysStoat<T> : IDisposable where T : PKM, new()
{
    public readonly PokeTradeHub<T> Hub;
    private readonly PokeBotRunner<T> _runner;
    private readonly ProgramConfig _config;
    
    private StoatClient _client = null!;
    private readonly CommandRegistry<T> _commandRegistry = new();
    private readonly CancellationTokenSource _cts = new();

    public SysStoat(PokeBotRunner<T> runner, ProgramConfig config)
    {
        _runner = runner;
        Runner = runner;
        Hub = runner.Hub;
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

        SysStoatSettings.HubConfig = Hub.Config;
    }

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

    public async Task AnnounceBotStatus(string status, bool isOnline)
    {
        if (!SysStoatSettings.Settings.ChannelStatus)
            return;

        var botName = string.IsNullOrEmpty(Hub.Config.BotName) ? "DudeBot" : Hub.Config.BotName;
        var emoji = isOnline ? SysStoatSettings.Settings.OnlineEmoji : SysStoatSettings.Settings.OfflineEmoji;
        var fullStatusMessage = $"{emoji} **Status**: {botName} is {status}!";

        foreach (var channelId in SysStoatSettings.Settings.ChannelWhitelist.List.Select(c => c.ID.ToString())) // wait, ids might be custom
        {
            try
            {
                // We don't have the exact channel string easily from ulong config if the user configured it poorly, 
                // but we can try iterating through the actual string. Wait, we don't have a way to convert ulong back to Revolt string.
                // It's a limitation for Channel Status unless configured correctly or using Stoat-specific lists.
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo("SysStoat", $"AnnounceBotStatus: Exception: {ex.Message}");
            }
        }
    }

    private async Task HandleBotStop(Exception ex)
    {
        LogUtil.LogInfo("SysStoat", $"Bot connection error: {ex.Message}.");
        await AnnounceBotStatus("Offline", false);
    }

    private async Task HandleBotStart()
    {
        LogUtil.LogInfo("SysStoat", "Bot connection success.");
        await AnnounceBotStatus("Online", true);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

        foreach (var bot in Hub.Bots.ToArray())
        {
            if (bot is ITradeBot tradeBot)
            {
                tradeBot.ConnectionError -= OnBotConnectionError;
                tradeBot.ConnectionSuccess -= OnBotConnectionSuccess;
            }
        }

        if (Runner != null)
        {
            Runner.BotAdded -= OnBotAdded;
            Runner.BotRemoved -= OnBotRemoved;
        }
        
        if (ReferenceEquals(Runner, _runner))
            Runner = null!;

        if (ReferenceEquals(SysStoatSettings.HubConfig, Hub.Config))
            SysStoatSettings.HubConfig = default!;

        try
        {
            _client?.StopAsync();
        }
        catch { }
    }

    public static PokeBotRunner<T> Runner { get; private set; } = default!;

    public async Task MainAsync(string token, CancellationToken ct)
    {
        try
        {
            _client = new StoatClient(ClientMode.WebSocket, new ClientConfig());
            await _client.LoginAsync(token, AccountType.Bot);
            
            _client.OnMessageRecieved += Client_OnMessageReceived;
            _client.OnReady += Client_OnReady;

            await _client.StartAsync().ConfigureAwait(false);

            _ = MonitorStatusAsync(ct);

            await Task.Delay(-1, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            LogUtil.LogError(ex.Message, "SysStoat");
        }
    }

    public async Task UpdateStatusAsync(string message)
    {
        try
        {
            if (_client == null || _client.Token == null) return;
            string gameStatus = message ?? Hub.Config.Stoat.BotGameStatus ?? "Trading Pokémon";
            var json = $"{{\"status\":{{\"presence\":\"Online\",\"text\":\"{gameStatus}\"}}}}";
            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-bot-token", Hub.Config.Stoat.Token);
            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var apiUrl = _client.Config.ApiUrl;
            if (!apiUrl.EndsWith("/")) apiUrl += "/";
            var req = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"), apiUrl + "users/@me") { Content = content };
            await httpClient.SendAsync(req);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Failed to update Stoat bot presence: {ex.Message}", "SysStoat");
        }
    }

    private async Task MonitorStatusAsync(CancellationToken token)
    {
        const int Interval = 60; // seconds
        while (!token.IsCancellationRequested)
        {
            await UpdateStatusAsync(null);
            await Task.Delay(Interval * 1000, token).ConfigureAwait(false);
        }
    }

    private async void Client_OnReady(SelfUser selfUser)
    {
        LogUtil.LogInfo("Stoat Bot Connected successfully.", "SysStoat");
        await UpdateStatusAsync(null);
    }

    private async void Client_OnMessageReceived(Message message)
    {
        if (message is not UserMessage userMessage || message.Author?.IsBot == true) return;

        ulong channelIdNumeric = StoatHelper<T>.ConvertId(message.ChannelId);
        if (!Hub.Config.Stoat.ChannelWhitelist.Contains(channelIdNumeric) && Hub.Config.Stoat.ChannelWhitelist.List.Count > 0)
            return;

        var content = userMessage.Content;
        if (string.IsNullOrEmpty(content) && userMessage.Attachments.Count > 0)
        {
            // Allow processing of raw attachments with .t by injecting the trade command
            content = $"{Hub.Config.Stoat.CommandPrefix}t";
        }
        if (string.IsNullOrEmpty(content)) return;

        var prefix = Hub.Config.Stoat.CommandPrefix;
        if (!content.StartsWith(prefix)) return;
        var parts = content.Substring(prefix.Length).Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;
        
        var cmd = parts[0].ToLower();

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);

        if (Hub.Config.Stoat.UserBlacklist.Contains(userIdNumeric)) return;

        bool isKnownCommand = await _commandRegistry.TryExecuteCommandAsync(cmd, userMessage, parts.Skip(1).ToList());
        if (isKnownCommand)
        {
            LogUtil.LogText($"Stoat Command executed: {cmd} from {message.Author?.Username ?? "Unknown"}");
        }
    }

    public async Task<bool> CheckPermissions(UserMessage message, RemoteControlAccessList allowedRoles, bool sudoOnly = false)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Config.Stoat.GlobalSudoList.Contains(userIdNumeric)) return true;
        if (sudoOnly) 
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "This command is restricted to bot administrators.");
            return false;
        }

        if (allowedRoles.AllowIfEmpty && allowedRoles.List.Count == 0) return true;

        await StoatHelper<T>.SendAsync(_client, message.ChannelId, "You do not have the required permissions to use this command.");
        return false;
    }

    public StoatClient Client => _client;
}
