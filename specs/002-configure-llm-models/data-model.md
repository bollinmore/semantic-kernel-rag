# Data Model: Configuration Schema

## AI Configuration

The application uses a hierarchical configuration structure rooted in the `AI` section.

```csharp
public class AppSettings
{
    public AIConfig AI { get; set; }
}

public class AIConfig
{
    public AIServiceConfig TextGeneration { get; set; }
    public AIServiceConfig TextEmbedding { get; set; }
}

public class AIServiceConfig
{
    public string Provider { get; set; } // "Ollama", "OpenAI"
    public string ModelId { get; set; }
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
}
```

## Configuration Mapping

### JSON (`appsettings.json`)

```json
{
  "AI": {
    "TextGeneration": {
      "Provider": "Ollama",
      "ModelId": "llama3.1",
      "Endpoint": "http://localhost:11434"
    },
    "TextEmbedding": {
      "Provider": "Ollama",
      "ModelId": "nomic-embed-text",
      "Endpoint": "http://localhost:11434"
    }
  }
}
```

### Environment Variables (`launchSettings.json`)

Environment variables map to the JSON structure using double underscores `__`.

- `AI__TextGeneration__Provider`
- `AI__TextGeneration__ModelId`
- `AI__TextGeneration__Endpoint`
- `AI__TextGeneration__ApiKey`
- `AI__TextEmbedding__Provider`
- ...
