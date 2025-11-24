#!/usr/bin/env node

const { Server } = require("@modelcontextprotocol/sdk/server/index.js");
const { StdioServerTransport } = require("@modelcontextprotocol/sdk/server/stdio.js");
const { CallToolRequestSchema, ListToolsRequestSchema } = require("@modelcontextprotocol/sdk/types.js");
const axios = require("axios");

// Configuration
const API_BASE_URL = "http://localhost:5228"; 

// Helper to log to stderr (visible in Cline logs)
function log(message, data) {
  const timestamp = new Date().toISOString();
  if (data) {
    console.error(`[${timestamp}] ${message}`, JSON.stringify(data, null, 2));
  } else {
    console.error(`[${timestamp}] ${message}`);
  }
}

const server = new Server(
  {
    name: "rag-mcp-server-proxy",
    version: "1.0.0",
  },
  {
    capabilities: {
      tools: {},
    },
  }
);

server.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [
      {
        name: "query_documents",
        description: "Query the knowledge base using natural language to retrieve information from ingested documents. Use this tool when the user asks questions about the internal documents.",
        inputSchema: {
          type: "object",
          properties: {
            query: {
              type: "string",
              description: "The question to ask.",
            },
          },
          required: ["query"],
        },
      },
      {
        name: "ingest_documents",
        description: "Ingest a file or directory into the knowledge base. Use this tool when the user asks to add, load, or process documents/folders.",
        inputSchema: {
          type: "object",
          properties: {
            path: {
              type: "string",
              description: "Absolute path to the file or directory.",
            },
          },
          required: ["path"],
        },
      },
    ],
  };
});

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;
  log(`Received tool call: ${name}`, args);

  try {
    if (name === "query_documents") {
      const { query } = args;
      if (!query) throw new Error("Query is required");

      log(`Sending query to API: ${query}`);
      const response = await axios.post(`${API_BASE_URL}/Query`, {
        query: query, // Use camelCase for standard JSON binding
      });

      log("Received API response", response.data);

      // Handle case-insensitive response properties
      const data = response.data || {};
      const answer = data.answer || data.Answer;
      const sources = data.sourceDocuments || data.SourceDocuments;
      
      let text = answer;
      
      // If answer is missing but we have sources, we can still provide them
      if (!text && sources && sources.length > 0) {
          text = "I found some relevant documents, but could not generate a direct answer.";
      } else if (!text) {
          text = "No relevant information found or no answer generated.";
      }

      if (sources && sources.length > 0) {
          text += "\n\n**Source Documents:**\n" + sources.map((s, i) => {
              const content = s.content || s.Content || "";
              // Limit content length to avoid overwhelming the context
              const snippet = content.length > 300 ? content.substring(0, 300) + "..." : content;
              return `[${i + 1}] ${snippet.replace(/\n/g, " ")}`;
          }).join("\n");
      }

      return {
        content: [
          {
            type: "text",
            text: text,
          },
        ],
      };
    } else if (name === "ingest_documents") {
      const { path } = args;
      if (!path) throw new Error("Path is required");

      log(`Sending ingestion request for: ${path}`);
      const response = await axios.post(`${API_BASE_URL}/Documents`, {
        path: path, // Use camelCase
      });
      
      log("Received ingestion response", response.data);
      
      const data = response.data || {};
      const jobId = data.jobId || data.JobId;
      const status = data.status || data.Status;

      return {
        content: [
          {
            type: "text",
            text: `Ingestion started.\nJob ID: ${jobId}\nStatus: ${status}`,
          },
        ],
      };
    } else {
      throw new Error(`Unknown tool: ${name}`);
    }
  } catch (error) {
    const errorDetails = error.response ? error.response.data : error.message;
    log(`Error processing ${name}`, errorDetails);

    // Check if error is Connection Refused (Server not running)
    if (error.code === 'ECONNREFUSED') {
         return {
            content: [
                {
                    type: "text",
                    text: "Error: Could not connect to the RagMcpServer. Please ensure the C# backend is running (dotnet run in src/RagMcpServer)."
                }
            ],
            isError: true
         };
    }

    return {
      content: [
        {
          type: "text",
          text: `Error executing ${name}: ${typeof errorDetails === 'object' ? JSON.stringify(errorDetails) : errorDetails}`,
        },
      ],
      isError: true,
    };
  }
});

async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  log("RagMcpServer Proxy started and connected via Stdio.");
}

main().catch((error) => {
  console.error("Fatal error:", error);
  process.exit(1);
});
