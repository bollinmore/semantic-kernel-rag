# Quickstart: Configuring AI Providers

This guide explains how to configure the AI models (LLM and Embeddings) for the RagMcpServer.

## Default Configuration (Ollama)

By default, the application is configured to use **Ollama**.

**Prerequisites**:
- Ollama installed and running (`http://localhost:11434`)
- Models pulled: `ollama pull llama3.1` and `ollama pull nomic-embed-text`

**Configuration (`appsettings.json`)**:
```json
{
  "AI": {
    "TextGeneration": {
      "Provider": "Ollama",
      "ModelId": "llama3.1",
      "Endpoint": "http://localhost:11434"
    },
    "TextEmbedding": {
      "Provider": "Ollama",
      "ModelId": "nomic-embed-text",
      "Endpoint": "http://localhost:11434"
    }
  }
}
```

## Configuring for LiteLLM (OpenAI Compatible)

To use LiteLLM (or any OpenAI-compatible provider), change the provider to `OpenAI` and point the `Endpoint` to your LiteLLM instance.

**Steps**:
1. Open `launchSettings.json` (or set Environment Variables).
2. Update the `AI__TextGeneration` settings.

**Example `launchSettings.json` profile**:

```json
"profiles": {
  "LiteLLM": {
    "commandName": "Project",
    "dotnetRunMessages": true,
    "launchBrowser": true,
    "applicationUrl": "https://localhost:7132;http://localhost:5132",
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development",
      "AI__TextGeneration__Provider": "OpenAI",
      "AI__TextGeneration__ModelId": "gpt-3.5-turbo", 
      "AI__TextGeneration__Endpoint": "http://localhost:4000",
      "AI__TextGeneration__ApiKey": "sk-fake-key"
    }
  }
}
```

*Note: `ModelId` in LiteLLM config refers to the model name mapped in LiteLLM's config.*

## Switching Providers

The application reads configuration **at startup**. 
To apply changes:
1. Modify `appsettings.json` OR select the appropriate Launch Profile in Visual Studio.
2. Restart the application.
