# Release Notes

## [9.1.14]
- **Ping Command Update**: The `$ping` command now randomly selects cool sci-fi/computerized responses instead of a generic message while continuing to display bot latency.
- **Medals System Fix**: Fixed a bug where the medals system repeatedly congratulated users on their first trade on every single trade interaction. The system now properly increments the user's trade count and triggers the correct milestone notifications (e.g. 50 trades, 100 trades).
- **System Tray Icon Label**: Hidden system tray icon tooltip now dynamically displays `Bot Name - Game Version` (e.g. `NexusBot.NET - SV`).
- **Instant Bot Start & Restart**: Bot start and restart operations now execute instantly without artificial delays, cleanly connecting and disconnecting integrations (Discord/Kook/Stoat) when starting or stopping all bots.
