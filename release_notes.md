# DudeBot.NET Version 6.5.14 Release Notes

## Overview
This update enhances the **Stoat (Revolt)** integration by adding full Pokemon stats and details to the initial "Trade Request Queued" channel message, achieving 100% parity with Discord channel embeds.

## Changes & Improvements

### 🎨 Rich Trade Queue Embed (Stoat)
* Fixed an issue where the initial `Trade Request Queued` message in Revolt channels lacked Pokémon stats and details.
* The channel queue message now builds and displays a massive embed with full Pokémon data (Level, Ball, IVs, EVs, Held Item, Moves, Tera Type, Origin, and more), exactly matching the Discord behavior.

### ⚙️ Modular Command Architecture (Stoat)
* Completely removed the legacy monolithic `switch` block for Revolt command handling.
* All commands now run through a reflection-based `CommandRegistry` utilizing the `[StoatCommand]` attribute.
* Cleanly decoupled all trade-related commands into `SysStoat.Commands.Trade.cs`.
* Migrated management & utility commands (`about`, `queueStatus`, `link`, `medals`, `help`) to `SysStoat.Commands.Management.cs`.
* Added `help` command listing all available Stoat commands.

### 🎨 Rich Trade Finished Embed (Stoat)
* When a Stoat (Revolt) trade completes, users now receive a full Pokémon detail embed matching Discord parity — no emojis, no external links, clean text:
  - Species, form, gender `(M)/(F)`, and shiny status
  - Level, Ball, Met Level, Met Date, and Met Location
  - Ability, Nature (with mint notation if applicable)
  - Language (e.g. `ENG (English)`), IVs, Held Item, Tera Type (SV)
  - Full move list

### 📥 Enhanced File Parsing
* `trade` and `batchTrade` handlers in Stoat now properly intercept and read `.pkm` / raw `.t` binary attachments via the Revolt Autumn CDN.
* `egg` command now correctly handles file attachments and raw text blobs, matching Discord behavior.

### 🔧 Bug Fixes
* Fixed `vv6.5.7` double-prefix issue — removed redundant `v` from version string across `StoatTradeNotifier`, `DiscordTradeNotifier`, `QueueHelper`, and `SysStoat.Commands.Management`.
* Resolved queue display showing `Wait: <1 min | vv6.5.7` — now correctly shows `v6.5.7`.
* Resolved several duplicated definitions of `GetVersionInfo` across the codebase.
* Resolved various queue retrieval bugs related to `TradeQueueInfo` positioning and object casting.
* Resolved CS8602/CS8604 null-reference warnings inside message dispatches.
* Addressed `StoatTradeNotifier` signature mismatching inside the core dispatch logic.
* **Fixed `.egg` command in Stoat** — eliminated hard `(T)pkm` cast that silently threw `InvalidCastException` when the PKM entity type didn't match; now uses `EntityConverter.ConvertToType` + safe pattern check, matching Discord behavior.
* **Fixed `.me` (mystery egg) command in Stoat** — `GetContext<T>()`, `GetPriorityOrder()`, and `GetPersonalTable()` were missing `PA8` (Legends: Arceus) and `PB7` (LGPE) entries, causing `GenerateLegalMysteryEgg` to immediately return `null` with the message "Mystery Eggs are currently disabled or could not be generated."
* **Fixed `.it` (item trade) command in Stoat** — removed overly strict `LegalityAnalysis.Valid` check on the carrier Pokémon; now only validates that the held item was recognized (`HeldItem != 0`) and is tradeable (`!IsUntradableHeld`), matching Discord behavior.
