# DudeBot.NET Release Notes

## [Unreleased]

### Added
- **Multi-Tier Queue Favoritism:** Implemented a new multi-tiered role prioritization system (`Tier 1` through `Tier 4`, along with `Favored`). This allows server administrators to have fine-grained control over queue skipping priorities for their users.
- **Operation Configuration:** Favoritism role tiers are now fully supported and can be directly configured under the **Operation** category inside the Discord Integration Settings via the WinForms GUI.

### Changed
- Refactored `RequestSignificance` and `QueueHelper` logic to automatically calculate queue position and bump user requests ahead of lower-tier users based on their assigned Discord roles.
