## 9.1.4

### Bug Fixes
* Fixed a severe issue where the HuggingFace AI Chatbot would fail to respond due to DNS blocking by local routers/ISPs for `api-inference.huggingface.co`. The bot now uses the newer `router.huggingface.co` endpoint to bypass these restrictions.
