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

