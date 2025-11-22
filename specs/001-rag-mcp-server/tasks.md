# Tasks: RAG MCP Server for Document Extraction

**Input**: Design documents from `/specs/001-rag-mcp-server/`
**Prerequisites**: plan.md (required), spec.md (required for user stories)

**Tests**: This plan includes tasks for unit and integration tests as specified in the implementation plan.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for the C# Web API.

- [x] T001 Create .NET solution and project structure in `src/` and `tests/`.
- [x] T002 [P] Add NuGet packages to `src/RagMcpServer/RagMcpServer.csproj`: Microsoft.SemanticKernel, ASP.NET Core, Microsoft.SemanticKernel.Connectors.Chroma.
- [x] T003 [P] Add NuGet packages to `tests/RagMcpServer.UnitTests/` and `tests/RagMcpServer.IntegrationTests/`: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing.
- [x] T004 [P] Configure `src/RagMcpServer/appsettings.json` with placeholders for ChromaDB and Ollama endpoint URLs.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core services that must be complete before any user story can be implemented.

- [x] T005 Implement a ChromaDB connection service to be used by other parts of the application in `src/RagMcpServer/Services/ChromaDbService.cs`.
- [x] T006 Implement a text embedding service that connects to the local Ollama `nomic-embed-text` model in `src/RagMcpServer/Services/OllamaEmbeddingService.cs`.
- [x] T007 Configure dependency injection for `ChromaDbService` and `OllamaEmbeddingService` in `src/RagMcpServer/Program.cs`.

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - Batch Document Ingestion (Priority: P1) 🎯 MVP

**Goal**: As an admin, I can provide a path to a file or directory to have its documents processed and indexed.
**Independent Test**: Submit a POST request to `/documents` with a valid path and verify a "202 Accepted" response. Check ChromaDB to confirm that text chunks from the documents have been added.

### Implementation for User Story 1

- [x] T008 [P] [US1] Create request and response DTOs for the ingestion endpoint in `src/RagMcpServer/Models/IngestionModels.cs`.
- [x] T009 [US1] Implement the core document processing logic (file reading, text extraction for .txt/.md, PDF parsing) in a new `src/RagMcpServer/Services/DocumentProcessingService.cs`.
- [x] T010 [US1] Implement text chunking logic within the `DocumentProcessingService.cs` to segment large documents.
- [x] T011 [US1] Implement the `POST /documents` API endpoint in `src/RagMcpServer/Controllers/DocumentsController.cs`, using the `DocumentProcessingService` to handle the request.
- [x] T012 [US1] Add logic to the `DocumentsController.cs` and `DocumentProcessingService.cs` to handle the manual re-scan of a directory.
- [x] T013 [P] [US1] Write unit tests for the `DocumentProcessingService` in `tests/RagMcpServer.UnitTests/`.
- [x] T014 [US1] Write integration tests for the `/documents` endpoint in `tests/RagMcpServer.IntegrationTests/`.

**Checkpoint**: User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - Document Querying (Priority: P2)

**Goal**: As a client, I can send a query to the server and receive a relevant answer from the indexed documents.
**Independent Test**: After ingesting documents, submit a POST request to `/query` with a question. Verify a "200 OK" response containing a relevant answer.

### Implementation for User Story 2

- [x] T015 [P] [US2] Create request and response DTOs for the query endpoint in `src/RagMcpServer/Models/QueryModels.cs`.
- [x] T016 [US2] Implement a `QueryService` to handle the RAG logic in `src/RagMcpServer/Services/QueryService.cs`. This service will:
    - Generate an embedding for the incoming query using `OllamaEmbeddingService`.
    - Search ChromaDB for relevant `Knowledge Chunks` using `ChromaDbService`.
    - Use Semantic Kernel to generate a final answer based on the query and retrieved chunks.
- [x] T017 [US2] Implement the `POST /query` API endpoint in `src/RagMcpServer/Controllers/QueryController.cs`, using the `QueryService`.
- [x] T018 [P] [US2] Write unit tests for the `QueryService` in `tests/RagMcpServer.UnitTests/`.
- [x] T019 [US2] Write integration tests for the `/query` endpoint in `tests/RagMcpServer.IntegrationTests/`.

**Checkpoint**: User Stories 1 AND 2 should now both work.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories.

- [x] T020 [P] Implement global error handling middleware in `src/RagMcpServer/Program.cs`.
- [x] T021 [P] Add structured logging (e.g., Serilog) throughout the application.
- [x] T022 [P] Create a `Dockerfile` for containerizing the application.
- [x] T023 Update the root `README.md` with detailed setup and usage instructions, referencing the `quickstart.md`.

---

## Dependencies & Execution Order

- **Setup (Phase 1)** & **Foundational (Phase 2)** must be completed before any user stories.
- **User Story 1 (Phase 3)** is the MVP and has no dependencies on other stories.
- **User Story 2 (Phase 4)** depends on the outcome of User Story 1 (ingested documents) but can be developed in parallel.
- **Polish (Phase 5)** can be addressed after the core user stories are functional.

### Parallel Opportunities

- Once the Foundational phase is complete, development for User Story 1 and User Story 2 can happen in parallel.
- Within the Polish phase, creating the Dockerfile (T021), writing unit tests (T017, T018), and writing integration tests (T019, T020) are highly parallelizable.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1.  Complete Phase 1: Setup
2.  Complete Phase 2: Foundational
3.  Complete Phase 3: User Story 1
4.  **STOP and VALIDATE**: Test the document ingestion flow independently. This is the core capability and represents the MVP.

### Incremental Delivery

1.  After the MVP is validated, proceed to Phase 4 (User Story 2) to add querying capabilities.
2.  Finally, complete the Polish phase to ensure the application is robust and well-documented.
