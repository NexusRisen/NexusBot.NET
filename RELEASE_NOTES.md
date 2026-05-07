# Release Notes

## [v6.1.5] - 2026-05-07

### Summary
This release focuses on a comprehensive "Great Memory Leak Purge," involving a complete architectural audit of resource management across all bot platforms and core systems. Stability for long-running sessions is significantly improved.

### Added
- **Automated Background Maintenance**: A new background service that periodically prunes stale trade history, prunes log buffers, and clears expired batch trade data.
- **Resource Lifecycle Guards**: Implemented `IDisposable` and cancellation support across all platform-specific bot modules (Discord, Kook, Twitch, YouTube).

### Changed
- **NLog Configuration Optimization**: Logging targets and rules are now surgically removed when bots disconnect, preventing indefinite memory growth in the NLog engine.
- **Connection Integrity**: Enhanced console connection logic with mandatory `try-finally` blocks to ensure USB and Socket handles are released even during hardware crashes.
- **Network Optimization**: Centralized `HttpClient` usage and ensured all network streams are deterministically disposed of.

### Fixed
- **Memory Leaks (GDI+)**: Fixed critical leaks in WinForms, Discord, and Kook modules related to `Bitmap`, `Graphics`, and `Image` objects.
- **Zombie Tasks**: Resolved issues where background reconnection loops and countdown timers would continue running after a bot was stopped or disposed.
- **Static Collection Growth**: Fixed "slow death" leaks where Discord DM caches and trade history lists grew indefinitely.
- **Build Warnings**: Resolved all platform-compatibility warnings (`CA1416`) in the Discord and Kook integrations.
