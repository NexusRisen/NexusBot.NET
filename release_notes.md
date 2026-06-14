# Release Notes

## DudeBot.NET v6.5.6

### New Features & Updates
- **Global Leaderboard Integration**: The Stoat bot has been fully integrated with the centralized Dudebot SQL database. Trade medals and usernames from Stoat now synchronize correctly with `dudebot.org/leaderboard`.
- **Stoat About Command**: Added `/about`, `/whoami`, and `/owner` commands to the Stoat bot module, providing real-time system stats, active plugin information, and contributor details natively within Stoat chat.

### Fixes and Improvements
- **Graceful Shutdown**: Mitigated an internal `SocketException` error trace when stopping the bot forcefully, allowing a cleaner shutdown process.
- **Connection Diagnostics**: Fixed the connection status visualizer in the WinForms UI so that it properly displays green when actively connected to the Switch instead of remaining yellow.
- **Cross-Platform Compatibility**: Replaced problematic emojis (like the hammer & wrench) with universally supported alternatives across all bot embeds (e.g. `💻` and `⚙️`) to ensure correct rendering on alternative chat clients.
