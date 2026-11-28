---
title: "ArchPrecept 產品需求"
lang: "zh-TW"
document_id: "DOC-PRD"
status: "draft"
---

# ArchPrecept 產品需求

本文件為需求基線草案。章節中的優先序與驗收描述產品應達到的行為。已確認政策、目前交付範圍與後續方向見[產品範圍與方向](../../docs/product/README.md)，目前套件與版本狀態見[套件使用手冊](../../docs/README.md)。

## 1. 需求範圍

初期驗收以 Rule Catalog、共用 Architecture Role Classifier、核心 Analyzer 與選配參考契約為重點。採用自有契約的 consumer 須能以明確映射觸發 Aggregate、Repository 與 Use Case 核心規則，其相依圖中沒有 ArchPrecept Reference Contracts。採用參考契約時也須能以低設定觸發相同規則。規則覆蓋及未評估狀態應依實際證據判讀，完整的 repository 層級報告列入 Conformance 需求。

P0 為核心能力或必要交付要求，P1 與 P2 保存後續需求與優先序。Architecture Testing 與 Conformance 節保留各自目標，個別需求是否已交付須依對應實作及版本判斷。

## 2. 使用者故事

### US-001：既有契約導入 Analyzer

作為既有 .NET 系統維護者，我希望指定自己專案中的 Aggregate Root、Use Case 與 Repository 契約，讓 Analyzer 能執行核心規則，而不必改用產品提供的介面。

### US-002：新專案採用參考契約

作為新專案開發者，我希望直接引用一組穩定的 Domain／Application 契約，使 Analyzer 能以最低設定正確辨識角色。

### US-003：理解 Diagnostic 原因

作為被 Diagnostic 阻擋的開發者，我希望知道規則 ID、角色判定依據、違反原因、修正方向與文件連結。

### US-004：避免錯誤合規宣告

作為架構師，我希望報告清楚顯示未被辨識或未執行的規則，而不是把沒有錯誤視為已通過。

### US-005：CI 統一驗證

作為 CI 維護者，我希望以單一入口執行 Build、Test、Analyzer、Architecture Test 與 Repository Check，並取得穩定 Exit Code 和機器可讀報告。

### US-006：AI Review 使用客觀證據

作為 AI Agent 或 Reviewer，我希望讀取 Rule Catalog 與 Diagnostic Output，將分析時間用於規則未涵蓋的工程判斷，而不是重做機械式檢查。

### US-007：漸進啟用規則

作為既有團隊，我希望先用 Warning 盤點，再逐步把已確認的規則提升為 Error，而不必一次修正所有歷史問題。

### US-008：維護文件與規則追溯

作為產品維護者，我希望產品規劃、需求、工程規範、Rule、Diagnostic、實作與測試能維持單一正式來源及雙向追溯，讓規則缺口、文件漂移與未被驗證的宣告可以由 CI 或審查流程發現。

## 3. 功能需求：Rule Model

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| RM-001 | 每條工程規則必須具有版本內唯一且可追溯的 Rule ID | P0 | CI 能檢查同版重複 ID 與文件一致性；正式版前可直接調整，不強制保留 preview 舊 ID |
| RM-002 | Rule Catalog 必須為機器可讀格式並有 Schema | P0 | 所有 Catalog File 通過 Schema Validation |
| RM-003 | Rule 必須記錄標題、理由、適用角色、Profile、執行機制及機械化程度 | P0 | 每個 P0 Rule 欄位完整，缺漏會使 CI 失敗 |
| RM-004 | Rule 必須能映射至 Diagnostic ID、Architecture Test 或 CLI Check | P0 | 所有公開 Diagnostic 均能反查 Rule ID |
| RM-005 | Rule 必須能引用外部方法論／作者來源，並區分支持界線與產品選擇 | P1 | 規則文件可定位原始來源及相關段落；Framework 知識只依明選範圍參考，不充當規則設計權威 |
| RM-006 | Rule 的生命週期與替代關係依生效版本政策治理 | P1 | 首個 non-preview 公開版本前不強制 deprecated／replacement 結構；有實際替代契約時清楚說明，正式版後依所選維護政策驗收 |
| RM-007 | Rule 狀態必須支援機械化、協助式、人工審查與不適用分類 | P0 | Report 能區分 `machine-enforced`、`machine-assisted`、`review-required`、`not-applicable` |
| RM-008 | Rule Catalog、Diagnostic Descriptor 與 Markdown 文件不得無聲漂移 | P1 | CI 會驗證 ID、標題與文件引用一致性 |

