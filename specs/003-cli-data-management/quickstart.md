# Quickstart: CLI Data Management

## Prerequisites
- .NET 8 SDK installed
- `RagMcpServer` running locally

## Running the Server

```bash
cd src/RagMcpServer
dotnet run
```
Server will start at `http://localhost:5000` (or configured port).

## Using the CLI

The CLI is located in `src/RagMcpServer.CLI`.

### Build
```bash
cd src/RagMcpServer.CLI
dotnet build
```

### Import Documents
Uploads all supported files from a local directory.
```bash
dotnet run -- inject -path "C:/MyDocs/KnowledgeBase"
```

### Search
Query the database.
```bash
dotnet run -- query "How do I configure the server?"
```

### Check Status
View vector database statistics.
```bash
dotnet run -- info -vector_db
```
