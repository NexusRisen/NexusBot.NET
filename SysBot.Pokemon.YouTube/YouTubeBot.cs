using Google.Apis.YouTube.v3.Data;
using PKHeX.Core;
using StreamingClient.Base.Util;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTube.Base;
using YouTube.Base.Clients;

namespace SysBot.Pokemon.YouTube;

public class YouTubeBot<T> : IDisposable where T : PKM, new()
{
    private readonly PokeTradeHub<T> Hub;

    private readonly YouTubeSettings Settings;

    private ChatClient? client;
    private readonly CancellationTokenSource _cts = new();
    private readonly Action<string> _echoForwarder;

    public YouTubeBot(YouTubeSettings settings, PokeTradeHub<T> hub)
    {
        Hub = hub;
        Settings = settings;
        Logger.LogOccurred += Logger_LogOccurred;
        client = default!;

        _echoForwarder = msg => client?.SendMessage(msg);

        Task.Run(async () =>
        {
            try
            {
                var connection = await YouTubeConnection.ConnectViaLocalhostOAuthBrowser(Settings.ClientID, Settings.ClientSecret, Scopes.scopes, true).ConfigureAwait(false);
                if (connection == null || _cts.IsCancellationRequested)
                    return;

                var channel = await connection.Channels.GetChannelByID(Settings.ChannelID).ConfigureAwait(false);
                if (channel == null || _cts.IsCancellationRequested)
                    return;

                client = new ChatClient(connection);
                client.OnMessagesReceived += Client_OnMessagesReceived;
                EchoUtil.Forwarders.Add(_echoForwarder);

                if (await client.Connect().ConfigureAwait(false))
                {
                    try { await Task.Delay(-1, _cts.Token).ConfigureAwait(false); } catch (OperationCanceledException) { }
                }
            }
            catch (Exception ex)
            {
                if (!_cts.IsCancellationRequested)
                    LogUtil.LogError(ex.Message, nameof(YouTubeBot<T>));
            }
        });
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        Logger.LogOccurred -= Logger_LogOccurred;
        if (client != null)
        {
            client.OnMessagesReceived -= Client_OnMessagesReceived;
            EchoUtil.Forwarders.Remove(_echoForwarder);
            try { client.Disconnect(); } catch { }
        }
    }

    private TradeQueueInfo<T> Info => Hub.Queues.Info;

    public void StartingDistribution(string message)
    {
        Task.Run(async () =>
        {
            try
            {
                await client.SendMessage("5...").ConfigureAwait(false);
                await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                await client.SendMessage("4...").ConfigureAwait(false);
                await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                await client.SendMessage("3...").ConfigureAwait(false);
                await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                await client.SendMessage("2...").ConfigureAwait(false);
                await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                await client.SendMessage("1...").ConfigureAwait(false);
                await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(message))
                    await client.SendMessage(message).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { LogUtil.LogError(ex.Message, nameof(YouTubeBot<T>)); }
        }, _cts.Token);
    }

    private static void Logger_LogOccurred(object? sender, Log e)
    {
        LogUtil.LogError(e.Message, nameof(YouTubeBot<T>));
    }

    private void Client_OnMessagesReceived(object? sender, IEnumerable<LiveChatMessage> messages)
    {
        foreach (var message in messages)
        {
            var msg = message.Snippet.TextMessageDetails.MessageText;
            try
            {
                var space = msg.IndexOf(' ');
                if (space < 0)
                    return;

                var cmd = msg[..(space + 1)];
                var args = msg[(space + 1)..];

                var response = HandleCommand(message, cmd, args);
                if (response.Length == 0)
                    return;
                client.SendMessage(response);
            }
            catch
            {
                // ignored
            }
        }
    }

    private string HandleCommand(LiveChatMessage m, string cmd, string args)
    {
        if (!m.AuthorDetails.IsChatOwner.Equals(true) && Settings.IsSudo(m.AuthorDetails.DisplayName))
            return string.Empty; // sudo only commands

        if (args.Length > 0)
            return "Commands don't use arguments. Try again with just the command code.";

        return cmd switch
        {
            "pr" => (Info.Hub.Ledy.Pool.Reload(Hub.Config.Folder.DistributeFolder)
                ? $"Reloaded from folder. Pool count: {Info.Hub.Ledy.Pool.Count}"
                : "Failed to reload from folder."),

            "pc" => $"The pool count is: {Info.Hub.Ledy.Pool.Count}",

            _ => string.Empty,
        };
    }
}
