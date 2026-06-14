# Release Notes

## DudeBot.NET v6.5.5

### New Features & Updates
- **Auto Legality Mod (ALM) Integration**: Integrated the latest bleeding-edge version of the Auto Legality Mod from the `santacrab2/PKHeX-Plugins` repository.
- **PKHeX.Core 26.5.5 Compatibility**: Resolved internal compilation mismatches to ensure seamless stability and binding with the latest stable `PKHeX.Core` release (version `26.5.5`).
- **Updated Plugin Binaries**: Upgraded `PKHeX.Core.AutoMod.dll` and `AutoModPlugins.dll` inside the deployment dependencies (`SysBot.Pokemon/deps/`).

### Fixes and Improvements
- Minor patch to `ShowdownEdits.cs` ensuring backwards compatibility with specific `Experience.IsValidNatureMetLevel2` encounter checks required by newer core library versions.
- All core legality unit tests fully pass with the updated libraries.
