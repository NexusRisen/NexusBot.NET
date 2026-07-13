# NexusBot.NET Release Notes

## [v8.0.5]

### Features & Improvements
- **Project Structure Overhaul**:
  - Fully decoupled the PKHeX.Core.AutoMod project from the SysBot.NET solution, migrating to a much cleaner dynamically linked library (.dll) dependency system located centrally in the SysBot.Pokemon/deps/ folder. This significantly reduces solution bloat and simplifies cross-repository integration.
- **AutoOT Mechanics & Stability**:
  - Re-engineered the AutoOT (ApplyAutoOT) extension method to perfectly handle complex trade logic directly at the trade screen, ensuring Trainer Information, Memories, and Trash Bytes are all perfectly sanitized and updated before transferring.
  - Pushed critical fixes upstream to the PKHeX-Plugins repository, ensuring that **Shiny PIDs** are dynamically recalculated whenever AutoOT alters the Original Trainer (OT/TID/SID) data, preventing invalid shiny checks on legally distributed Pokémon.
- **LGPE (Let's Go Pikachu / Eevee) Improvements**:
  - Overhauled LGPE's unique link-trade OT storage mechanics! Because LGPE lacks the ability to transmit OT data prior to the first trade presentation, the bot now perfectly saves the trainer's data via TradeCodeStorage upon their first connection. When the trainer reconnects, the bot seamlessly applies AutoOT using the cached data.
- **Dependency Cleanups**:
  - Cleaned up obsolete solution project references and corrected MSBuild dependencies across SysBot.Pokemon.Discord, SysBot.Pokemon.Kook, SysBot.Pokemon.Stoat, and SysBot.Tests to point to the newly compiled PKHeX.Core.AutoMod.dll.
