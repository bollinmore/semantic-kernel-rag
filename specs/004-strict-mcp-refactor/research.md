# Research: Strict MCP Refactor

## Decision Log

### 1. MCP Protocol Implementation in C#
- **Decision**: Implement a lightweight, zero-dependency MCP JSON-RPC handler using `System.Text.Json` and `Console.OpenStandardInput/Output`.
- **Rationale**: There is no official "Microsoft.Mcp" NuGet package yet. Implementing the protocol from scratch ensures full control and avoids external dependencies that might not match the strict "stdio" requirement or the specific MCP version.
- **Alternatives**:
  - *Search for community NuGet packages*: Risk of low quality or abandonment.
  - *Use generic JSON-RPC libraries*: Often tied to HTTP/WebSockets, adding unnecessary bloat for a stdio use case.

### 2. Client-Server Architecture
- **Decision**: `RagMcpClient` (formerly CLI) will be the parent process that spawns `RagMcpServer` as a child process.
- **Rationale**: Matches the standard MCP model where the host application (Client) manages the lifecycle of the context provider (Server).
- **Communication**: Standard Input/Output (stdio).
  - Server reads from `stdin` (Client's output) and writes to `stdout` (Client's input).
  - Logs must be written to `stderr` to avoid corrupting the JSON-RPC channel.

### 3. LLM Integration Location
- **Decision**: Shift all LLM "Chat" logic to the Client.
- **Rationale**: 
  - **Server**: Becomes a "dumb" vector database wrapper. It takes text, embeds it (via external API), and stores/retrieves it. It knows nothing about "answering questions".
  - **Client**: Holds the "system prompt", manages conversation history, and decides how to use the retrieved chunks.

### 4. Vector Store Persistence
- **Decision**: Continue using `SqliteDbService`.
- **Rationale**: Existing, working code. Fits the "local/embedded" nature of the project.

### 5. Embedding Generation
- **Decision**: Server calls external Embedding API (e.g., Ollama).
- **Rationale**: Server generates embeddings for `Inject` (storage) and `Query` (search). It acts as a self-contained memory unit.

## Unknowns & Risks

- **Risk**: `Console.WriteLine` in the Server accidentally writing to `stdout` and breaking JSON-RPC.
  - **Mitigation**: Configure Serilog in Server to write ONLY to `stderr` or a file.
- **Risk**: Large payloads (documents) over stdio.
  - **Mitigation**: Ensure `Console.OpenStandardInput()` buffer is read correctly in loops, not just `ReadLine()` which might hit length limits. Use standard stream reading.

## Implementation Details

### JSON-RPC Message Structure (MCP)

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "Inject",
    "arguments": {
      "text": "...",
      "metadata": { ... }
    }
  }
}
```

**Response**:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Injection successful"
      }
    ]
  }
}
```
