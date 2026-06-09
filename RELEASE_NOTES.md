# 🤖 DudeBot.NET Release Notes

Welcome to the official release notes for DudeBot.NET. This document tracks all recent feature additions, bug fixes, and system improvements.

---

## 🚀 v6.4.5 - 2026-06-09
### Features & Integrations
- **Kook Platform Parity**: Upgraded the `SysBot.Pokemon.Kook` integration to match the functionality of the Discord bot! Kook users can now seamlessly interact with all core features via native Kook Card Messages.
- **Event Requests (`.srp`, `.le`, `.er`)**: The bot now fully supports listing and requesting Wondercard events through Kook. Instead of being restricted to Discord, Kook dynamically renders a paginated card interface showing Pokémon species, levels, and original trainers.
- **Battle Ready Database (`.brl`, `.brr`)**: Introduced commands for Kook to list and request pre-configured competitive Pokémon from the `HOMEReadyPKMFolder`.
- **Hidden Trades (`.ht`)**: Added support for discrete trading over Kook, preventing logs from filling up public chat spaces.

---

## 🛠️ v6.4.3 - 2026-06-07
### Bug Fixes & Improvements
- **Convert Command & Eggs**: Fixed a bug where the `Convert` command and regular trades failed to correctly honor user-requested Original Trainer (OT), TID, and SID.
- **Codebase Health & Modernization**: Cleaned up the repository by addressing various build errors, nullability warnings, and async state issues across the solution.
