# Feature Specification: Strict MCP Refactor

**Feature Branch**: `004-strict-mcp-refactor`
**Created**: 2025-11-25
**Status**: Draft
**Input**: User description: "重構本專案，使 Server & Client 遵循嚴格的 MCP 規範。 Server 端只提供 Inject, Query 工具、處理向量訊息。 Client 端要負責啟動 Server, 處理 LLM 連線，把訊息回應給使用者。"

## Clarifications

### Session 2025-11-25
- Q: How should the Client handle Server process crashes? → A: The Client MUST detect the disconnection, attempt to gracefully restart the Server, and notify the user of the recovery.
- Q: What type of persistent store should the Server use for vector data? → A: SQLite.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Client Connects to Server (Priority: P1)

As a Client Application, I want to launch the Server process and establish an MCP connection so that I can discover and use the available tools.

**Why this priority**: Foundation for all other interactions. Without a working MCP connection, no features work.

**Independent Test**: Can be tested by running the Client, which spawns the Server. The Client logs should show a successful MCP initialization handshake and a list of discovered tools (`Inject`, `Query`) without errors.

**Acceptance Scenarios**:

1. **Given** the Server executable is built, **When** the Client starts, **Then** it spawns the Server process via stdio.
2. **Given** the connection is established, **When** the Client requests a list of tools, **Then** the Server responds with `Inject` and `Query`.

---

### User Story 2 - User Injects Data (Priority: P2)

As a User, I want to provide text documents to the Client so that they are stored in the Server's vector database for future retrieval.

**Why this priority**: Critical for the RAG (Retrieval Augmented Generation) functionality.

**Independent Test**: Can be tested by running a command in the Client to add a document. The Server logs/database should show the new record with its vector embedding.

**Acceptance Scenarios**:

1. **Given** a running Client-Server pair, **When** the User inputs a command to add a document, **Then** the Client calls the `Inject` tool on the Server.
2. **Given** an `Inject` call, **When** the Server receives the text, **Then** it generates an embedding and saves the text + vector to the database.

---

### User Story 3 - User Asks Question (RAG) (Priority: P1)

As a User, I want to ask a question and receive an answer based on my stored documents.

**Why this priority**: The primary value proposition of the system (RAG).

**Independent Test**: Can be tested by asking a question about previously injected specific data. The answer should reflect knowledge of that data.

**Acceptance Scenarios**:

1. **Given** a user question, **When** the Client processes it, **Then** it (or the LLM it orchestrates) decides to call the `Query` tool on the Server.
2. **Given** a `Query` tool call, **When** the Server processes it, **Then** it performs a vector similarity search and returns relevant text chunks.
3. **Given** relevant text chunks, **When** the Client receives them, **Then** it sends the question + chunks to the LLM and displays the final answer to the user.

---

### Edge Cases

- What happens when the Server process crashes? The Client MUST detect the disconnection, attempt to gracefully restart the Server, and notify the user of the recovery.
- What happens when the LLM is unavailable? The Client should report an error to the user, but the Server (vector DB) remains unaffected.
- What happens if `Query` returns no results? The Client/LLM should answer based on general knowledge or state it doesn't know.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The Server MUST implement the Model Context Protocol (MCP) over stdio.
- **FR-002**: The Server MUST expose an `Inject` tool that accepts text content and optional metadata.
- **FR-003**: The Server MUST expose a `Query` tool that accepts a query string and an optional limit count.
- **FR-004**: The Server MUST handle text embedding generation by delegating to an external embedding model API (e.g., Ollama or a similar service) for both `Inject` and `Query` tools.
- **FR-005**: The Server MUST NOT manage conversational LLM contexts or make calls to chat completion endpoints (e.g., OpenAI Chat API, Ollama Chat).
- **FR-006**: The Client MUST be responsible for spawning the Server process and managing its lifecycle.
- **FR-007**: The Client MUST manage the connection to the LLM (e.g., Ollama, OpenAI) for chat completion.
- **FR-008**: The Client MUST handle the orchestration of: User Input -> Tool Call (if needed) -> Context Retrieval -> LLM Generation -> Response.
- **FR-009**: The Server MUST store vector data in a persistent store, specifically SQLite with vector search capabilities.

### Non-Functional Requirements

- **NFR-001 (Architecture)**: Strict separation of concerns: Server = Context/Memory Provider; Client = Intelligence/Orchestrator.
- **NFR-002 (Performance)**: Tool execution overhead (MCP transport) should be negligible (<50ms local latency).
- **NFR-003 (Compatibility)**: The protocol implementation must adhere to the standard MCP specification to ensure potential compatibility with other MCP clients.

### Key Entities

- **Document**: A unit of text information injected by the user.
- **Embedding**: A vector representation of the Document text.
- **Context Chunk**: A snippet of text retrieved from the vector store relevant to a query.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The Server codebase contains zero references to "Chat Completion" APIs or logic (only Embeddings).
- **SC-002**: The Client successfully performs a full RAG cycle (Input -> Query Tool -> LLM -> Output) in under 5 seconds for a standard query (assuming local LLM speed permits).
- **SC-003**: Standard MCP Inspector tools (if available) or a generic MCP client can connect to the Server and list its tools.