# Release Notes

## [v6.1.9] - 2026-05-11

### Added
- **Intelligent Item Trades**: The `$it` (itemTrade) command now supports requesting multiple items in a single command (e.g., `$it master ball, rare candy, nugget`).
- **Automatic Batch Conversion**: Requests for multiple items are now automatically converted into a single batch trade for maximum efficiency.
- **Configurable Item Batch Limit**: Added a new setting, `Max Items per Item Trade` (`MaxItemBatchAmount`), to allow administrators to control the maximum number of items allowed in a single request.
- **"Mega-Batch" Engine (100+)**: Completely refactored the trade logic for **Scarlet/Violet, Sword/Shield, BDSP, Legends: Arceus, and Legends: ZA**. All hardcoded timeouts (10s, 15s, 25s, 45s) have been removed. The bot now respects the user's `TradeWaitTime` setting for *every* individual trade in a batch, enabling reliable sessions of **100+ items or Pokémon**.
- **Global Patience Tuning**: Added `Trade Animation Delay` (`TradeAnimationMaxDelaySeconds`) as a global setting to fine-tune the wait time between individual trades in a batch.
- **Kook Platform Parity**: Fully implemented the `itemTrade` command and batch queuing logic for the Kook platform, ensuring feature parity with the Discord bot.
- **Detailed Validation**: Enhanced the error feedback system for batch trades; users now receive a specific report identifying exactly which items or Pokémon were invalid/untradable.

### Changed
- **Official Versioning**: Project has migrated to **v6.1.9**.
- **Efficiency Defaults**: Reduced the default `Maximum Pokémon per Trade` from 10 to **3** for faster default interactions, while still allowing for manual increases up to 100+.
- **Game Compatibility**: Explicitly blocked all batch trade features (including `$bt` and multi-item `$it`) for **LGPE** (Let's Go Pikachu/Eevee) due to game-level limitations. Users will now receive a clear explanation if they attempt these operations in LGPE mode.

### Fixed
- **Scarlet/Violet Reliability**: Resolved critical issues where the SV bot would quit early during batch trades due to hidden hardcoded timers.
- **BDSP/LA Sync**: Fixed several "ghost" timeouts in Brilliant Diamond/Shiny Pearl and Legends: Arceus that previously caused synchronization failures during long sessions.
- **Stability**: Fixed multiple compilation errors and variable scope issues introduced during the project-wide timing refactor.
