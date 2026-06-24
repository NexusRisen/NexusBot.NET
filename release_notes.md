# NexusBot.NET Release Notes

## [7.0.4]

### Added
- **MGDB Auto-Updater:** The bot now automatically checks for updates to the `projectpokemon/EventsGallery` master branch upon startup and seamlessly downloads/extracts the latest wonder card repository locally into the `MGDB` folder.

### Removed
- **Events & HOMEReady Folders:** Completely removed the old `homeready` and `events` directory checking/parsing to simplify bot initialization and reduce file clutter, deprecating `listevents`, `eventrequest`, `battlereadylist`, and `battlereadyrequest` commands across Discord, Stoat, and Kook integrations.
