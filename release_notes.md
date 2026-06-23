# DudeBot.NET Release Notes

## [7.0.2]

### Added
- **Multi-Tier Queue Favoritism:** Implemented a new multi-tiered role prioritization system (`Tier 1` through `Tier 4`, along with `Favored`). This allows server administrators to have fine-grained control over queue skipping priorities for their users.
- **Operation Configuration:** Favoritism role tiers are now fully supported and can be directly configured under the **Operation** category inside the Discord Integration Settings via the WinForms GUI.
- **Scale Information for Eggs:** The Pokémon embed display now accurately extracts and lists the Scale property inside the "Origin & Physical" field for eggs in Scarlet/Violet.
- **HighDPI & UI Updates:** Added HighDPI text/control scaling settings and integrated system dark mode detection for improved UI responsiveness and crispness.

### Changed
- **ALM Event Updates:** Updated AutoLegalityMod (`AutoModPlugins`) to the latest version to support the generation of newer distributions, including the PJCS 2026 Garchomp event.
- **Upstream Core Updates:** Synced latest `PKHeX.Core` logic, including:
  - Performance improvements across basic casting and generation mapping (`enc->pk`).
  - Improved tooltips to actively identify SV locations by their ID.
  - Routine cleanup inside `SpeciesName.cs` alongside sanity checks for specific abilities and edge cases (e.g., Gen 5 Basculin-Blue).
  - Removal of incorrect format checks for Gen 3 Korean entries.
- Refactored `RequestSignificance` and `QueueHelper` logic to automatically calculate queue position and bump user requests ahead of lower-tier users based on their assigned Discord roles.

### Fixed
- **ALM Customization Overrides:** Fixed a bug causing the bot to occasionally ignore requested batch commands and Pokéball customizations during Egg generation or ALM-led processing. Custom preferences are now properly reapplied post-generation.
- **Discord Gateway Recovery:** Implemented a robust supervision loop for the Discord client to automatically dispose and reconstruct the client when encountering terminal gateway/connection failures. This prevents the bot from becoming permanently deadlocked offline.
