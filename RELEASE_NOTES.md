# RELEASE NOTES

## v6.2.5

### 🔧 Concurrency & State Management Overhaul
- **Thread-Safe Bot Lifecycle**: Completely overhauled the state transitions (`Start`, `Stop`, `Pause`, `Restart`) within `BotSource` and `RecoverableBotSource` to use proper thread-locking mechanisms. This eliminates race conditions and "infinite loops" caused by rapid UI inputs or simultaneous sudo commands in Discord.
- **Fixed Shadowing in Recovery Logic**: Resolved method shadowing in `RecoverableBotSource` by correctly utilizing `virtual` and `override`. The `BotRecoveryService` now accurately tracks intentional stops versus actual crashes, preventing false-positive recovery loops.
- **Smart "Start While Stopping" Handling**: Fixed the "undefined state" bug where calling `Start()` on a paused or stopping bot would silently fail. The bot now gracefully queues the `Start` action via a continuation task to execute immediately after the `Stop` process finalizes.
- **Safe Sudo Restarts**: Updated the Discord `!botRestart` command to utilize the newly synchronized `bot.Restart()` lifecycle method, preventing duplicate overlapping socket resets when multiple sudo users trigger the command concurrently.
