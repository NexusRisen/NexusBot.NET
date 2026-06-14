# DudeBot.NET Version 6.5.7 Release Notes

## Overview
This update introduces a complete architectural overhaul for the **Stoat (Revolt)** integration, migrating all commands to a new modular `CommandRegistry` system and achieving full feature and visual parity with the Discord bot.

## Changes & Improvements
### ⚙️ Modular Command Architecture (Stoat)
* Completely removed the legacy monolithic `switch` block for Revolt command handling.
* All commands now run through a reflection-based `CommandRegistry` utilizing the `[StoatCommand]` attribute.
* Cleanly decoupled all trade-related commands into `SysStoat.Commands.Trade.cs`.
* Migrated management & utility commands (such as `about`, `queueStatus`, `link`, `medals`) to `SysStoat.Commands.Management.cs`.

### 🎨 Visual Embed Parity
* Completely overhauled `EmbedHelper.cs` to supply a scalable and robust `CreateEmbed` utility.
* Stoat commands now use exact equivalents of Discord embeds, displaying accurate DudeBot icons, refined color schemes (Green/Red/Gold), and matching footers indicating the active version.

### 📥 Enhanced File Parsing
* `trade` and `batchTrade` handlers in Stoat have been updated to properly intercept and natively read `.pkm` and raw `.t` binary payload attachments via the Revolt Autumn CDN.

### 🔧 Bug Fixes
* Resolved several duplicated definitions of `GetVersionInfo` across the codebase.
* Resolved various queue retrieval bugs related to `TradeQueueInfo` positioning and object casting.
* Resolved CS8602/CS8604 null-reference warnings inside message dispatches.
* Addressed `StoatTradeNotifier` signature mismatching inside the core dispatch logic.
