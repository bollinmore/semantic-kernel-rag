# How to Use RagMcpServer with Cline

This project exposes a RAG (Retrieval-Augmented Generation) system as an **MCP (Model Context Protocol) Server** that Cline can use to query your internal documents.

Because the core logic is an ASP.NET Core Web API, we use a lightweight **Node.js Proxy** to bridge Cline (Stdio) and the running C# Server (HTTP).

## Prerequisites

1.  **.NET 8.0 SDK** (for the backend)
2.  **Node.js** (v18+ recommended)

## Setup Steps

### 1. Start the C# Backend
The MCP server proxy requires the backend API to be running to process requests.

```bash
cd src/RagMcpServer
dotnet run
```
*Keep this terminal window open. The server will listen on `http://localhost:5228`.*

### 2. Configure Cline
Edit your Cline MCP Settings file (typically found at `%APPDATA%\Code\User\globalStorage\saoudrizwan.claude-dev\settings\cline_mcp_settings.json` on Windows or `~/.config/Code/...` on Mac/Linux).

Add the following entry to the `mcpServers` object:

```json
"rag-mcp-server": {
  "command": "node",
  "args": [
    "C:\\Users\\alvin.chen\\Documents\\Insyde\\Proj\\bollinmore\\semantic-kernel-rag\\src\\RagMcpServer.Proxy\\index.js"
  ],
  "disabled": false,
  "autoApprove": []
}
```
**Note:** You must update the path in `args` to match the actual absolute path to the `src/RagMcpServer.Proxy/index.js` file on your machine.

### 3. Install Proxy Dependencies (First Time Only)
If you haven't already:
```bash
cd src/RagMcpServer.Proxy
npm install
```

## Usage

Once configured, reload Cline (or restart VS Code). You should see the following tools available:

-   **`ingest_documents`**: Provide a file or folder path to index documents.
    -   *Example:* "Please ingest the documents in C:\MyDocs"
-   **`query_documents`**: Ask questions about the ingested content.
    -   *Example:* "What does the internal documentation say about project X?"

## Troubleshooting

-   **"Error: Could not connect..."**: Ensure the `dotnet run` process is still active and listening on port 5228.
-   **No tools showing up**: Check Cline's "MCP Servers" tab for connection errors or logs.
