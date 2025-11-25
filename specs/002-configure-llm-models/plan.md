# Implementation Plan: Configure LLM and Embedding Models

**Branch**: `002-configure-llm-models` | **Date**: 2025-11-24 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `spec.md`

## Summary

Implement a flexible configuration system for AI services (LLM and Embeddings) allowing the user to select providers (Ollama, OpenAI/LiteLLM) via `appsettings.json` or Environment Variables. This involves creating strongly-typed configuration classes, refactoring `Program.cs` to conditionally register Semantic Kernel services based on these settings, and updating existing services to use the new configuration.

## Technical Context

**Language/Version**: C# (.NET 8)
**Primary Dependencies**: Microsoft.SemanticKernel
**Configuration**: `appsettings.json`, `launchSettings.json` (via Env Vars), `IOptions<T>` pattern.
**Storage**: N/A (Configuration is transient/file-based).
**Testing**: xUnit (RagMcpServer.UnitTests).
**Project Type**: Web API (ASP.NET Core).
**Constraints**: Must support "LiteLLM" (OpenAI-compatible) and "Ollama".

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Code Quality**: Uses standard .NET Options pattern and Dependency Injection.
- [x] **II. Testing Standards**: Unit tests for Configuration binding and Service registration logic.
- [x] **III. User Experience Consistency**: N/A (Backend feature), but consistent config structure improves DX.
- [x] **IV. Performance Requirements**: N/A (Startup time impact is negligible).

## Project Structure

### Documentation (this feature)

```text
specs/002-configure-llm-models/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Configuration Schema
├── quickstart.md        # How to configure providers
└── contracts/           # N/A (Internal Config)
```

### Source Code (repository root)

```text
src/RagMcpServer/
├── Configuration/
│   ├── AIConfig.cs           # Root config
│   └── ProviderConfig.cs     # Shared settings (Endpoint, Key, Model)
├── Services/
│   ├── IVectorDbService.cs
│   ├── OllamaChatCompletionService.cs # Refactor to use Config
│   └── OllamaEmbeddingService.cs      # Refactor to use Config
└── Program.cs                        # Update DI registration
```

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | | |