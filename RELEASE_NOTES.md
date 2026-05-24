# Release Notes

## [v6.3.1] - 2026-05-24

### Added
- **Architectural Unification with Official SysBot.NET**: Overhauled the core connection layer to strictly align with official `kwsch/SysBot.NET` standards for high-frequency data handling and stability.
- **Native Asynchronous Socket Communication**: Re-implemented true `async`/`await` patterns for wireless (Wi-Fi/LAN) connections, utilizing `ArrayPool<byte>.Shared` and native `ReceiveAsync`/`SendAsync` for zero-allocation performance.
- **Modernized USB Communication**: Fully migrated USB logic to `LibUsbDotNet 3.0` standards. Replaced legacy synchronous patterns with `Task.Run` wrappers and optimized buffer handling for better stability on modern drivers.
- **HID Keyboard Improvements**: Fixed build regressions and restored missing `HidWaitTime` constants across all game routines (LA, SWSH, etc.) to support high-speed input settings.

### Changed
- **De-allocated Shared Buffers**: Removed all class-level `_sharedBuffer` and `_destinationArray` fields across `SwitchUSB` and `SwitchSocketAsync`. Memory is now allocated locally and dynamically per task, preventing state contention and improving thread safety.
- **USB Resource Lifecycle Management**: Implemented `IDisposable` for `SwitchUSB` to ensure proper cleanup of native USB resources and prevent handle leaks.
- **Enhanced USB Error Recovery**: Integrated official stability improvements that ignore `Win32Error` during bulk memory reads, preventing unnecessary disconnections during invalid RAM access.

### Fixed
- **Memory Leak Mitigation**: Optimized buffer return paths in `SwitchSocketAsync` to ensure rented memory is always returned to the pool, even during network failures.
- **Build Compatibility**: Resolved namespace and breaking changes between LibUsbDotNet 2.x and 3.x to ensure full project compatibility with the latest ecosystem tools.
- **Batch Trade Preservation**: Verified that all v6.3.0 batch trade features remain fully functional following the architectural refactor.
