# Release Notes

## v6.2.3
### ⚡ Advanced Performance Optimizations
- **High-Frequency Queue Optimization:** 
    - Refactored `TradeQueueInfo.CheckPosition` to eliminate heavy LINQ allocations (`SelectMany`, `ToList`, `Sum`). This reduces the memory footprint of frequent status checks for users in the queue.
    - Optimized `CleanStuckTrades` to use a zero-allocation backward loop, removing the need for temporary list copies.
- **O(1) User Tracking:** 
    - Implemented high-performance Dictionary-based caching for `UserTracker`. 
    - Replaced linear $O(N)$ list searches with $O(1)$ constant-time lookups for both `NetworkID` and `RemoteID`.
    - This drastically improves response times when verifying user permissions or checking previous trade history.
- **Efficient Command Chaining:**
    - Optimized `SwitchRoutineExecutor.DaisyChainCommands` to use manual array concatenation and `Buffer.BlockCopy`.
    - This bypasses the overhead of LINQ `SelectMany` and `ToArray` chains, providing faster execution of macro sequences.
- **Core Improvements:** 
    - Drastic reduction in GC pressure and CPU cycles during high-traffic trade sessions.

## Version Details
- **Internal Version**: v6.2.3
- **Build Date**: May 16, 2026
