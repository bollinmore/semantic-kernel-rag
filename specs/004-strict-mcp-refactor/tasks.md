# Tasks: Strict MCP Refactor

**Feature**: `004-strict-mcp-refactor`
**Spec**: [specs/004-strict-mcp-refactor/spec.md](spec.md)
**Plan**: [specs/004-strict-mcp-refactor/plan.md](plan.md)

## Implementation Strategy

We will transform the existing REST API based architecture into a strict Model Context Protocol (MCP) system over stdio.
1.  **Phase 1 (Setup)**: Rename and restructure projects (CLI -> Client, Server -> Console App).
2.  **Phase 2 (Foundational)**: Implement the core MCP protocol (JsonRpc) and stdio transport on both ends. This satisfies **US1**.
3.  **Phase 3 (US2 - Inject)**: Re-enable the document ingestion flow using MCP Tools.
4.  **Phase 4 (US3 - Query)**: Re-enable the RAG query flow using MCP Tools and Client-side orchestration.
5.  **Phase 5 (Polish)**: Cleanup and final verification.

## Dependencies

- **US1 (Connect)** must be completed first to establish the communication channel.
- **US2 (Inject)** is implemented before **US3 (Query)** to ensure data exists for querying, despite US3 having higher business priority (P1 vs P2).
- **Setup** phase handles the project renaming which affects all subsequent file paths.

---

## Phase 1: Setup & Restructuring

**Goal**: Prepare the project structure by renaming the CLI to Client and converting the Server to a Console Application.

- [x] T001 Rename `RagMcpServer.CLI` project directory to `RagMcpClient` in `src/RagMcpClient`
- [x] T002 Rename `RagMcpServer.CLI.csproj` to `RagMcpClient.csproj` in `src/RagMcpClient/RagMcpClient.csproj`
- [x] T003 Update solution file `RagMcpServer.slnx` to reflect the project rename
- [x] T004 Clean up `src/RagMcpServer`: Delete `Controllers/` directory
- [x] T005 Clean up `src/RagMcpServer`: Delete `Middleware/` directory
- [x] T006 Clean up `src/RagMcpServer`: Delete `RagMcpServer.http`
- [x] T007 Modify `src/RagMcpServer/RagMcpServer.csproj` to be a Console Application (`<OutputType>Exe</OutputType>`) and remove web SDK reference if present
- [x] T008 [P] Update `src/RagMcpServer/Program.cs` to remove WebApplication builder and setup a basic Console entry point

## Phase 2: Foundational (US1 - Client Connects to Server)

**Goal**: Establish the MCP connection over stdio. The Client spawns the Server, and they handshake.
**Story**: [US1] Client Connects to Server (P1)

- [x] T009 [US1] Create `Mcp/Types.cs` in `src/RagMcpServer/Mcp/Types.cs` defining `McpRequest`, `McpResponse`, and `McpTool` classes per `data-model.md`
- [x] T010 [US1] Create `Mcp/McpServer.cs` in `src/RagMcpServer/Mcp/McpServer.cs` to handle stdio reading/writing and JSON-RPC dispatching
- [x] T011 [US1] Implement `Initialize` handling in `src/RagMcpServer/Mcp/McpServer.cs` to respond to handshake
- [x] T012 [US1] Implement `Tools/List` handling in `src/RagMcpServer/Mcp/McpServer.cs` (returning empty list for now)
- [x] T013 [US1] Update `src/RagMcpServer/Program.cs` to instantiate and run `McpServer`
- [x] T014 [US1] Copy `Mcp/Types.cs` to `src/RagMcpClient/Mcp/Types.cs` (or share via shared project if preferred, but copy is fine for isolation)
- [x] T015 [US1] Create `Mcp/McpClient.cs` in `src/RagMcpClient/Mcp/McpClient.cs` to manage Server process spawning (`Process.Start`) and stdio communication
- [x] T016 [US1] Create `Commands/TestConnectionCommand.cs` in `src/RagMcpClient/Commands/TestConnectionCommand.cs` to verify handshake and list tools
- [x] T017 [US1] Register `test-connection` command in `src/RagMcpClient/Program.cs`

## Phase 3: User Story 2 (Inject Data)

**Goal**: Enable document ingestion via the `Inject` MCP tool.
**Story**: [US2] User Injects Data (P2)

- [x] T018 [US2] Ensure `SqliteDbService.cs` in `src/RagMcpServer/Services/SqliteDbService.cs` is compatible with Console execution (check configuration loading)
- [x] T019 [US2] Ensure `OllamaEmbeddingService.cs` in `src/RagMcpServer/Services/OllamaEmbeddingService.cs` works in Console environment
- [x] T020 [US2] Update `src/RagMcpServer/Mcp/McpServer.cs` to register the `Inject` tool definition
- [x] T021 [US2] Implement `Inject` tool handler in `src/RagMcpServer/Mcp/McpServer.cs` calling `DocumentProcessingService.ProcessAndSaveAsync`
- [x] T022 [US2] Update `src/RagMcpServer/Services/DocumentProcessingService.cs` to ensure it uses the registered services correctly without HTTP context
- [x] T023 [US2] Update `src/RagMcpClient/Commands/InjectCommand.cs` to use `McpClient` to call the `Inject` tool instead of HTTP `ApiClient`
- [x] T024 [P] [US2] Remove `src/RagMcpClient/Services/ApiClient.cs` as it is replaced by `McpClient`

## Phase 4: User Story 3 (Query & RAG)

**Goal**: Enable RAG querying via the `Query` MCP tool and Client-side orchestration.
**Story**: [US3] User Asks Question (RAG) (P1)

- [x] T025 [US3] Update `src/RagMcpServer/Mcp/McpServer.cs` to register the `Query` tool definition
- [x] T026 [US3] Implement `Query` tool handler in `src/RagMcpServer/Mcp/McpServer.cs` calling `QueryService.SearchAsync`
- [x] T027 [US3] Update `src/RagMcpServer/Services/QueryService.cs` to return raw chunks instead of formatted answers (Server is dumb now)
- [x] T028 [US3] Create `Services/AgentService.cs` in `src/RagMcpClient/Services/AgentService.cs` to handle LLM connection (Ollama) and RAG orchestration
- [x] T029 [US3] Implement RAG logic in `src/RagMcpClient/Services/AgentService.cs`: Input -> McpClient.Call("Query") -> Context -> LLM -> Output
- [x] T030 [US3] Update `src/RagMcpClient/Commands/QueryCommand.cs` to use `AgentService` instead of HTTP `ApiClient`
- [x] T031 [US3] Add configuration for LLM endpoint (Ollama) in `src/RagMcpClient/appsettings.json` (create if missing)

## Phase 5: Polish & Cleanup

**Goal**: Final cleanups and verification.

- [x] T032 Remove any remaining `Microsoft.AspNetCore.*` references from `src/RagMcpServer/RagMcpServer.csproj` to ensure strict Console app
- [x] T033 Verify `Serilog` logging in `src/RagMcpServer` is writing to `Console.Error` (stderr) or file, NOT `Console.Out` (stdout) which is for MCP
- [x] T034 Delete `src/RagMcpClient/Services/ApiClient.cs` if not done in Phase 3
- [x] T035 Update `README.md` or `quickstart.md` with new Client usage instructions