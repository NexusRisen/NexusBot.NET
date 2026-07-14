## 9.1.0

### Features
* Added the Held Item string to the Pokémon title line in the Discord embed.
* Re-organized the Discord embed slightly by separating the Overview and Stats on new lines instead of separating them with `|`, while keeping the Moves side-by-side using inline formatting.

### Bug Fixes
* Fixed an issue where the Held Item was being forcibly wiped for Pokémon Legends: Z-A due to legacy Arceus logic.
* Fixed an issue where the embed would display `Item: (None)` for Pokémon without an item.

## 9.0.8

### Bug Fixes
* Also restore TID and SID manually after PKHeX's PA9 AutoOT logic executes, as a precautionary measure to prevent the trainer IDs from being wiped along with the OT Name.

## 9.0.7

### Bug Fixes
* Fixed a core issue where AutoOT would fail during PLZA legality checks because PKHeX's internal `ApplyAutoOT` method unexpectedly clears the Original Trainer Name for PA9 format Pokémon. The bot now manually restores the correct trainer name immediately after this routine.

## 9.0.6

## 9.0.5

### Bug Fixes
* Fixed an issue in PLZA where trades would fail legality checks with "OT Name too short" because the bot was reading the trade partner's Language and Gender from the wrong memory offsets.

## 9.0.4

### Bug Fixes
* Fixed a bug where using simple species names (e.g., `.t pikachu`) would result in a failed trade with an empty Nickname and invalid Language ID.

