## 9.0.4

### Bug Fixes
* Fixed a bug where using simple species names (e.g., `.t pikachu`) would result in a failed trade with an empty Nickname and invalid Language ID.

## 9.0.3

### Changes
* Ported detailed `LegalityReport` functionality from PokeBot to improve Discord embed feedback for failed trades.
* Added `IsEffectivelyLegal` legality filter for PA9 (Legends Z-A) to allow trades of unreleased game origin Pokemon while maintaining strict legality checks on stats, moves, and abilities.
* Fixed a compilation error in `SimpleLegalityFeedback` caused by deprecated `CheckIdentifier` values from PKHeX.Core.
