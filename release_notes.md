# NexusBot.NET v7.0.8

This update completely overhauls the internal storage and tracking mechanisms, bringing everything to the local filesystem for faster, disconnected execution.

### Major Changes
- **Completely removed Remote SQL Database**: All traces of the remote database and connection strings have been purged. NexusBot is now entirely local-only.
- **Removed Account Linking**: The ability to cross-link accounts via tokens has been deprecated and completely removed along with the `/link` commands.
- **Removed Global Leaderboards**: In tandem with the database removal, the global hall of fame and `/leaderboard` tracking have been removed.

### Architectural Improvements
- **Decoupled Medals**: Trade counts and medals are no longer tracked in messy shared data files. All trades are now simply logged and incremented into a standalone `data/medals.json` isolated completely from trade queues.
- **Purged Heartbeat API**: The active-bots heartbeat broadcast is completely removed to keep NexusBot isolated.
- **Purged Cloud Blacklists**: The cloud-based global guild blacklist check has been removed, maintaining only the local manual configuration file limits.
