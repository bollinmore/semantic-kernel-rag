# Research: Add RAG API Server

**Feature**: `005-rag-api-server`
**Date**: 2025-12-02

## Decisions

### 1. Hybrid Hosting Model
- **Decision**: Run ASP.NET Core `WebApplication` and `McpServer` concurrently in the same process.
- **Rationale**:
  - **Shared State**: Both interfaces (MCP and HTTP) need access to the same Semantic Kernel instance and Vector Database. SQLite (Write-Ahead Logging) handles concurrency reasonably well, but sharing the in-memory `Kernel` and `IVectorDbService` singleton instances prevents initialization overhead and potential locking conflicts if they were separate processes.
  - **Deployment**: Single executable is easier to deploy than managing two separate services.
- **Implementation Detail**:
  - Use `WebApplication.CreateBuilder(args)` instead of `Host.CreateDefaultBuilder`.
  - Configure `McpServer` as a `BackgroundService` OR run it explicitly in `Program.cs` using `Task.WhenAll`.
  - **Chosen Approach**: Register `McpServer` as a `HostedService` (`services.AddHostedService<McpServerWorker>()`) so it shares the Host lifecycle. This allows `app.RunAsync()` to manage the whole application lifecycle, including graceful shutdown. Wait, `McpServer` uses Stdio. If it blocks on `Console.ReadLine`, it might be fine in a `BackgroundService`.

### 2. Logging Strategy (Critical)
- **Decision**: Enforce strictly Stderr logging for all infrastructure.
- **Rationale**: The MCP protocol uses `stdout` for JSON-RPC communication. Any stray log message (e.g., "Now listening on: http://localhost:5000") printed to `stdout` will break the MCP client connection.
- **Implementation**:
  - Existing `Program.cs` already configures Serilog to write to `standardErrorFromLevel`.
  - **Risk**: `WebApplication.CreateBuilder` adds default providers (Console, Debug, EventSource).
  - **Mitigation**: Call `builder.Logging.ClearProviders()` immediately after creation, then add Serilog.
  - **Verification**: Use `launchSettings.json` profiles to test running the app and inspecting stdout/stderr streams separately.

### 3. API Framework
- **Decision**: Use **Controllers** (`MapControllers`).
- **Rationale**: While Minimal APIs are lighter, Controllers provide better organization for complex request/response models (`SearchRequest`, `SearchResult` with Metadata) and are more familiar for the requested FluentValidation integration.
- **Alternatives Considered**: Minimal APIs (good, but validation logic often clutters `Program.cs` or requires extension methods).

### 4. Validation
- **Decision**: Use **FluentValidation**.
- **Rationale**: Explicitly requested by user. Provides powerful separation of validation rules from the model itself.
- **Implementation**: Register `IValidator<SearchRequest>` and use a filter or manual validation in the controller.

## Unknowns & Clarifications

### Resolved
- **Hosting**: Can we run Web API in the existing project? **Yes**, by changing SDK to `Microsoft.NET.Sdk.Web` or adding the Framework Reference.
- **Concurrency**: How to run both loops? **HostedService** for MCP is the cleanest way if MCP is built to be cancellable. If `McpServer.RunAsync` blocks indefinitely on Stdio, it's perfect for `ExecuteAsync` in a `BackgroundService`.

### Outstanding
- None.

## References
- [ASP.NET Core Background Tasks](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [FluentValidation ASP.NET Core](https://docs.fluentvalidation.net/en/latest/aspnet.html)
