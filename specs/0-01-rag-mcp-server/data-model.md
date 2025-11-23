# Data Model: RAG MCP Server

**Date**: 2025-11-22
**Feature**: RAG MCP Server for Document Extraction
**Spec**: [spec.md](./spec.md)

This document defines the key data entities for the RAG MCP Server, based on the feature specification.

## Entity: Document

Represents a single file ingested into the system.

| Field Name     | Type     | Description                                     |
|----------------|----------|-------------------------------------------------|
| `id`           | string   | A unique identifier for the document.           |
| `sourcePath`   | string   | The original file path of the document.         |
| `content`      | string   | The full text content extracted from the file.  |
| `status`       | string   | The processing status (e.g., `Pending`, `Processed`, `Failed`). |
| `lastModified` | datetime | The timestamp of when the document was last processed. |

## Entity: Knowledge Chunk

Represents a segment of text extracted from a `Document` that is suitable for embedding and indexing.

| Field Name      | Type     | Description                                           |
|-----------------|----------|-------------------------------------------------------|
| `id`            | string   | A unique identifier for the chunk.                    |
| `documentId`    | string   | Foreign key linking to the parent `Document`.         |
| `content`       | string   | The text content of the chunk.                        |
| `embedding`     | float[]  | The vector embedding of the `content`.                |
| `metadata`      | object   | Additional metadata (e.g., source document, page number). |

## Relationships

- A `Document` can be broken down into one or more `Knowledge Chunks`.
- Each `Knowledge Chunk` belongs to exactly one `Document`.
