# DudeBot.NET Release Notes

## 🚀 v6.4.6 - 2026-06-09

### 🐛 Bug Fixes & Improvements
- **SQL Case Sensitivity Fix**: Fixed an issue where MySQL servers running on Windows would throw a `Duplicate column name` error during database initialization due to case-sensitivity in the `information_schema` queries.
- **SQL Index Optimization**: Added an automatic `CREATE INDEX` operation for `TotalTrades` on the `Users` table, vastly improving the query performance of the global leaderboard.
- **Legacy Fallbacks Removed**: Removed legacy, non-game-specific `OT`, `TID`, and `SID` fields entirely from the SQL database footprint. The bot now explicitly enforces that every Pokémon game retains a completely independent identity (`OT_SV`, `OT_SWSH`, etc.) natively at the database level.

### 🔐 Security & Hardening
- **Deobfuscated SQL Credentials**: The default central database credentials (`GetConnectionString()`) and tests are now strictly and explicitly hardcoded in `DatabaseService.cs` and `SqlTests.cs`. We completely removed `InternalTransform` payload obfuscation to prioritize codebase readability and transparency. This ensures that all spawned instances reliably point to the master database.
