# ArchPrecept 協作入口

> 本文件是專案擁有的協作入口。保存於 planning 的副本僅供審閱；核准並寫入根目錄後才生效。文件、Framework 安裝與產品變更各依其授權範圍執行。

ArchPrecept 提供可獨立採用的 .NET 架構分析器與選配參考契約。本機目錄保留 `dotnet-architecture-kit`，產品與遠端 repository 身分為 `ArchPrecept`。不得把這個套件產品當成一個待建立的 Backend Application。

## 依任務讀取來源

- 產品現況、已發布能力與目錄入口：[README](README.md)。
- 已確認、暫定及被取代的產品決策：[產品規劃書第 3 節](docs/product/product-plan.md#3-已確認的關鍵決策)。
- 需求及驗收：[PRD](docs/product/prd.md)，只讀與任務相關的 Requirement；文件為 draft 不代表其中已明確確認的個別決策都失效，也不代表整份文件已核准。
- 開發工作與規範適用性：[開發導覽](.dev/README.MD)、[Framework 採納文件](.dev/standards/framework-adoption.md)。先看文件狀態及適用範圍。
- Diagnostic 現行行為：[診斷目錄](docs/diagnostics/README.md)。現行限制與目標規則語意須分開判斷。
- 規則理由與官方依據：[設計依據](docs/product/rule-design-rationale.md)、[方法論來源](docs/product/methodology-references.md)。正式文案直接說明理由、適用範圍與取捨，不以私人或跨 repo 討論作為規範來源。
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
6. `docs/` 保存產品文件；`rules/catalog.xml` 是已交付的產品機器規則來源，設定結構見 `rules/configuration.xsd`；`.dev/` 保存開發治理及工作證據。不要複製第二份產品 Rule Catalog 或把產品文件搬入 `.dev/`。
7. Domain、EventSourcing、Messaging 與 Application.Messaging 依各自契約維持能力邊界。事件 port、唯讀集合或測試替身不等於 broker 交付、深層不可變性或儲存交易保證。

## 變更與驗證

- 首個不含 `-preview` 後綴的公開版本發布前不考慮舊版相容性，不為舊 API／Diagnostic ID／Profile／schema／suppression 保留轉接層或平行模式；本版實際行為仍須如實描述。同版號已發布成品不可覆寫。正式版前的文件、使用說明與可讀 API XML 要求見[版本政策](docs/releases/version-policy-proposal.md)。
- 與方法論、準則或所選 Framework 知識有真正同範圍理念衝突時，獨立討論受影響規則及權威來源；保留已接受而較嚴的政策理由，不因另一設計合法就自行放寬。
- 依當次授權與既有 Issue 範圍推進；計畫核准不自動擴張成安裝、產品實作、發布或清理授權。已授權的必要工作不重複索取許可。
- 先重用 accepted inputs，再選需求、設計、實作、審查或驗證步驟。不為形式重建完整 SDLC 文件，也不重跑與本次變更無關的昂貴驗收。
- 用能驗證受影響行為的檢查。文件草案以來源、連結、適用範圍與差異檢查為主；Analyzer 變更需考慮正例、反例、誤判及自有契約；套件邊界變更再選 package consumer 驗證。
- 本機 SDK 依 [global.json](global.json)。完整建置／測試／封裝入口為 [eng/Build-Packages.ps1](eng/Build-Packages.ps1)；consumer 驗證入口為 [eng/Test-PackageConsumer.ps1](eng/Test-PackageConsumer.ps1)。這些入口有不同成本，不因文件或 SHA 改變就一律執行。
- 暫存或可重建驗證資料先確認 RAM 磁碟映射與容量，再使用`F:\tmp` 下有界任務目錄；不得靜默退回 C:。持久決策、憑證及正式安裝復原紀錄留在獲准的持久位置。不修改全機 TEMP/TMP。
- 跨 repo／線上聊天研究、討論與中間設計資料在各階段完成及長工作接續前保存到該 F: 任務目錄，保留來源定位及必要摘錄；需要時讀回原資料，不只依賴對話摘要。正式整合時只將有效設計理由、契約及權威參考寫入 docs，不搬入研究歷程。
- 文件與程式直接修改有效內容；不在文內堆疊修改日誌或被取代的提案。完成可審閱且已檢查的功能／任務時，以聚焦的 Git commit 保存。提交前評估尚未分享的零碎提交是否適合整理；已分享歷史不因減少數量而逕行重寫，發布紀錄與必要授權來源證據另依其責任保留。
- 提交、合併與發布沿 [現行 CI/CD 規則](docs/releases/cicd-plan.md) 及當次授權。Framework 的 PR skill 不會替本專案新增必經 PR、required reviewer 或 branch protection。

公開 Issue／PR 保存功能、設計、驗收、相依、技術證據與完成範圍；操作授權、交接與執行紀錄留在使用者對話及專案既有工作流程，不以 Issue 正文／留言當成授權來源。本選配未啟用 workflow store；需要其正式記錄而機制不可用時，標明未完成，不暗中建立儲存庫或改存 Issue。

## Framework 與協作狀態

選配基線為 Framework v0.21.0：16 個工程／context skills、engineering-common 0.4.0、dotnet-backend 0.1.6、Codex adapter、original naming。精確 selection 保存位置為 `.ai/project/installation.json`；安裝狀態以實際 lock 與 inspect 讀回為準，selection 或本文件存在不代表已安裝。知識僅作明選範圍內的參考，沒有 normative-rule bindings；software-architect 的完整 UseCase 關係／playbook 讀取缺口仍須揭露。

不把來源 repository 的 `.dev` 政策、provider、記憶服務、模型偏好或 sub-agent 設定移植到本專案。此選配未包含 Framework sub-agent profiles、Claude adapter 或 workflow-records store。若日後需要，另以具體用途評估。

任務回報清楚區分：已修改、已驗證、已發布、未評估與建議。零 Diagnostic 不是整體合規證明；局部 suppression 也不是原規則已修復。
