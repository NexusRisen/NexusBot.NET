# Release Notes - v6.2.0

## New Features
- **AI Chatbot Integration**: Integrated Hugging Face AI to assist users with Pokémon-related questions in Discord.
- **Natural Language Requests**: Users can now request Pokémon using natural language by mentioning the bot (e.g., "@DudeBot I want a competitive Charizard").
- **Automated AI Queueing**: The AI can automatically generate legal Showdown sets and, upon user confirmation (Yes/No), add them directly to the trade queue.
- **AI Legality Guard**: Implemented a real-time legality validation loop that verifies AI-generated Pokémon using PKHeX.Core before they reach the queue.
- **Dynamic Bot Naming**: The bot now automatically retrieves its name from the Discord Developer Portal, personalizing both status announcements and AI interactions.
- **Concurrent Request Handling**: Optimized Discord message processing with thread-safe state management for high-traffic servers.

## Improvements
- **Legality Engine**: Updated internal prompt logic to strictly forbid illegal or shiny-locked Pokémon unless officially released.
- **User Settings**: Added new AI Settings category in Discord Integration for API Key and Model customization.
- **Code Stability**: Fixed several internal naming conflicts and improved dependency management for JSON processing.

## Version Details
- **Internal Version**: v6.2.0
- **Build Date**: May 12, 2026
