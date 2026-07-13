# NexusBot.NET Release Notes

## [v8.0.6]

### Features & Improvements
- **Trading Flexibility**:
  - Implemented seamless single-species name trading support (e.g., `.t pikachu`). Users can now trade by just providing the Pokémon's name without needing a full Showdown set.
  - Added intelligent filtering to safely bypass non-critical structural parsing errors from PKHeX (e.g., missing nicknames or language ID flags), letting the Auto-Legality Mod (ALM) perfectly handle legalization automatically.
- **Showdown Translation Enhancements**:
  - Showdown translation checking is now completely case-insensitive. Inputting `.t PIKACHU` or `.t pikachu` will reliably match and parse correctly without requiring specific capitalization.
