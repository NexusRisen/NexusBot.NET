# Release Notes

## [v6.1.1] - 2026-05-03

### Changed
- **Dependency Updates**: Updated core libraries to their latest versions for improved stability and performance.
  - `Kook.Net` updated to **0.11.0**.
  - `Google.Apis` family (Core, Auth, and YouTube v3) updated to **1.74.0** / **1.73.0.4134**.

## [v6.1.0] - 2026-04-30

### Added
- **Localization Improvements**: Added comprehensive English translation dictionary and updated translation logic to ensure stable language switching.
- **Log Localization**: Expanded the log translation engine to support common bot activity messages and status updates across all supported languages.
- **Status Localization**: Bot runner status and IP/Port labels are now correctly translated based on the selected language.

### Fixed
- **UI Stability**: Fixed a critical freeze issue when switching between Program Game Modes caused by event re-entrancy.
- **English Language Persistence**: Resolved an issue where the UI would not fully revert to English when switching back from other languages.
