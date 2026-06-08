# DudeBot.NET Release Notes

## v6.4.4 - 2026-06-08
### Bug Fixes & Improvements
- **BatchNormalizer Fixes**: Resolved an issue where high-number, 6-digit Trainer IDs (from Generation 7 and newer) were being rejected or overflowing when submitted via the Showdown format.
- The `Convert` command on Discord and Kook now automatically translates 6-digit `TID:` and standard `SID:` inputs into native ALM batch commands (`.TrainerTID7` and `.TrainerSID7`).

## v6.4.3 - 2026-06-07
### Bug Fixes & Improvements
- **Convert Command & Eggs**: Fixed a bug where the `Convert` command and regular trades were failing to honor user-requested OT, TID, and SID.
- **Codebase Health**: Addressed various build errors and nullability warnings across the repository.
