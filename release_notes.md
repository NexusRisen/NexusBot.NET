# Release Notes

## [8.0.0]

### Added
- **AI Chatbot File Generation:** Users can now reply "File" or "pkm" when the AI generates a legal Showdown set to receive the exact `.pkm` file directly as a Discord attachment instead of joining the trade queue.

### Improved
- **AI Rate Limiting:** Implemented robust rate-limit handling for the Hugging Face API. Added a `SemaphoreSlim` queue to manage concurrent AI requests and explicitly parse `Retry-After` headers on HTTP 429 errors, gracefully handling high server traffic without triggering API bans.

### Fixed
- **Stability and Code Quality:** Resolved compiler warnings related to nullable references in `MGDBUpdater.cs` and uninitialized fields in `ReportIssueForm.cs`, preventing potential `NullReferenceException` crashes.
- **Automated Tests:** Fixed the test suite by updating the missing `FluentAssertions` dependency in `SysBot.Tests`, ensuring all core logic tests pass correctly.
- **MGDB Initialization Crash:** Fixed a critical bug where the Mystery Gift Database (MGDB) updater would attempt to delete the bot's own installation files on startup. The database is now securely sandboxed in the shared `AppData\Local` directory.
- **Multi-Instance Support:** Implemented a system-wide Mutex for the MGDB updater. Multiple bots launched simultaneously will now safely coordinate and share the same central database without race conditions or file lock crashes.