## 4. 功能需求：Architecture Role Classification

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| RC-001 | 所有 Analyzer 必須共用中央 Role Classifier | P0 | 不再由各規則重複實作 `LooksLikeUseCase` 等判斷 |
| RC-002 | Classifier 必須支援本產品內建契約的完整 Metadata Name | P0 | 未加入 runtime PackageReference 仍能辨識已安裝於 Consumer 的契約 |
| RC-003 | Classifier 必須支援 Custom Interface 與 Base Type Mapping | P0 | Consumer 可指定自有 Aggregate／Use Case／Repository 契約並觸發規則 |
| RC-004 | Mapping 必須沿 Interface／Base Type 繼承鏈傳遞 | P0 | 衍生 Contract 與 Implementation 均能被辨識 |
| RC-005 | Classifier 必須支援可設定的 Suffix 與 Namespace Convention | P1 | 團隊可啟用、停用或覆寫慣例 |
| RC-006 | 每次 Role Match 必須記錄 Evidence 與 Confidence | P0 | Diagnostic／Report 可指出 `configured-contract`、`built-in-contract`、`inherited-contract`、`structural-match` 或 `naming-convention` |
| RC-007 | 只靠 Naming Convention 的判定不得假裝與明確契約同等可靠 | P0 | Default Severity 或 Report Confidence 有明確差異 |
| RC-008 | 無法辨識角色時必須回報未評估，而不是通過 | P0 | Use Case／Aggregate Coverage 為 `not-evaluated` 並附原因 |
| RC-009 | Classifier 必須避免以簡單名稱誤認不同 Namespace／Assembly 的型別 | P0 | 無關命名案例不產生目標規則診斷 |
| RC-010 | 分類結果必須能在同一 Compilation 中共用與快取 | P1 | Analyzer Benchmark 不因規則數量線性重複掃描相同型別 |

## 5. 功能需求：Roslyn Analyzer

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| AN-001 | Analyzer 必須能獨立安裝 | P0 | Consumer Dependency Graph 不含 Reference Contracts |
| AN-002 | Analyzer Project 不得直接參考 Reference Contract Project／Package | P0 | 建置與 package inspection 均證明相依數為 0 |
| AN-003 | Analyzer 必須使用 Roslyn Symbol／Semantic Model，而非文字搜尋作為 C# 語意最終判定 | P0 | 核心規則不以 grep／檔名作正式結論 |
| AN-004 | Diagnostic 必須有穩定 ID、Category、Title、Message、Help Link 與 Rule ID | P0 | Package Inspection 與文件驗證通過 |
| AN-005 | 每條規則 Severity 必須可透過 `.editorconfig` 設定 | P0 | `none`、`suggestion`、`warning`、`error` 均有測試 |
| AN-006 | Analyzer 必須停用 Generated Code 或依規則明確定義 Generated Code Policy | P0 | 自動產生程式碼不出現未核准 Diagnostic |
| AN-007 | Analyzer 必須啟用 Concurrent Execution，並建立效能基線 | P1 | 可重複的基準測量能分辨 Analyzer 對建置時間的影響 |
| AN-008 | Core Rule 不得預設依賴 ASP.NET Core、EF Core、Wolverine 或 MediatR | P0 | Core Consumer 不需安裝上述 Framework |
| AN-009 | ASP.NET Core 與 Messaging 規則必須由 Profile 控制 | P1 | 非相關宿主不會收到 Controller／Message Handler Diagnostic |
| AN-010 | Diagnostic 訊息必須指出被判定角色與違反原因 | P0 | 測試驗證 Message Argument 與 Evidence |
| AN-011 | Code Fix 僅能用於低風險、語意明確的修正 | P2 | 高風險架構重構不提供自動 Code Fix |
| AN-012 | 現有 Diagnostic 重構後不得發生未記錄的 ID 或語意變更 | P0 | 代表性案例可辨識非預期的診斷行為變更 |

