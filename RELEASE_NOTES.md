# DudeBot.NET Release Notes

## v6.4.2 - 2026-06-06

### Fixed
- Stopped the trade hub heartbeat task during disposal so released hubs are not kept alive by an uncancelled background loop.
- Cleared stale Twitch, Discord, and Kook static references during integration disposal to support cleaner shutdowns and reloads.
- Unsubscribed Discord lifecycle handlers consistently and removed duplicate Discord client disposal during shutdown.
- Disposed the Discord service provider created for command modules.

### Changed
- Updated the application version from `v6.4.1` to `v6.4.2`.
- Aligned WinForms package metadata with the `6.4.2` release version.
- Reused `DudeBot.Version` in the medals leaderboard footer instead of hardcoding the release string.

### Tested
- Full solution build with `dotnet build SysBot.NET.sln --no-restore`.
- SysBot test project with non-SQL tests enabled. SQL tests require a reachable MySQL host.
