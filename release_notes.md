# NexusBot.NET Release Notes

## [8.0.2]

### Enhancements & Core Standard Updates
- **PKHeX Core Standards Alignment:** Realigned `StatNature` checks and unminted state tracking (`Nature.Random`) to match the latest PKHeX 26.5.5 requirements.
- **PLZA Nature Legality Enforcement:** Brought the system fully in line with FusionBot's advanced `PA9` (Pokémon Legends: Z-A) nature manipulation logic. 
  - The trade helper now correctly reads user-specified `.StatNature=` batch commands in Showdown sets.
  - Legality-aware checks verify whether requested natures are strictly legal for a given static encounter. It will gracefully apply valid fallback Mints (`StatNature`) rather than producing entirely invalid Pokémon.
- **PLZA Box Format Slot Size (0x158):** Corrected `BoxFormatSlotSize` handling for PLZA memory offsets. `SetBoxPokemonAbsolute` now correctly writes exactly `0x158` (344 bytes / `SIZE_PARTY`), enabling perfectly mapped box reads and generations without arbitrary byte padding.

### Bug Fixes & Code Quality
- Cleaned up nullable reference warnings (`CS8600`, `CS8602`, `CS8603`, `CS8604`) tied to `.GetLegal()` and `.GenerateEgg()` wrapper return types across all bot variants (BDSP, SV, LA, SWSH).
- Fixed a compilation error regarding an invalid enum type (`PokeTradeResult.Stop` replaced with `PokeTradeResult.IllegalTrade`).
- All 84 unit tests are fully operational and successful. The solution compiles cleanly with 0 warnings and 0 errors.
