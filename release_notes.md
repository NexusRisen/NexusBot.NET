# Release Notes

## DudeBot.NET v6.5.4

### New Features
- **Stoat.chat Integration**: Added native support for Stoat.chat as a fully-featured integration. The bot can now connect and handle all major trade queues directly on Stoat.
- **Stoat Commands**: Implemented Discord-parity commands for Stoat (`$trade`, `$medals`, `$leaderboard`, `$specialrequestpokemon`, `$linkcode`, `$link`, etc.).
- **Stoat SQL Synchronization**: The bot now fully maps Stoat user IDs using standard DudeBot hashing, integrating directly with existing SQLite databases.
- **Cross-Platform Account Linking**: Added `$linkcode` and `$link` to easily merge stats, Medals, and OT/SID/TID tracking across Discord, Kook, Slack, and Stoat.

### Fixes and Improvements
- Updated integration dependencies to gracefully handle missing bot tokens without crashing upon initialization.
- Fixed `System.Text.Json` source generator type collision warnings when multiple integrations registered settings components with identical class names.
- Internal test suite enhancements in `SysBot.Tests` for `SysStoat` logic without live connections.
