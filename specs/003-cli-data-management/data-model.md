# Data Model: CLI Data Management

## Entities

### CLI Commands

#### InjectCommand
| Argument | Type | Description | Required |
|----------|------|-------------|----------|
| `-path` | string | Path to a file or directory to import | Yes |
| `--server` | string | URL of the API server | No (default: http://localhost:5000) |

#### QueryCommand
| Argument | Type | Description | Required |
|----------|------|-------------|----------|
| `text` | string | The natural language query | Yes |
| `--server` | string | URL of the API server | No (default: http://localhost:5000) |

#### InfoCommand
| Argument | Type | Description | Required |
|----------|------|-------------|----------|
| `-vector_db` | flag | Specific flag to query vector DB info | Yes |
| `--server` | string | URL of the API server | No (default: http://localhost:5000) |

## API Models

### Upload Documents
**Request**: `multipart/form-data`
- `files`: List of files (binary content).

**Response**: `200 OK`
```json
{
  "processedFiles": 5,
  "failedFiles": 0,
  "message": "Successfully processed 5 files."
}
```

### Server Info
**Response**: `200 OK`
```json
{
  "vectorDb": {
    "collectionName": "rag-collection",
    "documentCount": 142,
    "provider": "Sqlite"
  }
}
```
