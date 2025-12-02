# Tasks: Add RAG API Server

**Branch**: `005-rag-api-server`
**Spec**: [specs/005-rag-api-server/spec.md](../spec.md)
**Status**: Pending

## Phase 1: Setup
**Goal**: Initialize ASP.NET Core environment within the existing application and configure logging/hosting dependencies.

- [x] T001 Update `RagMcpServer.csproj` to support Web API (FrameworkReference `Microsoft.AspNetCore.App`) and add NuGet packages (`Swashbuckle.AspNetCore`, `FluentValidation.AspNetCore`).
- [x] T002 Update `appsettings.json` to include Kestrel configuration (port 5000) and allow configuring `AI:VectorDbPath`.
- [x] T003 Refactor `Program.cs` to use `WebApplication.CreateBuilder` instead of `Host.CreateDefaultBuilder`, ensuring `McpServer` is registered as a Hosted Service (or equivalent non-blocking execution) and Logging is strictly directed to Stderr.
- [x] T004 Create `src/RagMcpServer/Workers/McpServerWorker.cs` to wrap existing `McpServer.RunAsync` logic in a `BackgroundService`.

## Phase 2: Foundational
**Goal**: Create core data models and validation infrastructure required for all user stories.

- [x] T005 [P] Create `src/RagMcpServer/Models/SearchRequest.cs` with properties `Query`, `TopK`, `MinScore`.
- [x] T006 [P] Create `src/RagMcpServer/Models/SearchResult.cs`, `ResultItem.cs`, and `Metadata.cs` DTOs matching the Data Model.
- [x] T007 Create `src/RagMcpServer/Validators/SearchRequestValidator.cs` using FluentValidation to enforce rules (Query length, TopK range, MinScore range).
- [x] T008 [P] Register FluentValidation and Swagger services in `Program.cs`.

## Phase 3: User Story 1 - Retrieve Relevant Chunks (P1)
**Goal**: Implement the core search endpoint logic.
**Independent Test**: POST /api/rag/collections/default/search returns 200 OK with relevant chunks.

- [x] T009 [US1] Create `src/RagMcpServer/Controllers/RagController.cs` with `POST collections/{collectionName}/search` endpoint stub.
- [x] T010 [US1] Implement search logic in `RagController` using `IVectorDbService` to fetch results.
- [x] T011 [US1] Map vector DB results to `SearchResult` DTOs, including metadata and similarity scores.
- [x] T012 [US1] Implement `min_score` filtering logic within `RagController` (or Service layer if reusable).
- [x] T013 [US1] Create integration test `tests/RagMcpServer.IntegrationTests/ApiTests/SearchEndpointTests.cs` to verify happy path (200 OK) and parameter validation (400 Bad Request).

## Phase 4: User Story 2 - External Configuration (P1)
**Goal**: Ensure database connection is configurable via `appsettings.json`.
**Independent Test**: Changing `appsettings.json` connection string effectively changes the target DB.

- [x] T014 [US2] Verify `IVectorDbService` (likely `SqliteDbService`) correctly reads connection string from `AIConfig` (loaded from appsettings) in the new Web Host context.
- [x] T015 [US2] Add integration test `tests/RagMcpServer.IntegrationTests/ApiTests/ConfigurationTests.cs` to verify `appsettings.json` override behavior.

## Phase 5: User Story 3 - Structured API Response (P2)
**Goal**: Refine API response structure and enable Swagger UI for integrators.
**Independent Test**: Swagger UI is accessible at /swagger and response format matches spec.

- [x] T016 [US3] Configure Swagger Gen in `Program.cs` to produce OpenAPI v3 spec with XML comments (if enabled).
- [x] T017 [US3] Enable `app.UseSwagger()` and `app.UseSwaggerUI()` in `Program.cs` pipeline.
- [x] T018 [US3] Verify `SearchResult` JSON serialization exactly matches the Spec (camelCase, nested metadata).

## Phase 6: Polish
**Goal**: Final cleanups, error handling, and documentation.

- [x] T019 Implement global exception handler (or middleware) to return 500 Internal Server Error for DB failures without exposing sensitive details (stderr logging only).
- [x] T020 Ensure `collection_name` path parameter is correctly handled (return 404 if not found/supported, based on `IVectorDbService` capability).
- [x] T021 Manual verification: Run `dotnet run` and verify both MCP (stdin/out) and API (http://localhost:5000/swagger) work simultaneously.

## Dependencies

- **Phase 1 (Setup)** blocks all other phases.
- **Phase 2 (Foundational)** blocks Phase 3, 4, 5.
- **Phase 3 (US1)** blocks meaningful testing of US2 and US3.
- **Phase 4 (US2)** can be done in parallel with Phase 3 but requires functional app.
- **Phase 5 (US3)** is a refinement on top of Phase 3.

## Parallel Execution Examples

- **Models & Validators**: T005, T006, T007 can be implemented in parallel by different developers.
- **Controller & Tests**: T009 (Controller Stub) and T013 (Tests) can be started simultaneously (TDD).

## Implementation Strategy

1. **MVP (Setup + US1)**: Get the server running with a hardcoded or basic search endpoint. Ensure standard output is clean for MCP.
2. **Refinement (US2 + US3)**: Make it configurable and add Swagger.
3. **Hardening**: robust error handling and validation.
