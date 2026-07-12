# NexusBot.NET Release Notes

## [Unreleased]

### Features & Improvements
- **Infinite Medals Architecture**: 
  - Overhauled the Discord Medals system to be uncapped! Users are now continuously congratulated every 50 trades instead of silently stopping at 1,000.
  - Implemented graceful fallback clamping, which automatically defaults to the highest graphical milestone (1,000) when a user surpasses it, preventing broken embed thumbnails.
- **Security & Stability**:
  - Hardcoded the Custom Medals Base URL deeply into the application architecture (`MedalHelpers.cs`) to prevent accidental overriding or URL configuration errors via the settings UI.
  - Added native cache-busting arguments directly to the URL fetching logic, forcing Discord's strict CDN to instantly serve updated medal graphics when we push new assets to the GitHub repository.
- **Global Asset Centralization**:
  - Engineered a brand new `AssetManager` module that automatically centralizes and routes over 50 external image assets across the entire solution.
  - Hardcoded Pokémon sprites, Held Items (via Serebii), Eggs, and Icons are now strictly maintained in one core location, preventing broken image links and increasing maintainability.
  - Automatically attaches `?v=4` cache-busting arguments to all GitHub graphics, ensuring Discord's CDN instantly serves up-to-date versions of assets.
- **Ping Command Overhaul**:
  - Replaced the generic GIF image in the `$ping` command with real-time websocket connection latency reporting (e.g. `Connection Latency: 42ms`), providing immediate insight into the bot's health and connection stability.
- **Custom AI Core Identity**:
  - Generated and implemented a brand new, highly-customized futuristic neon "AI Core" identity icon for the `$info` command thumbnail!
- **Pokémon Encoding Fixes**:
  - Fixed multiple files displaying broken encoding (e.g. `PokÃ©mon`), standardizing properly UTF-8 encoded text across the entire Discord bot output!
