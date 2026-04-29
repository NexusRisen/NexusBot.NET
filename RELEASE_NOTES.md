# Release Notes

## [v6.0.8] - 2026-04-29

### Updated
- **PKHeX.Core**: Updated to the latest version from `hexbyt3/PKHeX`.
- **AutoLegalityMod**: Updated to the latest version from `santacrab2/PKHeX-Plugins`.
- **Google.Apis.YouTube.v3**: Bumped from 1.73.0.4099 to 1.73.0.4119 (Merged PR #16).

### Changed
- **Refactoring for PKHeX API Changes**: 
    - Migrated from `.GetContext()` extension methods to the native `.Context` property across the codebase (impacts `PKM`, `IEncounterTemplate`, `ITrainerInfo`, `ShowdownSet`, etc.).
    - Updated `SysBot.Pokemon.csproj` to explicitly reference `PKHeX.Core.AutoMod.dll`.
    - Adjusted `BallVerifier` and `AwakeningUtil` calls to match new static method signatures in `PKHeX.Core`.

### Fixed
- Resolved compilation errors in `SysBot.Pokemon` resulting from the `PKHeX.Core` API overhaul.
- Fixed unreachable patterns in Smogon set generation for Nidoran species.
