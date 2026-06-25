# Release Notes

## [7.0.6]
- **MGDB Updater Improvements:** 
  - Forced the auto-updater to explicitly create the `MGDB` folder alongside the bot's executable using absolute paths (`AppContext.BaseDirectory`), resolving issues where it would create the folder in unpredictable working directories.
  - Added a startup console log that prints the exact directory path where the MGDB folder is located.
