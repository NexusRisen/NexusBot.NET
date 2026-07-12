using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Discord.Helpers;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.Discord.Helpers.TradeModule;
using SysBot.Pokemon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

[Summary("Queues new Link Code trades")]
public class TradeModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    #region Medal Achievement Command

    [Command("medals")]
    [Alias("ml")]
    [Summary("Shows your current trade count and medal status")]
    public async Task ShowMedalsCommand()
    {
        if (!SysCord<T>.Runner.Hub.Config.Discord.EnableMedals)
        {
            await ReplyAsync("The medals system is currently disabled.").ConfigureAwait(false);
            return;
        }

        var tradeCodeStorage = new TradeCodeStorage();
        int totalTrades = new MedalStorage().GetTradeCount(Context.User.Id);

        if (totalTrades == 0)
        {
            await ReplyAsync($"{Context.User.Username}, you haven't made any trades yet.\nStart trading to earn your first medal!");
            return;
        }

        int currentMilestone = MedalHelpers.GetCurrentMilestone(totalTrades);
        var embed = MedalHelpers.CreateMedalsEmbed(Context.User, currentMilestone, totalTrades);
        await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
    }

    #endregion
    #region Trade Commands

    [Command("trade")]
    [Alias("t")]
    [Summary("Makes the bot trade you a Pokémon converted from the provided Showdown Set.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public Task TradeAsync([Summary("Showdown Set")][Remainder] string content)
    {
        var userID = Context.User.Id;
        var code = Info.GetRandomTradeCode(userID);
        return ProcessTradeAsync(code, content);
    }

    [Command("trade")]
    [Alias("t")]
    [Summary("Makes the bot trade you a Pokémon converted from the provided Showdown Set.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public Task TradeAsync([Summary("Trade Code")] int code, [Summary("Showdown Set")][Remainder] string content)
        => ProcessTradeAsync(code, content);

    [Command("trade")]
    [Alias("t")]
    [Summary("Makes the bot trade you the provided Pokémon file.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public Task TradeAsyncAttach([Summary("Trade Code")] int code, [Summary("Ignore AutoOT")] bool ignoreAutoOT = false)
    {
        var userID = Context.User.Id;
        var sig = Context.User.GetFavor();
        return ProcessTradeAttachmentAsync(code, sig, Context.User, ignoreAutoOT: ignoreAutoOT);

    }

    [Command("trade")]
    [Alias("t")]
    [Summary("Makes the bot trade you the attached file.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task TradeAsyncAttach([Summary("Ignore AutoOT")] bool ignoreAutoOT = false)
    {
        var userID = Context.User.Id;
        var code = Info.GetRandomTradeCode(userID);
        var sig = Context.User.GetFavor();

        await Task.Run(async () =>
        {
            await ProcessTradeAttachmentAsync(code, sig, Context.User, ignoreAutoOT: ignoreAutoOT).ConfigureAwait(false);
        }).ConfigureAwait(false);

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
    }

    [Command("hidetrade")]
    [Alias("ht")]
    [Summary("Makes the bot trade you a Pokémon without showing trade embed details.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public Task HideTradeAsync([Summary("Showdown Set")][Remainder] string content)
    {
        var userID = Context.User.Id;
        var code = Info.GetRandomTradeCode(userID);
        return ProcessTradeAsync(code, content, isHiddenTrade: true);
    }

    [Command("hidetrade")]
    [Alias("ht")]
    [Summary("Makes the bot trade you a Pokémon without showing trade embed details.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public Task HideTradeAsync([Summary("Trade Code")] int code, [Summary("Showdown Set")][Remainder] string content)
        => ProcessTradeAsync(code, content, isHiddenTrade: true);

    [Command("hidetrade")]
    [Alias("ht")]
    [Summary("Makes the bot trade you the provided file without showing trade embed details.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public Task HideTradeAsyncAttach([Summary("Trade Code")] int code, [Summary("Ignore AutoOT")] bool ignoreAutoOT = false)
    {
        var sig = Context.User.GetFavor();
        return ProcessTradeAttachmentAsync(code, sig, Context.User, isHiddenTrade: true, ignoreAutoOT: ignoreAutoOT);
    }

    [Command("hidetrade")]
    [Alias("ht")]
    [Summary("Makes the bot trade you the attached file without showing trade embed details.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task HideTradeAsyncAttach([Summary("Ignore AutoOT")] bool ignoreAutoOT = false)
    {
        var userID = Context.User.Id;
        var code = Info.GetRandomTradeCode(userID);
        var sig = Context.User.GetFavor();

        await ProcessTradeAttachmentAsync(code, sig, Context.User, isHiddenTrade: true, ignoreAutoOT: ignoreAutoOT).ConfigureAwait(false);

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
    }

    [Command("tradeUser")]
    [Alias("tu", "tradeOther")]
    [Summary("Makes the bot trade the mentioned user the attached file.")]
    [RequireSudo]
    public async Task TradeAsyncAttachUser([Summary("Trade Code")] int code, [Remainder] string _)
    {
        if (Context.Message.MentionedUsers.Count > 1)
        {
            await ReplyAsync("Too many mentions. Queue one user at a time.").ConfigureAwait(false);
            return;
        }

        if (Context.Message.MentionedUsers.Count == 0)
        {
            await ReplyAsync("A user must be mentioned in order to do this.").ConfigureAwait(false);
            return;
        }

        var usr = Context.Message.MentionedUsers.ElementAt(0);
        var sig = usr.GetFavor();
        await ProcessTradeAttachmentAsync(code, sig, usr).ConfigureAwait(false);
    }

    [Command("tradeUser")]
    [Alias("tu", "tradeOther")]
    [Summary("Makes the bot trade the mentioned user the attached file.")]
    [RequireSudo]
    public Task TradeAsyncAttachUser([Remainder] string _)
    {
        var userID = Context.User.Id;
        var code = Info.GetRandomTradeCode(userID);
        return TradeAsyncAttachUser(code, _);
    }

    #endregion
    #region Special Trade Commands

    [Command("egg")]
    [Alias("Egg")]
    [Summary("Trades a single egg generated from the provided Pokémon name.")]
    public async Task TradeEgg([Remainder] string egg)
    {
        var userID = Context.User.Id;
        var code = Info.GetRandomTradeCode(userID);
        await TradeEggAsync(code, egg).ConfigureAwait(false);
    }

    [Command("egg")]
    [Alias("Egg")]
    [Summary("Trades a single egg generated from the provided Pokémon name.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task TradeEggAsync([Summary("Trade Code")] int code, [Summary("Showdown Set")][Remainder] string content)
    {
        await ProcessEggRequestInternal(code, content, false).ConfigureAwait(false);
    }

    [Command("begg")]
    [Alias("bEgg")]
    [Summary("Trades multiple eggs at once.")]
    public async Task BatchTradeEgg([Remainder] string egg)
    {
        var userID = Context.User.Id;
        var code = Info.GetRandomTradeCode(userID);
        await BatchTradeEggAsync(code, egg).ConfigureAwait(false);
    }

    [Command("begg")]
    [Alias("bEgg")]
    [Summary("Trades multiple eggs at once.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task BatchTradeEggAsync([Summary("Trade Code")] int code, [Summary("Showdown Set")][Remainder] string content)
    {
        await ProcessEggRequestInternal(code, content, true).ConfigureAwait(false);
    }

    private async Task ProcessEggRequestInternal(int code, string content, bool isBatch)
    {
        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        var batchSettings = SysCord<T>.Runner.Config.Trade.BatchSettings;

        _ = Task.Run(async () =>
        {
            try
            {
                var normalizedContent = BatchNormalizer.NormalizeBatchCommands(content);
                normalizedContent = ReusableActions.StripCodeBlock(normalizedContent);
                var trades = isBatch ? BatchHelpers<T>.ParseBatchTradeContent(normalizedContent) : [normalizedContent];

                if (!isBatch && content.Contains("---"))
                {
                    await Helpers<T>.ReplyAndDeleteAsync(Context, "The $egg command is only for single trades. Please use $begg for batch trades.", 5);
                    return;
                }

                if (isBatch)
                {
                    if (!batchSettings.AllowEggBatchTrades)
                    {
                        await Helpers<T>.ReplyAndDeleteAsync(Context, "Batch trades for eggs are currently disabled by the bot administrator.", 2);
                        return;
                    }

                    if (trades.Count > batchSettings.MaxEggsPerBatch)
                    {
                        await Helpers<T>.ReplyAndDeleteAsync(Context, $"You can only request up to {batchSettings.MaxEggsPerBatch} eggs at a time.", 2);
                        return;
                    }
                }

                var pkmList = new List<T>();
                var errors = new List<BatchTradeError>();

                for (int i = 0; i < trades.Count; i++)
                {
                    var set = new ShowdownSet(trades[i]);
                    var template = AutoLegalityWrapper.GetTemplate(set);
                    var sav = AutoLegalityWrapper.GetTrainerInfo<T>();

                    // Generate the egg using ALM's GenerateEgg method
                    var pkm = AutoLegalityWrapper.GenerateEgg(sav, template, out var result);

                    if (result == LegalizationResult.Regenerated && pkm != null)
                    {
                        if (APILegality.AllowTrainerOverride && template.Regen.Trainer != null)
                            pkm.SetAllTrainerData(template.Regen.Trainer);
                    }

                    if (result != LegalizationResult.Regenerated)
                    {
                        var reason = result == LegalizationResult.Timeout
                            ? "Egg generation took too long."
                            : "Failed to generate egg from the provided set.";
                        
                        var speciesName = set.Species > 0 ? GameInfo.Strings.Species[set.Species] : "Unknown";
                        errors.Add(new BatchTradeError
                        {
                            TradeNumber = i + 1,
                            SpeciesName = speciesName,
                            ErrorMessage = reason,
                            ShowdownSet = string.Join("\n", set.GetSetLines())
                        });
                        continue;
                    }

                    pkm = EntityConverter.ConvertToType(pkm!, typeof(T), out _) ?? pkm;
                    if (pkm is not T pk)
                    {
                        errors.Add(new BatchTradeError
                        {
                            TradeNumber = i + 1,
                            SpeciesName = GameInfo.Strings.Species[set.Species],
                            ErrorMessage = "Oops! I wasn't able to create an egg for that.",
                            ShowdownSet = string.Join("\n", set.GetSetLines())
                        });
                        continue;
                    }

                    pk.ResetPartyStats();
                    pkmList.Add(pk);
                }

                if (errors.Count > 0)
                {
                    await BatchHelpers<T>.SendBatchErrorEmbedAsync(Context, errors, trades.Count);
                    return;
                }

                if (pkmList.Count == 0)
                    return;

                if (pkmList.Count == 1)
                {
                    var sig = Context.User.GetFavor();
                    await Helpers<T>.AddTradeToQueueAsync(Context, code, Context.User.Username, pkmList[0], sig, Context.User).ConfigureAwait(false);
                }
                else
                {
                    await BatchHelpers<T>.ProcessBatchContainer(Context, pkmList, code, pkmList.Count);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                await Helpers<T>.ReplyAndDeleteAsync(Context, "An error occurred while processing the request.", 2);
            }
        });

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
    }

    [Command("fixOT")]
    [Alias("fix", "f")]
    [Summary("Fixes OT and Nickname of a Pokémon you show via Link Trade if an advert is detected.")]
    [RequireQueueRole(nameof(DiscordManager.RolesFixOT))]
    public async Task FixAdOT()
    {
        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        var code = Info.GetRandomTradeCode(userID);
        await ProcessFixOTAsync(code);
    }

    [Command("fixOT")]
    [Alias("fix", "f")]
    [Summary("Fixes OT and Nickname of a Pokémon you show via Link Trade if an advert is detected.")]
    [RequireQueueRole(nameof(DiscordManager.RolesFixOT))]
    public async Task FixAdOT([Summary("Trade Code")] int code)
    {
        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        await ProcessFixOTAsync(code);
    }

    private async Task ProcessFixOTAsync(int code)
    {
        var userID = Context.User.Id;
        var trainerName = Context.User.Username;
        var sig = Context.User.GetFavor();
        var lgcode = Info.GetRandomLGTradeCode(userID);

        await QueueHelper<T>.AddToQueueAsync(Context, code, trainerName, sig, new T(),
            PokeRoutineType.FixOT, PokeTradeType.FixOT, Context.User, false, 1, 1, false, false, lgcode: lgcode).ConfigureAwait(false);

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
    }

    [Command("dittoTrade")]
    [Alias("dt", "ditto")]
    [Summary("Makes the bot trade you a Ditto with a requested stat spread and language.")]
    public async Task DittoTrade([Summary("A combination of \"ATK/SPA/SPE\" or \"6IV\"")] string keyword,
        [Summary("Language")] string language, [Summary("Nature")] string nature)
    {
        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        var code = Info.GetRandomTradeCode(userID);
        await ProcessDittoTradeAsync(code, keyword, language, nature);
    }

    [Command("dittoTrade")]
    [Alias("dt", "ditto")]
    [Summary("Makes the bot trade you a Ditto with a requested stat spread and language.")]
    public async Task DittoTrade([Summary("Trade Code")] int code,
        [Summary("A combination of \"ATK/SPA/SPE\" or \"6IV\"")] string keyword,
        [Summary("Language")] string language, [Summary("Nature")] string nature)
    {
        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        await ProcessDittoTradeAsync(code, keyword, language, nature);
    }

    private async Task ProcessDittoTradeAsync(int code, string keyword, string language, string nature)
    {
        keyword = keyword.ToLower().Trim();

        if (!Enum.TryParse(language, true, out LanguageID lang))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context, $"Couldn't recognize language: {language}.", 2);
            return;
        }

        nature = nature.Trim()[..1].ToUpper() + nature.Trim()[1..].ToLower();
        var set = new ShowdownSet($"{keyword}(Ditto)\nLanguage: {lang}\nNature: {nature}");
        var template = AutoLegalityWrapper.GetTemplate(set);
        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var pkm = sav.GetLegal(template, out var result);

        if (pkm == null)
        {
            await ReplyAsync("Set took too long to legalize.");
            return;
        }

        TradeExtensions<T>.DittoTrade((T)pkm);
        var la = new LegalityAnalysis(pkm);

        if (pkm is not T pk || !la.Valid)
        {
            var reason = result == "Timeout" ? "That set took too long to generate." : "I wasn't able to create something from that.";
            var imsg = $"Oops! {reason} Here's my best attempt for that Ditto!";
            await Context.Channel.SendPKMAsync(pkm, imsg).ConfigureAwait(false);
            return;
        }

        pk.ResetPartyStats();

        // Ad Name Check
        if (TradeExtensions<T>.HasAdName(pk, out string ad))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context, "Detected Adname in the Pokémon's name or trainer name, which is not allowed.", 5);
            return;
        }
        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk,
            PokeRoutineType.LinkTrade, PokeTradeType.Specific).ConfigureAwait(false);

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
    }

    [Command("itemTrade")]
    [Alias("it", "item")]
    [Summary("Makes the bot trade you a Pokémon holding the requested item.")]
    public async Task ItemTrade([Remainder] string item)
    {
        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        var code = Info.GetRandomTradeCode(userID);
        await ProcessItemTradeAsync(code, item);
    }

    [Command("itemTrade")]
    [Alias("it", "item")]
    [Summary("Makes the bot trade you a Pokémon holding the requested item.")]
    public async Task ItemTrade([Summary("Trade Code")] int code, [Remainder] string item)
    {
        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        await ProcessItemTradeAsync(code, item);
    }

    private async Task ProcessItemTradeAsync(int code, string item)
    {
        var itemNames = item.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var batchSettings = SysCord<T>.Runner.Config.Trade.BatchSettings;
        var maxItemBatch = batchSettings.MaxItemBatchAmount;

        if (typeof(T) == typeof(PB7) && itemNames.Length > 1)
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context, "Batch trades are not supported in Let's Go Pikachu/Eevee. You can only request one item at a time.", 2);
            return;
        }

        if (itemNames.Length > 1 && !batchSettings.AllowBatchTrades)
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context, "Batch trades are currently disabled. You can only request one item at a time.", 2);
            return;
        }

        if (itemNames.Length > maxItemBatch)
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context, $"You can only request up to {maxItemBatch} items at a time.", 2);
            return;
        }

        var pkmList = new List<T>();
        var errors = new List<BatchTradeError>();

        Species species = Info.Hub.Config.Trade.TradeConfiguration.ItemTradeSpecies == Species.None
            ? Species.Pikachu
            : Info.Hub.Config.Trade.TradeConfiguration.ItemTradeSpecies;
        var baseSpeciesName = SpeciesName.GetSpeciesNameGeneration((ushort)species, 2, 8);

        for (int i = 0; i < itemNames.Length; i++)
        {
            var itemName = itemNames[i];
            var set = new ShowdownSet($"{baseSpeciesName} @ {itemName}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var pkm = sav.GetLegal(template, out var result);

            if (pkm == null)
            {
                errors.Add(new BatchTradeError { TradeNumber = i + 1, SpeciesName = baseSpeciesName, ErrorMessage = "Set took too long to legalize.", ShowdownSet = itemName });
                continue;
            }

            pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;

            if (pkm.HeldItem == 0)
            {
                errors.Add(new BatchTradeError { TradeNumber = i + 1, SpeciesName = baseSpeciesName, ErrorMessage = "Sorry, the item you entered wasn't recognized.", ShowdownSet = itemName });
                continue;
            }

            if (pkm.HeldItem == 535 || pkm.HeldItem == 534) //Blue and Red Orbs
            {
                const ushort goldBottleCap = 796; // PA9 ID
                var oldItem = pkm.HeldItem;

                pkm.HeldItem = goldBottleCap;

                var speciesName = GameInfo.Strings.Species[pkm.Species];
                var oldItemName = GameInfo.Strings.Item[oldItem];
                var newItemName = GameInfo.Strings.Item[goldBottleCap];

                Base.LogUtil.LogInfo($"Replaced illegal item '{oldItemName}' with '{newItemName}' for {speciesName}", "BlockItem");
            }

            if (TradeRestrictions.IsUntradableHeld(pkm.Context, pkm.HeldItem))
            {
                errors.Add(new BatchTradeError { TradeNumber = i + 1, SpeciesName = baseSpeciesName, ErrorMessage = "Sorry, the item you entered can't be traded.", ShowdownSet = itemName });
                continue;
            }

            var la = new LegalityAnalysis(pkm);
            if (pkm is not T pk || !la.Valid)
            {
                var reason = result == "Timeout" ? "That set took too long to generate." : "I wasn't able to create something from that.";
                errors.Add(new BatchTradeError { TradeNumber = i + 1, SpeciesName = baseSpeciesName, ErrorMessage = reason, ShowdownSet = itemName });
                continue;
            }

            pk.ResetPartyStats();
            pkmList.Add(pk);
        }

        if (errors.Count > 0)
        {
            await BatchHelpers<T>.SendBatchErrorEmbedAsync(Context, errors, itemNames.Length);
            return;
        }

        if (pkmList.Count == 0)
            return;

        if (pkmList.Count == 1)
        {
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pkmList[0],
                PokeRoutineType.LinkTrade, PokeTradeType.Specific).ConfigureAwait(false);
        }
        else
        {
            await BatchHelpers<T>.ProcessBatchContainer(Context, pkmList, code, pkmList.Count);
        }

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
    }

    #endregion

    #region List Commands

    [Command("tradeList")]
    [Alias("tl")]
    [Summary("Prints the users in the trade queues.")]
    [RequireSudo]
    public async Task GetTradeListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.LinkTrade);
        var embed = new EmbedBuilder();
        embed.AddField(x =>
        {
            x.Name = "Pending Trades";
            x.Value = msg;
            x.IsInline = false;
        });
        await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
    }

    [Command("fixOTList")]
    [Alias("fl", "fq")]
    [Summary("Prints the users in the FixOT queue.")]
    [RequireSudo]
    public async Task GetFixListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.FixOT);
        var embed = new EmbedBuilder();
        embed.AddField(x =>
        {
            x.Name = "Pending Trades";
            x.Value = msg;
            x.IsInline = false;
        });
        await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
    }



    #endregion

    #region Batch Trades

    [Command("batchTrade")]
    [Alias("bt")]
    [Summary("Makes the bot trade multiple Pokémon from the provided list.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task BatchTradeAsync([Summary("List of Showdown Sets separated by '---'")][Remainder] string content = "")
    {
        if (typeof(T) == typeof(PB7))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context, "Batch trades are not supported in Let's Go Pikachu/Eevee.", 2);
            return;
        }

        var batchSettings = SysCord<T>.Runner.Config.Trade.BatchSettings;

        // Check if batch trades are allowed
        if (!batchSettings.AllowBatchTrades)
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                $"Batch trades are currently disabled by the bot administrator.", 6);
            return;
        }

        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        var processingMessage = await Context.Channel.SendMessageAsync($"{Context.User.Mention} Processing your batch trade request...");

        _ = Task.Run(async () =>
        {
            try
            {
                var batchPokemonList = new List<T>();
                var errors = new List<BatchTradeError>();

                // 1. Process attachments first
                if (Context.Message.Attachments.Any())
                {
                    var attachmentResult = await BatchHelpers<T>.ProcessAttachmentsForBatch(Context).ConfigureAwait(false);
                    batchPokemonList.AddRange(attachmentResult.Pokemon);
                    errors.AddRange(attachmentResult.Errors);
                }

                // 2. Process text content if provided
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var normalizedContent = BatchNormalizer.NormalizeBatchCommands(content);
                    normalizedContent = ReusableActions.StripCodeBlock(normalizedContent);
                    var trades = BatchHelpers<T>.ParseBatchTradeContent(normalizedContent);

                    int startIndex = batchPokemonList.Count + errors.Count + 1;
                    for (int i = 0; i < trades.Count; i++)
                    {
                        var (pk, error, set, legalizationHint) = await BatchHelpers<T>.ProcessSingleTradeForBatch(trades[i]);
                        if (pk != null)
                        {
                            batchPokemonList.Add(pk);
                        }
                        else
                        {
                            var speciesName = set != null && set.Species > 0
                                ? GameInfo.Strings.Species[set.Species]
                                : "Unknown";
                            errors.Add(new BatchTradeError
                            {
                                TradeNumber = startIndex + i,
                                SpeciesName = speciesName,
                                ErrorMessage = error ?? "Unknown error",
                                LegalizationHint = legalizationHint,
                                ShowdownSet = set != null ? string.Join("\n", set.GetSetLines()) : trades[i]
                            });
                        }
                    }
                }

                await processingMessage.DeleteAsync().ConfigureAwait(false);

                if (batchPokemonList.Count == 0 && errors.Count == 0)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} No valid Pokémon or Showdown sets were found in your request.");
                    return;
                }

                // Check for eggs in the batch
                int eggCount = batchPokemonList.Count(p => p.IsEgg);
                if (eggCount > 1)
                {
                    if (!batchSettings.AllowEggBatchTrades)
                    {
                        await Helpers<T>.ReplyAndDeleteAsync(Context,
                            "Batch trades containing multiple eggs are currently disabled by the bot administrator.", 5);
                        return;
                    }

                    if (eggCount > batchSettings.MaxEggsPerBatch)
                    {
                        await Helpers<T>.ReplyAndDeleteAsync(Context,
                            $"You can only include up to {batchSettings.MaxEggsPerBatch} eggs in a single batch trade. Your request had {eggCount}.", 5);
                        return;
                    }
                }

                // Use configured max trades per batch
                int maxTradesAllowed = batchSettings.MaxPkmsPerTrade;
                int totalRequested = batchPokemonList.Count + errors.Count;

                if (totalRequested > maxTradesAllowed)
                {
                    await Helpers<T>.ReplyAndDeleteAsync(Context,
                        $"You can only process up to {maxTradesAllowed} trades at a time. Your request had {totalRequested}.\nPlease reduce the number of trades in your batch.", 5);
                    return;
                }

                if (errors.Count > 0)
                {
                    await BatchHelpers<T>.SendBatchErrorEmbedAsync(Context, errors, totalRequested);
                    return;
                }

                if (batchPokemonList.Count > 0)
                {
                    var batchTradeCode = Info.GetRandomTradeCode(userID);
                    await BatchHelpers<T>.ProcessBatchContainer(Context, batchPokemonList, batchTradeCode, batchPokemonList.Count);
                }
            }
            catch (Exception ex)
            {
                try { await processingMessage.DeleteAsync().ConfigureAwait(false); } catch { }
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} An error occurred while processing your batch trade. Please try again.");
                Base.LogUtil.LogError($"Batch trade processing error: {ex.Message}", nameof(BatchTradeAsync));
            }
        });

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
    }

    #endregion

    #region Private Helper Methods

      private async Task ProcessTradeAsync(int code, string content, bool isHiddenTrade = false)
    {
        var userID = Context.User.Id;
        if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
        {
            await Helpers<T>.ReplyAndDeleteAsync(Context,
                "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                // Detect custom trainer info BEFORE generating the Pokemon
                var ignoreAutoOT = content.Contains("OT:") || content.Contains("TID:") || content.Contains("SID:");

                var result = await Helpers<T>.ProcessShowdownSetAsync(content, ignoreAutoOT);

                if (result.Pokemon == null)
                {
                    await Helpers<T>.SendTradeErrorEmbedAsync(Context, result);
                    return;
                }

                var sig = Context.User.GetFavor();

                await Helpers<T>.AddTradeToQueueAsync(
                    Context, code, Context.User.Username, result.Pokemon, sig, Context.User,
                    isHiddenTrade: isHiddenTrade,
                    lgcode: result.LgCode,
                    ignoreAutoOT: ignoreAutoOT,
                    isNonNative: result.IsNonNative
                );
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                var msg = "Oops! An unexpected problem happened with this Showdown Set.";
                await Helpers<T>.ReplyAndDeleteAsync(Context, msg, 2);
            }
        });

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, isHiddenTrade ? 0 : 2);
    }

    private async Task ProcessTradeAttachmentAsync(int code, RequestSignificance sig, SocketUser user, bool isHiddenTrade = false, bool ignoreAutoOT = false)
    {
        var pk = await Helpers<T>.ProcessTradeAttachmentAsync(Context);
        if (pk == null)
            return;

        await Helpers<T>.AddTradeToQueueAsync(Context, code, user.Username, pk, sig, user,
            isHiddenTrade: isHiddenTrade, ignoreAutoOT: ignoreAutoOT);

        if (Context.Message is IUserMessage userMessage)
            _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, isHiddenTrade ? 0 : 2);
    }

    #endregion
}
