## NexusBot.NET v9.0.2

### Fixes
* **AutoOT Fixes**: Addressed an issue where short OT names were erroneously filling trailing trash bytes, causing legitimate legal AutoOT applications to fail legality checks.
* **Legality Hardening**: Completely removed the legacy `isUnreleasedPA9` hack. The bot now explicitly enforces PKHeX/ALM legality checks for PLZA (PA9) Pokémon, ensuring users with updated PKHeX-Plugins can properly legalize and queue PLZA trades.
* **Trade Injection Safeguards**: Introduced rigorous, mid-trade legality analysis for both single and batch link trades. If an illegal Pokémon slips through or fails generation, the trade is aborted and the user is instantly notified via Discord, effectively preventing the bot from soft-locking.

