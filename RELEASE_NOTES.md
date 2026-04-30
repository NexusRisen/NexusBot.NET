# Release Notes

## [v6.0.9] - 2026-04-29

### Added
- **Kook Platform Integration**: Introduced native support for the Kook communication platform, tailored for the Chinese Pokémon community.
- **SysBot.Pokemon.Kook**: A dedicated project for Kook bot implementation utilizing the `Kook.Net` library.
- **Cross-Platform Commands**: Standard trade and management commands are now accessible via Kook channels.

### Changed
- **Multi-Platform Architecture**: Refactored bot initialization to support concurrent integrations (Discord, Kook, Twitch, YouTube).
- **Version Bump**: Updated all components and metadata to version 6.0.9.

### Fixed
- **Naming Ambiguity**: Resolved naming collisions between Discord and Kook configuration types to ensure stable source generation.
- **Event Handling**: Standardized WebSocket message received signatures across platform integrations.
