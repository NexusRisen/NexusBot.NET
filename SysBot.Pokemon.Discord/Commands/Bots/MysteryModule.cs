using Discord;
using Discord.Commands;
using SysBot.Pokemon.Helpers;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class MysteryModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
        private static readonly Dictionary<EntityContext, List<ushort>> BreedableSpeciesCache = [];
        private static readonly Dictionary<EntityContext, List<ushort>> LegalSpeciesCache = [];
        private const int DefaultMaxGenerationAttempts = 30;

        [Command("mysteryegg")]
        [Alias("me")]
        [Summary("Trades an egg generated from a random Pokémon.")]
        public async Task TradeMysteryEggAsync()
        {
            // LGPE does not support eggs/breeding
            var context = GetContext();
            if (context == EntityContext.None || typeof(T).Name == "PB7")
            {
                await ReplyAsync("Mystery Eggs are not available for Let's Go Pikachu/Eevee as the game does not support breeding.").ConfigureAwait(false);
                return;
            }

            var userID = Context.User.Id;
            if (Info.IsUserInQueue(userID))
            {
                await ReplyAsync("You already have an existing trade in the queue. Please wait until it is processed.").ConfigureAwait(false);
                return;
            }

            var code = Info.GetRandomTradeCode(userID);
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessMysteryEggTradeAsync(code).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogUtil.LogSafe(ex, nameof(MysteryModule<T>));
                }
            });
        }

        [Command("batchMysteryEgg")]
        [Alias("bme")]
        [Summary("Trades multiple Mystery Eggs at once.")]
        public async Task BatchMysteryEggAsync([Summary("Number of eggs")] int count = 2)
        {
            // LGPE does not support eggs/breeding
            var context = GetContext();
            if (context == EntityContext.None || typeof(T).Name == "PB7")
            {
                await ReplyAsync("Mystery Eggs are not available for Let's Go Pikachu/Eevee as the game does not support breeding.").ConfigureAwait(false);
                return;
            }

            var batchSettings = SysCord<T>.Runner.Config.Trade.BatchSettings;
            if (!batchSettings.AllowMysteryEggBatchTrades)
            {
                await ReplyAsync("Batch Mystery Eggs are currently disabled by the bot administrator.").ConfigureAwait(false);
                return;
            }

            var userID = Context.User.Id;
            if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
            {
                await Helpers<T>.ReplyAndDeleteAsync(Context,
                    "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
                return;
            }

            // Validate count
            int maxEggs = batchSettings.MaxMysteryEggsPerBatch;
            if (count < 1 || count > maxEggs)
            {
                await Helpers<T>.ReplyAndDeleteAsync(Context,
                    $"Invalid number of eggs. Please specify between 1 and {maxEggs} eggs.", 5);
                return;
            }

            var processingMessage = await Context.Channel.SendMessageAsync($"{Context.User.Mention} Generating {count} Mystery Eggs...");

            _ = Task.Run(async () =>
            {
                try
                {
                    var batchEggList = new List<T>();
                    var failedCount = 0;

                    // Generate all mystery eggs
                    for (int i = 0; i < count; i++)
                    {
                        var egg = TradeModuleHelpers.GenerateLegalMysteryEgg<T>();
                        if (egg != null)
                        {
                            batchEggList.Add(egg);
                        }
                        else
                        {
                            failedCount++;
                        }
                    }

                    await processingMessage.DeleteAsync();

                    // Check if we generated any eggs
                    if (batchEggList.Count == 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} Failed to generate any Mystery Eggs. Please try again.");
                        return;
                    }

                    // Warn if some eggs failed
                    if (failedCount > 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} Warning: Failed to generate {failedCount} egg(s). Proceeding with {batchEggList.Count} egg(s).");
                    }

                    // Add batch to queue
                    var batchTradeCode = Info.GetRandomTradeCode(userID);
                    await ProcessBatchMysteryItems(Context, batchEggList, batchTradeCode, count, "Mystery Egg", SysBot.Pokemon.Helpers.AssetManager.GetAssetUrl("Assets/Eggs/mysteryegg3.png"));
                }
                catch (Exception ex)
                {
                    try { await processingMessage.DeleteAsync(); } catch { }
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} An error occurred while processing your batch Mystery Egg request. Please try again.");
                    Base.LogUtil.LogError($"Batch Mystery Egg processing error: {ex.Message}", nameof(BatchMysteryEggAsync));
                }
            });

            if (Context.Message is IUserMessage userMessage)
                _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
        }

        [Command("mysterypokemon")]
        [Alias("mp")]
        [Summary("Trades a random legal Pokémon.")]
        public async Task TradeMysteryPokemonAsync()
        {
            var userID = Context.User.Id;
            if (Info.IsUserInQueue(userID))
            {
                await ReplyAsync("You already have an existing trade in the queue. Please wait until it is processed.").ConfigureAwait(false);
                return;
            }

            var code = Info.GetRandomTradeCode(userID);
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessMysteryPokemonTradeAsync(code).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogUtil.LogSafe(ex, nameof(MysteryModule<T>));
                }
            });
        }

        [Command("batchMysteryPokemon")]
        [Alias("bmp")]
        [Summary("Trades multiple Mystery Pokémon at once.")]
        public async Task BatchMysteryPokemonAsync([Summary("Number of Pokémon")] int count = 2)
        {
            var batchSettings = SysCord<T>.Runner.Config.Trade.BatchSettings;
            if (!batchSettings.AllowMysteryPokemonBatchTrades)
            {
                await ReplyAsync("Batch Mystery Pokémon are currently disabled by the bot administrator.").ConfigureAwait(false);
                return;
            }

            var userID = Context.User.Id;
            if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
            {
                await Helpers<T>.ReplyAndDeleteAsync(Context,
                    "You already have an existing trade in the queue that cannot be cleared. Please wait until it is processed.", 2);
                return;
            }

            // Validate count
            int maxPkms = batchSettings.MaxMysteryPokemonPerBatch;
            if (count < 1 || count > maxPkms)
            {
                await Helpers<T>.ReplyAndDeleteAsync(Context,
                    $"Invalid number of Pokémon. Please specify between 1 and {maxPkms} Pokémon.", 5);
                return;
            }

            var processingMessage = await Context.Channel.SendMessageAsync($"{Context.User.Mention} Generating {count} Mystery Pokémon...");

            _ = Task.Run(async () =>
            {
                try
                {
                    var batchList = new List<T>();
                    var failedCount = 0;

                    // Generate all mystery pokemon
                    for (int i = 0; i < count; i++)
                    {
                        var pk = TradeModuleHelpers.GenerateLegalMysteryPokemon<T>();
                        if (pk != null)
                        {
                            batchList.Add(pk);
                        }
                        else
                        {
                            failedCount++;
                        }
                    }

                    await processingMessage.DeleteAsync();

                    // Check if we generated any pokemon
                    if (batchList.Count == 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} Failed to generate any Mystery Pokémon. Please try again.");
                        return;
                    }

                    // Warn if some failed
                    if (failedCount > 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} Warning: Failed to generate {failedCount} Pokémon. Proceeding with {batchList.Count} Pokémon.");
                    }

                    // Add batch to queue
                    var batchTradeCode = Info.GetRandomTradeCode(userID);
                    await ProcessBatchMysteryItems(Context, batchList, batchTradeCode, count, "Mystery Pokémon", SysBot.Pokemon.Helpers.AssetManager.GetAssetUrl("Assets/Bot/Extras/mystery_box.png"));
                }
                catch (Exception ex)
                {
                    try { await processingMessage.DeleteAsync(); } catch { }
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} An error occurred while processing your batch Mystery Pokémon request. Please try again.");
                    Base.LogUtil.LogError($"Batch Mystery Pokemon processing error: {ex.Message}", nameof(BatchMysteryPokemonAsync));
                }
            });

            if (Context.Message is IUserMessage userMessage)
                _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
        }

        private static async Task ProcessBatchMysteryItems(SocketCommandContext context, List<T> batchList, int batchTradeCode, int totalItems, string typeName, string imageUrl)
        {
            var sig = context.User.GetFavor();
            var firstItem = batchList[0];
            var trainer = new PokeTradeTrainerInfo(context.User.Username, context.User.Id);
            var notifier = new DiscordTradeNotifier<T>(firstItem, trainer, batchTradeCode, context.User, 1, totalItems, true, lgcode: [], fallbackChannel: context.Channel);

            int uniqueTradeID = TradeUtil.GenerateUniqueTradeID();

            var detail = new PokeTradeDetail<T>(
                firstItem,
                trainer,
                notifier,
                PokeTradeType.Batch,
                batchTradeCode,
                sig == RequestSignificance.Favored,
                null,
                1,
                batchList.Count,
                true,
                true,
                uniqueTradeID
            )
            {
                BatchTrades = batchList
            };

            var trade = new TradeEntry<T>(detail, context.User.Id, PokeRoutineType.Batch, context.User.Username, uniqueTradeID);
            var hub = SysCord<T>.Runner.Hub;
            var Info = hub.Queues.Info;
            var added = Info.AddToTradeQueue(trade, context.User.Id, false, sig);

            // Send trade code once
            await EmbedHelper.SendTradeCodeEmbedAsync(context.User, batchTradeCode).ConfigureAwait(false);

            // Start queue position updates for Discord notification
            if (added != QueueResultAdd.AlreadyInQueue && notifier is DiscordTradeNotifier<T> discordNotifier)
            {
                await discordNotifier.SendInitialQueueUpdate().ConfigureAwait(false);
            }

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                await context.Channel.SendMessageAsync("You are already in the queue!").ConfigureAwait(false);
                return;
            }

            var position = Info.CheckPosition(context.User.Id, uniqueTradeID, PokeRoutineType.Batch);
            var botct = Info.Hub.Bots.Count;
            var baseEta = position.Position > botct ? Info.Hub.Config.Queues.EstimateDelay(position.Position, botct) : 0;

            // Send initial batch summary message
            await context.Channel.SendMessageAsync($"{context.User.Mention} - Added batch of {batchList.Count} {typeName}s to the queue! Position: {position.Position}. Estimated: {baseEta:F1} min(s).").ConfigureAwait(false);

            // Create and send embeds for each item
            if (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.UseEmbeds)
            {
                for (int i = 0; i < batchList.Count; i++)
                {
                    var pk = batchList[i];
                    var embed = CreateMysteryItemEmbed(context, pk, i + 1, batchList.Count, position.Position, typeName, imageUrl);
                    await context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                    // Small delay between embeds to avoid rate limiting
                    if (i < batchList.Count - 1)
                    {
                        await Task.Delay(500);
                    }
                }
            }
        }

        private static Embed CreateMysteryItemEmbed(SocketCommandContext context, T pk, int itemNumber, int totalItems, int queuePosition, string typeName, string imageUrl)
        {
            var embedBuilder = new EmbedBuilder()
                .WithColor(global::Discord.Color.Gold)
                .WithTitle($"\u2728 {typeName} {itemNumber} of {totalItems}")
                .WithDescription($"A mysterious {typeName.ToLower()} containing a random Pokémon!")
                .WithImageUrl(imageUrl)
                .WithFooter($"Batch Trade {itemNumber} of {totalItems}" + (itemNumber == 1 ? $" | Position: {queuePosition}" : $"\nNexusBot.NET {NexusBot.Version}"))
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"{typeName} for {context.User.Username}")
                    .WithIconUrl(context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl())
                    .WithUrl(SysBot.Pokemon.Helpers.AssetManager.GetAssetUrl("Assets/Bot/Extras/FromTheHeart2.png")));

            return embedBuilder.Build();
        }

        private async Task ProcessMysteryEggTradeAsync(int code)
        {
            var mysteryEgg = TradeModuleHelpers.GenerateLegalMysteryEgg<T>();
            if (mysteryEgg == null)
            {
                await ReplyAsync("Failed to generate a legal mystery egg. Please try again later.").ConfigureAwait(false);
                return;
            }

            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(
                Context, code, Context.User.Username, sig, mysteryEgg,
                PokeRoutineType.LinkTrade, PokeTradeType.Specific, Context.User,
                isMysteryEgg: true, lgcode: Info.GetRandomLGTradeCode(Context.User.Id)
            ).ConfigureAwait(false);

            if (Context.Message is IUserMessage userMessage)
                _ = DeleteMessageAfterDelay(userMessage, 2000);
        }

        private async Task ProcessMysteryPokemonTradeAsync(int code)
        {
            var mysteryPk = TradeModuleHelpers.GenerateLegalMysteryPokemon<T>();
            if (mysteryPk == null)
            {
                await ReplyAsync("Failed to generate a legal mystery Pokémon. Please try again later.").ConfigureAwait(false);
                return;
            }

            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(
                Context, code, Context.User.Username, sig, mysteryPk,
                PokeRoutineType.LinkTrade, PokeTradeType.Specific, Context.User,
                lgcode: Info.GetRandomLGTradeCode(Context.User.Id)
            ).ConfigureAwait(false);

            if (Context.Message is IUserMessage userMessage)
                _ = DeleteMessageAfterDelay(userMessage, 2000);
        }

        private static async Task DeleteMessageAfterDelay(IUserMessage message, int delayMilliseconds)
        {
            await Task.Delay(delayMilliseconds).ConfigureAwait(false);
            try { await message.DeleteAsync().ConfigureAwait(false); } catch { }
        }

        private static List<Pictocodes> GenerateRandomPictocodes(int count)
        {
            var random = new Random();
            var values = Enum.GetValues<Pictocodes>();
            var result = new List<Pictocodes>(count);
            for (int i = 0; i < count; i++)
                result.Add(values[random.Next(values.Length)]);
            return result;
        }

        private static EntityContext GetContext() => typeof(T).Name switch
        {
            "PB8" => EntityContext.Gen8b,
            "PK8" => EntityContext.Gen8,
            "PK9" => EntityContext.Gen9,
            _ => EntityContext.None
        };
    }
}
