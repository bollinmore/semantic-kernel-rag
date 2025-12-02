# Feature Specification: Add RAG API Server

**Feature Branch**: `005-rag-api-server`  
**Created**: 2025-12-02  
**Status**: Draft  
**Input**: User description: "新增 RAG API Server 是為了建立一個專門的後端服務，讓外部應用程式能夠透過標準化的 HTTP 介面，從 RAG 資料庫中檢索相關文件區塊（chunks），並獲取帶有元資料的搜尋結果。這能實現 RAG 系統的模組化，讓資料提取邏輯獨立於前端或 LLM 應用，避免直接暴露資料庫連線，提升安全性與可維護性 。透過 appsettings.json 配置 RAG 資料庫資訊，確保部署時的靈活性，支持不同環境（如開發、生產）的快速切換 。​ 使用者故事： 作為 RAG 應用開發者，我希望有一個 API 端點接收查詢參數（如 collection_name、query、top_k），以從指定資料集合中提取最相關的內容區塊。 作為系統管理員，我需要透過配置檔設定資料庫連線，避免硬編碼，提高部署一致性。 作為 LLM 整合者，我期望回應包含 count、results 陣列，每個結果有 content 與 metadata（page_number、chunk_index、source），以便後續生成回應 。​"

## Clarifications

### Session 2025-12-02

- Q: Should the search results include a similarity score? → A: Yes, include score at the root of ResultItem.
- Q: Should the API support filtering search results by a minimum similarity score? → A: Yes, add optional `min_score` parameter.
- Q: How should the collection be specified in the API request? → A: Add a `collection_name` path parameter to the URL (e.g., `/api/rag/collections/{collection_name}/search`).
- Q: Should the API expose interactive documentation (e.g., Swagger UI)? → A: Yes, enable Swagger UI at `/swagger`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Retrieve Relevant Chunks (Priority: P1)

As a RAG Application Developer, I want to access an HTTP API endpoint that accepts search parameters (collection name, query text, top-k) so that I can retrieve the most relevant document chunks for my application.

**Why this priority**: This is the core functionality of the RAG API Server, enabling external applications to leverage the vector database.

**Independent Test**: Can be fully tested by sending HTTP requests to the endpoint with various queries and verifying the returned chunks match the expected content from the database.

**Acceptance Scenarios**:

1. **Given** a populated vector database and a running API server, **When** I send a POST request to `/api/rag/collections/{collection_name}/search` with valid `query` and `top_k` in the body, **Then** the system returns a 200 OK response containing a list of `top_k` most relevant chunks.
2. **Given** a URL with a non-existent `collection_name`, **When** I send the search request, **Then** the system returns a 404 Not Found or 400 Bad Request indicating the collection does not exist.
3. **Given** missing required parameters (e.g., no `query`), **When** I send the request, **Then** the system returns a 400 Bad Request.
4. **Given** a `min_score` parameter is provided, **When** the search is performed, **Then** only results with a similarity `score` greater than or equal to `min_score` are returned.

---

### User Story 2 - External Configuration (Priority: P1)

As a System Administrator, I want to configure the RAG database connection details via `appsettings.json` so that I can easily switch between development and production environments without modifying the code.

**Why this priority**: Essential for deployment flexibility and security (avoiding hardcoded secrets).

**Independent Test**: Can be tested by changing the connection string in `appsettings.json`, restarting the server, and verifying the application connects to the new database target.

**Acceptance Scenarios**:

1. **Given** the application is configured with Development database settings, **When** the application starts, **Then** it connects to the Development database.
2. **Given** the configuration is updated to Production database settings, **When** the application is restarted, **Then** it connects to the Production database.

### Edge Cases

- **Database Unavailable**: If the vector database is unreachable, the API returns a 500 Internal Server Error with a safe error message (no sensitive details).
- **Invalid Parameters**: If `top_k` is negative or `query` is empty, the API returns 400 Bad Request.
- **Non-existent Collection**: If `collection_name` path parameter does not exist, the API returns 404 Not Found or 400 Bad Request (depending on DB driver behavior), clearly indicating the error.
- **Large Query**: If the query text exceeds a reasonable limit (e.g., 4000 characters), the API returns 400 Bad Request.
- **`min_score` out of range**: If `min_score` is outside a valid range (e.g., < 0 or > 1), the API returns 400 Bad Request.
- **URL Encoding**: If `collection_name` contains special characters, it must be properly URL-encoded in the request.

