# 001-rag-mcp-server

## specify

建立一個 RAG 可以用來讓LLM提取內部文件的系統，這個系統最後會被封裝成一個 MCP server 然後透過其他的 MCP client 調用。此程式需要支援一次輸入多個檔案或是資料夾進行批次處理。

## clarify

## plan

使用 Microsoft Semantic Kernel 框架開發 RAG, 需要使用 C# .NET 當作開發語言，向量資料庫採用 Chroma, 嵌入模型採用 nomic-embed-text, 預計採用 vscode 以及 Cline 套件來測試這個 MCP server.

## checklist

## tasks

## analyze

## implement

---

# 002-configure-llm-models

## specify

提供設定LLM以及embeddings模型的機制，在啟動服務前，讀取設定檔的設定來決定要怎麼啟用。

## clarify

## plan

預設採用 Ollama llama3.1 作為 LLM, nomic-embed-text. 在 launchSettings.json 提供調整的選項，允許使用 LiteLLM 連接到遠端的LLM，需要能讓使用者設定 Host Endpoint, API key, model name.

## checklist

## tasks

## analyze

## implement

---

# 003-cli-data-management

## specify

建立一個CLI，提供匯入資料、檢索資料以及查詢當前的向量資料庫資訊的功能。

## clarify

## plan

此CLI使用 .NET SDK 開發，預計有以下指令：
1. inject [-file <path>] [-dir <path>]
2. query <msg>
3. info -vector_ddb

inject 接受單一檔案或是指定的資料夾，匯入後依照原先的資料切割流程轉為 embeddings 匯入向量資料庫
query 接受自然語言從預先建立好的向量資料庫中提取資料，並交由LLM回答
info 目前固定接受 "-vector_ddb" 參數，查詢當前的向量資料庫摘要(包含維度資訊)

## checklist

## tasks

## analyze

## implement

---

# 004-

## specify

重構本專案，使 Server & Client 遵循嚴格的 MCP 規範。
Server 端只提供 Inject, Query 工具、處理向量訊息。
Client 端要負責啟動 Server, 處理 LLM 連線，把訊息回應給使用者。

## clarify

## plan

修改 RagMcpServer 使其符合 MCP server 規範，接受來自 LLM 呼叫調用工具，只接受 MCP JSON-RPC 通訊格式，而不是使用 Resetful API 呼叫。
RagMcpServer 並不負責處理 LLM 連線功能，輸入與輸出一律採用 JSON-RPC 格式。

重命名 RagMcpServer.CLI 為 RagMcpClient, 負責啟動 RagMcpServer, 保留目前所有指令。另外 RagMcpClient 在啟動 MCP server 後，必須進行初始化設定，從 MCP server 中取得所有支援的工具並記下來。RagMcpClient 要負責接收使用者的訊息，傳遞給LLM來決定是否調用 MCP tool 後然後把 LLM 整理過後的回應印到終端機上給使用者看。

## checklist

## tasks

## analyze

## implement
