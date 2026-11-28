# ArchPrecept 協作入口

ArchPrecept 提供可獨立採用的 .NET 架構分析器與選配參考契約。開發本產品時依 Analyzer、契約和套件的能力邊界選擇適用規範。

## 依任務讀取來源

- 產品現況、已發布能力與目錄入口：[README](README.md)。
- 產品定位、能力及已確認決策見[產品文件](docs/product/README.md#已確認的產品決策)。
- 需求及驗收：[PRD](.dev/product/prd.md)，只讀與任務相關的 Requirement；文件為 draft 不代表其中已明確確認的個別決策都失效，也不代表整份文件已核准。
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
6. `docs/` 保存對外產品文件。機器規則以 `rules/catalog.xml` 為正式來源，consumer 設定結構以 `rules/configuration.xsd` 為準。`.dev/` 保存產品需求與開發規範。各項內容的正式位置見[文件治理](.dev/governance/document-governance-and-traceability.md)。
7. Domain、EventSourcing、Messaging 與 Application.Messaging 依各自契約維持能力邊界。事件 port、唯讀集合或測試替身不等於 broker 交付、深層不可變性或儲存交易保證。

## 變更與驗證

- 收到發布要求時，主動執行 [CI/CD 的發布前文件與規則檢查](.dev/releases/cicd-plan.md#發布要求觸發的文件與規則檢查)，依該節使用可確認的低成本模型，檢查當版說明、規則、實作及成品證據的落差，修正並複查後才執行可能觸發上傳的動作。模型不可用或必要檢查未完成時如實回報；不得以 CI 成功代替。此為本專案明選的發布審查政策。
- 首個不含任何 prerelease 後綴的公開版本發布前不考慮舊版相容性，不為舊 API／Diagnostic ID／Profile／schema／suppression 保留轉接層或平行模式。每版如實描述實際行為，已發布成品依[版本政策](docs/releases/version-policy.md)維持不可覆寫。正式版前的文件、使用說明與可讀 API XML 驗收見[首個正式版條件](.dev/releases/cicd-plan.md#首個正式版條件)。
- 與方法論、準則或所選 Framework 知識有真正同範圍理念衝突時，獨立討論受影響規則及權威來源；保留已接受而較嚴的政策理由，不因另一設計合法就自行放寬。
- 依當次授權與既有 Issue 範圍推進；計畫核准不自動擴張成安裝、產品實作、發布或清理授權。已授權的必要工作不重複索取許可。
- 先重用 accepted inputs 與現有能力，再選完成需求的最小設計與工作步驟。不得把一次驗收擴張為通用框架、額外抽象或長期維護機制；不為形式重建完整 SDLC 文件。
- 新增或擴大測試／驗證前，先用簡短說明指出要防止的具體缺陷、對使用者的影響，以及現有工具為何不足；同時考量誤報、執行與維護成本。沒有實際風險或明確需求支持時，不新增檢查；不為這項判斷另建表單、台帳或審批流程。
- 優先使用語言、編譯器、SDK 與既有測試／建置入口。測試應針對可觀察行為或必要交付結果，不綁定非契約性的內部結構、文字格式、生成細節或固定數量；正常重構與等價寫法不應迫使維護者同步修改測試。
- 不為滿足自訂測試而新增產品不需要的程式、補件、相依或建置步驟。遇到這種情況先縮減或移除不合理檢查，不能以測試全綠當成設計合理或使用者接受的證明。
- 依實際影響選取最小足夠的驗證。文件修改先核對內容、來源、連結及差異；Analyzer 行為修改選相關正反例、誤判與自有契約案例；套件邊界受影響時才選對應 consumer 情境。明選的必要閘門依其適用範圍執行，不因文件、SHA、交接或驗證報告改變而重跑無關測試、完整矩陣或模型審查。
- 開發與封裝先用開發主機的 `dotnet --list-sdks` 盤點已安裝 SDK，優先選主機已有的最新正式版，以 `dotnet --version` 核對實際解析結果；[global.json](global.json) 應與此選擇一致。遇到舊 pin 或 PATH 未找到 dotnet，先查主機既有安裝並調整選擇。唯一允許使用已安裝舊版的情境是處理具體安全性或相容性議題，例如驗證明列支援的 consumer SDK。
- 禁止主動下載或安裝指定版本的 .NET SDK，不得為滿足舊 pin 下載、安裝舊版；RAM 磁碟、暫存目錄、隔離安裝及工作區同樣適用。只有主機尚未安裝所需新版時，才可詢問使用者是否要更新至新版，須取得明確同意後才能執行。
- 完整建置／測試／封裝入口為 [eng/Build-Packages.ps1](eng/Build-Packages.ps1)；consumer 驗證入口為 [eng/Test-PackageConsumer.ps1](eng/Test-PackageConsumer.ps1)。依實際影響選擇驗證。
- 文件與程式直接修改有效內容。完成可審閱且已檢查的功能／任務時，以聚焦的 Git commit 保存。提交前評估尚未分享的零碎提交是否適合整理。已分享歷史不得僅為減少提交數量而重寫。
- 提交、合併與發布沿 [現行 CI/CD 規則](.dev/releases/cicd-plan.md) 及當次授權。Framework 的 PR skill 不會替本專案新增必經 PR、required reviewer 或 branch protection。

公開 Issue／PR 可保存功能範圍、設計、驗收條件、相依、驗證結論與完成範圍。操作授權與交接留在對話或既有工作流程。

## 暫存與工作資料

- 暫存及可重建資料依使用者指定位置處理。本機設定可選用 RAM disk，使用前確認掛載、容量及任務目錄。憑證與正式安裝復原紀錄留在獲准的持久位置，保持全機 TEMP／TMP 設定。
- 測試輸出、一次性驗證資料、備份、工作快照及跨專案研究過程保存在 Git 之外。若需在 checkout 內手動暫存，先核對現有忽略規則並集中使用 `artifacts/`。可重建且無後續用途的資料可直接丟棄。
- 正式文件直接更新有效的設計理由、契約和可查證來源。依授權與需求維護驗收結論，連同有效範圍與仍未評估的條件記錄在適合的 Issue／PR 或對話。

## Framework 與協作狀態

Framework 的適用條件與參考知識見[採用規範](.dev/standards/framework-adoption.md)。產品規則依本專案已確認決策與 Rule Catalog；執行特定工作前依適用的專案 binding 和 skill 判斷可讀資源。

任務回報指出當次修改、已執行的驗證、已發布項目、未評估範圍及建議。依實際覆蓋範圍說明 Diagnostic 結果與 suppression 的影響。
