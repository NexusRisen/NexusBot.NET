# NexusBot.NET Release Notes

## [Unreleased]

- **Kook & Discord Command Fixes**: Fixed an issue where bots were case-sensitive for commands and missing the `'c'` (clone) alias. Command names are now matched correctly regardless of casing.
- **Kook Permission Checks**: Fixed an issue in the Kook integration where user roles were not being properly downloaded, causing role-based permission checks to silently fail.
- **Kook Stability & Error Handling**: 
  - Wrapped Kook bot instantiation in an automatic reconnect and rebuild loop, mirroring the Discord implementation's resilience against network drops.
  - Added a null reference check to prevent the bot from crashing when receiving Kook image uploads with no text attached.
  - Wired up the missing Kook `Ready` event so the bot now properly announces its "Online" status in whitelisted channels upon first connecting.
  - Implemented graceful logout upon bot shutdown/restart to prevent orphan or ghost sessions from lingering on Kook's servers.