## 6. 功能需求：Reference Contract Libraries

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| CT-001 | Reference Contracts 必須是選配能力 | P0 | Analyzer-only Consumer 可完整 restore／build |
| CT-002 | Domain Contracts 應提供穩定的 Entity、Aggregate Root、Value Object 與 Domain Event 語意 | P1 | Public API Review、Unit Tests 與文件完成 |
| CT-003 | Application Contracts 應提供 Use Case、Repository Port、Query Port 與必要結果型別 | P1 | 不引用 Transport／UI Framework 型別 |
| CT-004 | 訊息契約與 Application 發布 port 必須維持 Transport-neutral | P1 | 不公開 Wolverine、Kafka、RabbitMQ 或 ASP.NET 型別；[責任、相依與遷移](../../docs/product/messaging-contract.md) |
| CT-005 | Event Sourcing 支援必須明確定義 Replay、Pending Event、Version 與 Commit 語意 | P1 | 行為測試涵蓋重播、不重複記錄與版本檢查；[核心契約與驗收案例](../../docs/product/event-sourcing-contract.md) |
| CT-006 | Contracts 不得相依 Analyzer、Architecture Testing 或 Conformance | P0 | Dependency Graph 驗證通過 |
| CT-007 | Core Contracts 不得成為完整 Runtime Framework | P0 | 不包含 Outbox、ORM、Bus、DI Scanning 等具體實作 |
| CT-008 | Public API 變更必須說明當前契約與行為差異 | P1 | Candidate 可列出受影響 API 與驗證；首個 non-preview 公開版本前不以維持舊 preview 相容為 gate |
| CT-009 | 參考契約的維護 Ownership 與實際程式碼來源必須可稽核 | P1 | 引用或沿用的材料可定位來源與授權，且可確認此 repository 的維護責任 |

## 7. 功能需求：Architecture Testing

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| AT-001 | 提供 Assembly／Project／Namespace Dependency Assertion | P1 | 可驗證 Domain 不依賴 Infrastructure 等規則 |
| AT-002 | Architecture Rule 必須能對應 Rule ID | P1 | Test Failure 可連回 Rule Catalog |
| AT-003 | Architecture Testing 必須能被一般測試專案使用 | P1 | `dotnet test` 可直接執行，不需專屬 Runner |
| AT-004 | 提供 Profile，但允許 Target Repository 自訂額外規則 | P1 | Consumer 能擴充而不 Fork 產品原始碼 |
| AT-005 | Architecture Testing 不得重複 Roslyn 可更精準處理的單一語意規則 | P1 | 責任矩陣有明確分工 |

## 8. 功能需求：Conformance Tool

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| CF-001 | 提供 Repository-level `validate` 能力 | P1 | 單一命令可彙整啟用的檢查 |
| CF-002 | 提供 `explain <rule-id>` 能力 | P1 | 顯示理由、適用性、執行機制與文件位置 |
| CF-003 | 提供 `inventory` 能力 | P1 | 列出專案、Profile、角色映射、成品與版本資訊 |
| CF-004 | 提供 `report` 能力 | P1 | 可輸出 Console、JSON、Markdown；SARIF 依機制整合 |
| CF-005 | Status 必須支援 `passed`、`failed`、`not-evaluated`、`not-applicable`、`environment-blocked`、`owner-deferred` | P1 | Schema 與 CLI Exit Code 測試通過 |
| CF-006 | CLI 必須提供穩定 Exit Code | P1 | CI 能區分規則失敗、環境阻擋與設定錯誤 |
| CF-007 | CLI 不得自行重寫 Roslyn Semantic Rules | P1 | Semantic Diagnostic 由 Analyzer Output 匯入 |
| CF-008 | 提供 Rule Coverage 與 Evaluation Coverage | P1 | 報告顯示啟用、已評估、未評估與人工審查數量 |
| CF-009 | 提供 Compatibility Check | P2 | 可比較 AI Context、Rule Catalog、Analyzer 與 Contracts 相容範圍 |
| CF-010 | Waiver／Suppression 必須可追蹤理由、Owner 與有效期限 | P2 | 過期或缺少證據的 Waiver 可使驗證失敗 |

