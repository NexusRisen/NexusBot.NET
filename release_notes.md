# DudeBot.NET Release Notes

## DudeBot.NET v6.5.3
### ✨ New Features
- **Cross-Platform Account Linking**: Users can now link their Discord, Slack, and Kook profiles to a single unified account using `$linkcode` and `$link` commands. This allows trade stats, queue positions, and preferences to be shared seamlessly across platforms!
- **Database Additions**: Added `LinkedAccounts` and `AccountLinkTokens` MySQL tables for securely managing cross-platform profiles and temporary tokens.

### 🚀 Bot Recovery Service Rewrite
A complete overhaul of the background recovery system for bots, providing more stability and better diagnostics.
- **Hard Recovery Protocol**: Introduced `HardRecoveryThreshold`, which forces a complete disconnect and reconnect if a bot fails repeatedly, mitigating total lockups.
- **UI Enhancements**: Added an active indicator for the WinForms Bots tab. The status lamp now turns orange and explicitly shows `Recovering... (Attempt X)` when a bot is undergoing a crash recovery cycle.
- **Improved Diagnostics**: Detailed crash reasons are now captured from the internal aggregate exception and immediately sent directly in Discord crash notifications.
- **Configurable Delays**: Added `VerificationDelaySeconds` to explicitly control how long the bot waits to ensure a restarted session is stable before marking it as recovered.
- **Unlimited Attempts Support**: Settings like `MaxRecoveryAttempts` and `HardRecoveryThreshold` now support `-1` for unlimited recoveries or completely disabling hard disconnects, respectively.
- **Under-the-Hood Improvements**: Eliminated slow reflection-based maintenance in favor of a clean, performant `IRecoveryMaintenance` interface.

### 🛠️ Stability & Testing
- **Unit Testing Suite**: Implemented extensive unit tests for `BotRecoveryService` ensuring accurate backoff delay calculations, configuration verification (including infinite limit tests), and robust history clearing logic.
- **Slack & Kook Code Cleanup**: Resolved multiple strict nullable reference warnings across `SysSlack` and `SysKook` files, improving compilation safety and bringing the project to 0 warnings.
