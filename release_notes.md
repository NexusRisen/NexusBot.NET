# NexusBot.NET Release Notes

## [Unreleased]

### Features & Improvements
- **Infinite Medals Architecture**: 
  - Overhauled the Discord Medals system to be uncapped! Users are now continuously congratulated every 50 trades instead of silently stopping at 1,000.
  - Implemented graceful fallback clamping, which automatically defaults to the highest graphical milestone (1,000) when a user surpasses it, preventing broken embed thumbnails.
- **Security & Stability**:
  - Hardcoded the Custom Medals Base URL deeply into the application architecture (`MedalHelpers.cs`) to prevent accidental overriding or URL configuration errors via the settings UI.
  - Added native cache-busting arguments directly to the URL fetching logic, forcing Discord's strict CDN to instantly serve updated medal graphics when we push new assets to the GitHub repository.
- **Pokémon Encoding Fixes**:
  - Fixed multiple files displaying broken encoding (e.g. `PokÃ©mon`), standardizing properly UTF-8 encoded text across the entire Discord bot output!
