# ArchPrecept 協作入口

> 待檢核草案。預計位置為 repository 根目錄 `AGENTS.md`；目前保存在審閱資料夾，尚未成為專案生效指引。核准文件內容、安裝 Framework 與執行產品變更是分開的授權。

ArchPrecept 提供可獨立採用的 .NET 架構分析器與選配參考契約。本機目錄保留 `dotnet-architecture-kit`，產品與遠端 repository 身分為 `ArchPrecept`。不得把這個套件產品當成一個待建立的 Backend Application。

## 依任務讀取來源

- 產品現況、已發布能力與目錄入口：[README](README.md)。
- 已確認、暫定及被取代的產品決策：[產品規劃書第 3 節](docs/product/product-plan.md#3-已確認的關鍵決策)。
- 需求及驗收：[PRD](docs/product/prd.md)，只讀與任務相關的 Requirement；文件為 draft 不代表其中已明確確認的個別決策都失效，也不代表整份文件已核准。
- 開發工作與規範適用性：[開發導覽](.dev/README.MD)、[Framework 採納文件](.dev/standards/framework-adoption.md)。先看文件狀態及適用範圍。
- Diagnostic 現行行為：[診斷目錄](docs/diagnostics/README.md)。現行限制與目標規則語意須分開判斷。
- GitHub 工作入口：[ArchPrecept Issues](https://github.com/YuChia-Wei/ArchPrecept/issues)。既有工作優先，避免為同一缺口建立重複項目。

## 程式探索

<!-- codebase-memory-mcp:start -->
# Codebase Knowledge Graph (codebase-memory-mcp)

This project uses codebase-memory-mcp to maintain a knowledge graph of the codebase.
ALWAYS prefer MCP graph tools over grep/glob/file-search for code discovery.

## Priority Order
1. `search_graph` — find functions, classes, routes, variables by pattern
2. `trace_path` — trace who calls a function or what it calls
3. `get_code_snippet` — read specific function/class source code
4. `query_graph` — run Cypher queries for complex patterns
5. `get_architecture` — high-level project summary

## When to fall back to grep/glob
- Searching for string literals, error messages, config values
- Searching non-code files (Dockerfiles, shell scripts, configs)
- When MCP tools return insufficient results

## Examples
- Find a handler: `search_graph(name_pattern=".*OrderHandler.*")`
- Who calls it: `trace_path(function_name="OrderHandler", direction="inbound")`
- Read source: `get_code_snippet(qualified_name="pkg/orders.OrderHandler")`
<!-- codebase-memory-mcp:end -->

先確認 graph 對應目前 checkout；未索引時先使用 `index_repository`。Graph 不足或不可用時說明範圍，再用有界的檔案搜尋，不把空結果當成程式不存在。

## 規範與產品邊界

1. 先判斷任務是在開發 ArchPrecept 本身、定義下游應用規則，或維護 consumer 範例。C# 副檔名不構成選用 `dotnet-backend` Profile 的理由。
2. 引用已接受且適用的決策、Requirement 及規則來源。不要因 skill、範例或新版本提了另一做法，就重開已接受決策。
3. Framework 知識包的存在、skill 的可讀資源、目標規範採納與實際執行證據分開呈現。沒有 verified installation 時，不聲稱 installed reader、runtime discovery 或規範覆蓋已通過。
4. 同一適用範圍有真正衝突時，列出來源、狀態、影響及可選處置；只暫停受影響的判斷。不要用「專案一律優先」或「最新文件一律優先」掩蓋衝突。
5. 保持 Analyzer 與 reference contracts 解耦，以及 analyzer-only／custom-contract 的產品方向。某項能力仍待完成時標明缺口，不強迫 consumer 遷移來隱藏限制。
6. `docs/` 保存產品文件；`rules/` 是規劃中的機器規則責任位置，須先確認實際存在的內容；`.dev/` 保存開發治理及工作證據。不要複製第二份產品 Rule Catalog 或把產品文件搬入 `.dev/`。
7. Domain、EventSourcing、Messaging 與 Application.Messaging 依各自契約維持能力邊界。事件 port、唯讀集合或測試替身不等於 broker 交付、深層不可變性或儲存交易保證。

## 變更與驗證

- 依當次授權與既有 Issue 範圍推進；計畫核准不自動擴張成安裝、產品實作、發布或清理授權。已授權的必要工作不重複索取許可。
- 先重用 accepted inputs，再選需求、設計、實作、審查或驗證步驟。不為形式重建完整 SDLC 文件，也不重跑與本次變更無關的昂貴驗收。
- 用能驗證受影響行為的檢查。文件草案以來源、連結、適用範圍與差異檢查為主；Analyzer 變更需考慮正例、反例、誤判及自有契約；套件邊界變更再選 package consumer 驗證。
- 本機 SDK 依 [global.json](global.json)。完整建置／測試／封裝入口為 [eng/Build-Packages.ps1](eng/Build-Packages.ps1)；consumer 驗證入口為 [eng/Test-PackageConsumer.ps1](eng/Test-PackageConsumer.ps1)。這些入口有不同成本，不因文件或 SHA 改變就一律執行。
- 暫存或可重建驗證資料先確認 RAM 磁碟映射與容量，再使用有界任務目錄；持久決策、憑證及正式安裝復原紀錄留在獲准的持久位置。不修改全機 TEMP/TMP。
- 提交、合併與發布沿 [現行 CI/CD 規則](docs/releases/cicd-plan.md) 及當次授權。Framework 的 PR skill 不會替本專案新增必經 PR、required reviewer 或 branch protection。

## Framework 與協作狀態

本次選配方向為 common 與 .NET 知識、所選工程及 context skills、Codex adapter。建議等待 v0.21.0 正式發布後，按實際 catalog 更新版本與變更計畫；目前沒有安裝或自動升級。

不把來源 repository 的 `.dev` 政策、provider、記憶服務、模型偏好或 sub-agent 設定移植到本專案。此選配未包含 Framework sub-agent profiles、Claude adapter 或 workflow-records store。若日後需要，另以具體用途評估。

任務回報清楚區分：已修改、已驗證、已發布、未評估與建議。零 Diagnostic 不是整體合規證明；局部 suppression 也不是原規則已修復。
