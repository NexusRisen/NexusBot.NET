namespace SysBot.Pokemon;

/// <summary>
/// Indicates the significance of request data.
/// </summary>
public enum RequestSignificance
{
    /// <summary>
    /// Default significance
    /// </summary>
    None,

    /// <summary>
    /// Above-average significance
    /// </summary>
    Favored,

    /// <summary>
    /// Tier 4 Priority (Lowest priority skip)
    /// </summary>
    Tier4,

    /// <summary>
    /// Tier 3 Priority
    /// </summary>
    Tier3,

    /// <summary>
    /// Tier 2 Priority
    /// </summary>
    Tier2,

    /// <summary>
    /// Tier 1 Priority (Highest priority skip)
    /// </summary>
    Tier1,

    /// <summary>
    /// Highest significance (testing purposes)
    /// </summary>
    Owner,
}