## 9. 功能需求：設定與 Profile

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| CFG-001 | 支援啟用 Core、ASP.NET Core、Messaging、Event Sourcing 等 Profile | P0 | 非啟用 Profile 的規則不執行或標示不適用 |
| CFG-002 | 支援 Custom Role Mapping | P0 | 可指定 Interface、Base Type、Namespace、Suffix |
| CFG-003 | 支援 Rule Severity 與 Enablement | P0 | `.editorconfig` 與 Profile 合併規則有文件與測試 |
| CFG-004 | 設定必須有 Schema 與清楚錯誤訊息 | P0 | 無效 Key／Type 會得到可操作錯誤 |
| CFG-005 | 設定優先序必須固定且文件化 | P1 | Repo、Project、Profile、Rule Override 的結果可預測 |
| CFG-006 | 敏感或高成本規則不得無聲自動啟用 | P1 | Release Note 會揭露 Default 變更 |

## 10. 功能需求：文件、導入、治理與追溯

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| AD-001 | 每個 Diagnostic 必須有獨立文件 | P0 | 含理由、不合規／合規範例、適用 Profile、修正與例外 |
| AD-002 | 提供 Analyzer-only／Custom Contracts 範例 | P0 | Sample 與 Consumer Test 均能建置 |
| AD-003 | 提供 Reference Contracts 範例 | P1 | 顯示低設定 strict adoption |
| AD-004 | 提供 ASP.NET Core、Worker、Console Job 等不同宿主範例 | P1 | 證明產品不侷限 Backend |
| AD-005 | 提供當前採用與設定指引，包含從命名慣例改採明確 Mapping | P1 | 使用者可依當前文件完成角色映射；正式版前不強制提供舊 preview migration／相容實作 |
| AD-006 | 提供 Compatibility Manifest | P1 | AI Context 與 Guardrails 能聲明相容版本範圍 |
| AD-007 | 明列程式碼與文件的授權及必要歸屬 | P0 | 產品與第三方授權及必要歸屬可由 LICENSE 與 NOTICE 查核 |
| AD-008 | 文件不得宣稱所有架構品質都能自動判定 | P0 | README 與 Diagnostic Docs 清楚說明判斷邊界 |
| AD-009 | 每條公開規則必須直接說明設計依據、為何選它、代價與方法論對齊／加強 | P0 | 具外部作者／權威來源及支持界線、產品選擇、適用性、正反例與未評估／限制；工具 Diagnostic 說明證據契約，不虛構方法論歸因 |
| AD-010 | 每個可獨立採用套件必須有完整使用說明 | P0 | 含安裝／認證前提、平台、最小範例、模式與設定、責任／相依、錯誤處理及限制；Analyzer-only／custom-contract 可按其真實能力使用 |
| AD-011 | 公開 API 必須提供對人與 AI 可讀且隨包交付的 XML 文件 | P0 | 有意義的 summary 及依簽章／行為適用的 typeparam、param、returns／value、exception、remarks；涵蓋不變條件、所有權／可變性、生命週期與限制；生成、nupkg 內容及 feed consumer 可讀分別留證 |

### 10.1 已選規則政策與文件邊界

