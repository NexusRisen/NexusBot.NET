# Release Notes

## [v6.2.9] - 2026-05-20

### Added
- **Full support for Pokémon Legends: Z-A (`PA9`)**: Added game-specific rules for trade processing, special request modules, and Showdown translation (including Alpha status and specific Poké Balls).
- **Kook Bot Status Announcements**: Added settings for online/offline status reporting with rich Card message support.
- **Unified Trade Helper**: Introduced a centralized `PokeTradeHelper<T>` to standardize Showdown processing, legalization, and bug workarounds across all bot platforms.

### Changed
- **System-Wide UTC Migration**: Converted all logic-critical timing (trade timeouts, queue weighting, cooldowns, rate limits) from local time to `DateTime.UtcNow` for global consistency.
- **Unified Trade ID Generation**: Standardized trade identifier generation in `TradeUtil`, eliminating potential ID collisions across different bot integrations.
- **Centralized Trade Models**: Relocated shared models (`ProcessedPokemonResult`, `BatchTradeResult`) to a common structure for better architectural integrity.
- **Relocated BatchNormalizer**: Moved advanced command normalization logic to the shared Pokémon project, making it available to Discord, Kook, and Twitch bots.

### Fixed
- **Discord Queue Race Condition**: Hardened thread-safety in the trade queue to prevent users from bypassing the "already in queue" check through concurrent commands.
- **Batch Trade ID Bug**: Fixed an issue where batch trades were incorrectly assigned an ID of `0` in certain scenarios.
- **`BatchTradeTracker` Reliability**: Fixed a race condition in Pokémon receipt tracking and streamlined background maintenance logic.
- **YouTube Bot Permissions**: Corrected an inverted administrative command check.
- **Resource Management**: Fixed potential memory and socket leaks in Twitch and AI services by ensuring proper disposal of WebSocket and HTTP clients.
- **Translation Stability**: Added null checks and duplicate key protection to the translation cache.

### Technical Debt
- Simplified reflection-based cleanup logic in `BotRecoveryService`.
- Standardized `IDisposable` implementation across all integration modules.
- Cleaned up redundant local methods and fragmented logic in Discord and Twitch projects.
