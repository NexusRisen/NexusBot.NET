## 9.0.6

### Diagnostics / Debugging
* Added deeper debug logging to PLZA AutoOT logic to help track down where the OT name is cleared during trade legality initialization.

## 9.0.5

### Bug Fixes
* Fixed an issue in PLZA where trades would fail legality checks with "OT Name too short" because the bot was reading the trade partner's Language and Gender from the wrong memory offsets.

## 9.0.4

### Bug Fixes
* Fixed a bug where using simple species names (e.g., `.t pikachu`) would result in a failed trade with an empty Nickname and invalid Language ID.

