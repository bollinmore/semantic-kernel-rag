# RagMcpServer CLI

The `RagMcpServer.CLI` is a command-line interface tool for managing documents and interacting with the RAG (Retrieval-Augmented Generation) server.

The CLI acts as an **MCP Client**. It automatically spawns the `RagMcpServer` process locally and communicates with it via standard input/output (stdio). You do **not** need to start the server separately.

> **Note**: For HTTP REST API access with Swagger UI, see the [API Server Quickstart](../specs/005-rag-api-server/quickstart.md).

## Prerequisites

- .NET 8.0 SDK or later
- Ollama running (for the Server to access Embeddings/LLMs)

## Installation / Building

Navigate to the CLI project directory:

```bash
cd src/RagMcpClient
dotnet build
```

## Global Options

- `-h, --help`: Show help and usage information.
- `-v, --version`: Show version information.

## Commands

### `inject`

Imports documents from a local directory into the vector database.

**Usage:**

```bash
dotnet run -- inject <PATH> [OPTIONS]
```

**Arguments:**

- `<PATH>`: (Required) Path to the directory containing documents (`.txt`, `.md`) to import.

**Options:**

- `-s, --server-path <PATH>`: Optional path to the `RagMcpServer` executable. If omitted, the CLI attempts to auto-detect it in standard build output directories.

**Example:**

```bash
dotnet run -- inject "C:/Documents/KnowledgeBase"
```

### `query`

Asks a question to the RAG system using natural language.

**Usage:**

```bash
dotnet run -- query <QUERY> [OPTIONS]
```

**Arguments:**

- `<QUERY>`: The question or query text.

**Options:**

- `-s, --server-path <PATH>`: Optional path to the `RagMcpServer` executable.

**Example:**

```bash
dotnet run -- query "How do I configure the server?"
```

### `info`

Displays status information about the server and vector database. (Note: Ensure this command is implemented in the latest version).

### `test-connection`

Verifies that the CLI can successfully spawn and handshake with the MCP Server.

**Usage:**

```bash
dotnet run -- test-connection
```

### `help`

To see available commands:

```bash
dotnet run -- help
```

To see help for a specific command:

```bash
dotnet run -- [command] --help
# Example:
dotnet run -- inject --help
```