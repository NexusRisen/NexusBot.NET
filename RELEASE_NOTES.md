# Release Notes - v6.2.6

## [v6.2.6] - 2026-05-19

### Added
- **Medals Toggle**: Added a new setting `Enable Medals System` in the Discord integration section. Users can now enable or disable the medal system (including the `$medals` command and medal display in trade embeds).
- **Kook Integration**: Implemented initial bot connection status handlers in `SysKook.cs`, providing better internal visibility into Kook bot lifecycle events.

### Fixed
- **Medal Milestones**: Corrected a logic bug in `MedalHelpers.cs` where the **750 trades** milestone was missing, ensuring users can now correctly achieve the "Pokémon Legend" status.
- **Legends Z-A Safety**: Re-enabled the block on PLZA dump trades in `DumpModule.cs` as they are currently reported as unstable.
- **Resource Management**: Added missing `CancellationToken` support to `PokeBotRunnerImpl` in the WinForms project to prevent background tasks from hanging after the application closes.
- **Twitch Stability**: Fixed a potential infinite loop in the Twitch reconnection logic by adding a cancellation check and improved error handling.

### Changed
- **Content Policy**: Renamed the 200-trade milestone from an inappropriate joke to **"Advanced Trainer"** to align with community standards.
- **Logging**: Improved startup error reporting in the Console application. Startup failures now log the specific exception message rather than a generic error.
