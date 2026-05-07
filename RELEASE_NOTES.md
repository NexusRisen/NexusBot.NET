# Release Notes

## [v6.1.4] - 2026-05-07

### Added
- **Main Creators**: Officially added **Havok** and **Link** as the main creators of DudeBot in the information command.

### Changed
- **About Command Rewrite**: Completely redesigned the Discord `about`/`info` command with a cleaner layout and improved information architecture.
- **Resource Management**: Implemented `IDisposable` across core classes (`IPokeBotRunner`, `BotSource`, `PokeTradeHub`, `BotSynchronizer`, `LedyDistributor`) to ensure proper cleanup of unmanaged resources.

### Fixed
- **Memory Leaks (GDI+)**: Fixed numerous memory leaks involving `System.Drawing.Image`, `Bitmap`, and `Graphics` objects in trade notification and Picto Code generation logic.
- **Memory Leaks (Event Handlers)**: Fixed potential memory leaks caused by unsubscribed event handlers in Discord and Kook bot modules.
- **Socket Exhaustion**: Optimized `HttpClient` usage by switching to a shared singleton instance for attachment downloads and image loading.
- **Bot Lifecycle**: Fixed resource leaks occurring during bot restarts and game mode switching in the WinForms application.
- **Logging Cache**: Improved cleanup of bot-specific logging buffers and caches when bots are removed.

---

## [v6.1.3] - 2026-05-06
- Initial release of DudeBot.NET.
- Integrated Discord, Kook, and Twitch bot platforms.
- Support for multiple Pokémon generations and automation routines.
