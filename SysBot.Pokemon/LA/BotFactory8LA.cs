using PKHeX.Core;
using System;

namespace SysBot.Pokemon;

public sealed class BotFactory8LA : BotFactory<PKHeX.Core.PA8>
{
    public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PKHeX.Core.PA8> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
    {
        PokeRoutineType.FlexTrade or PokeRoutineType.Idle
            or PokeRoutineType.LinkTrade
            or PokeRoutineType.Clone
            or PokeRoutineType.Dump
            or PokeRoutineType.FixOT
            => new PokeTradeBotLA(Hub, cfg),

        PokeRoutineType.RemoteControl => new RemoteControlBotLA(cfg, Hub),

        _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
    };

    public override bool SupportsRoutine(PokeRoutineType type) => type switch
    {
        PokeRoutineType.FlexTrade or PokeRoutineType.Idle
            or PokeRoutineType.LinkTrade
            or PokeRoutineType.Clone
            or PokeRoutineType.Dump
            or PokeRoutineType.FixOT
            => true,

        PokeRoutineType.RemoteControl => true,

        _ => false,
    };
}
