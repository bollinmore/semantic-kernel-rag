# Tasks: CLI Data Management

**Feature Branch**: `003-cli-data-management`
**Status**: Pending
**Total Tasks**: 19
**MVP Scope**: User Story 1 (Import) + Foundation

## Implementation Strategy

We will build this feature as a separate Console Application (`RagMcpServer.CLI`) that communicates with the existing `RagMcpServer` via HTTP. This requires first updating the Server API to support file uploads and status queries (Foundation Phase), followed by implementing the CLI commands for each User Story.

The CLI will use `Spectre.Console.Cli` for the user interface.

## Phase 1: Setup

- [ ] T001 Create `RagMcpServer.CLI` console project in `src/RagMcpServer.CLI`
- [ ] T002 Add `Spectre.Console.Cli` and `System.Net.Http.Json` packages to `src/RagMcpServer.CLI/RagMcpServer.CLI.csproj`
- [ ] T003 Create `src/RagMcpServer.CLI/Program.cs` with basic Spectre Console app skeleton

## Phase 2: Foundation (Server API Updates)

**Goal**: Enable file uploads and stats retrieval on the backend.

- [ ] T004 [P] Update `src/RagMcpServer/Services/IVectorDbService.cs` to add `GetDocumentCountAsync` method
- [ ] T005 Update `src/RagMcpServer/Services/SqliteDbService.cs` to implement `GetDocumentCountAsync` using SQL count query
- [ ] T006 [P] Create `src/RagMcpServer/Controllers/InfoController.cs` with `GET /Info` endpoint
- [ ] T006a [P] Create integration tests for `InfoController` in `tests/RagMcpServer.IntegrationTests/InfoControllerTests.cs`
- [ ] T007 Update `src/RagMcpServer/Services/DocumentProcessingService.cs` to add `GetDocumentChunksFromStreamAsync` for handling uploaded files
- [ ] T008 Update `src/RagMcpServer/Controllers/DocumentsController.cs` to add `POST /Documents/upload` endpoint handling `multipart/form-data`
- [ ] T008a [P] Create integration tests for `DocumentsController.upload` in `tests/RagMcpServer.IntegrationTests/DocumentsControllerTests.cs`

## Phase 3: User Story 1 - Import Documents (Priority: P1)

**Goal**: Users can import documents from a local directory via CLI.
**Independent Test**: Run `dotnet run -- inject -path ./test-docs` and verify 200 OK response.

- [ ] T009 [US1] Create `src/RagMcpServer.CLI/Services/ApiClient.cs` with `UploadDocumentsAsync` method
- [ ] T010 [US1] Create `src/RagMcpServer.CLI/Commands/InjectCommand.cs` implementing directory walking and file filtering
- [ ] T011 [US1] Register `inject` command in `src/RagMcpServer.CLI/Program.cs`

## Phase 4: User Story 2 - Search/Retrieve Data (Priority: P2)

**Goal**: Users can query the system from CLI.
**Independent Test**: Run `dotnet run -- query "test query"` and see results.

- [ ] T012 [US2] Update `src/RagMcpServer.CLI/Services/ApiClient.cs` to add `QueryAsync` method calling `POST /Query`
- [ ] T013 [US2] Create `src/RagMcpServer.CLI/Commands/QueryCommand.cs` to display search results in a table
- [ ] T014 [US2] Register `query` command in `src/RagMcpServer.CLI/Program.cs`

## Phase 5: User Story 3 - View Vector Database Status (Priority: P3)

**Goal**: Users can check system status.
**Independent Test**: Run `dotnet run -- info -vector_db` and see document count.

- [ ] T015 [US3] Update `src/RagMcpServer.CLI/Services/ApiClient.cs` to add `GetServerInfoAsync` method calling `GET /Info`
- [ ] T016 [US3] Create `src/RagMcpServer.CLI/Commands/InfoCommand.cs` to display vector DB stats
- [ ] T017 [US3] Register `info` command in `src/RagMcpServer.CLI/Program.cs`

## Dependencies

1. Phase 1 & 2 MUST be completed before any User Story tasks.
2. US1, US2, US3 are independent on the Client side, but dependent on Phase 2 (Server) updates.
3. T011, T014, T017 (Registration) depend on their respective Command creations.

## Parallel Execution Examples

- Developer A: T004, T005, T006 (Server Info/Stats logic)
- Developer B: T007, T008 (Server Upload logic)
- Developer C: T009, T010 (Client Inject logic - once Server Upload is agreed/mocked)
