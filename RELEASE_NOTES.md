# DudeBot.NET Release Notes (v6.4.1)

This release focuses on expanding platform support with a comprehensive Kook integration and stabilizing the core ecosystem for multi-platform synchronization.

## 🚀 New Features
- **Full Kook Platform Integration**:
  - Parity with Discord for essential commands (`$trade`, `$clone`, `$fixOT`, `$ditto`, `$item`, etc.).
  - Robust role-based permission system (Role can Trade/Clone/FixOT).
  - Support for `$batchTrade`, `$pokepaste`, and `$specialrequestpokemon` (SRP).
  - Automatic status announcements (Online/Offline) via Kook CardMessages.
  - Channel whitelist and user blacklist support.
- **Enhanced Integration Settings**:
  - Expanded `KookSettings` to include granular permission controls matching Discord's security model.
  - Improved `RemoteControlAccessList` utilization across platforms.

## 🛠️ Improvements & Bug Fixes
- **Dependency Management**: Unified all projects to target `net10.0` and updated essential NuGet packages.
- **Core Stability**: Refined the `SysKook` message handler for better performance and reliability.
- **Queue Management**: Synchronized queue clear/status logic to prevent platform-specific inconsistencies.
- **Legality Updates**: Integrated latest `PKHeX.Core` improvements via NuGet.

## 📦 Maintenance
- Merged security and performance updates for `MySqlConnector` and `Google.Apis.YouTube.v3`.
- Optimized GitHub Actions workflows for streamlined CI/CD.

---
*DudeBot.NET v6.4.1 | Cross-Platform Synchronized Intelligence*
