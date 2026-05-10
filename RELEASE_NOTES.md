# Release Notes

## [v6.1.6] - 2026-05-10

### Summary
This release migrates the entire DudeBot.NET image infrastructure to the new Nexus-Risen-Edition-Sprite-Images repository. It introduces a modernized, language-neutral sprite generation engine and improves asset organization across all bot platforms.

### Added
- **New Asset Bank Integration**: Integrated the `NexusRisen/Nexus-Risen-Edition-Sprite-Images` repository for all bot assets.
- **Alphabetical Range Mapping**: Automated folder selection (A-G, H-N, O-T, U-Z) based on species names for optimized repository access.

### Changed
- **Dynamic Sprite Engine**: Refactored `PokeImg` to utilize English species names and standardized form suffixes, ensuring 100% reliability in image resolution regardless of the user's input language.
- **Static Asset Paths**: Updated all hardcoded URLs for medals, icons, status indicators, and DMs to point to the new structured repository.
- **Form Suffix Logic**: Standardized regional form suffixes (`-alola`, `-galar`, `-hisui`) and Gigantamax indicators to match the new asset naming convention.

### Fixed
- **Multi-Language URL Resolution**: Fixed an issue where non-English Showdown sets could fail to resolve image URLs by forcing English translation during the link-building phase.
- **Ambiguous Form Naming**: Improved form name sanitization to prevent broken image links for species with complex form strings.
