# DudeBot.NET - Global Synchronization & Database Overhaul

## New Features & Enhancements

### 🌐 Global Remote Synchronization
- **Shockhosting Integration**: The bot now utilizes a centralized MySQL database (genacnh.com) for real-time data sharing across all hosters globally.
- **Unified Progression**: Medals, Trade Counts, and Trainer Information (OT/TID/SID) are now synchronized instantly. A user's progress on Hoster A will be immediately available to Hoster B anywhere in the world.
- **Zero-Touch Configuration**: Hosters no longer need to manually configure database settings. The bot automatically connects to the global network on startup.

### 🎮 Multi-Game Simultaneous Support
- **Independent Game Codes**: Added dedicated, independent trade code storage for every supported Pokémon title:
  - **Scarlet & Violet (SV)**
  - **Sword & Shield (SWSH)**
  - **Brilliant Diamond & Shining Pearl (BDSP)**
  - **Legends: Arceus (LA)**
  - **Legends: Z-A (PLZA)**
  - **Let's Go Pikachu & Eevee (LGPE)**
- **No Overlap**: Users can now trade in multiple games simultaneously without their trade codes overwriting each other.
- **LGPE Pictocode Support**: Fully integrated 3-icon Pictocodes for LGPE into the global database synchronization logic.

### 🔒 Security & Anti-Cheat
- **AES-256 Data Encryption**: All user data (Trade Codes, Medals, Trainer IDs) is now encrypted using industry-standard AES-256 before being saved to the database. This prevents manual database editing or cheating.
- **Advanced Code Obfuscation**: The database connection string, server IP, and credentials are hidden using multi-pass bitwise XOR transformations and raw byte arrays, protecting them from reverse-engineering and string-scanning tools.

### 🛠️ Database Optimization
- **Schema Overhaul**: Replaced the generic "TradeCode" column with game-specific columns for better data integrity.
- **Automatic Schema Maintenance**: The bot now automatically repairs and upgrades the remote database schema as new features are added.

## Technical Fixes
- Removed legacy local JSON-only storage in favor of a hybrid SQL-first approach with automatic local failover.
- Optimized database connection pooling to ensure high-performance responsiveness during peak trading hours.
- Verified and fixed build compatibility across all game generation modules.

---
*Thank you for being part of the DudeBot network. Enjoy the new synchronized experience!*
