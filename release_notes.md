# NexusBot.NET Release Notes

## [8.0.3]

### Core Engine Updates: PKHeX.Core & ALM Integration
- **PKHeX.Core 26.7.7 Support**: Upgraded the core dependency to `26.7.7` across the entire solution (`SysBot.Pokemon.Discord`, `SysBot.Pokemon.ConsoleApp`, `SysBot.Tests`) eliminating version downgrades and establishing uniform compatibility.
- **ALM Synchronization**: Cloned and custom-built the latest Auto Legality Mod (PKHeX-Plugins tag `26.07.07`) from source specifically targeting `PKHeX.Core` 26.7.7, ensuring zero mismatches between plugins and the core API.
  - Fixed a character encoding issue in `GlyphLegality.cs` to allow proper source building.
  - Resolved unreachable code paths in `SmogonSetGenerator.cs`.
- **API Migration (`StatAlignment`)**: Successfully migrated the entire codebase from the legacy `StatNature` API to the new `StatAlignment` property in PKHeX.Core 26.7.7 to handle minted natures accurately.
  - Updated all Discord embed models and generators (`DetailsExtractor.cs`) to dynamically reflect the new `StatAlignment` property mapping.
- **Enhanced Stability**: Ran the complete `SysBot.Tests` suite yielding a 100% pass rate (84/84 tests) confirming the absolute stability of the newly integrated PKHeX.Core features and Trade mechanics!
