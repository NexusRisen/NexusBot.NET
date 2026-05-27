# DudeBot.NET - The Elite Ecosystem Update (v6.3.3)

## New Features & Enhancements

### 🏆 Website Leaderboard Integration
- **Live Hall of Fame**: The bot now fully communicates with **DudeBOT.ORG**. Top trainers are displayed in real-time on our official website.
- **Dynamic Rebuild Triggers**: When users reach new medal milestones, the bot automatically triggers a website update to ensure the community rankings are always fresh.
- **$leaderboard Command**: Added a new Discord command to easily view the global community rankings.

### 🎮 Multi-Game Simultaneous Support
- **Independent Game Codes**: Added dedicated, independent trade code storage for every supported Pokémon title:
  - **Scarlet & Violet (SV)**
  - **Sword & Shield (SWSH)**
  - **Brilliant Diamond & Shining Pearl (BDSP)**
  - **Legends: Arceus (LA)**
  - **Legends: Z-A (PLZA)**
  - **Let's Go Pikachu & Eevee (LGPE)**
- **Intelligent Game Routing**: Fixed game detection logic to ensure hosters can run any combination of games simultaneously without data conflicts.

### 🌐 Global SQL Synchronization
- **High-Performance Database**: Optimized connection pooling and added aggressive timeouts to prevent bot hanging during peak trade periods.
- **Unified Progression**: Medals, Trade Counts, and Trainer Information (OT/TID/SID) are synchronized instantly across all global hosters.

### 🔒 Maximum Security
- **Anti-Cheat Encryption**: All user progression and trade codes are secured using **AES-256 Data Encryption**.
- **Byte-Array Obfuscation**: Enhanced the multi-pass XOR obfuscation for database credentials and server IPs to protect your hosting network from discovery.

## Technical Improvements
- Fixed critical bugs in trade queue management and stuck trade cleaning.
- Implemented real-time SQL-based server blacklisting for instant administrative control.
- Restored original core features while seamlessly merging the new global synchronization architecture.

---
*Thank you for being part of the DudeBot network. Enjoy the most advanced synchronized bot ecosystem yet!*
