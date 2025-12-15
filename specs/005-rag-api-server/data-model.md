# Data Model: RAG API Server

## Entities

### SearchRequest
Represents a semantic search query submitted by a client.

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| query | string | Yes | - | The semantic query text. |
| top_k | int | No | 5 | The number of top results to return. |
| min_score | float | No | 0.0 | Minimum similarity score (0.0 to 1.0). |

*Note: `collection_name` is passed as a URL path parameter, not a body field, per spec clarification.*

### SearchResult
The response container for search operations.

| Field | Type | Description |
|-------|------|-------------|
| count | int | The total number of results returned. |
| results | List<ResultItem> | The list of matching document chunks. |

### ResultItem
A single document chunk returned from the search.

| Field | Type | Description |
|-------|------|-------------|
| content | string | The text content of the chunk. |
| score | float | The cosine similarity score (0.0 to 1.0). |
| metadata | Metadata | Contextual information about the chunk. |

### Metadata
Context and provenance information for a chunk.

| Field | Type | Description |
|-------|------|-------------|
| page_number | int | The page number in the original source document. |
| chunk_index | int | The sequence index of the chunk in the document. |
| source | string | The file path or identifier of the source document. |

## Validation Rules

- **SearchRequest**:
  - `query`: Must not be empty. Max length 4000 characters.
  - `top_k`: Must be > 0 and <= 100.
  - `min_score`: Must be >= 0.0 and <= 1.0.

## Database Mapping (Existing)
- The entities above map to the existing `InfoModels.DocumentChunk` and internal vector DB representations, but are decoupled DTOs for the API surface.
