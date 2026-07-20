# Release Notes

## [Unreleased]

- Fixed an issue where the SV bot would get stuck on the news screen.
- Implemented a workaround for a bug in PKHeX's Auto Legality Mod where `ApplyAutoOT` clears the Original Trainer Name (fixing the "OT Name too short" error). This fix has been applied across all supported games (SV, SWSH, BDSP, LA, LGPE, and PLZA).
- Bumped `actions/setup-dotnet` from v5 to v6.
- Implemented dynamic hourly cache busting for progression medal images to ensure Discord automatically fetches updated sprites from GitHub.
