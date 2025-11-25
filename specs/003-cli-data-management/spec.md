# Feature Specification: CLI Data Management

**Feature Branch**: `003-cli-data-management`  
**Created**: 2025-11-25  
**Status**: Draft  
**Input**: User description: "建立一個CLI，提供匯入資料、檢索資料以及查詢當前的向量資料庫資訊的功能。"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Import Documents from Local Directory (Priority: P1)

As an administrator, I want to import a collection of documents from a specific local directory into the system so that they can be processed and indexed for future retrieval.

**Why this priority**: Without data, the system cannot function. Mass import is the most basic requirement for populating the RAG system.

**Independent Test**: Can be fully tested by pointing the CLI to a test directory containing text files and verifying via the "Info" command that the document count has increased.

**Acceptance Scenarios**:

1. **Given** a local directory containing supported text files (e.g., .txt, .md), **When** the user runs the import command providing the directory path, **Then** the CLI should read the files and send them to the server for ingestion, displaying a progress indicator or summary of success/failure counts.
2. **Given** a directory path that does not exist, **When** the user runs the import command, **Then** the CLI should display a clear error message indicating the path is invalid.
3. **Given** a directory with some unsupported file types, **When** the user runs the import command, **Then** the CLI should skip unsupported files and report them in the final summary without crashing.

---

### User Story 2 - Search/Retrieve Data (Priority: P2)

As a user, I want to query the system using natural language from the command line so that I can quickly verify if the system retrieves relevant information from the imported documents.

**Why this priority**: This validates the core value proposition of the RAG system (retrieval) and allows users to interact with the data.

**Independent Test**: Can be tested by running a search command with a known term present in the imported data and asserting that the output contains the expected text segments.

**Acceptance Scenarios**:

1. **Given** the system has indexed documents, **When** the user runs a search command with a query string, **Then** the CLI should display a list of relevant results, including the source file name and the relevant text snippet.
2. **Given** the system is empty or no matches are found, **When** the user runs a search command, **Then** the CLI should indicate that no results were found.

---

### User Story 3 - View Vector Database Status (Priority: P3)

As an administrator, I want to check the current status of the vector database (specifically the number of indexed documents) so that I can confirm that imports were successful and monitor the system state.

**Why this priority**: Provides visibility into the system's state, essential for debugging and verification.

**Independent Test**: Can be tested by running the info command and comparing the output against the known number of imported files.

**Acceptance Scenarios**:

1. **Given** a running system, **When** the user runs the info/status command, **Then** the CLI should display the total count of documents currently indexed in the vector database.
2. **Given** the server is unreachable, **When** the user runs the info command, **Then** the CLI should report a connection error.

### Edge Cases

- What happens when the API server is offline? The CLI should timeout gracefully and show a friendly "Connection refused" error, not a stack trace.
- How does the system handle very large files during import? The CLI should probably handle them normally, assuming the server has logic for chunking, but should report any server-side errors (e.g., 413 Payload Too Large) clearly.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a command-line interface executable or script.
- **FR-002**: System MUST allow users to specify a source directory path for bulk document import.
- **FR-003**: System MUST iterate through files in the specified directory and send them to the ingestion API.
- **FR-004**: System MUST filter for supported file extensions (e.g., .txt, .md, .pdf) during import.
- **FR-005**: System MUST provide a search command that accepts a text query string.
- **FR-006**: System MUST format and display search results including score (if available), source filename, and content preview.
- **FR-007**: System MUST provide a status/info command that retrieves the total document count from the server.
- **FR-008**: System MUST display a summary after an import operation (Total found, Success, Failed).

### Non-Functional Requirements

- **NFR-001 (Usability)**: The CLI must provide help text (`--help`) explaining available commands and arguments.
- **NFR-002 (Performance)**: The search command should display results within 2 seconds of receiving the response from the server.
- **NFR-003 (Reliability)**: The CLI must not crash on individual file read errors during import; it should log the error and continue with the next file.

### Key Entities

- **Document**: Represents a file on the local disk being imported.
- **SearchResult**: Represents a single match returned from the vector database, containing content and metadata.
- **CollectionStats**: Represents metadata about the vector store, such as the total count of embeddings/documents.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can trigger an import of 50 text files and receive a completion summary in under 1 minute (assuming local server).
- **SC-002**: Search results are displayed to the user in a readable format (source + snippet).
- **SC-003**: The "Info" command accurately reflects the number of successfully imported documents.
- **SC-004**: 100% of network connection errors are handled with a user-friendly error message instead of a raw exception dump.