# Quickstart: RagMcp Refactor

## Prerequisites

- .NET 8 SDK
- Ollama running locally (default: `http://localhost:11434`) with an embedding model (e.g., `nomic-embed-text`) and a chat model (e.g., `llama3`).

## Build

1. **Build the Server**:
   ```bash
   dotnet build src/RagMcpServer/RagMcpServer.csproj
   ```

2. **Build the Client**:
   ```bash
   dotnet build src/RagMcpServer.CLI/RagMcpServer.CLI.csproj
   ```
   *(Note: Project name might change to `RagMcpClient` per plan)*

## Running

1. **Start the Client**:
   The Client will automatically spawn the Server.
   ```bash
   dotnet run --project src/RagMcpServer.CLI/RagMcpServer.CLI.csproj -- query "What is the meaning of life?"
   ```

2. **Inject Data**:
   ```bash
   dotnet run --project src/RagMcpServer.CLI/RagMcpServer.CLI.csproj -- inject ./data/documents
   ```

## Configuration

- **Client**: Needs to know the path to the Server executable if not standard.
  - Can be set via environment variable `RAG_SERVER_PATH` or command line arg.
- **Server**: Needs to know the Ollama URL.
  - Set via `appsettings.json` or environment variables `AI:Ollama:Endpoint`.
