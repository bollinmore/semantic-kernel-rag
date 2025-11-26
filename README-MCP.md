# How to Use RagMcpServer with Cline

This project provides a RAG (Retrieval-Augmented Generation) system as a **Model Context Protocol (MCP) Server**. 

Since the server implements MCP over **stdio**, Cline can run the C# executable directly. No intermediate proxy is required.

## Prerequisites

1.  **.NET 8.0 SDK** (to build and run the server)
2.  **Ollama** (running locally for Embeddings/LLM support)

## Setup Steps

### 1. Build the Server

Ensure the server is built so that the executable exists.

```bash
cd src/RagMcpServer
dotnet build -c Debug
```

Note the path to the project or the built executable.
- Project Path: `.../src/RagMcpServer/RagMcpServer.csproj`
- Executable Path: `.../src/RagMcpServer/bin/Debug/net8.0/RagMcpServer.dll` (or `.exe` on Windows)

### 2. Configure Cline

Edit your Cline MCP Settings file (typically found at `%APPDATA%\Code\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json` on Windows or `~/.config/Code/...` on Mac/Linux).

Add an entry to the `mcpServers` object. You can run it using `dotnet run` (easiest for dev) or by executing the binary directly.

#### Option A: Using `dotnet run` (Recommended for Development)

```json
"rag-mcp-server": {
  "command": "dotnet",
  "args": [
    "run",
    "--project",
    "/ABSOLUTE/PATH/TO/src/RagMcpServer/RagMcpServer.csproj"
  ],
  "disabled": false,
  "autoApprove": []
}
```

#### Option B: Running the Binary Directly

```json
"rag-mcp-server": {
  "command": "dotnet",
  "args": [
    "/ABSOLUTE/PATH/TO/src/RagMcpServer/bin/Debug/net8.0/RagMcpServer.dll"
  ],
  "disabled": false,
  "autoApprove": []
}
```

*Replace `/ABSOLUTE/PATH/TO/...` with the actual absolute path on your machine.*

### 3. Restart Cline

Reload VS Code or restart Cline. You should see the `rag-mcp-server` connected in the MCP Servers tab.

## Available Tools

Once connected, the following tools will be available to Cline:

-   **`Inject`**: Add documents to the knowledge base.
    -   Arguments: `text` (string), `metadata` (object)
-   **`Query`**: Search the knowledge base.
    -   Arguments: `query` (string), `limit` (int, optional)

## Troubleshooting

-   **"Error: Could not connect..."**: 
    -   Check if the path in `args` is correct.
    -   Try running the command manually in your terminal to see if it starts without errors.
    -   Ensure `Ollama` is running, as the server might fail to start if dependencies are missing (check server logs).
-   **Logs**: You can check the "MCP Servers" output in VS Code to see the stderr logs from the C# server.