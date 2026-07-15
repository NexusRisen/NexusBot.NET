## 9.1.6

### Bug Fixes
* **Critical AI Fix**: Fixed a bug where the Hugging Face AI Chatbot would return \HTTP 404 Not Found\. The API endpoint string was incorrectly formatting the URL when a model identifier contained a slash (like \Qwen/Qwen2.5-7B-Instruct\). It now correctly uses the standard \1/chat/completions\ OpenAPI endpoint and injects the model ID in the JSON body.

## 9.1.5

### Bug Fixes
* Fixed an issue with the AI Chatbot where Hugging Face returned \"Model not supported by provider hf-inference"\. The API router no longer forces the hf-inference provider, allowing dynamic model routing to active providers.
* Updated the default AI Chatbot model to \Qwen/Qwen2.5-7B-Instruct\ which is entirely ungated (Apache 2.0) and requires zero user verification to use on Hugging Face.
