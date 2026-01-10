# Release v1.1.8 - Alien Tech UI Overhaul

## 🎨 UI Modernization & "Alien Tech" Theme
This release introduces a comprehensive visual overhaul of the entire application, featuring a futuristic "Alien Tech" aesthetic inspired by high-end gaming interfaces.

### Key Visual Changes:
- **Glass Panel Containers**: Legacy `GroupBox` controls have been replaced with custom-painted, semi-transparent glass panels (`AlienTechPanel`) featuring:
  - Dark gradient backgrounds
  - Cyan accent corners
  - Chamfered edges
  - Subtle transparency effects
- **Unified Button Styling**: All buttons now share a consistent "Alien Tech" design:
  - Vertical gradient fills
  - Dynamic hover effects
  - Cyan accent lines and text
  - Custom chamfered borders
- **Modernized Input Fields**: Text boxes and combo boxes now feature:
  - Flat styles with dark backgrounds (RGB 20,20,20)
  - Cyan text for high contrast
  - Single-line borders for a clean look
- **Responsive Layouts**: Improved resize handling across all sections (Configuration, Logs, Developer Tools) to ensure UI elements scale correctly on different window sizes.

## 🛠 Developer Tools Overhaul
The Developer section has been completely rewritten to match the new design language while improving usability:
- **Manual Connection**: Simplified connection controls with modernized input fields for IP/Port.
- **Memory Scanner**: streamlined scanner interface with dedicated "Alien Tech" panels.
- **Memory Monitor & Pointer Tools**: Unified styling for advanced debugging tools.
- **Performance**: Removed legacy region clipping (ChamferedRegion) to eliminate visual artifacts (black edges) and improve rendering performance.

## 🔧 Technical Improvements
- **Code Refactoring**: Migrated away from legacy `GroupBox` control lookups to direct control references, improving type safety and maintainability.
- **High DPI Support**: Enhanced scaling logic to ensure the new custom controls look crisp on high-resolution displays.
- **Control Declarations**: Fixed missing control definitions in the Designer file to ensure stability.

## 🚀 Deployment
- **Self-Contained Executable**: This release is packaged as a single self-contained `PokeBot.exe`, requiring no external .NET runtime installation.

---
*PokeBot v1.1.8 - bringing the future of automation to your desktop.*
