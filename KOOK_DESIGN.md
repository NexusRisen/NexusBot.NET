# Design Document: Kook Platform Integration

## 1. Overview
This document outlines the plan to integrate support for the Kook platform (a Chinese Discord-like service) into SysBot.NET. The integration will follow the patterns established by the existing Discord integration, utilizing the `Kook.Net` library for API communication.

## 2. Goals
- Provide full support for Kook bots to handle trade requests and other commands.
- Implement configuration settings mirroring Discord's capabilities.
- Ensure seamless integration with the existing WinForms and Console applications.

## 3. Architecture

### 3.1. Settings & Configuration
- **New File**: `SysBot.Pokemon/Settings/Integrations/KookSettings.cs`
  - Will contain properties like `Token`, `ChannelWhitelist`, `RoleCanTrade`, etc., similar to `DiscordSettings.cs`.
- **Modified File**: `SysBot.Pokemon/TradeHub/PokeTradeHubConfig.cs`
  - Add `KookSettings Kook { get; set; } = new();` property.

### 3.2. Bot Implementation
- **New Project**: `SysBot.Pokemon.Kook`
  - **Dependencies**: `Kook.Net`, `SysBot.Base`, `SysBot.Pokemon`.
  - **Key Class**: `SysKook<T>` (mirroring `SysCord<T>`)
    - Handles `KookSocketClient` initialization.
    - Manages connection lifecycle (Login, Start, Reconnect).
    - Dispatches commands received via messages.

### 3.3. Application Integration
- **Modified File**: `SysBot.Pokemon.WinForms/PokeBotRunnerImpl.cs`
  - Add `AddKookBot()` method to `AddIntegrations()`.
- **Modified File**: `SysBot.Pokemon.ConsoleApp/PokeBotRunnerImpl.cs`
  - Add `AddKookBot()` method to `AddIntegrations()`.

## 4. Implementation Details

### 4.1. KookSettings
Mirror `DiscordSettings` with relevant adjustments:
- Use Kook-specific terminology where appropriate (e.g., "Card Messages" instead of "Embeds" if we want to get fancy, but standard embeds are supported).
- Maintain compatibility with `RemoteControlAccessList`.

### 4.2. SysKook<T>
- Use `KookSocketClient` from `Kook.Net`.
- Implement `MainAsync(string token, CancellationToken token)`.
- Re-use `PokeTradeHub` and `PokeBotRunner` for core logic.
- Commands will be handled similarly to `SysCord`, possibly sharing logic or adapting the same pattern of command registration.

## 5. Testing Plan
- **Unit Tests**: Verify that Kook settings are correctly serialized and deserialized.
- **Manual Verification**: Test bot initialization and response with a Kook bot token.
- **Regression Testing**: Ensure existing Discord/Twitch integrations are unaffected.

## 6. Risks & Mitigation
- **API Differences**: Kook API differs slightly from Discord. *Mitigation*: `Kook.Net` abstraction layer minimizes these differences.
- **Language Barriers**: Documentation for Kook is primarily in Chinese. *Mitigation*: Utilize `Kook.Net` (which has English/Chinese docs) and existing community knowledge.
