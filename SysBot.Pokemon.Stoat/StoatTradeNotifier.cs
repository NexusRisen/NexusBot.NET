using PKHeX.Core;
using StoatSharp;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Stoat;

public class StoatTradeNotifier<T> : IPokeTradeNotifier<T>, IDisposable where T : PKM, new()
{
    private T Data { get; set; }
    private PokeTradeTrainerInfo Info { get; }
    private int Code { get; }
    private string UserId { get; }
    private List<Pictocodes>? LGCode { get; }
    private StoatClient _client;

    public StoatTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string userId, StoatClient client, List<Pictocodes>? lgcode = null)
    {
        Data = data;
        Info = info;
        Code = code;
        UserId = userId;
        _client = client;
        LGCode = lgcode;
    }

    public Action<PokeRoutineExecutor<T>>? OnFinish { get; set; }

    public void UpdateBatchProgress(int currentBatchNumber, T currentPokemon, int uniqueTradeID) { Data = currentPokemon; }

    private async Task SendDM(string message)
    {
        try 
        {
            var user = await UserHelper.GetUserAsync(_client.Rest, UserId);
            if (user != null)
            {
                var dm = await UserHelper.GetUserDMChannelAsync(_client.Rest, UserId);
                if (dm != null)
                {
                    var embed = new EmbedBuilder()
                        .SetColor(new StoatColor("#663399"))
                        .SetDescription(message)
                        .Build();
                    await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
                }
            }
        } catch (Exception ex) { LogUtil.LogError($"Failed to send DM to Stoat user {UserId}: {ex.Message}", "StoatTradeNotifier"); }
    }

    private string GetPokemonSummary()
    {
        if (Data.IsEgg)
            return "**Requested Pokémon:**\n🥚 Egg\n\n";

        string langCode = ((LanguageID)Data.Language).GetLanguageCode();
        GameStrings strings = GameInfo.GetStrings(langCode);
        var speciesName = strings.Species[Data.Species];
        string shinyStr = Data.IsShiny ? "✨ " : "";
        string itemStr = Data.HeldItem > 0 ? $"\n**Item:** {strings.itemlist[Data.HeldItem]}" : "";
        string abilityStr = $"\n**Ability:** {strings.abilitylist[Data.Ability]}";
        string natureStr = $"\n**Nature:** {strings.natures[(int)Data.Nature]}";
        return $"**Requested Pokémon:**\n{shinyStr}{speciesName} (Lv. {Data.CurrentLevel}){itemStr}{abilityStr}{natureStr}\n\n";
    }

    public Task SendInitialQueueUpdate() => SendDM("Your trade request has been queued.");

    public async Task SendInitialQueueUpdateToChannel(string channelId, string userName)
    {
        var text = $"**User:** {userName}\n\n{GetPokemonSummary()}Your trade request has been queued.";
        try 
        {
            var ch = await _client.Rest.GetChannelAsync(channelId);
            if (ch != null)
            {
                string imageUrl = SysBot.Pokemon.Helpers.TradeExtensions<T>.PokeImg(Data, false, true, null);
                var embed = new EmbedBuilder()
                    .SetColor(new StoatColor("#663399"))
                    .SetDescription(text)
                    .SetImage(imageUrl)
                    .Build();
                await MessageHelper.SendMessageAsync(ch, string.Empty, embeds: new[] { embed });
            }
        } 
        catch (Exception ex) { LogUtil.LogError($"Failed to send channel update to Stoat channel {channelId}: {ex.Message}", "StoatTradeNotifier"); }
    }

    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        _ = Task.Run(() => {
            var speciesName = SpeciesName.GetSpeciesName(Data.Species, 2);
            if (Data is PB7 && LGCode != null && LGCode.Count == 3)
                _ = SendDM($"Initializing trade for {speciesName}. Please be ready.\n**Code:** {LGCode[0]}, {LGCode[1]}, {LGCode[2]}");
            else
                _ = SendDM($"Initializing trade for {speciesName}. Please be ready.\n**Code:** {Code:D8}");
        });
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var trainer = string.IsNullOrEmpty(Info.TrainerName) ? string.Empty : $" {Info.TrainerName}";
        _ = SendDM($"I'm waiting for you{trainer}! My IGN is *{routine.InGameName}*.");
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        OnFinish?.Invoke(routine);
        _ = SendDM($"Trade canceled: {msg}");
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result) => _ = SendDM("Trade finished. Enjoy!");
    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message) => _ = SendDM(message);
    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message) => _ = SendDM(message.Summary);
    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message) => _ = SendDM(message);
    public void Dispose() { GC.SuppressFinalize(this); }
}
