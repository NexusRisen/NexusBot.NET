# DudeBot.NET - The Elite Ecosystem Update (v6.3.5)

This update refines the global synchronization system to handle independent trainer identities across different Pokémon games while maintaining unified progression.

## 🚀 Key Highlights

### 🆔 Independent Game Identities
- **Per-Game Trainer Info**: The bot now stores separate **OT, TID, and SID** for every game (SV, SWSH, BDSP, LA, PLZA, and LGPE).
- **Automatic Recognition**: When a user trades for the first time in a specific game, their trainer info is saved. On the next trade in that same game, the bot will automatically recognize them.
- **Shared Reputation**: Even though identities (OT/TID/SID) are now game-specific, your **Medals and Trade Counts** remain shared across the entire network. Your rank follows you no matter which game you play.

### 🌐 Global Multi-Game Synchronization
- **Simultaneous Support**: Hosters can run all games at once without any data overlap. Each game now has its own dedicated, encrypted columns in the SQL database.
- **Heartbeat System**: Active hosters now send a live signal to the database, allowing the website to show real-time "Online" counts.

### 🏆 Hall of Fame & Live Stats
- **Strictly SQL Leaderboard**: The official website (**DudeBOT.ORG**) now uses a professional real-time fetching system. Rankings are updated instantly via SQL without needing website redeployments.
- **Live Network Stats**: Home page displays live hoster counts and community trade totals directly from the global database.

### 🔒 Maximum Security
- **Anti-Cheat Encryption**: All progression data is secured using industry-standard **AES-256 Encryption**.
- **Stealth Protection**: All hosting credentials and server IPs are obfuscated using multi-pass bitwise XOR logic.
- **Instant SQL Blacklist**: Real-time server banning managed directly through the MySQL database.

---
*Thank you for being part of the DudeBot network. Enjoy the most advanced synchronized bot ecosystem yet!*
