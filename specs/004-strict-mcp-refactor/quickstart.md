# Quickstart: RagMcp Refactor

## Prerequisites

- .NET 8 SDK
- Ollama running locally (default: `http://localhost:11434`) with an embedding model (e.g., `nomic-embed-text`) and a chat model (e.g., `llama3.1`).

## Build

1. **Build the Server**:
   ```bash
   dotnet build src/RagMcpServer/RagMcpServer.csproj
   ```

2. **Build the Client**:
   ```bash
   dotnet build src/RagMcpClient/RagMcpClient.csproj
   ```

## Running

1. **Test Connection**:
   Verify the Client can spawn and talk to the Server.
   ```bash
   dotnet run --project src/RagMcpClient/RagMcpClient.csproj -- test-connection
   ```
   *Note: If the server is not found automatically, use `--server-path` to point to `src/RagMcpServer/bin/Debug/net8.0/RagMcpServer.exe` (or without .exe on Mac/Linux).*

2. **Inject Data**:
   Ingest documents from a folder.
   ```bash
   dotnet run --project src/RagMcpClient/RagMcpClient.csproj -- inject ./data/documents
   ```

3. **Query (RAG)**:
   Ask a question based on ingested data.
   ```bash
   dotnet run --project src/RagMcpClient/RagMcpClient.csproj -- query "What is the meaning of life?"
   ```

## Configuration

- **Client**: 
  - `src/RagMcpClient/appsettings.json`: Configure LLM (Ollama) endpoint and model.
  - `--server-path`: Command line argument to specify custom path to Server executable.

- **Server**: 
  - `src/RagMcpServer/appsettings.json`: Configure Embedding provider (Ollama) and SQLite connection string.
  - `RAG_DB_PATH`: Environment variable (optional) to override DB location.