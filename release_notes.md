# Release Notes

## [8.0.1]

### Added
- **`$pokemon` Command**: Introduced a new `$pokemon` command in Discord to quickly link users to an online Pokédex reference.
- **Legends: Z-A Evolution Guard**: Added a `DisallowTradeEvolutionsZA` toggle in TradeSettings. When enabled, this automatically halts trades involving Pokémon that evolve via trade in *Pokémon Legends: Z-A* to prevent in-game crashes.

### Improved
- **PKHeX Core Standards**: Realigned `StatNature` checks and unminted state tracking (`Nature.Random`) to the latest PKHeX 26.5.5 requirements.
- **Dependency Upgrades**: Upgraded `Microsoft.NET.Test.Sdk` package dependency to 18.7.0.

### Fixed
- **Discord Command Deadlocks**: Fixed a severe application lockup issue caused by two heavy Discord commands executing concurrently. Command handling is now safely synchronized.
- **Original Trainer Retention**: Fixed an issue where ALM (AutoLegalityWrapper) would mistakenly randomize Original Trainer info (OT Name, TID, and SID) when generating from showdown sets. Honors the intended OT properly now.
- **Box Format Slot Size**: Fixed BoxFormatSlotSize handling for PLZA (`0x158`) enabling correct box reads and generation handling for the format.
