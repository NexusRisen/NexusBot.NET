# DudeBot.NET - The Global Synchronization Update (v6.3.6)

This release officially introduces the **DudeBot Synchronized Ecosystem**. For the first time, your bot hosters and your community website work together in real-time, powered by a high-performance MySQL backend.

## 🚀 Key Features

### 🆔 Independent Trainer Identities
- **Game-Specific Recognition**: DudeBot now remembers your **OT, TID, and SID** independently for every game (SV, SWSH, BDSP, LA, PLZA, and LGPE).
- **Seamless Switching**: Your trainer name in *Scarlet/Violet* will no longer overwrite your name in *Sword/Shield*. The bot intelligently maps your identity to the specific game being hosted.
- **Persistent Captured Data**: Once you trade for the first time in a game, all hosters globally will recognize you automatically on your next trade.

### 🏆 Shared Global Progression
- **Unified Medal System**: Even though your names are game-specific, your **Medals and Reputation** are shared globally. Your rank and achievements follow you everywhere in the network.
- **Hall of Fame**: Top trainers are now displayed in real-time on our official website (**DudeBOT.ORG**).

### 🌐 Real-Time Network Statistics
- **Live Hoster Counts**: We've implemented a "Heartbeat" system. You can now see exactly how many bots are online and hosting across the globe.
- **Community Milestones**: A live counter on the website tracks the total number of trades performed by the entire DudeBot community.

### 🎮 Simultaneous Multi-Game Support
- **Conflict-Free Hosting**: Hosters can now run multiple games (e.g., SV and LGPE) simultaneously using the same database without any data overlap.
- **Automated Routing**: The program automatically detects the game generation and routes all data to its dedicated encrypted SQL column.

## 🔒 Security & Performance
- **AES-256 Encryption**: Every medal, trade count, and identity is secured with military-grade encryption to prevent cheating.
- **Hybrid SQL-PHP Architecture**: Optimized for speed and compatible with GitHub Pages.
- **Stealth Obfuscation**: Server infrastructure and credentials are hidden using multi-pass bitwise XOR logic.
- **Fail-Safe Reliability**: Aggressive 5-second SQL timeouts and automatic local mode failover ensure the bot never hangs.

---
*Thank you for being part of the most advanced Pokemon bot ecosystem. Synchronization, perfected.*
