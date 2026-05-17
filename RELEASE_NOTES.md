# RELEASE NOTES

## v6.2.4

### 🥚 Advanced Egg & Batch Trade Overhaul
- **New Command: `$begg`**: Dedicated command for batch egg trading. Users can now use `$egg` for single trades and `$begg` for multiple eggs.
- **Granular Egg Control**: Added new settings to `config.json` and WinForms to manage eggs separately from regular Pokémon.
  - `AllowEggBatchTrades`: Enable or disable batch trading for eggs.
  - `MaxEggsPerBatch`: Set a specific limit for how many eggs can be in one trade session.
- **Improved `$egg` Validation**: The standard `$egg` command now detects if you're trying to send a batch and prompts you to use `$begg`.

### 🎁 Expanded Mystery Trading
- **New Command: `$mysterypokemon` (`$mp`)**: Request a completely random legal Pokémon.
- **New Command: `$batchMysteryPokemon` (`$bmp`)**: Request a batch of random legal Pokémon.
- **Separate Mystery Settings**: Mystery trades now have their own independent batch controls.
  - `AllowMysteryEggBatchTrades` / `MaxMysteryEggsPerBatch`
  - `AllowMysteryPokemonBatchTrades` / `MaxMysteryPokemonPerBatch`
- **Dynamic Image Support**: Enhanced mystery trade embeds with high-quality sprite assets.

### ⚙️ Performance & Configuration
- **Memory Optimization**: Implemented static caching for legal species and breedable data to prevent memory spikes during high-volume mystery requests.
- **Rate Limit Protection**: Added artificial delays between batch trade notifications to avoid Discord rate limiting on large requests.
- **Configuration Update**: `config.json` has been updated with the new `BatchSettings` structure.

---
- **Internal Version**: v6.2.4
- **Release Date**: May 17, 2026
