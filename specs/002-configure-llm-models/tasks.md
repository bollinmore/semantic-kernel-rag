# Tasks: Configure LLM and Embedding Models

**Input**: Design documents from `/specs/002-configure-llm-models/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, quickstart.md

**Tests**: Included per `plan.md` Constitution Check (Unit tests for Config & DI).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

## Path Conventions

- `src/RagMcpServer/`
- `tests/RagMcpServer.UnitTests/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create configuration data structures and prepare the project for DI changes.

- [ ] T001 Create `AIConfig`, `AIServiceConfig`, and `TextEmbeddingConfig` classes in `src/RagMcpServer/Configuration/AIConfig.cs` per `data-model.md`
- [ ] T002 [P] Update `src/RagMcpServer/appsettings.json` with the new `AI` section structure (defaulting to Ollama)
- [ ] T003 [P] Create unit test for configuration binding in `tests/RagMcpServer.UnitTests/Configuration/AIConfigTests.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Refactor Program.cs to support conditional service registration.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T004 Configure `IOptions<AIConfig>` binding in `src/RagMcpServer/Program.cs`
- [ ] T005 Create a `ServiceFactory` or extension method in `src/RagMcpServer/Extensions/ServiceCollectionExtensions.cs` to handle conditional registration logic (placeholder for now)
- [ ] T006 [P] Verify application startup still works with existing hardcoded services (regression check)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Configure LLM Provider (Priority: P1) 🎯 MVP

**Goal**: Enable switching LLM providers via configuration.

**Independent Test**: Change `Provider` to "OpenAI" (mocked/LiteLLM) and verify correct service is injected.

### Tests for User Story 1

- [ ] T007 [US1] Create unit tests for LLM service registration logic in `tests/RagMcpServer.UnitTests/Services/ChatCompletionRegistrationTests.cs`

### Implementation for User Story 1

- [ ] T008 [P] [US1] Refactor `OllamaChatCompletionService.cs` in `src/RagMcpServer/Services/` to accept `IOptions<AIConfig>` and use configured values
- [ ] T009 [P] [US1] Ensure `OpenAIChatCompletionService` (from Semantic Kernel) is available or add package `Microsoft.SemanticKernel.Connectors.OpenAI` if missing
- [ ] T010 [US1] Update `src/RagMcpServer/Program.cs` (or extension method) to switch between `OllamaChatCompletionService` and `OpenAIChatCompletionService` based on `AI:TextGeneration:Provider`
- [ ] T011 [US1] Validate startup with "Ollama" provider configured (default)
- [ ] T012 [US1] Validate startup with "OpenAI" provider configured (manual verification or log check)

**Checkpoint**: LLM provider switching is functional.

---

## Phase 4: User Story 2 - Configure Embedding Provider (Priority: P2)

**Goal**: Enable independent switching of Embedding providers.

**Independent Test**: Configure LLM=Ollama, Embedding=OpenAI and verify.

### Tests for User Story 2

- [ ] T013 [US2] Create unit tests for Embedding service registration logic in `tests/RagMcpServer.UnitTests/Services/EmbeddingRegistrationTests.cs`

### Implementation for User Story 2

- [ ] T014 [P] [US2] Refactor `OllamaEmbeddingService.cs` in `src/RagMcpServer/Services/` to accept `IOptions<AIConfig>` and use configured values
- [ ] T015 [US2] Update `src/RagMcpServer/Program.cs` (or extension method) to switch `ITextEmbeddingGenerationService` implementation based on `AI:TextEmbedding:Provider`
- [ ] T016 [US2] Verify independent configuration works (e.g., mix providers)

**Checkpoint**: Both LLM and Embedding services are independently configurable.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Documentation and final cleanup.

- [ ] T017 [P] Update `README.md` with new configuration instructions (referencing `quickstart.md`)
- [ ] T018 Verify `quickstart.md` instructions against the final implementation
- [ ] T019 [P] Add basic validation logic (e.g. check for empty Endpoint) in `Program.cs` or Config classes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup.
- **User Stories (Phase 3+)**: Depend on Foundational. US1 and US2 can technically run in parallel, but US1 is MVP.
- **Polish**: Depends on all stories.

### Parallel Opportunities

- T002 (appsettings) and T003 (Config Tests) can run parallel to T001.
- T008 (Refactor Chat Service) and T014 (Refactor Embedding Service) are independent.
- T009 (Package check) is independent.

## Implementation Strategy

### MVP First (User Story 1)

1. Define Config (Phase 1).
2. Setup DI logic (Phase 2).
3. Implement LLM switching (Phase 3).
4. Test LLM switching.

### Full Feature

1. Complete MVP.
2. Implement Embedding switching (Phase 4).
3. Final Polish.
