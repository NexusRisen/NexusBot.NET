## [v8.1.0]

- **Trade Queue Thread Safety**: Fixed an `InvalidOperationException` (Collection was modified; enumeration operation may not execute) caused by deferred LINQ execution outside of `lock (_sync)` when fetching queue summaries (`GetUserList`).
- **Translation Substring Matching**: Fixed a bug where a substring match during translation parsing could incorrectly match a shorter Pokémon name (e.g., matching "Ka" before "Pikachu"), resulting in the wrong species (like Totodile instead of Pikachu).
- **PA9 Legality Bypass**: Completely bypassed PKHeX legality checks internally specifically for PA9 (Pokémon Legends: Z-A). This ensures generation and trade mechanisms do not reject validly parsed PA9 templates while waiting for actual legal encounter data in AutoLegalityMod.