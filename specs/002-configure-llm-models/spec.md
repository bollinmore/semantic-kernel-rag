# Feature Specification: Configure LLM and Embedding Models

**Feature Branch**: `002-configure-llm-models`  
**Created**: 2025-11-24  
**Status**: Draft  
**Input**: User description: "提供設定LLM以及embeddings模型的機制，在啟動服務前，讀取設定檔的設定來決定要怎麼啟用。"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Configure LLM Provider (Priority: P1)

As a System Administrator, I want to configure the LLM provider and model details in the application settings so that I can control which AI model the application uses without modifying the code.

**Why this priority**: This is the core functionality requested. Without this, the application is hardcoded to a specific setup.

**Independent Test**: Verify that changing the configuration settings changes the behavior/model used by the application.

**Acceptance Scenarios**:

1. **Given** the application is stopped and configured to use "Ollama" with model "llama3", **When** the application starts, **Then** the Chat Completion Service is initialized using the Ollama implementation with "llama3".
2. **Given** the application is configured with an invalid or unsupported provider, **When** the application starts, **Then** it logs an error and terminates (or fails to start up the service container).

---

### User Story 2 - Configure Embedding Provider (Priority: P2)

As a System Administrator, I want to configure the Embedding provider independently from the LLM provider so that I can mix and match services (e.g., local embeddings with cloud LLM) to optimize for performance and cost.

**Why this priority**: Semantic search relies heavily on embeddings. Being able to configure this independently is crucial for flexibility.

**Independent Test**: Configure LLM to Provider A and Embeddings to Provider B, and verify both initialize correctly.

**Acceptance Scenarios**:

1. **Given** the configuration specifies "Ollama" for Embeddings, **When** the application starts, **Then** the Embedding Generation Service is initialized using the Ollama implementation.
2. **Given** the configuration differs from the LLM provider, **When** the application starts, **Then** the Embedding service uses its own specified provider, distinct from the LLM service.

---

### Edge Cases

- What happens when the configuration file is missing key properties (e.g., Endpoint)? -> The system should throw a configuration validation exception at startup.
- What happens when the configured model is not available at the endpoint? -> The service might initialize, but runtime requests will fail. (The scope here is primarily *configuration* and *initialization*, not runtime health checking, though immediate validation is a plus).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST read AI service configuration from the standard configuration source (e.g., settings file, Environment Variables).
- **FR-002**: System MUST allow defining a "Provider" type for the LLM service (e.g., "Ollama", "OpenAI").
- **FR-003**: System MUST allow defining a "Provider" type for the Embedding service (e.g., "Ollama", "OpenAI").
- **FR-004**: System MUST allow configuring specific parameters for each service: Model ID, Endpoint URL, and API Key (if applicable).
- **FR-005**: System MUST initialize the appropriate implementation of the Chat Completion Service based on the configured LLM provider.
- **FR-006**: System MUST initialize the appropriate implementation of the Embedding Generation Service based on the configured Embedding provider.
- **FR-007**: System MUST validate that the required configuration for the selected provider is present at startup.

### Key Entities *(include if feature involves data)*

- **AIConfig**: Root configuration object containing sections for `TextGeneration` (LLM) and `TextEmbedding`.
- **ServiceSettings**: Configuration class containing `Provider` (enum/string), `ModelId` (string), `Endpoint` (Uri), and `ApiKey` (string/secret).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Administrator can switch the active LLM implementation from one provider to another by modifying the configuration and restarting the service, verifying the change via logs or behavior within 1 minute.
- **SC-002**: System successfully initializes with valid configuration for both Ollama and a secondary option (like OpenAI or a mocked alternative) without code changes.
- **SC-003**: System reports a specific configuration error message to the log output if required settings (like Endpoint for Ollama) are missing.