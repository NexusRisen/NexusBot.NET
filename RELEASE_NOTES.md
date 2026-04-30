# Release Notes

## [v6.1.0] - 2026-04-30

### Added
- **Localization Improvements**: Added comprehensive English translation dictionary and updated translation logic to ensure stable language switching.
- **Log Localization**: Expanded the log translation engine to support common bot activity messages and status updates across all supported languages.
- **Status Localization**: Bot runner status and IP/Port labels are now correctly translated based on the selected language.

### Fixed
- **UI Stability**: Fixed a critical freeze issue when switching between Program Game Modes caused by event re-entrancy.
- **English Language Persistence**: Resolved an issue where the UI would not fully revert to English when switching back from other languages.
