using Discord;
using Discord.Commands;
using PKHeX.Core;
using SysBot.Pokemon.Discord.Helpers;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public static class BatchHelpers<T> where T : PKM, new()
{
    public static List<string> ParseBatchTradeContent(string content)
    {
        return TradeModuleHelpers.ParseBatchTradeContent(content);
    }

    public static async Task<(T? Pokemon, string? Error, ShowdownSet? Set, string? LegalizationHint)> ProcessSingleTradeForBatch(string tradeContent)
    {
        var result = await Helpers<T>.ProcessShowdownSetAsync(tradeContent);

        if (result.Pokemon != null)
        {
            return (result.Pokemon, null, result.ShowdownSet, null);
        }

        return (null, result.Error, result.ShowdownSet, result.LegalizationHint);
    }

    public static async Task SendBatchErrorEmbedAsync(SocketCommandContext context, List<BatchTradeError> errors, int totalTrades)
    {
        var embed = new EmbedBuilder()
            .WithTitle("❌ Batch Trade Validation Failed")
            .WithColor(Color.Red)
            .WithDescription($"**{errors.Count}** out of **{totalTrades}** Pokémon could not be processed.\n*Showing up to 24 errors.*")
            .WithFooter("Please fix the invalid sets and try again.")
            .WithTimestamp(DateTimeOffset.Now);

        int fieldsLimit = errors.Count > 25 ? 24 : 25;

        foreach (var error in errors.Take(fieldsLimit))
        {
            var fieldValue = $"**Error:** {error.ErrorMessage}";
            if (!string.IsNullOrEmpty(error.LegalizationHint))
            {
                fieldValue += $"\n💡 **Hint:** {error.LegalizationHint}";
            }

            if (!string.IsNullOrEmpty(error.ShowdownSet))
            {
                var lines = error.ShowdownSet.Split('\n').Take(2);
                fieldValue += $"\n**Set:** {string.Join(" | ", lines)}...";
            }

            if (fieldValue.Length > 1024)
            {
                fieldValue = fieldValue[..1021] + "...";
            }

            embed.AddField($"Trade #{error.TradeNumber} - {error.SpeciesName}", fieldValue);
        }

        if (errors.Count > 25)
        {
            embed.AddField("⚠️ And More...", $"There are {errors.Count - 24} more errors not shown here.", inline: false);
        }

        var replyMessage = await context.Channel.SendMessageAsync(embed: embed.Build());
        _ = Helpers<T>.DeleteMessagesAfterDelayAsync(replyMessage, context.Message, 20);
    }

    public static async Task<BatchTradeResult<T>> ProcessAttachmentsForBatch(SocketCommandContext context)
    {
        var pokemonList = new List<T>();
        var errors = new List<BatchTradeError>();
        var attachments = context.Message.Attachments;

        if (attachments == null || !attachments.Any())
            return new BatchTradeResult<T>(pokemonList, errors);

        int tradeIndex = 1;
        foreach (var attachment in attachments)
        {
            try
            {
                if (attachment.Filename.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    var textResult = await ProcessTextFileForBatch(attachment, tradeIndex).ConfigureAwait(false);
                    pokemonList.AddRange(textResult.Pokemon);
                    errors.AddRange(textResult.Errors);
                    tradeIndex += textResult.Pokemon.Count + textResult.Errors.Count;
                }
                else
                {
                    var att = await DiscordNetUtil.DownloadPKMAsync(attachment).ConfigureAwait(false);
                    var pk = Helpers<T>.GetRequest(att);

                    if (pk != null)
                    {
                        pokemonList.Add(pk);
                    }
                    else
                    {
                        errors.Add(new BatchTradeError
                        {
                            TradeNumber = tradeIndex,
                            SpeciesName = attachment.Filename,
                            ErrorMessage = "Attachment is not a valid Pokémon file or compatible text file."
                        });
                    }
                    tradeIndex++;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new BatchTradeError
                {
                    TradeNumber = tradeIndex,
                    SpeciesName = attachment.Filename,
                    ErrorMessage = $"Error downloading or processing: {ex.Message}"
                });
                tradeIndex++;
            }
        }

        return new BatchTradeResult<T>(pokemonList, errors);
    }

    private static async Task<BatchTradeResult<T>> ProcessTextFileForBatch(IAttachment attachment, int startIndex)
    {
        var pokemonList = new List<T>();
        var errors = new List<BatchTradeError>();

        try
        {
            using var client = new System.Net.Http.HttpClient();
            var content = await client.GetStringAsync(attachment.Url).ConfigureAwait(false);
            
            content = BatchNormalizer.NormalizeBatchCommands(content);
            content = ReusableActions.StripCodeBlock(content);
            var trades = ParseBatchTradeContent(content);

            for (int i = 0; i < trades.Count; i++)
            {
                var (pk, error, set, legalizationHint) = await ProcessSingleTradeForBatch(trades[i]);
                if (pk != null)
                {
                    pokemonList.Add(pk);
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
        catch (Exception ex)
        {
            errors.Add(new BatchTradeError
            {
                TradeNumber = startIndex,
                SpeciesName = attachment.Filename,
                ErrorMessage = $"Failed to process text file: {ex.Message}"
            });
        }

        return new BatchTradeResult<T>(pokemonList, errors);
    }

    public static async Task ProcessBatchContainer(SocketCommandContext context, List<T> batchPokemonList,
        int batchTradeCode, int totalTrades)
    {
        var sig = context.User.GetFavor();
        var firstPokemon = batchPokemonList[0];

        await QueueHelper<T>.AddBatchContainerToQueueAsync(context, batchTradeCode, context.User.Username,
            firstPokemon, batchPokemonList, sig, context.User, totalTrades).ConfigureAwait(false);
    }

    public static string BuildDetailedBatchErrorMessage(List<BatchTradeError> errors, int totalTrades)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"**Batch Trade Validation Failed**");
        sb.AppendLine($"❌ {errors.Count} out of {totalTrades} Pokémon could not be processed.\n");

        foreach (var error in errors)
        {
            sb.AppendLine($"**Trade #{error.TradeNumber} - {error.SpeciesName}**");
            sb.AppendLine($"Error: {error.ErrorMessage}");

            if (!string.IsNullOrEmpty(error.LegalizationHint))
            {
                sb.AppendLine($"💡 Hint: {error.LegalizationHint}");
            }

            if (!string.IsNullOrEmpty(error.ShowdownSet))
            {
                var lines = error.ShowdownSet.Split('\n').Take(3);
                sb.AppendLine($"Set Preview: {string.Join(" | ", lines)}...");
            }

            sb.AppendLine();
        }

        sb.AppendLine("**Please fix the invalid sets and try again.**");
        return sb.ToString();
    }
}
