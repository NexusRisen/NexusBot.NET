# DudeBot.NET Release Notes

## 🚀 v6.4.6 - 2026-06-09

### 🐛 Bug Fixes & Improvements
- **SQL Case Sensitivity Fix**: Fixed an issue where MySQL servers running on Windows would throw a `Duplicate column name` error during database initialization due to case-sensitivity in the `information_schema` queries.
- **SQL Index Optimization**: Added an automatic `CREATE INDEX` operation for `TotalTrades` on the `Users` table, vastly improving the query performance of the global leaderboard.
- **Data Integrity Fixes**: Base parameters (`OT`, `TID`, `SID`) are correctly inserted in the primary DB insert call during `SaveUser()` to avoid null values.

### 🔐 Security & Hardening
- **Deobfuscated SQL Credentials**: The default central database credentials (`GetConnectionString()`) and tests are now strictly and explicitly hardcoded in `DatabaseService.cs` and `SqlTests.cs`. We completely removed `InternalTransform` payload obfuscation to prioritize codebase readability and transparency. This ensures that all spawned instances reliably point to the master database.
