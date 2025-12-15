# Implementation Plan: Add RAG API Server

**Branch**: `005-rag-api-server` | **Date**: 2025-12-02 | **Spec**: [specs/005-rag-api-server/spec.md](../spec.md)
**Input**: Feature specification from `specs/005-rag-api-server/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement a semantic search HTTP API endpoint within the existing `RagMcpServer` application. The server will expose a `POST /api/rag/collections/{collectionName}/search` endpoint to allow external applications to retrieve relevant document chunks. The implementation will leverage ASP.NET Core (converting the existing Console app or hosting a web server alongside), enable Swagger documentation, and ensure configuration-driven database connections, all while maintaining the existing MCP functionality.

## Technical Context

**Language/Version**: C# / .NET 10.0 (Preview)
**Primary Dependencies**: 
- `Microsoft.AspNetCore.App` (Framework Reference)
- `Microsoft.Extensions.Hosting` (Existing)
- `FluentValidation` (New, for request validation)
- `Swashbuckle.AspNetCore` (New, for Swagger)
**Storage**: SQLite (Existing `SqliteDbService`)
**Testing**: `RagMcpServer.IntegrationTests` (Existing, needs update for HTTP)
**Target Platform**: Windows/Linux (Cross-platform .NET)
**Project Type**: Console Application (converting to/hybrid with Web API)
**Performance Goals**: < 1s response time for search
**Constraints**: 
- Must run alongside existing MCP Stdio loop.
- **CRITICAL**: HTTP logs must NOT pollute stdout (reserved for MCP JSON-RPC).
**Scale/Scope**: Single endpoint, existing monolith structure.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **I. Code Quality**: Adheres to C#/.NET standards, utilizing dependency injection and proper separation of concerns (Controllers/Services).
- [x] **II. Testing Standards**: Includes integration tests for the new API endpoint.
- [x] **III. User Experience Consistency**: API design follows RESTful conventions; Swagger UI provided for DX.
- [x] **IV. Performance Requirements**: Performance goal defined (< 1s); existing SQLite implementation is lightweight.

## Project Structure

### Documentation (this feature)

```text
specs/005-rag-api-server/
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
├── RagMcpServer/
│   ├── Controllers/             # New: API Controllers
│   ├── Models/
│   │   ├── SearchRequest.cs     # New: DTOs
│   │   └── SearchResult.cs
│   ├── Program.cs               # Modified: Add WebHost/Kestrel
│   ├── RagMcpServer.csproj      # Modified: Add FrameworkReference, NuGet packages
│   └── appsettings.json         # Modified: Kestrel config, Connection Strings
└── RagMcpServer.IntegrationTests/
    └── ApiTests/                # New: Integration tests for API
```

**Structure Decision**: Hybrid approach. Modify `RagMcpServer` to host an ASP.NET Core Web API alongside the MCP server. This avoids code duplication and allows sharing the `Kernel` and `IVectorDbService` instances.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Hybrid Console/Web App | MCP requires Stdio; API requires HTTP. | Running two separate processes (one for MCP, one for API) would require complex IPC or shared database access (locking issues with SQLite). Merging them allows shared memory state and simpler deployment. |
