# Release Notes

## [7.0.7]
- **Bug Fixes:**
  - Fixed an issue where the application would freeze/deadlock indefinitely when clicking "Start". This was caused by the MGDB auto-updater blocking the main UI thread during asynchronous network operations.
- **Improvements:**
  - The MGDB updater now runs automatically in the background when the program boots up, rather than waiting for a bot to start.
