# DudeBot.NET Version 6.5.18 Release Notes

## Overview
This release focuses on providing a massive visual upgrade to the sprite generation logic, bringing flawless support for image links generated against the `NexusRisen/Nexus-Risen-Edition-Sprite-Images` repository.

## Changes & Improvements

### 🖼️ Sprite Sync & Reliability
* **Punctuation Matching:** Fixed critical broken links by preserving correct punctuation (`'`, `.`, `_`) for species like Farfetch'd, Sirfetch'd, Mr. Mime, and Type: Null, allowing their exact repository filenames to be targeted.
* **Form Image Fallbacks:** The sprite generator detects Pokémon that lack individual form sprites in the repository (e.g. Aegislash-Blade, Cramorant's gulps, and many more). These safely fall back to the Pokémon's base image instead of returning broken 404 images.
* **Complex Form Mappings:** Implemented specific mapping rules for edge-case forms like Alcremie and Minior. Alcremie swirl forms correctly append the repository's required sweet modifier (`-strawberry`), and Minior core forms map accurately to the repo's `c-color` structure.
* **Mega Evolution Support:** Automatically handles parsing and routing Mega Evolutions safely back to their base species images, as the repository does not maintain Mega form sprites.
* **Naming Standardization:** Safely handles naming mismatches (e.g., mapping Pumpkaboo and Gourgeist's `Jumbo` to `Super`) and correctly handles Ogerpon Tera mask forms without breaking image paths.

## Impact
Zero 404 broken image links across all 1025 Pokémon species, both shiny and non-shiny, regardless of form or generation context!
