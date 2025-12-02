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

# 004-strict-mcp-refactor

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

---

# 005-

## specify

新增 RAG API Server 是為了建立一個專門的後端服務，讓外部應用程式能夠透過標準化的 HTTP 介面，從 RAG 資料庫中檢索相關文件區塊（chunks），並獲取帶有元資料的搜尋結果。這能實現 RAG 系統的模組化，讓資料提取邏輯獨立於前端或 LLM 應用，避免直接暴露資料庫連線，提升安全性與可維護性 。透過 appsettings.json 配置 RAG 資料庫資訊，確保部署時的靈活性，支持不同環境（如開發、生產）的快速切換 。​

使用者故事：

作為 RAG 應用開發者，我希望有一個 API 端點接收查詢參數（如 collection_name、query、top_k），以從指定資料集合中提取最相關的內容區塊。

作為系統管理員，我需要透過配置檔設定資料庫連線，避免硬編碼，提高部署一致性。

作為 LLM 整合者，我期望回應包含 count、results 陣列，每個結果有 content 與 metadata（page_number、chunk_index、source），以便後續生成回應 。​

## clarify

## plan

在 src\RagApiServer 目錄下建立 ASP.NET Core Web API 專案，使用 Minimal API 或 Controller 架構一個 POST /search 端點，接收指定的 Request Body 並依序驗證必填欄位（collection_name、source_type、query），選填欄位（top_k、top_n、score_threshold）設預設值 。​

專案結構：新增 Program.cs 初始化 Host，注入 RAG 客戶端（如 Qdrant 或 Milvus），透過 appsettings.json 讀取連線字串、索引設定；新增 Models 資料夾定義 Request/Response DTO。

核心邏輯：端點處理請求，建立 vector 查詢（使用 query embedding），篩選 score > threshold 的 top_k 結果，序列化 metadata 並回傳 JSON。

配置與依賴：appsettings.json 包含 RAG:Url、RAG:ApiKey、CollectionDefaults；新增 NuGet 套件如 Microsoft.Extensions.Hosting、RAGClient SDK；啟用 CORS、Swagger 與健康檢查 。​

驗證與錯誤處理：使用 FluentValidation 檢查輸入，回傳 400 Bad Request 於無效參數，500 於資料庫錯誤；記錄查詢效能 。​

## checklist

## tasks

## analyze

## implement
