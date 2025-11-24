# RAG MCP Server

This project implements a Retrieval-Augmented Generation (RAG) system using the Microsoft Semantic Kernel framework. It is exposed as a RESTful API for ingesting documents and answering questions based on their content.

## Prerequisites

- .NET 8 SDK
- Docker
- Ollama

## Quickstart

For detailed setup and usage instructions, please see the [Quickstart Guide](./specs/001-rag-mcp-server/quickstart.md).

## Configuration

You can configure the AI providers (LLM and Embeddings) in `appsettings.json` or via Environment Variables.
See [AI Configuration Guide](./specs/002-configure-llm-models/quickstart.md) for details.

## Project Structure

- `src/`: Contains the C# source code for the web API.
- `tests/`: Contains unit and integration tests.
- `specs/`: Contains the feature specification, implementation plan, and other design documents.
