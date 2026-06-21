# DudeBot.NET Release Notes

## [Unreleased]

### Added
- **Stoat Integration**: Added a new Stoat integration module (`SysBot.Pokemon.Stoat`) for enhanced remote control and automation functionality.
- **New UI Themes**: Major aesthetic overhaul in the WinForms application, introducing several new themes:
  - **Cyberpunk Theme**: A neon-lit, high-contrast aesthetic.
  - **Dracula Theme**: Based on the popular dark programming color palette.
  - **Gengar Theme**: Deep ghostly purples and reds mirroring Gengar's authentic colors.
  - **Pikachu Theme**: Electric yellows, reds, and browns representing Pikachu's authentic colors.

### Changed
- **Theme Refinements**: Refined the existing "Dark Theme" and "Modern Theme" with modernized, sleeker color palettes.
- **Documentation**: Updated `README.md` to reflect current integrations, notably adding Stoat and removing Slack.

### Removed
- **Slack Integration**: Completely removed the Slack integration (`SysBot.Pokemon.Slack`) and its associated commands/configuration to focus on the active chat platforms.

### Fixed
- **Thread Pool Starvation**: Fixed a severe deadlock vulnerability during high concurrency operations where `AutoLegalityWrapper` blocked thread pool threads while waiting for long-running processes.
- **Crash Prevention**: Implemented global exception handlers for UI threads and background tasks to prevent the program from terminating during unexpected errors.
