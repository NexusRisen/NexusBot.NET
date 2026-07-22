namespace SysBot.Pokemon;

public enum PokeTradeResult
{
    Success,

    // Trade Partner Failures
    NoTrainerFound,

    TrainerTooSlow,

    TrainerLeft,

    TrainerOfferCanceledQuick,

    TrainerRequestBad,

    IllegalTrade,

    SuspiciousActivity,

    UserCanceled,

    // Recovery -- General Bot Failures
    // Anything below here should be retried once if possible.
    RoutineCancel,

    ExceptionConnection,

    ExceptionInternal,

    RecoverStart,

    RecoverPostLinkCode,

    RecoverOpenBox,

    RecoverReturnOverworld,

    RecoverEnterUnionRoom,
}

public static class PokeTradeResultExtensions
{
    public static bool ShouldAttemptRetry(this PokeTradeResult t) => t >= PokeTradeResult.RoutineCancel;

    public static string ToUserString(this PokeTradeResult t) => t switch
    {
        PokeTradeResult.NoTrainerFound => "No trading partner found. Canceling the trade.",
        PokeTradeResult.TrainerTooSlow => "Trainer was too slow to respond.",
        PokeTradeResult.TrainerLeft => "Trainer left the trade.",
        PokeTradeResult.TrainerOfferCanceledQuick => "Trainer canceled the offer too quickly.",
        PokeTradeResult.TrainerRequestBad => "Trainer requested an invalid or un-tradable Pokémon.",
        PokeTradeResult.IllegalTrade => "Illegal Pokémon detected.",
        PokeTradeResult.SuspiciousActivity => "Suspicious activity detected.",
        PokeTradeResult.UserCanceled => "Trade was canceled.",
        _ => t.ToString(),
    };
}
