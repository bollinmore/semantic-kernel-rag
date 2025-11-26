# Data Model: Strict MCP Refactor

## Core Entities

### Document
Represents the raw information provided by the user.

- **Id** (string): Unique identifier (UUID).
- **Text** (string): The content of the document.
- **Metadata** (dictionary): Arbitrary key-value pairs (e.g., filename, source, date).
- **CreatedAt** (datetime): Timestamp of ingestion.

### Embedding
The vector representation of a text chunk.

- **Id** (string): Unique identifier.
- **Vector** (float[]): The high-dimensional vector.
- **TextChunk** (string): The specific segment of text this vector represents.
- **DocumentId** (string): Reference to the parent Document.

## MCP Protocol Entities

### McpRequest
- **jsonrpc** (string): "2.0"
- **id** (int/string): Message ID.
- **method** (string): Method name (e.g., "initialize", "tools/list", "tools/call").
- **params** (object): Method-specific parameters.

### McpResponse
- **jsonrpc** (string): "2.0"
- **id** (int/string): Message ID matching the request.
- **result** (object): Successful result data.
- **error** (object): Error details (code, message) if failed.

### Tool Definition
- **name** (string): Unique name (e.g., "Inject", "Query").
- **description** (string): Human-readable description for the LLM.
- **inputSchema** (object): JSON Schema defining valid arguments.

## Database Schema (SQLite)

### Table: `vectors`
| Column | Type | Constraints |
|--------|------|-------------|
| id | TEXT | PRIMARY KEY |
| text | TEXT | NOT NULL |
| metadata | TEXT | JSON string |
| vector | BLOB | Binary representation of float[] |
