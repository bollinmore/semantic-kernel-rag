# Implementation Plan: CLI Data Management

**Branch**: `003-cli-data-management` | **Date**: 2025-11-25 | **Spec**: [Link](spec.md)
**Input**: Feature specification from `specs/003-cli-data-management/spec.md`

## Summary

The feature adds a Command Line Interface (CLI) to the system, enabling users to:
1.  **Import (Inject)** documents from a local directory to the server.
2.  **Query** the system using natural language.
3.  **Inspect (Info)** the status of the vector database.

The implementation involves creating a new .NET Console Application (`RagMcpServer.CLI`) using `Spectre.Console.Cli` that communicates with the existing `RagMcpServer` via HTTP. The server will be updated with new endpoints to support file uploads and status retrieval.

## Technical Context

**Language/Version**: C# (.NET 8)
**Primary Dependencies**: `Spectre.Console.Cli`, `System.Net.Http`
**Storage**: SQLite (Server-side only)
**Testing**: xUnit
**Target Platform**: Windows/Linux/macOS (Cross-platform CLI)
**Project Type**: Web API + Console Client
**Performance Goals**: Fast startup for CLI, efficient file upload streaming.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Code Quality**: Adheres to .NET 8 standards, uses `Spectre.Console.Cli` for clean code.
- [x] **II. Testing Standards**: Integration tests will cover the new API endpoints.
- [x] **III. User Experience Consistency**: CLI uses consistent styling (help text, progress bars).
- [x] **IV. Performance Requirements**: API handles streams efficiently; Vector DB stats query is optimized (direct SQL).

## Project Structure

### Documentation (this feature)

```text
specs/003-cli-data-management/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
src/
├── RagMcpServer/           # Existing Web API
│   ├── Controllers/        # Updated with Upload/Info
│   └── Services/           # Updated processing logic
└── RagMcpServer.CLI/       # NEW: Console Application
    ├── Commands/           # Inject, Query, Info commands
    ├── Services/           # Http Client wrappers
    └── Program.cs          # Entry point
```

**Structure Decision**: Separate Client Project. This ensures clean separation of concerns and allows the CLI to be distributed independently of the server.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New Project | CLI functionality | Embedding CLI in Web API host is messy and couples concerns. |