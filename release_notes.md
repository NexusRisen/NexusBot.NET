## 9.1.11

### Features & Improvements
* **Conversational AI & Educational Mode**: The AI system prompt has been updated to act as an expert on Pokémon mechanics, PKHeX, and Sysbots. It can now teach users and hold normal conversations on these topics.
* **AI Reply Support**: The AI now supports continuing conversations when a user directly replies to an AI message.
* **Interactive AI Buttons**: Introduced Discord buttons for AI responses (such as Queue and File options) to allow users to take direct action directly from the AI chat.
* **Markdown Copyable Sets**: Replaced `[SHOWDOWN]` tags with native Discord Markdown code blocks in AI messages, allowing users to easily copy and paste generated Showdown sets.

### Bug Fixes
* **MGDB Updater Fix**: Fixed an issue where the Mystery Gift Database (MGDB) updater was never being called during initialization. It now correctly checks for GitHub updates on startup and automatically defaults to a local `mgdb` folder if no path is specified in the configuration.