---

### User Story 3 - Structured API Response (Priority: P2)

As an LLM Integrator, I want the API response to include the total count of results and a detailed array of items (content + metadata) so that I can accurately construct prompts and citations for the LLM.

**Why this priority**: Ensures the retrieved data is usable and provides necessary context (provenance) for the generated answers.

**Independent Test**: Inspect the JSON response payload to verify the structure and presence of `count`, `results`, `content`, `page_number`, `chunk_index`, and `source`.

**Acceptance Scenarios**:

1. **Given** a successful search request, **When** I receive the response, **Then** it contains a `count` integer field matching the number of items in `results`.
2. **Given** a result item, **When** I check its properties, **Then** it contains `content` (text), `metadata.page_number`, `metadata.chunk_index`, and `metadata.source`.
3. **Given** the API server is running, **When** I navigate to `/swagger`, **Then** I see the interactive API documentation (Swagger UI).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose an HTTP POST endpoint at `/api/rag/collections/{collectionName}/search` for semantic search.
- **FR-002**: The search endpoint MUST accept a JSON payload containing `query` (string), `top_k` (integer), and an optional `min_score` (float).
- **FR-003**: System MUST use the configured vector database service to execute the semantic search based on the input query and the collection specified in the URL path.
- **FR-004**: System MUST return a JSON response containing a `count` of results and a `results` array.
- **FR-005**: Each item in the `results` array MUST include the document `content`, a similarity `score` (float), and a `metadata` object.
- **FR-006**: The `metadata` object MUST include `page_number`, `chunk_index`, and `source` (file path).
- **FR-007**: System MUST read database configuration (e.g., connection string, provider) from `appsettings.json`.
- **FR-008**: System MUST support `includeSources` option (defaulting to required behavior) if aligned with existing patterns, or ensure metadata always includes source info as requested.
- **FR-009**: System MUST handle database connection errors gracefully and return a 500 Internal Server Error with a generic message (logging details internally).
- **FR-010**: If `min_score` is provided, the system MUST filter out results with a similarity `score` below the specified `min_score` before returning `top_k` results.
- **FR-011**: System MUST enable and expose Swagger UI at the `/swagger` endpoint.

### Non-Functional Requirements

- **NFR-001 (Security)**: The system MUST NOT expose raw database connection strings or internal paths in API error responses.
- **NFR-002 (Performance)**: The API SHOULD return results within 1 second for typical queries on the standard dataset size (under 100k chunks).
- **NFR-003 (Interoperability)**: The API MUST follow standard REST/JSON conventions.
- **NFR-004 (Configuration)**: Configuration changes in `appsettings.json` MUST NOT require code recompilation.

### Key Entities *(include if feature involves data)*

- **SearchRequest** (Body):
  - `query` (string)
  - `top_k` (int)
  - `min_score` (float, optional)
- **SearchResult**:
  - `count` (int)
  - `results` (List<ResultItem>)
- **ResultItem**:
  - `content` (string)
  - `score` (float)
  - `metadata` (Metadata)
- **Metadata**:
  - `page_number` (int)
  - `chunk_index` (int)
  - `source` (string)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of search requests with valid parameters return a valid JSON response with status 200.
- **SC-002**: The API response structure matches the defined schema (including all metadata fields) in 100% of successful tests.
- **SC-003**: Changing the database connection string in `appsettings.json` directs queries to the new database without code changes.
- **SC-004**: When `min_score` is provided, results returned always have a score greater than or equal to `min_score`.
- **SC-005**: Swagger UI is accessible at `/swagger` and accurately documents the RAG API endpoints.

## Assumptions

- **Authentication**: No API authentication (API Key, OAuth) is required for this MVP; the service is assumed to run in a trusted network or strictly for local/development use initially.
- **Database**: The system uses the existing SQLite vector database implementation.
- **Host**: The functionality will be added to the existing ASP.NET Core `RagMcpServer` application.