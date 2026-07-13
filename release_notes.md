## [v8.0.9]

- **Trade Queue Thread Safety**: Fixed an `InvalidOperationException` (Collection was modified; enumeration operation may not execute) caused by deferred LINQ execution outside of `lock (_sync)` when fetching queue summaries (`GetUserList`).
- **Translation Substring Matching**: Fixed a bug where a substring match during translation parsing could incorrectly match a shorter Pokémon name (e.g., matching "Ka" before "Pikachu"), resulting in the wrong species (like Totodile instead of Pikachu).

## [v8.0.8]

- **Updater Fix**: Fixed a silent failure in the auto-updater where the update script would not wait long enough for the application to fully close before attempting to overwrite the executable, causing the updater to loop on the old version.

## [v8.0.7]

- **Trade Bot Language ID Fix**: Updated all `PokeTradeBot` variants to properly validate and preserve requested languages, defaulting invalid trade partner languages (e.g. `0`) to English (`2`) to prevent "Language ID out of range" legality errors.
- **Nickname Legality Fix**: Replaced `cln.ClearNickname()` with `SpeciesName.GetSpeciesNameGeneration()` to correctly substitute the translated species name when nicknames are cleared. This bypasses the "Nickname is empty" ALM error for Asian languages.
- **Fixed-OT Encounter Fallback**: Added a secondary fallback generation mechanism for encounters tied to specific Original Trainers/Languages (e.g., in-game trades) to ensure they pass legality checks if the initial requested language fails.
- **Custom Batch Commands (FusionBot Port)**: Ported and improved custom Showdown commands to `BatchNormalizer.cs`:
  - `Nickname: Suggest` randomly assigns a fun nickname from an internal dictionary.
  - `EVs: Random` or `EVs: Suggest` legally randomizes EVs or applies a 252/252/4 spread.
  - `IVs: Random` or `IVs: [1-6]IV` randomizes IVs entirely or forces a specific number of stats to 31.
  - `HT: HP / Atk / Def` enables Hyper Training for explicitly specified stats.
  - `StatNature: [nature]` seamlessly converts to Mint Nature.
