# Release Notes

## [9.1.13]
- **EVs/IVs Showdown Parsing Fix**: Fixed an issue in `BatchNormalizer` where using a standard Showdown EV or IV string (e.g., `EVs: 252 Atk / 4 SpD / 252 Spe`) was incorrectly replaced with an invalid batch command, causing Pokémon to be generated with 0 EVs and randomized IVs.
- **Console Restart Discord Fix**: Fixed an issue where clicking the "Restart All" button in the WinForms UI would completely kill the environment (including the Discord bot connection) before restarting. It now properly restarts only the Switch consoles.
- **Medal Pop-up Spam Fix**: Fixed an issue where the celebratory Medal milestone pop-up would spam the chat periodically. The milestone pop-up now properly triggers exactly once on the user's very first trade, and subsequent progress can be checked using the `$medals` command.
