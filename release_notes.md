# PokeBot v1.1.5a Release Notes

## 🚀 Enhancements

### GUI Improvements
- **Performance**: Optimized layout transitions to significantly reduce loading times when switching to the Bots section.
- **Visuals**: Fixed PropertyGrid flickering, text duplication, and striping issues by enabling double buffering.
- **Usability**: 
  - Moved Start/Stop/Reboot buttons to be exclusive to the Bots section for better context.
  - Cleaned up context menu items (removed underscores).
  - Added proper "Web Integrations" label in configuration.

### Update System
- **Fail-Safe Mechanism**: Added strict validation to ensure updates only proceed when running as `PokeBot.exe`. This prevents accidental overwrites of development builds or renamed executables.
- **Dynamic Versioning**: Implemented automated version retrieval from git tags, eliminating hardcoded version constants.
- **Security**: Enhanced asset validation to strictly download `PokeBot.exe` from release assets.

## 🐛 Bug Fixes
- Fixed architecture mismatch issues (x64 vs x86) in bootstrapper.
- Fixed file locking issues during update processes.
- Fixed context menu command parsing for special characters.
