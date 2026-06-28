# Release Notes

## [Unreleased]

### Added
- **AI Chatbot File Generation:** Users can now reply "File" or "pkm" when the AI generates a legal Showdown set to receive the exact `.pkm` file directly as a Discord attachment instead of joining the trade queue.

### Improved
- **AI Rate Limiting:** Implemented robust rate-limit handling for the Hugging Face API. Added a `SemaphoreSlim` queue to manage concurrent AI requests and explicitly parse `Retry-After` headers on HTTP 429 errors, gracefully handling high server traffic without triggering API bans.
