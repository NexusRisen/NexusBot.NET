# DudeBot.NET Release Notes

All notable changes to this project will be documented in this file.

## [Unreleased]
### Added
- **Automated Testing Suite Expansion**:
  - **AutoOT Validation**: Added tests to verify Original Trainer (OT) name sanitation across different languages (Asian vs. Latin) and ensure Pokémon legality is maintained after applying AutoOT data.
  - **Batch Trade Processing**: Added comprehensive tests for `BatchNormalizer` to validate command conversion (Scale, Met Date, Egg Date) and automatic Alcremie topping injection logic.
  - **Parsing Robustness**: Added tests for `TradeModuleHelpers` to ensure correct splitting of multi-set batch trade content using various delimiters.

## [v6.4.1] - 2026-06-02
### Added
- **Kook Platform Integration**:
  - Complete parity with Discord for core commands (`$trade`, `$clone`, `$fixOT`, `$item`, etc.).
  - Role-based permission system for granular access control.
  - Support for `$batchTrade`, `$pokepaste`, and Special Request Pokémon (SRP).
  - Automatic Online/Offline status announcements via CardMessages.
- **Improved Networking**: Refactored `NetUtil` to resolve ambiguity and improved overall connection stability.

### Changed
- **Version Alignment**: All projects now target `net10.0`.
- **Dependency Updates**: Updated `PKHeX.Core`, `MySqlConnector`, and `Google.Apis.YouTube.v3` to their latest versions.

---
*DudeBot.NET | Cross-Platform Pokemon Trading Intelligence*
