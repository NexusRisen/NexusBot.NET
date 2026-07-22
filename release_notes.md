# Release Notes

## [9.1.15]
- **Medals System Fix & Integrations**:
  - Fixed trade count incrementing logic so user trade counts increase strictly upon trade completion (preventing duplicate/premature increments).
  - Fixed the `$medals` (`$ml`) command to properly display the 0-trade status embed card for new users.
  - Implemented `$medals` / `$ml` command handling and configuration settings for Stoat and Kook integrations.
  - Centralized milestone calculation and congratulatory messaging logic in core `SysBot.Pokemon`.
