using Kook;
using Kook.WebSocket;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using PKHeX.Drawing.PokeSprite;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CA1416

namespace SysBot.Pokemon.Kook;

public class KookTradeNotifier<T> : IPokeTradeNotifier<T>, IDisposable
    where T : PKM, new()
{
    private T Data { get; set; }
    private PokeTradeTrainerInfo Info { get; }
    private int Code { get; }
    private List<Pictocodes>? LGCode { get; }
    private SocketUser Trader { get; }
    private readonly KookSocketClient _client;

    public KookTradeNotifier(T data, PokeTradeTrainerInfo info, int code, SocketUser trader, KookSocketClient client, List<Pictocodes>? lgcode = null)
    {
        Data = data;
        Info = info;
        Code = code;
        Trader = trader;
        _client = client;
        LGCode = lgcode;
    }

    public Action<PokeRoutineExecutor<T>>? OnFinish { get; set; }

    public void UpdateBatchProgress(int currentBatchNumber, T currentPokemon, int uniqueTradeID)
    {
        Data = currentPokemon;
    }

    public Task SendInitialQueueUpdate()
    {
        return Trader.SendTextAsync("Your trade request has been queued.");
    }

    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        _ = Task.Run(async () =>
        {
            var speciesName = SpeciesName.GetSpeciesName(Data.Species, 2);
            if (Data is PB7 && LGCode != null && LGCode.Count == 3)
            {
                await SendLGLinkCodeCardAsync(LGCode, $"Initializing trade for {speciesName}. Please be ready.");
            }
            else
            {
                await Trader.SendTextAsync($"Initializing trade for {speciesName}. Code: {Code:D8}. Please be ready.");
            }
        });
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var name = Info.TrainerName;
        var trainer = string.IsNullOrEmpty(name) ? string.Empty : $" {name}";
        Trader.SendTextAsync($"I'm waiting for you{trainer}! My IGN is **{routine.InGameName}**.").ConfigureAwait(false);
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        Trader.SendTextAsync($"Trade canceled: {msg}").ConfigureAwait(false);
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
    {
        Trader.SendTextAsync("Trade finished. Enjoy!").ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
    {
        Trader.SendTextAsync(message).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
    {
        Trader.SendTextAsync(message.Summary).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
    {
        Trader.SendTextAsync(message).ConfigureAwait(false);
    }

    private async Task SendLGLinkCodeCardAsync(List<Pictocodes> lgcode, string text)
    {
        try
        {
            using var ms = CreateLGLinkCodeImage(lgcode);
            var assetUrl = await _client.Rest.CreateAssetAsync(ms, "lgcode.png");

            var card = new CardBuilder()
                .AddModule<SectionModuleBuilder>(s => s.WithText($"{text}\n**Code**: {lgcode[0]}, {lgcode[1]}, {lgcode[2]}"))
                .AddModule<ContainerModuleBuilder>(c => c.AddElement(new ImageElementBuilder().WithSource(assetUrl)))
                .Build();

            await Trader.SendCardAsync(card);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Failed to send LG Picto Code card: {ex.Message}", "KookTradeNotifier");
            await Trader.SendTextAsync($"{text}\nCode: {lgcode[0]}, {lgcode[1]}, {lgcode[2]}");
        }
    }

    private MemoryStream CreateLGLinkCodeImage(List<Pictocodes> lgcode)
    {
        List<System.Drawing.Image> spritearray = [];
        try
        {
            foreach (Pictocodes cd in lgcode)
            {
                var showdown = new ShowdownSet(cd.ToString());
                var sav = BlankSaveFile.Get(EntityContext.Gen7b, "pip");
                PKM pk = sav.GetLegalFromSet(showdown).Created;

                using System.Drawing.Image png = pk.Sprite();
                var destRect = new Rectangle(-40, -65, 137, 130);
                var destImage = new Bitmap(137, 130);
                destImage.SetResolution(png.HorizontalResolution, png.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(png, destRect, 0, 0, png.Width, png.Height, GraphicsUnit.Pixel);
                }
                spritearray.Add(destImage);
            }

            int outputImageWidth = spritearray[0].Width + 20;
            int outputImageHeight = spritearray[0].Height - 65;

            using Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(spritearray[0], new Rectangle(0, 0, spritearray[0].Width, spritearray[0].Height),
                    new Rectangle(new Point(), spritearray[0].Size), GraphicsUnit.Pixel);
                graphics.DrawImage(spritearray[1], new Rectangle(50, 0, spritearray[1].Width, spritearray[1].Height),
                    new Rectangle(new Point(), spritearray[1].Size), GraphicsUnit.Pixel);
                graphics.DrawImage(spritearray[2], new Rectangle(100, 0, spritearray[2].Width, spritearray[2].Height),
                    new Rectangle(new Point(), spritearray[2].Size), GraphicsUnit.Pixel);
            }

            var ms = new MemoryStream();
            outputImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            return ms;
        }
        finally
        {
            foreach (var img in spritearray)
                img.Dispose();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
