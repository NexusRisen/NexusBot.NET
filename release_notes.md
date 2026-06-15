# DudeBot.NET Version 6.5.17 Release Notes

## Overview
This update improves image fetching across all games, seamlessly mapping PKHeX files and Showdown setups to their correct sprite form images.

## Changes & Improvements

### 🖼️ Sprite Handling Accuracy
* Fixed an issue where `pokeimg` was not loading correctly for certain forms and newer games like Legends ZA by utilizing `ShowdownParsing.GetStringFromForm`. This guarantees form strings mirror Showdown conventions perfectly.
* Ensured that Pokémon files natively possessing the Gigantamax factor correctly trigger `-Gigantamax.png` sprites across all platforms (including Stoat), resolving missing sprites for file/Showdown imports.

