# Release Notes

## v6.2.1 (Draft)
### ✨ Hugging Face AI Enhancements
- **Chat History (Memory):** The AI now remembers the last 10 messages in a conversation, allowing for follow-up questions and context-aware responses.
- **Dynamic Identity:** The AI now automatically uses the bot's Discord username as its persona name.
- **Improved API Integration:** Switched to the modern Chat Completions API for better instruction following and formatting.
- **Robust Retry Logic:** Added automatic retry handling for "503 Service Unavailable" errors when models are loading.
- **Advanced Configuration:** Added new settings for `Max Tokens`, `Temperature`, and `Top P` in `config.json` to fine-tune AI behavior.
- **Few-Shot Prompting:** Updated the system prompt with examples to ensure more consistent Showdown set formatting.
- **PKHeX.Core & ALM Data Integration:** The AI now "reads" actual game data! It automatically extracts species-specific abilities and metadata from `PKHeX.Core` based on the user's request and includes it in the prompt context.
- **ALM Override Awareness:** The AI is now trained to use `~` RegenTemplate overrides to ensure 100% legality for complex requests (e.g., specific Pokéballs, Tera Types, or levels).
- **Auto OT Awareness:** The AI now understands that the bot applies Auto OT automatically. It knows to skip providing OT information for standard requests while preserving original details for Event Pokémon.

### 🛠️ AI Commands
- **New `$ai` Command:** A built-in help guide for AI features (uses the configured command prefix).
- **New `$clearAI` Command:** Allows users to clear their conversation history and start a fresh chat.

### 📦 Dependency Updates
- Bumped `LibUsbDotNet` from 2.2.75 to 2.2.85.

---
