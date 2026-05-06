# Release Notes

## [v6.1.3] - 2026-05-06

### Added
- **Dynamic Bot Tracking**: Added `BotAdded` and `BotRemoved` events to `PokeBotRunner` to allow integrations (Discord, Kook) to dynamically manage bot event subscriptions.
- **Bot Logger Cleanup**: Implemented `LogUtil.ClearBotLogger` to prevent unbounded memory growth in the bot logger cache.

### Fixed
- **Critical Memory Leak Remediation**:
  - Fixed event subscription leaks in `SysCord` and `SysKook` by replacing anonymous lambdas with named delegates and ensuring unsubscription during disposal.
  - Resolved resource leaks in `Pokepaste.cs` by ensuring all GDI+ objects (`Bitmap`, `Image`, `Graphics`) are properly disposed.
  - Fixed an infinite reconnection loop leak in `TwitchBot.cs` using a disposal flag.
  - Ensured static `QueuePool` in `TwitchBot` is cleared upon disposal.
  - Fixed build errors in `SysBot.Pokemon.Discord` and `SysBot.Pokemon.Kook` related to incorrect bot property access.
- **Resource Optimization**:
  - Centralized `HttpClient` in `NetUtil.cs` to prevent socket exhaustion.
  - Optimized image composition and overlay logic in `QueueHelper.cs` with proper resource disposal.

### Changed
- **Dependency Updates**: Merged PR #22 to bump `Google.Apis.YouTube.v3` from 1.73.0.4134 to 1.74.0.4137.
- **Version Update**: Incremented application version to **v6.1.3**.
