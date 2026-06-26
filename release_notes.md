# NexusBot.NET v7.0.9

Welcome to NexusBot.NET v7.0.9! This release focuses on crucial bug fixes for hardware initialization, CI/CD stability, and documentation rendering.

## 🐛 Bug Fixes & Hardware Compatibility
- **sys-botbase 2.5 Boot Sequence Fixes**: Resolved a major issue where Pokémon Brilliant Diamond/Shining Pearl (BDSP) and Pokémon Scarlet/Violet (SV) would fail to launch when using `sys-botbase` v2.5. We've introduced a hardware delay to ensure the virtual controller fully mounts before pushing HID configuration packets (`keySleepTime`/`pollRate`), preventing the initial inputs from being dropped.

## 🛠️ CI/CD & Testing
- **Automated Test Stability**: Fixed intermittent race conditions during CI/CD test execution by explicitly disabling assembly parallelization using `xunit.runner.json`. This ensures tests no longer encounter `FileNotFoundException` during assembly loading.

## 📝 Documentation
- **Markdown & Encoding Fixes**: Repaired multiple encoding issues (mojibake) across various markdown files, including `CONTRIBUTING.md` and `GOVERNANCE.md`, ensuring all emojis and special characters render flawlessly on GitHub.
