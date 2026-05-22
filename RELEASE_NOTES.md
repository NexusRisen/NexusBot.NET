# Release Notes

## [v6.3.0] - 2026-05-22

### Added
- **Mystery Gift Batch Trades**: Users can now request multiple events from the Mystery Gift database in a single batch trade using the new `.bsrp` (Batch Special Request Pokémon) command.
- **Customizable Batch Limits**: Added new configuration settings to allow bot administrators to enable/disable and set specific limits for Mystery Gift, Mystery Egg, and Mystery Pokémon batch trades.
- **Pokémon Legends: Z-A (PLZA) Support**: Full integration for `PA9` format, including game-specific legality rules, Alpha status handling, and specialized Poké Ball support.
- **Enhanced Legality Handling**: Improved automatic legalization for mystery events, including better handling of handling trainer data and memory checks.
- **Kook Integration Upgrades**: Added rich Card message support for bot status announcements (online/offline) in the Kook integration.

### Changed
- **Standardized Trade Flow**: Centralized Showdown processing and legalization logic into `PokeTradeHelper<T>` for consistent behavior across Discord, Kook, and Twitch integrations.
- **Global UTC Synchronization**: Migrated all time-sensitive logic (cooldowns, timeouts, and rate limits) to `DateTime.UtcNow` to ensure consistency across different time zones.
- **Improved Trade Identifiers**: Standardized unique trade ID generation to prevent collisions in high-traffic multi-bot environments.

### Fixed
- **Queue Race Conditions**: Resolved a thread-safety issue in the trade queue that could occasionally allow duplicate entries.
- **Batch Tracking Reliability**: Fixed a race condition in `BatchTradeTracker` and improved the reliability of Pokémon receipt tracking during batch operations.
- **Resource Management**: Optimized socket and memory usage in Twitch and AI service integrations by ensuring proper disposal of long-lived connections.
