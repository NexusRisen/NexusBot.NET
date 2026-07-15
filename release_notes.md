## 9.1.10

### Features & Improvements
* **AI Legality Enforcements**: Integrated PKHeX Core and AutoLegality Mod (ALM) deeper into the AI prompt logic. The bot now explicitly extracts valid Abilities, Base Stats, and known Level-Up Moves natively from the PKHeX engine for a given species and game version to prevent the AI from generating illegal sets.
* **Unreleased Game Detection (Legends Z-A)**: Fixed a bug where the AI would hallucinate moves for Legends Z-A. The AI now correctly identifies when PKHeX lacks encounter data for unreleased games (like PA9) and avoids generating impossible movesets, ensuring smoother fallbacks.
