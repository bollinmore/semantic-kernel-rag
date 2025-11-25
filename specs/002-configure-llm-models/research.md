# Research: Configure LLM and Embedding Models

**Branch**: `002-configure-llm-models`
**Feature**: Configurable LLM and Embedding Providers

## Key Decisions

### 1. Configuration Structure
**Decision**: Use a hierarchical configuration structure in `appsettings.json` with a central `AI` section containing `TextGeneration` and `TextEmbedding` subsections. Each subsection will specify a `Provider` (enum/string) and specific settings.

**Rationale**:
- Allows independent configuration of LLM (TextGeneration) and Embeddings.
- Standardizes how provider settings are defined.
- "Provider" key acts as the discriminator for startup logic.

**Proposed Schema**:
```json
{
  "AI": {
    "TextGeneration": {
      "Provider": "Ollama", // or "OpenAI", "LiteLLM"
      "ModelId": "llama3.1",
      "Endpoint": "http://localhost:11434",
      "ApiKey": ""
    },
    "TextEmbedding": {
      "Provider": "Ollama",
      "ModelId": "nomic-embed-text",
      "Endpoint": "http://localhost:11434",
      "ApiKey": ""
    }
  }
}
```

### 2. Strongly-Typed Configuration (Options Pattern)
**Decision**: Implement `AIConfig`, `TextGenerationConfig`, and `TextEmbeddingConfig` classes and bind them using `IOptions<T>`.

**Rationale**:
- Type safety.
- Decouples services from `IConfiguration`.
- Enables validation via `DataAnnotations` (e.g., `[Required]`).
- Allows easy overriding via Environment Variables (required for `launchSettings.json` support).

### 3. Service Registration (Startup Logic)
**Decision**: Use a switch/factory approach in `Program.cs` to register the correct implementation of `IChatCompletionService` and `ITextEmbeddingGenerationService` based on the configured `Provider`.

**Rationale**:
- `IChatCompletionService` is the Semantic Kernel abstraction.
- We can register `OllamaChatCompletionService` or `OpenAIChatCompletionService` (standard SK implementation or custom wrapper) based on the config string.
- This happens at startup (singleton/scoped), meeting the "restart to apply" requirement.

### 4. LiteLLM Support
**Decision**: Treat LiteLLM as an "OpenAI-compatible" provider.

**Rationale**:
- LiteLLM typically exposes an OpenAI-compatible API.
- We can use the standard `OpenAIChatCompletionService` from Semantic Kernel (or a custom wrapper if needed for specific header handling) pointing to the LiteLLM endpoint.
- The user simply configures `Provider: "OpenAI"` (or a semantic alias "LiteLLM" that maps to OpenAI logic) and sets the `Endpoint` to the LiteLLM URL.

## Alternatives Considered

- **Runtime Switching**: Injecting a factory into services to switch providers per-request.
    - *Rejected*: Complexity is not needed. The requirement is "read config before startup".
- **LaunchSettings.json direct parsing**:
    - *Rejected*: `launchSettings.json` sets Environment Variables. .NET's `IConfiguration` automatically reads these. We don't need to parse the file manually; we just need to respect the `AI__TextGeneration__Provider` naming convention for env vars.

## Unknowns & Clarifications

- **Current Services**: `OllamaChatCompletionService` currently hardcodes model names. Needs refactoring to accept `IOptions<AIConfig>`.
- **OpenAI Integration**: Does the project already have the `Microsoft.SemanticKernel.Connectors.OpenAI` package?
    - *Action*: Check csproj. If not, need to add it to support LiteLLM/OpenAI.
