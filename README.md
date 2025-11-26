# RAG MCP Server

This project implements a Retrieval-Augmented Generation (RAG) system using the Microsoft Semantic Kernel framework. It is implemented as a **Model Context Protocol (MCP)** Server that runs over standard input/output (stdio), making it compatible with MCP clients like `RagMcpClient` or AI assistants like Cline.

## Prerequisites

- .NET 8 SDK
- Ollama (for local LLM and Embeddings)
- Docker (optional, for containerization)

## Architecture

- **RagMcpServer**: The core server that implements the MCP protocol. It manages the Vector Database (SQLite) and handles document ingestion and retrieval. It runs as a local process.
- **RagMcpClient**: A Command-Line Interface (CLI) tool that acts as an MCP Client. It spawns the Server process, manages the connection, and provides user-friendly commands for injecting documents and querying.

## Quickstart

### 1. Build the Project

```bash
dotnet build
```

### 2. Use the CLI

Navigate to the Client directory and run commands:

```bash
cd src/RagMcpClient
dotnet run -- help
```

See [README-CLI.md](./README-CLI.md) for detailed CLI usage.

### 3. Use with Cline

You can configure Cline to use the `RagMcpServer` directly. See [README-MCP.md](./README-MCP.md) for instructions.

## Configuration

The server uses `appsettings.json` for configuration (e.g., LLM endpoints, Vector DB settings).
See [AI Configuration Guide](./specs/002-configure-llm-models/quickstart.md) for details.

## Project Structure

- `src/RagMcpServer/`: The MCP Server implementation (C#).
- `src/RagMcpClient/`: The CLI Client implementation (C#).
- `tests/`: Unit and integration tests.
- `specs/`: Feature specifications and design documents.