# RagMcpServer CLI

The `RagMcpServer.CLI` is a command-line interface tool for managing documents and interacting with the RAG (Retrieval-Augmented Generation) server.

## Prerequisites

- .NET 8.0 SDK or later
- A running instance of `RagMcpServer`

## Installation / Building

Navigate to the CLI project directory:

```bash
cd src/RagMcpServer.CLI
dotnet build
```

## Global Options

- `-h, --help`: Show help and usage information.
- `-s, --server <URL>`: Specify the API server URL (Default: `http://localhost:5228`).

## Commands

### `inject`

Imports documents from a local directory into the vector database.

**Usage:**

```bash
dotnet run -- inject -p <PATH> [OPTIONS]
```

**Options:**

- `-p, --path <PATH>`: (Required) Path to the directory containing documents (`.txt`, `.md`) to import.
- `-s, --server <URL>`: URL of the API server.

**Example:**

```bash
dotnet run -- inject -p "C:/Documents/KnowledgeBase"
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

- `-s, --server <URL>`: URL of the API server.

**Example:**

```bash
dotnet run -- query "How do I configure the server?"
```

### `info`

Displays status information about the server and vector database.

**Usage:**

```bash
dotnet run -- info [OPTIONS]
```

**Options:**

- `-v, --vector_db`: Display detailed vector database statistics (Collection Name, Document Count, Provider).
- `-s, --server <URL>`: URL of the API server.

**Example:**

```bash
dotnet run -- info -v
```

### `help`

To see available commands:

```bash
dotnet run -- help
```

To see help for a specific command (native flags):

```bash
dotnet run -- [command] --help
# Example:
dotnet run -- inject --help
```
