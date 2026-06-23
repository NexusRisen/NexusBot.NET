# DudeBot.NET Release Notes

## [Unreleased]

### Added
- **Multi-Tier Queue Favoritism:** Implemented a new multi-tiered role prioritization system (`Tier 1` through `Tier 4`, along with `Favored`). This allows server administrators to have fine-grained control over queue skipping priorities for their users.
- **Operation Configuration:** Favoritism role tiers are now fully supported and can be directly configured under the **Operation** category inside the Discord Integration Settings via the WinForms GUI.

### Changed
- **ALM Event Updates:** Updated AutoLegalityMod (`AutoModPlugins`) to the latest version to support the generation of newer distributions, including the PJCS 2026 Garchomp event.
- Refactored `RequestSignificance` and `QueueHelper` logic to automatically calculate queue position and bump user requests ahead of lower-tier users based on their assigned Discord roles.

### Fixed
- **Discord Gateway Recovery:** Implemented a robust supervision loop for the Discord client to automatically dispose and reconstruct the client when encountering terminal gateway/connection failures. This prevents the bot from becoming permanently deadlocked offline.
