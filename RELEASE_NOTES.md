# Release Notes

## v6.2.2
### 🚀 High-Performance Connection Engine
- **Zero-Allocation Socket Logic:** Refactored `SwitchSocketAsync` to use a fixed-size shared buffer pattern (0x40001). This eliminates the need for dynamic `List<byte>` resizing and frequent array allocations, drastically reducing Garbage Collection (GC) pressure.
- **Precision Timing (TCP NoDelay):** Disabled Nagle's Algorithm on all Wi-Fi connections. This removes artificial network jitter, ensuring that bot commands and `BaseDelay` timings are executed with micro-second accuracy.
- **Optimized Memory Management:**
    - Refactored `FlexRead` into `FlexReadIntoBuffer`, enabling direct streaming from the socket into pre-allocated memory.
    - Optimized `ReadBytesFromCmdAsync` to utilize the shared buffer for all standard memory reads (up to 256KB), bypassing `ArrayPool` for better deterministic performance.
    - Updated `SwitchUSB` to use a shared header buffer, bringing memory efficiency to USB connections.
- **Stability Enhancements:** These architectural changes provide a deterministic memory footprint, ensuring maximum stability for long-term hosting and multi-bot "farm" configurations.

## Version Details
- **Internal Version**: v6.2.2
- **Build Date**: May 15, 2026
