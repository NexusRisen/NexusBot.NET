# PokeBot v1.1.6 - UI Overhaul & Dynamic Memory Support

## Changelog

### 🆕 New Feature: Dynamic Memory Offset Detection
*   **Auto-Fix Capabilities**: If hardcoded offsets fail (e.g., after a game update), the bot now intelligently scans memory to locate the correct addresses dynamically using unique signatures.
*   **Future-Proofing**: Reduces the need for manual updates when game versions change.

### 🎨 UI Overhaul: Memory Scanner
*   **Redesigned Layout**: The Memory Scanner interface has been rebuilt with a clean, grid-based layout.
*   **Improved Usability**: Fixed misalignment issues; buttons and input fields are now logically grouped and properly spaced.
*   **Visual Clarity**: "Start Offset" and "Length" fields are now clearly visible and accessible.

### 🔌 Enhanced Connectivity & Safety
*   **Logical Grouping**: Moved Game Selection to the "Manual Connection" section for a more intuitive workflow.
*   **Game Validation**: Added a safety check that validates the connected game's TitleID against your selection (e.g., warns if you select PLZA but are connected to SWSH).

### 🐛 Bug Fixes
*   Resolved build errors related to method scoping in `PointerScanner`.
*   Fixed UI event handling for bot verification.
*   Improved stability during bot initialization.

---
*Self-contained build included: No .NET runtime installation required.*
