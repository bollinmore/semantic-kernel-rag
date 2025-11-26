# Implementation Plan: Strict MCP Refactor

**Branch**: `004-strict-mcp-refactor` | **Date**: 2025-11-25 | **Spec**: [specs/004-strict-mcp-refactor/spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-strict-mcp-refactor/spec.md`

## Summary

Refactor the system to strictly adhere to the Model Context Protocol (MCP). The `RagMcpServer` will strip all REST APIs and implement a stdio-based JSON-RPC server exposing `Inject` and `Query` tools. It will delegate embedding generation to an external service (Ollama) but will NOT handle conversational logic.

The `RagMcpServer.CLI` will be renamed to `RagMcpClient`. It will be responsible for:
1. Spawning and managing the `RagMcpServer` process.
2. Establishing the MCP handshake.
3. Managing the user interaction loop (CLI commands).
4. Orchestrating the RAG flow: User -> Client -> (LLM decision) -> Server Tool -> Client -> LLM -> User.

## Technical Context

**Language/Version**: C# (.NET 8)
**Primary Dependencies**: 
- `System.Text.Json` (for JSON-RPC)
- `Microsoft.SemanticKernel` (Client: Chat; Server: Embeddings)
- `Spectre.Console` (Client UI)
- `Microsoft.Data.Sqlite` (Server Storage)
**Storage**: SQLite (Vector Store)
**Communication**: Standard Input/Output (stdio) using JSON-RPC 2.0
**Project Type**: Console Application (Server) + Console Application (Client)

## Constitution Check

- [x] **I. Code Quality**: Adheres to .NET standards. Clean separation of concerns.
- [x] **II. Testing Standards**: Integration tests will verify the full MCP loop.
- [x] **III. User Experience Consistency**: CLI usage remains consistent (`inject`, `query` commands preserved).
- [x] **IV. Performance Requirements**: Stdio is low-latency.

## Project Structure

### Documentation (this feature)

```text
specs/004-strict-mcp-refactor/
├── plan.md              # This file
├── research.md          # Protocol decisions
├── data-model.md        # Entities & Protocol schemas
├── quickstart.md        # Usage instructions
├── contracts/           # Tool input schemas (YAML)
└── tasks.md             # Tasks
```

### Source Code

```text
src/
├── RagMcpServer/             # The MCP Server (Context Provider)
│   ├── Program.cs            # Main entry (stdio loop)
│   ├── Mcp/                  # MCP Protocol handling
│   │   ├── McpServer.cs
│   │   └── Types.cs
│   ├── Services/             # Business Logic (Embeddings, DB)
│   │   ├── SqliteDbService.cs
│   │   └── EmbeddingService.cs
│   └── Configuration/
└── RagMcpClient/             # The MCP Client (formerly RagMcpServer.CLI)
    ├── Program.cs
    ├── Mcp/                  # MCP Client handling
    │   ├── McpClient.cs
    │   └── Types.cs
    ├── Services/             # Orchestration
    │   └── AgentService.cs   # RAG Logic
    └── Commands/             # CLI Commands (Spectre)
        ├── InjectCommand.cs
        └── QueryCommand.cs
```

**Structure Decision**: Renaming `RagMcpServer.CLI` to `RagMcpClient` and refactoring `RagMcpServer` to remove WebAPI components.