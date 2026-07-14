## NexusBot.NET v9.0.1

### Features
* **Per-Game Databases**: Local SQLite database storage now completely isolates each game mode into its own sandboxed file (e.g. `nexusbot_SV.db`, `nexusbot_PLZA.db`), preventing crossover and improving performance.
* **UI Error Notifications**: Added explicit GUI message boxes when the Discord bot lacks sufficient permissions to operate, keeping you in the loop on server/channel issues.
* **DM Fallbacks**: If direct messages fail (Error 50007), the bot will now automatically attempt to notify you of the issue directly within the server channel instead of silently failing.

### Fixes
* **Bot Stability**: Resolved thread-safety issues during shutdown (`StopAll`) by ensuring collections are iterated securely, preventing "Collection was modified" exceptions.
* **Seamless Migration**: Old JSON files for Medals and Trade Codes are safely migrated and converted directly into SQLite.
