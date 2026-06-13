using System;

namespace SysBot.Base;

/// <summary>
/// Interface for components that require maintenance or cleanup during a bot recovery cycle.
/// </summary>
public interface IRecoveryMaintenance
{
    /// <summary>
    /// Called periodically or during recovery cycles to clean up resources,
    /// reset trackers, or perform maintenance to ensure stability.
    /// </summary>
    void PerformMaintenance();
}
