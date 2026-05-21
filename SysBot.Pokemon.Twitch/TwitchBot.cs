using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace SysBot.Pokemon.Twitch
{
    public class TwitchBot<T> : IDisposable where T : PKM, new()
    {
        internal static readonly List<TwitchQueue<T>> QueuePool = new();

        private static PokeTradeHub<T> Hub = default!;

        private readonly string Channel;

        private readonly TwitchClient client;
        private readonly WebSocketClient? _webSocketClient;

        private readonly TwitchSettings Settings;
        private readonly Action<string> _echoForwarder;
        private readonly CancellationTokenSource _cts = new();
        private bool _isDisposed;

        public TwitchBot(TwitchSettings settings, PokeTradeHub<T> hub)
        {
            Hub = hub;
            Settings = settings;

            var credentials = new ConnectionCredentials(settings.Username.ToLower(), settings.Token);

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = settings.ThrottleMessages,
                ThrottlingPeriod = TimeSpan.FromSeconds(settings.ThrottleSeconds),

                WhispersAllowedInPeriod = settings.ThrottleWhispers,
                WhisperThrottlingPeriod = TimeSpan.FromSeconds(settings.ThrottleWhispersSeconds),

                // message queue capacity is managed (10_000 for message & whisper separately)
                // message send interval is managed (50ms for each message sent)
            };

            Channel = settings.Channel;
            _webSocketClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(_webSocketClient);

            var cmd = settings.CommandPrefix;
            client.Initialize(credentials, Channel, cmd, cmd);

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            client.OnWhisperCommandReceived += Client_OnWhisperCommandReceived;
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnLeftChannel += Client_OnLeftChannel;

            client.OnMessageSent += Client_OnMessageSent;
            client.OnWhisperSent += Client_OnWhisperSent;
            client.OnMessageThrottled += Client_OnMessageThrottled;
            client.OnWhisperThrottled += Client_OnWhisperThrottled;
            client.OnError += Client_OnError;
            client.OnConnectionError += Client_OnConnectionError;

            client.Connect();

            _echoForwarder = msg => client.SendMessage(Channel, msg);
            EchoUtil.Forwarders.Add(_echoForwarder);

            // Turn on if verified
            // Hub.Queues.Forwarders.Add((bot, detail) => client.SendMessage(Channel, $"{bot.Connection.Name} is now trading (ID {detail.ID}) {detail.Trainer.TrainerName}"));
        }

        private void Client_OnMessageSent(object? sender, OnMessageSentArgs e)
            => LogUtil.LogText($"[{client.TwitchUsername}] - Message Sent in {e.SentMessage.Channel}: {e.SentMessage.Message}");

        private void Client_OnWhisperSent(object? sender, OnWhisperSentArgs e)
            => LogUtil.LogText($"[{client.TwitchUsername}] - Whisper Sent to @{e.Receiver}: {e.Message}");

        private void Client_OnMessageThrottled(object? sender, OnMessageThrottledArgs e)
            => LogUtil.LogError($"Message Throttled: {e.Message}", "TwitchBot");

        private void Client_OnWhisperThrottled(object? sender, OnWhisperThrottledArgs e)
            => LogUtil.LogError($"Whisper Throttled: {e.Message}", "TwitchBot");

        private void Client_OnError(object? sender, OnErrorEventArgs e)
            => LogUtil.LogError(e.Exception.Message + Environment.NewLine + e.Exception.StackTrace, "TwitchBot");

        private void Client_OnConnectionError(object? sender, OnConnectionErrorArgs e)
            => LogUtil.LogError(e.BotUsername + Environment.NewLine + e.Error.Message, "TwitchBot");

        public void Dispose()
        {
            _isDisposed = true;
            _cts.Cancel();
            _cts.Dispose();
            QueuePool.Clear();
            EchoUtil.Forwarders.Remove(_echoForwarder);
            client.OnLog -= Client_OnLog;
            client.OnJoinedChannel -= Client_OnJoinedChannel;
            client.OnMessageReceived -= Client_OnMessageReceived;
            client.OnWhisperReceived -= Client_OnWhisperReceived;
            client.OnChatCommandReceived -= Client_OnChatCommandReceived;
            client.OnWhisperCommandReceived -= Client_OnWhisperCommandReceived;
            client.OnConnected -= Client_OnConnected;
            client.OnDisconnected -= Client_OnDisconnected;
            client.OnLeftChannel -= Client_OnLeftChannel;

            client.OnMessageSent -= Client_OnMessageSent;
            client.OnWhisperSent -= Client_OnWhisperSent;
            client.OnMessageThrottled -= Client_OnMessageThrottled;
            client.OnWhisperThrottled -= Client_OnWhisperThrottled;
            client.OnError -= Client_OnError;
            client.OnConnectionError -= Client_OnConnectionError;

            try
            {
                client.Disconnect();
                _webSocketClient?.Dispose();
            }
            catch { }
        }

        internal static TradeQueueInfo<T> Info => Hub.Queues.Info;

        public void StartingDistribution(string message)
        {
            Task.Run(async () =>
            {
                try
                {
                    client.SendMessage(Channel, "5...");
                    await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                    client.SendMessage(Channel, "4...");
                    await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                    client.SendMessage(Channel, "3...");
                    await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                    client.SendMessage(Channel, "2...");
                    await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                    client.SendMessage(Channel, "1...");
                    await Task.Delay(1_000, _cts.Token).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(message))
                        client.SendMessage(Channel, message);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { LogUtil.LogError(ex.Message, "TwitchBot"); }
            }, _cts.Token);
        }

        private bool AddToTradeQueue(T pk, int code, OnWhisperReceivedArgs e, RequestSignificance sig, PokeRoutineType type, out string msg)
        {
            // var user = e.WhisperMessage.UserId;
            var userID = ulong.Parse(e.WhisperMessage.UserId);
            var name = e.WhisperMessage.DisplayName;

            var trainer = new PokeTradeTrainerInfo(name, ulong.Parse(e.WhisperMessage.UserId));
            var notifier = new TwitchTradeNotifier<T>(pk, trainer, code, e.WhisperMessage.Username, client, Channel, Hub.Config.Twitch);
            var tt = type == PokeRoutineType.SeedCheck ? PokeTradeType.Seed : PokeTradeType.Specific;
            var uniqueTradeID = TradeUtil.GenerateUniqueTradeID();
            var detail = new PokeTradeDetail<T>(pk, trainer, notifier, tt, code, sig == RequestSignificance.Favored, uniqueTradeID: uniqueTradeID);
            var trade = new TradeEntry<T>(detail, userID, type, name, uniqueTradeID);

            var added = Info.AddToTradeQueue(trade, userID, sig == RequestSignificance.Owner);

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                msg = $"@{name}: Sorry, you are already in the queue.";
                return false;
            }

            var position = Info.CheckPosition(userID, uniqueTradeID, type);
            msg = $"@{name}: Added to the {type} queue, unique ID: {detail.ID}. Current Position: {position.Position}";

            var botct = Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                msg += $". Estimated: {eta:F1} minutes.";
            }
            return true;
        }

        private void Client_OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            if (!Hub.Config.Twitch.AllowCommandsViaChannel || Hub.Config.Twitch.UserBlacklist.Contains(e.Command.ChatMessage.Username))
                return;

            var msg = e.Command.ChatMessage;
            var c = e.Command.CommandText.ToLower();
            var args = e.Command.ArgumentsAsString;
            var response = HandleCommand(msg, c, args, false);
            if (response.Length == 0)
                return;

            var channel = e.Command.ChatMessage.Channel;
            client.SendMessage(channel, response);
        }

        private void Client_OnConnected(object? sender, OnConnectedArgs e)
        {
            LogUtil.LogText($"[{client.TwitchUsername}] - Connected {e.AutoJoinChannel} as {e.BotUsername}");
        }

        private async void Client_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            LogUtil.LogText($"[{client.TwitchUsername}] - Disconnected.");
            while (!client.IsConnected && !_isDisposed)
            {
                try
                {
                    client.Reconnect();
                    await Task.Delay(5000, _cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogUtil.LogError($"Twitch reconnection failed: {ex.Message}", "TwitchBot");
                }
            }
        }

        private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            LogUtil.LogInfo($"Joined {e.Channel}", e.BotUsername);
            client.SendMessage(e.Channel, "Connected!");
        }

        private void Client_OnLeftChannel(object? sender, OnLeftChannelArgs e)
        {
            LogUtil.LogText($"[{client.TwitchUsername}] - Left channel {e.Channel}");
            client.JoinChannel(e.Channel);
        }

        private void Client_OnLog(object? sender, OnLogArgs e)
        {
            LogUtil.LogText($"[{client.TwitchUsername}] -[{e.BotUsername}] {e.Data}");
        }

        private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            LogUtil.LogText($"[{client.TwitchUsername}] - Received message: @{e.ChatMessage.Username}: {e.ChatMessage.Message}");
            if (client.JoinedChannels.Count == 0)
                client.JoinChannel(e.ChatMessage.Channel);
        }

        private void Client_OnWhisperCommandReceived(object? sender, OnWhisperCommandReceivedArgs e)
        {
            if (!Hub.Config.Twitch.AllowCommandsViaWhisper || Hub.Config.Twitch.UserBlacklist.Contains(e.Command.WhisperMessage.Username))
                return;

            var msg = e.Command.WhisperMessage;
            var c = e.Command.CommandText.ToLower();
            var args = e.Command.ArgumentsAsString;
            var response = HandleCommand(msg, c, args, true);
            if (response.Length == 0)
                return;

            client.SendWhisper(msg.Username, response);
        }

        private void Client_OnWhisperReceived(object? sender, OnWhisperReceivedArgs e)
        {
            LogUtil.LogText($"[{client.TwitchUsername}] - @{e.WhisperMessage.Username}: {e.WhisperMessage.Message}");
            if (QueuePool.Count > 100)
            {
                var removed = QueuePool[0];
                QueuePool.RemoveAt(0); // First in, first out
                client.SendMessage(Channel, $"Removed @{removed.DisplayName} ({(Species)removed.Pokemon.Species}) from the waiting list: stale request.");
            }

            var user = QueuePool.FindLast(q => q.UserName == e.WhisperMessage.Username);
            if (user == null)
                return;
            QueuePool.Remove(user);
            var msg = e.WhisperMessage.Message;
            try
            {
                int code = Util.ToInt32(msg);
                var sig = GetUserSignificance(user);
                var _ = AddToTradeQueue(user.Pokemon, code, e, sig, PokeRoutineType.LinkTrade, out string message);
                client.SendMessage(Channel, message);
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(TwitchBot<T>));
                LogUtil.LogError($"{ex.Message}", nameof(TwitchBot<T>));
            }
        }

        private RequestSignificance GetUserSignificance(TwitchQueue<T> user)
        {
            var name = user.UserName;
            if (name == Channel)
                return RequestSignificance.Owner;
            if (Settings.IsSudo(user.UserName))
                return RequestSignificance.Favored;
            return user.IsSubscriber ? RequestSignificance.Favored : RequestSignificance.None;
        }

        private string HandleCommand(TwitchLibMessage m, string c, string args, bool whisper)
        {
            bool sudo() => m is ChatMessage ch && (ch.IsBroadcaster || Settings.IsSudo(m.Username));
            bool subscriber() => m is ChatMessage { IsSubscriber: true };

            switch (c)
            {
                // User Usable Commands
                case "donate":
                    return Settings.DonationLink.Length > 0 ? $"Here's the donation link! Thank you for your support :3 {Settings.DonationLink}" : string.Empty;

                case "discord":
                    return Settings.DiscordLink.Length > 0 ? $"Here's the Discord Server Link, have a nice stay :3 {Settings.DiscordLink}" : string.Empty;

                case "tutorial":
                case "help":
                    return $"{Settings.TutorialText} {Settings.TutorialLink}";

                case "trade":
                case "t":
                    var _ = TwitchCommandsHelper<T>.AddToWaitingList(args, m.DisplayName, m.Username, ulong.Parse(m.UserId), subscriber(), out string msg);
                    if (msg.Contains("Please read what you are supposed to type") && Settings.TutorialLink.Length > 0)
                        msg += $"\nUsage Tutorial: {Settings.TutorialLink}";
                    return msg;

                case "ts":
                case "queue":
                case "position":
                    var userID = ulong.Parse(m.UserId);
                    var tradeEntry = Info.GetDetail(userID);
                    if (tradeEntry != null)
                    {
                        var uniqueTradeID = tradeEntry.UniqueTradeID;
                        return $"@{m.Username}: {Info.GetPositionString(userID, uniqueTradeID)}";
                    }
                    else
                    {
                        return $"@{m.Username}: You are not currently in the queue.";
                    }
                case "tc":
                case "cancel":
                case "remove":
                    return $"@{m.Username}: {TwitchCommandsHelper<T>.ClearTrade(ulong.Parse(m.UserId))}";

                case "code" when whisper:
                    return TwitchCommandsHelper<T>.GetCode(ulong.Parse(m.UserId));

                // Sudo Only Commands
                case "tca" when !sudo():
                case "pr" when !sudo():
                case "pc" when !sudo():
                case "tt" when !sudo():
                case "tcu" when !sudo():
                    return "This command is locked for sudo users only!";

                case "tca":
                    Info.ClearAllQueues();
                    return "Cleared all queues!";

                case "pr":
                    return Info.Hub.Ledy.Pool.Reload(Hub.Config.Folder.DistributeFolder) ? $"Reloaded from folder. Pool count: {Info.Hub.Ledy.Pool.Count}" : "Failed to reload from folder.";

                case "pc":
                    return $"The pool count is: {Info.Hub.Ledy.Pool.Count}";

                case "tt":
                    return Info.Hub.Queues.Info.ToggleQueue()
                        ? "Users are now able to join the trade queue."
                        : "Changed queue settings: **Users CANNOT join the queue until it is turned back on.**";

                case "tcu":
                    return TwitchCommandsHelper<T>.ClearTrade(args);

                default: return string.Empty;
            }
        }
    }
}
