# DudeBot.NET - The Elite Synchronized Ecosystem (v6.3.6)

This release marks the culmination of the DudeBot transformation into a fully integrated, real-time hosting ecosystem. v6.3.6 perfectly synchronizes your bot hosters and your official website via a high-performance MySQL backend.

## 🚀 Key Highlights

### 🆔 Independent Game Identities
- **Per-Game Accuracy**: The system now stores independent **OT, TID, and SID** profiles for every supported Pokémon game (SV, SWSH, BDSP, LA, PLZA, and LGPE).
- **Persistent Recognition**: Your identity in *Scarlet* will never overwrite your identity in *Sword*. The bot intelligently routes data to its dedicated game profile.
- **Global Data Capture**: Once your trainer info is captured, all hosters in the global network will recognize you instantly.

### 🌐 Global SQL Synchronization
- **Shockhosting Backend**: All hosters connect to a centralized MySQL server (`genacnh.com`) for real-time synchronization.
- **Unified Progression**: Medals and total trade counts are shared globally across all games and hosters. Your reputation follows you everywhere.
- **Bot Heartbeat System**: Active hosters send live signals to the network, enabling real-time "Online" counts on the website.

### 🏆 Website Hall of Fame & Live Stats
- **Real-Time Leaderboard**: **DudeBOT.ORG** now features a live, ranked "Hall of Fame" fetching data directly from the SQL network.
- **Live Network Health**: The website homepage now displays live hoster counts, registered user totals, and community transfer milestones.
- **Discord Integration**: Use `$leaderboard` to share the community rankings with your Discord server.

### 🔒 Maximum Security & Anti-Cheat
- **AES-256 Encryption**: All progression data, trade codes, and identities are secured with industry-standard encryption.
- **Stealth Infrastructure**: Connection credentials and server IPs are protected by multi-pass bitwise XOR obfuscation.
- **Instant SQL Blacklist**: Manage server bans in real-time via the database; blacklisted servers are auto-kicked instantly.

## 🛠️ Technical Fixes
- **Build v6.3.6**: Verified 100% build stability across all generation modules.
- **Optimized SQL**: Enhanced connection pooling and implemented aggressive 5-second timeouts for maximum reliability.
- **Dependency Update**: Bumped `MySqlConnector` to **v2.5.0** for improved .NET 10 performance.

---
*Powered by .NET 10 and the Elite DudeBot Community. Synchronization, perfected.*
