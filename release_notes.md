# Release Notes

## [7.0.5]
- **MGDB Auto-Updater:** The bot now gracefully handles GitHub API rate limit errors when checking for MGDB wonder card repository updates. It correctly falls back to skipping the API check if the files already exist, or proceeds directly to downloading the full `.zip` if the directory is empty.
- **Settings:** Corrected the default original trainer (OT) name across `LegalitySettings.cs` and `config.json` to properly be `NexusBot.NET` instead of `dudebot.NET` / `NexusBot.Net`.
