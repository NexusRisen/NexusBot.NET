# Release Notes

## [9.1.16]
- **Trade Notification & Cancellation Fixes**:
  - Fixed an issue where trade timeouts (such as "No trading partner found") spammed redundant `NoTrainerFound` and false `UserCanceled` notifications.
  - Set `IsCanceled` state when trades are canceled so queue removal does not emit secondary `UserCanceled` messages.
  - Formatted `PokeTradeResult` cancellation reasons into human-readable notifications across Discord, Stoat, Twitch, and Kook notifiers.
  - Added abort checks to batch and single trade routines to prevent duplicate failure notifications.
