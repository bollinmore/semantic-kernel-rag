# Research & Decisions: RAG MCP Server

**Date**: 2025-11-22
**Feature**: RAG MCP Server for Document Extraction
**Spec**: [spec.md](./spec.md)

This document records the decisions made to resolve the "NEEDS CLARIFICATION" items from the implementation plan.

## 1. C# Client for ChromaDB

### Research Task
Investigate the best C# client library for interacting with a ChromaDB vector database from a .NET application.

### Findings
- There is a community-developed `ChromaDB.Client` library available on NuGet.
- More importantly, Microsoft Semantic Kernel provides its own official connector for ChromaDB: `Microsoft.SemanticKernel.Connectors.Chroma`.

### Decision
Use the official `Microsoft.SemanticKernel.Connectors.Chroma` connector.

### Rationale
- **Integration**: It is designed to work seamlessly within the Semantic Kernel ecosystem, simplifying the architecture.
- **Maintenance**: As an official connector, it is more likely to be well-maintained and updated with new versions of Semantic Kernel.
- **Simplicity**: It avoids adding an extra third-party dependency when the primary framework already provides the necessary functionality.

### Alternatives Considered
- **`ChromaDB.Client`**: A viable community library, but less ideal than the official connector as it would be a separate dependency to manage.

## 2. Integration of `nomic-embed-text` Model

### Research Task
Determine the best approach to use the `nomic-embed-text` embedding model within a Microsoft Semantic Kernel application.

### Findings
- Semantic Kernel is model-agnostic and allows for the integration of custom text embedding generation services.
- To use a specific local model like `nomic-embed-text`, the model must be served via an HTTP endpoint. Tools like Ollama or custom Python servers (e.g., using FastAPI/Flask) are commonly used for this purpose.
- The C# application can then be configured to treat this local endpoint as a text embedding service. Semantic Kernel's `HttpClient` can be used to make the requests.

### Decision
The `nomic-embed-text` model will be hosted locally via an Ollama-compatible server. The Semantic Kernel application will be configured with a custom `ITextEmbeddingGenerationService` that communicates with this local HTTP endpoint.

### Rationale
- **Flexibility**: This approach decouples the model serving from the C# application, allowing the model to be updated or swapped independently.
- **Compatibility**: Semantic Kernel is designed to work with various model endpoints, and this pattern is a standard way to integrate local or custom models.
- **Performance**: A local model server can provide fast embedding generation without reliance on external APIs.

### Alternatives Considered
- **Direct Model Loading**: Loading the model directly into the C# process (e.g., via ONNX) is more complex and has limited support for this specific model. An HTTP service is a much more straightforward and robust integration path.
