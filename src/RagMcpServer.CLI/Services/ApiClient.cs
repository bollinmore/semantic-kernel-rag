using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace RagMcpServer.CLI.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient 
        { 
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(30)
        };
    }

    public async Task<UploadResponse> UploadFilesAsync(IEnumerable<string> filePaths)
    {
        using var content = new MultipartFormDataContent();
        int fileCount = 0;

        foreach (var filePath in filePaths)
        {
            var fileStream = File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "files", Path.GetFileName(filePath));
            fileCount++;
        }

        if (fileCount == 0)
        {
            return new UploadResponse { Message = "No files to upload." };
        }

        var response = await _httpClient.PostAsync("/Documents/upload", content);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UploadResponse>() ?? new UploadResponse();
    }

    public async Task<QueryResponse?> QueryAsync(string queryText)
    {
        var request = new { query = queryText, includeSources = true };
        var response = await _httpClient.PostAsJsonAsync("/Query", request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
             return null;
        }
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<QueryResponse>();
    }

    public async Task<InfoResponse?> GetServerInfoAsync()
    {
        return await _httpClient.GetFromJsonAsync<InfoResponse>("/Info");
    }
}

public class UploadResponse
{
    public int ProcessedFiles { get; set; }
    public int FailedFiles { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<QueryResultSource> Sources { get; set; } = new();
}

public class QueryResultSource
{
    public string Title { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public double Score { get; set; }
}

public class InfoResponse
{
    public VectorDbInfo VectorDb { get; set; } = new();
}

public class VectorDbInfo
{
    public string CollectionName { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
    public string Provider { get; set; } = string.Empty;
}