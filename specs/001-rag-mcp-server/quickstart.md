# Quickstart: RAG MCP Server

**Date**: 2025-11-22
**Feature**: RAG MCP Server for Document Extraction

This guide provides the basic steps to set up the necessary dependencies and run the RAG MCP Server.

## Prerequisites

1.  **.NET 8 SDK**: [Installation Guide](https://dotnet.microsoft.com/download/dotnet/8.0)
2.  **Docker**: [Installation Guide](https://docs.docker.com/get-docker/)
3.  **Ollama**: [Installation Guide](https://ollama.ai/)
4.  **A command-line tool for making HTTP requests**, such as `curl` or Postman.

## Step 1: Set up Dependencies



### Run the Embedding Model
This project uses the `nomic-embed-text` model, served by Ollama.

**Note**: The vector database is now SQLite-based. A file named `rag.db` will be created in the application's working directory upon first ingestion if it doesn't already exist. No separate database server is required.

1.  **Pull the model:**
    ```bash
    ollama pull nomic-embed-text
    ```

2.  **Run the Ollama server:** (This will typically run in the background after installation)
    ```bash
    ollama serve
    ```

## Step 2: Configure and Run the Server

1.  **Navigate to the source directory:**
    ```bash
    cd src/RagMcpServer
    ```

2.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

3.  **Run the application:**
    ```bash
    dotnet run
    ```
    The server will start and listen on the configured port (e.g., `http://localhost:5000`).

## Step 3: Test the API

### Ingest Documents
Send a `POST` request to the `/documents` endpoint to start the ingestion process.

```bash
curl -X POST -H "Content-Type: application/json" \
  -d "{\"path\": \"/path/to/your/documents\"}" \
  http://localhost:5228/documents
```

### Query the System
Send a `POST` request to the `/query` endpoint to ask a question.

```bash
curl -X POST -H "Content-Type: application/json" \
  -d "{\"query\": \"What is the main topic of the ingested documents?\"}" \
  http://localhost:5228/query
```
