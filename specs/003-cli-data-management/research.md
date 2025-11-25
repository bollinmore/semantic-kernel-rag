# Research: CLI Data Management

**Feature**: CLI Data Management (Import, Query, Info)
**Status**: Completed

## Decisions

### 1. CLI Framework
- **Decision**: Use `Spectre.Console.Cli`.
- **Rationale**: 
  - The project currently has no CLI infrastructure.
  - `Spectre.Console.Cli` is a modern, widely adopted, and "idiomatic" .NET library for building command-line interfaces.
  - It supports subcommands (`inject`, `query`, `info`) cleanly.
  - It provides excellent default help generation and output formatting (tables, progress bars) which matches the "user-friendly" requirement.
- **Alternatives Considered**:
  - `System.CommandLine`: Official but verbose and complex for this scope.
  - `Cocona`: Very simple, but `Spectre` offers better output styling out-of-the-box.

### 2. Architecture Pattern
- **Decision**: Client-Server Model (Remote CLI).
- **Rationale**:
  - The feature spec implies a distinction between "local directory" (user's machine) and "system" (server).
  - The existing `RagMcpServer` is a Web API.
  - Building the CLI as a separate client that talks to the API via HTTP allows:
    - Remote administration.
    - Separation of concerns.
    - No database locking issues (SQLite) since only the server accesses the DB.
- **Implication**:
  - The `DocumentsController` needs to be updated to support file uploads (currently it only scans server-side paths).
  - A new `Info` endpoint is needed.

### 3. API Changes
- **New Endpoint**: `POST /Documents/upload`
  - Accepts: `multipart/form-data` (List of files).
  - Action: Reads content, chunks it, embeds it, saves to DB.
  - Reason: Enables the CLI to send local files to the server.
- **New Endpoint**: `GET /Info`
  - Returns: JSON object with vector DB stats (e.g., Document Count, Collection Name).
  - Reason: Support the `info` command.

### 4. Vector DB Counting
- **Decision**: Use direct SQLite connection to query count.
- **Rationale**:
  - The `IMemoryStore` abstraction in Semantic Kernel does not currently expose a `Count` method.
  - Since `SqliteDbService` owns the connection string, it can safely open a read-only connection to query `SELECT COUNT(*)`.
  - We will need to determine the table name used by the SK connector (likely `SKMemoryTable` or collection-based).

## Unknowns Resolved
- **CLI Library**: Selected `Spectre.Console.Cli`.
- **Import Mechanism**: Switched from "Server Path" to "File Upload" to support true client-side CLI.
- **Stats Retrieval**: Identified gap in `IMemoryStore`, resolved via direct SQL query.
