# DudeBot.NET - The Identity Overhaul (v6.3.5)

This critical update focuses on perfecting how DudeBot identifies and remembers trainers across different Pokémon titles, ensuring absolute accuracy for every game in your synchronized hosting network.

## 🆔 Independent Game Identities
The core of this update is the separation of trainer identities to match how Pokémon games work in reality.
- **Per-Game Trainer Profiles**: The system now stores independent **OT (Original Trainer)**, **TID (Trainer ID)**, and **SID (Secret ID)** values for every supported game:
  - **Scarlet & Violet (SV)**
  - **Sword & Shield (SWSH)**
  - **Brilliant Diamond & Shining Pearl (BDSP)**
  - **Legends: Arceus (LA)**
  - **Legends: Z-A (PLZA)**
  - **Let's Go Pikachu & Eevee (LGPE)**
- **Smart Data Routing**: The bot now automatically detects which game is being hosted and retrieves/saves the specific identity for that game. This prevents a user's *Scarlet* name from overwriting their *Arceus* name.
- **Captured Persistence**: Once a user trades in a specific game for the first time, their identity for that game is locked in the SQL database and shared across all hosters instantly.

## 🏆 Shared Global Progression
While your name and IDs are now game-specific, your status in the community is still unified.
- **Global Medals**: Your hard-earned medals and total trade counts follow you across every game and every hoster.
- **Network Reputation**: A user's rank is calculated based on their total activity across the entire DudeBot ecosystem.

## 🛠️ Performance & Stability
- **Verified Stability**: All 53 core system tests have been executed and passed, verifying the integrity of the new SQL-mapping logic and queue management.
- **Zero-Touch Sync**: Hosters remain fully automated with the high-performance connection pooling and 5-second fail-safe timeouts implemented in previous versions.

---
*Powered by .NET 10 and the Elite DudeBot Community. Global synchronization, perfected.*