產品政策與取捨見[規則設計依據](../../docs/product/rule-design-rationale.md)，目前觸發條件見[診斷目錄](../../docs/diagnostics/README.md)。角色辨識、泛型與繼承契約、訊息角色等判定精確度的改善，依所選政策與適用範圍驗收。尚待定義的政策見[規則設計依據](../../docs/product/rule-design-rationale.md#尚未定義的政策)。

### 10.2 文件治理與雙向追溯需求

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| DG-001 | 產品範圍文件、PRD 與治理文件必須視為持續演進的受控基線 | P0 | 可辨識現行文件、適用狀態與責任歸屬 |
| DG-002 | Markdown 是文件正式來源 | P0 | 若交付 Word／PDF，由同一份 Markdown 產生，修訂回到正式來源 |
| DG-003 | 產品範圍文件與 PRD 必須使用穩定路徑與檔名 | P0 | 正式路徑為 `docs/product/README.md` 與 `.dev/product/prd.md`；版本不寫入正本檔名 |
| DG-004 | `docs/`、`rules/` 與 `.dev/` 的責任必須固定且文件化 | P0 | `docs/` 保存產品文件、`rules/` 保存機器可讀規則、`.dev/` 保存產品需求與開發治理 |
| DG-005 | 必須建立工程規範 → PRD Requirement → Rule ID → 執行機制 → 實作 → 測試／證據的正向追溯 | P0 | 每個公開 Rule 可列出完整鏈結或明確缺口狀態 |
| DG-006 | 必須建立 Diagnostic／Architecture Test／CLI Check → Rule ID → Requirement → 工程規範的反向追溯 | P0 | 所有公開執行機制都能反查規範來源；缺少來源會使文件驗證失敗 |
| DG-007 | 公開規則須揭露缺少執行機制的範圍 | P0 | 尚無機制的適用情境可查知原因，列為覆蓋缺口 |
| DG-008 | 公開 Diagnostic 須具備 Rule ID、設計來源與可觀察行為的驗證 | P0 | 發布檢查可定位需要補齊的規則、文件或測試 |
| DG-009 | 文件與審查須辨識適用狀態 | P1 | 可區分現行、草案及已取代的政策，草案中已確認的決策在其範圍內適用 |
| DG-010 | 重大產品、需求與規則變更必須可追查 | P1 | 現行正文說明有效語意與理由；Git 保存差異與決策歷史；preview 不強制舊規則並存、replacement 或 migration 結構 |
| DG-011 | Framework 採用須維持產品與規則的來源責任 | P1 | 採用範圍可由 [Framework 採用文件](../standards/framework-adoption.md) 查證，內容責任符合 DG-004 |
| DG-012 | 文件與追溯的維護責任須可辨識 | P1 | 規則或發布條件變更時能找到受影響的正式文件與驗證入口 |
| DG-013 | 重大產品邊界、Requirement 語意與文件 Ownership 變更須直接更新正式文件及可審查 Git 差異 | P1 | 現行理由、適用範圍與驗收一致；Git 可追查歷史，有獨立比較用途時才另建 ADR，不強制追加變更日誌 |
| DG-014 | Word／PDF 等衍生輸出必須可定位來源文件及其版本 | P0 | 可由衍生輸出反查 Markdown 正式來源 |
| DG-015 | 規則與方法論或明選 Framework 知識有真正衝突時，只討論受影響範圍 | P0 | 列雙方精確來源／狀態、相同適用條件、互斥要求、consumer 影響及處置選項，由產品負責人決定；較嚴政策或方法論未規定形式不自動構成衝突 |

## 11. 功能需求：Build、Packaging 與 Release

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| RL-001 | Repository 預設 `IsPackable=false` | P0 | 未核准 Project 無法被一般 `dotnet pack` 意外發布 |
| RL-002 | 發布成品必須符合允許的 Package ID 與套件相依 | P0 | 封裝檢查拒絕未允許的套件、缺漏或不同的相依 |
| RL-003 | Build 必須可重現且支援 Source Link／Symbols | P1 | Package Inspection 通過 |
| RL-004 | 每個 Candidate 必須從本機 Feed 安裝至 Consumer | P0 | 實際 `.nupkg` restore／build／test 成功 |
| RL-005 | Analyzer Package Layout 必須正確 | P0 | Analyzer 以 Analyzer Asset 載入，不成為 Runtime Reference |
| RL-006 | Release 必須附 Checksum、Inventory、當前使用與變更說明及平台／成品矩陣 | P1 | 發布證據可定位不可變成品與支援範圍；preview 不強制 migration scaffolding，正式版後依所選維護政策 |
| RL-007 | Validation、Feed Publication、Hosted Release 與下載後驗證是不同 Checkpoint | P1 | Release Workflow 有獨立狀態與失敗處理 |
| RL-008 | 公開發布須確認實際使用材料的授權與歸屬 | P0 | 權利未確認的內容會阻擋公開發布 |
| RL-009 | 所有可建置、可發布的產品原始碼必須集中於 `src/`，並依 `Contracts`、`Analyzers`、`ArchitectureTesting`、`Conformance` 等能力分組 | P0 | 可發布的產品 Project 位於 `src/`；對應測試 Project 位於 `tests/<Capability>/` 且不屬於發布成品 |
| RL-010 | 產品 Project、預設 Assembly Name 與根 Namespace 必須使用 `ArchPrecept.<Capability>` 階層；測試 Project 加 `.Tests` | P0 | Repository 不含產品身分中的 `Lab`、`BuildingBlocks` 或 `Backend`；命名檢查不建立或推定 NuGet Package ID |
| RL-011 | 首個不帶任何 prerelease 後綴的公開版本必須通過規則理由、完整使用與 API XML 交付 gate | P0 | AD-009～AD-011 逐項有實際證據；XML 內容、生成、nupkg 隨包、feed consumer 可讀與語意審查分別驗收，不以現有 preview pipeline 已通過推定完成 |

## 12. 非功能需求

| ID | 類別 | 需求 |
|---|---|---|
| NFR-001 | 效能 | Analyzer 必須使用 Concurrent Execution、共用分類結果並避免重複全域掃描；建立大型 Solution Benchmark |
| NFR-002 | 決定性 | 同一 Source、Config 與 Tool Version 必須產生相同 Diagnostic 與 Report |
| NFR-003 | 離線 | Analyzer、Architecture Testing 與核心 Conformance Validation 不得依賴外部網路服務 |
| NFR-004 | 跨平台 | 支援產品聲明的 Windows、Linux 與 macOS .NET SDK 環境 |
| NFR-005 | 可測試性 | 每個 Rule、Classifier Evidence、Config Merge 與 Report Status 都必須有自動測試 |
| NFR-006 | 可維護性 | 規則分類、Diagnostic Descriptor、Known Type 與 Config Parser 必須模組化，不得集中成單一巨型 Analyzer |
| NFR-007 | 可解釋性 | 所有失敗必須提供 Rule ID、原因、角色 Evidence 與 Help Link |
| NFR-008 | 相容性 | Public API、Diagnostic Behavior、Config Schema 與 Report Schema 皆須版本化 |
| NFR-009 | 安全性 | CI Workflow 採最小權限；發布 Token、Signing Key 與秘密不得寫入 repo |
| NFR-010 | 品質 | Warning-as-error、Nullable、Deterministic Build 與 Package Content Check 應納入自身 CI |
| NFR-011 | 可存取性 | 文件使用結構化 Heading、表格標題與可閱讀的程式碼範例 |
| NFR-012 | 語言 | 對外文件至少能維持英文技術識別；繁體中文文件不得使用不一致的中國技術用語 |
| NFR-013 | 追溯完整性 | 公開規則、Diagnostic 與測試的追溯關係必須可由機器驗證，缺口不得無聲忽略 |
| NFR-014 | 文件一致性 | Markdown 正本與衍生輸出不得出現內容分叉；Repository 應能判定輸出是否來自目前正本版本 |

## 13. 整體驗收

- 核心 Analyzer 以同一角色分類機制支援明確映射、內建及繼承契約。驗收涵蓋可觸發的正反例、僅命名慣例及無關同名型別，並核對預期評估範圍、角色證據與未評估原因。
- 公開 Diagnostic 可查到規則識別碼、理由、可採用的修正與適用範圍。規則欄位、設定結構、實作與文件對齊。無執行機制的規劃規則標示覆蓋缺口。
- Reference Contracts 各自可採用並符合其相依與行為契約。以實際 `.nupkg` 驗證所選 consumer 模式，涵蓋 Analyzer-only、自有契約與參考契約。
- 依[版本與支援政策](../../docs/releases/version-policy.md)及[發布流程](../releases/cicd-plan.md)驗收當版成品。首個無後綴版本另按 AD-009～AD-011、RL-011 與[首個正式版條件](../releases/cicd-plan.md#首個正式版條件)驗收逐規則說明、完整套件使用與隨包 XML 交付。
- 後續工具須按自身實際支援的架構或 CI 範圍驗收。Conformance 報告須區分通過、失敗、尚未評估、不適用、環境阻擋及擁有者延後。

需求 ID 在重排時保留。需求、規則或驗收語意變更時，更新適用條目與對應來源。[文件治理與規則追溯](../governance/document-governance-and-traceability.md)
