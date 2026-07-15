## 9.1.5

### Bug Fixes
* Fixed an issue with the AI Chatbot where Hugging Face returned \"Model not supported by provider hf-inference"\. The API router no longer forces the hf-inference provider, allowing dynamic model routing to active providers.
* Updated the default AI Chatbot model to \Qwen/Qwen2.5-7B-Instruct\ which is entirely ungated (Apache 2.0) and requires zero user verification to use on Hugging Face.
