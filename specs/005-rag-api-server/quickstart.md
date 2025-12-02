# Quickstart: RAG API Server

## Prerequisites
- .NET 10.0 SDK installed.
- Valid `appsettings.json` configured with your LLM/Embedding settings.
- Populated vector database (run the ingestion tool or MCP server previously).

## Running the Server

1. **Navigate to the server directory**:
   ```bash
   cd src/RagMcpServer
   ```

2. **Start the application**:
   The application now runs both the MCP server (stdio) and the API server (HTTP).
   ```bash
   dotnet run
   ```
   *Note: The API server listens on port 5000 (HTTP) by default. Check console output (stderr) for exact URL.*

## Using the API

### Swagger UI
Open your browser to: `http://localhost:5000/swagger`
You can interactively test the endpoints here.

### Example Search Request (cURL)

**Endpoint**: `POST /api/rag/collections/default/search`

```bash
curl -X POST "http://localhost:5000/api/rag/collections/default/search" \
     -H "Content-Type: application/json" \
     -d 
{
           "query": "How do I configure the server?",
           "top_k": 3,
           "min_score": 0.5
         }
```

### Configuration
Update `appsettings.json` to change the binding port or database settings:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  },
  "AI": {
    "VectorDbPath": "rag_vectors.db"
  }
}
```

