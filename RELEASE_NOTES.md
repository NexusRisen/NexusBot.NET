# DudeBot.NET Release Notes (v6.4.0)

## Major Infrastructure Update

This release marks a significant milestone in the DudeBot.NET ecosystem, focusing on modernization and infrastructure reliability.

### 🔄 Core Engine Migration
- **PKHeX.Core NuGet Migration:** Successfully transitioned the entire project to use the official **PKHeX.Core** NuGet package (v26.5.5). This provides a more stable foundation and streamlines future dependency management.
- **Custom ALM Integration:** Integrated a specialized fork of **PKHeX-Plugins** (santacrab2/cherrytree) to ensure seamless compatibility with the latest Core library.
- **API Modernization:** Refactored multiple systems to align with the latest PKHeX 2026 standards, including `EntityContext` handling and unified `PKM` data structures.

### 🧪 Enhanced Testing Suite
- **Logic Verification:** Added a new suite of independent command logic tests in `SysBot.Tests`.
- **Independent Validation:** Verified `$convert`, `$legalize`, and `$lc` core logic in a headless environment, ensuring reliability independent of the Discord infrastructure.
- **Stability:** Executed the full 66-test battery with 100% success rate across all Pokémon generations.

### 🛠️ Bug Fixes & Refinement
- **VC Legality Patch:** Restored legacy support for Generation 1/2 Virtual Console transfers by re-implementing nature-experience correlation logic manually.
- **Inventory System:** Updated SWSH fossil and treasure pouch constructors to match updated Core signatures.
- **Metadata Reliability:** Resolved `GameVersionPriorityType` serialization warnings to ensure cleaner logs and more reliable configuration handling.

---
*DudeBot.NET v6.4.0 | Synchronized Intelligence*
