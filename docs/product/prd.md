---
title: "ArchPrecept"
subtitle: "產品需求文件（PRD）"
author: "產品負責人：YuChia Wei"
date: "2026-10-21"
lang: "zh-TW"
document_id: "DOC-PRD"
document_version: "0.7"
status: "draft"
last_reviewed: "2026-10-25"
canonical_path: "docs/product/prd.md"
canonical: true
source_format: "markdown"
owners:
  - "YuChia Wei"
related_documents:
  - "DOC-PRODUCT-PLAN"
  - "DOC-GOV-TRACEABILITY"
supersedes: "0.6"
---

# 目錄

```text
文件控制
執行摘要

產品需求文件（PRD）
├─ 1. PRD 概述
├─ 2. 使用者故事
├─ 3. 功能需求：Rule Model
├─ 4. 功能需求：Architecture Role Classification
├─ 5. 功能需求：Roslyn Analyzer
├─ 6. 功能需求：Reference Contract Libraries
├─ 7. 功能需求：Architecture Testing
├─ 8. 功能需求：Conformance Tool
├─ 9. 功能需求：設定與 Profile
├─ 10. 功能需求：文件、導入、治理與追溯
├─ 11. 功能需求：Build、Packaging 與 Release
├─ 12. 非功能需求
├─ 13. 核心資料模型
├─ 14. 測試策略
├─ 15. MVP 驗收條件
└─ 16. Definition of Done

附錄
├─ A. 目前程式碼基線
├─ B. 需求識別碼
├─ C. 詞彙表
└─ D. PRD 維護方式
```

# 文件控制

> **文件狀態：需求基線草案（Requirements Baseline Draft）**<br>
> **目標 GitHub Repository：** `YuChia-Wei/ArchPrecept`<br>
> **版本：** `0.7`<br>
> **上游文件：** 《ArchPrecept 產品規劃書》<br>
> **適用狀態：** 全文件仍為 draft；已確認的產品政策及本文明定的文件交付要求各在其範圍內適用，不代表全部 PRD 已核准、完成或可立即發布。

| 欄位 | 內容 |
|---|---|
| 文件 ID | `DOC-PRD` |
| 建議正式路徑 | `docs/product/prd.md` |
| 正式來源 | Markdown；Word／PDF 僅為衍生輸出 |
| 文件目的 | 將產品規劃轉換為可追蹤、可測試且具有明確驗收標準的產品需求 |
| 適用對象 | 產品負責人、維護者、開發者、Reviewer、測試與 Release 維護者 |
| 工作名稱 | `ArchPrecept` |
| Repository 名稱 | `YuChia-Wei/ArchPrecept` |
| 需求範圍 | Rule Model、Role Classification、Analyzer、Reference Contracts、Architecture Testing、Conformance、設定、文件治理、雙向追溯、打包與 Release |
| 關聯治理文件 | 《ArchPrecept 文件治理與規則追溯規範》 |
| 需求治理 | Requirement ID 不因重排而變更；需求變更同步驗收與證據。Preview 不考慮向後相容，見版本政策 |
| 本版排除 | 具體版本發布授權、商標權利確認、最終定價與商業化策略 |

產品名稱與程式碼前綴採 ArchPrecept，本機目錄 `dotnet-architecture-kit` 保留。現行套件與交付入口見 [README](../../README.md)，Messaging 與 ES 契約見[訊息責任邊界](messaging-contract.md)及[Event Sourcing 核心契約](event-sourcing-contract.md)。首個不帶 `-preview` 公開版本之前不考慮向後相容；可直接調整 ID、schema、defaults 與 API，不強制維持 legacy／replacement／migration scaffolding。已發布 bytes 不可覆寫，正式版維護承諾仍待選定。

# 執行摘要

本 PRD 將《ArchPrecept 產品規劃書》中的產品方向，轉換為可實作與驗收的功能需求、非功能需求、資料模型、測試策略及 MVP 完成條件。

首個外部可用 Prerelease 的核心，不是增加更多零散 Diagnostic，而是先完成中央 Architecture Role Classifier、Analyzer 與 Reference Contract Libraries 的相依解耦，以及 Analyzer-only／Custom Contracts 的完整 Consumer Test。產品必須證明：即使採用端不使用本產品定義的 `IAggregateRoot`、`IUseCase` 或 Repository 介面，仍可透過明確角色映射執行主要架構規則。

本 PRD 同時要求所有規則具備版本內唯一的 Rule ID、可解釋的角色判定 Evidence、明確的機械化程度及 `not-evaluated` 狀態，以避免把「沒有 Diagnostic」誤認為「已完成合規驗證」。來源、授權與 Ownership 的產品決策由產品規劃書第 14 章管理；本文件保留對應的發布閘門與驗收要求。

產品規劃書與 PRD 本身也是受治理的產品資產。Repository 內必須以 Markdown 維持單一正式來源，並讓工程規範、Requirement、Rule ID、Diagnostic／Architecture Test、實作與測試形成雙向追溯。未來安裝 AI Collaboration Framework 時，應以 Gap Analysis 建立 `.dev/` 開發治理層，而不是以搬移產品文件取代需求與規則對齊。

## 1. PRD 概述

### 1.1 產品目標

建立一套可由 .NET Repository 漸進採用的工程護欄，將軟體工程規範轉換為：

- 穩定的 Rule Model；
- 可獨立使用的 Roslyn Analyzer；
- 可設定、可解釋的架構角色分類；
- 選配且低相依的 Reference Contracts；
- Cross-project Architecture Testing；
- Repository-level Conformance Evidence。

### 1.2 MVP 定義

MVP 指首個可供外部 Repository 實際安裝評估的 Prerelease，至少包含：

- Rule Catalog 與 Rule／Diagnostic Mapping；
- 共用 Architecture Role Classifier；
- Analyzer-only／Custom Contracts 支援；
- Built-in Reference Contracts 辨識；
- 既有核心 Diagnostic 的重構版本；
- Domain、Application、Messaging 參考契約候選；
- Consumer Test Matrix；
- Diagnostic 文件與相容性基線；
- 可重現打包與本機 Feed 驗證；
- 產品規劃書、PRD 與文件治理規劃的穩定 Markdown 正本；
- 公開 Diagnostic、Rule、工程規範與測試的第一版雙向 Traceability Mapping。

Architecture Testing 與完整 Conformance CLI 可在 MVP 後續里程碑完成，但其設定與 Rule Model 必須在 MVP 架構中預留。

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
| RC-009 | Classifier 必須避免以簡單名稱誤認不同 Namespace／Assembly 的型別 | P0 | `UnrelatedNaming.FalsePositiveTests` 全數通過 |
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
| AN-007 | Analyzer 必須啟用 Concurrent Execution，並建立效能基線 | P1 | Benchmark 記錄於 Release Evidence |
| AN-008 | Core Rule 不得預設依賴 ASP.NET Core、EF Core、Wolverine 或 MediatR | P0 | Core Consumer 不需安裝上述 Framework |
| AN-009 | ASP.NET Core 與 Messaging 規則必須由 Profile 控制 | P1 | 非相關宿主不會收到 Controller／Message Handler Diagnostic |
| AN-010 | Diagnostic 訊息必須指出被判定角色與違反原因 | P0 | 測試驗證 Message Argument 與 Evidence |
| AN-011 | Code Fix 僅能用於低風險、語意明確的修正 | P2 | 高風險架構重構不提供自動 Code Fix |
| AN-012 | 現有 Diagnostic 重構後不得發生未記錄的 ID 或語意變更 | P0 | Baseline／Golden Tests 通過 |

## 6. 功能需求：Reference Contract Libraries

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| CT-001 | Reference Contracts 必須是選配能力 | P0 | Analyzer-only Consumer 可完整 restore／build |
| CT-002 | Domain Contracts 應提供穩定的 Entity、Aggregate Root、Value Object 與 Domain Event 語意 | P1 | Public API Review、Unit Tests 與文件完成 |
| CT-003 | Application Contracts 應提供 Use Case、Repository Port、Query Port 與必要結果型別 | P1 | 不引用 Transport／UI Framework 型別 |
| CT-004 | 訊息契約與 Application 發布 port 必須維持 Transport-neutral | P1 | 不公開 Wolverine、Kafka、RabbitMQ 或 ASP.NET 型別；[責任、相依與遷移](messaging-contract.md) |
| CT-005 | Event Sourcing 支援必須明確定義 Replay、Pending Event、Version 與 Commit 語意 | P1 | 行為測試涵蓋重播、不重複記錄與版本檢查；[核心契約與驗收案例](event-sourcing-contract.md) |
| CT-006 | Contracts 不得相依 Analyzer、Architecture Testing 或 Conformance | P0 | Dependency Graph 驗證通過 |
| CT-007 | Core Contracts 不得成為完整 Runtime Framework | P0 | 不包含 Outbox、ORM、Bus、DI Scanning 等具體實作 |
| CT-008 | Public API 變更必須說明當前契約與行為差異 | P1 | Candidate 可列出受影響 API 與驗證；首個 non-preview 公開版本前不以維持舊 preview 相容為 gate |
| CT-009 | 參考契約的維護 Ownership 與實際程式碼來源必須可稽核 | P1 | Provenance 記錄含來源 Commit（適用時）、授權與此 repo 的 Canonical Owner；不作為規則方法論歸因 |

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
| AD-007 | 提供 Provenance Record | P0 | 程式碼／文件來源、授權與維護 Ownership 可稽核；不把 provenance 或私人討論當成產品規則設計理由 |
| AD-008 | 文件不得宣稱所有架構品質都能自動判定 | P0 | README 與 Diagnostic Docs 清楚說明判斷邊界 |
| AD-009 | 每條公開規則必須直接說明設計依據、為何選它、代價與方法論對齊／加強 | P0 | 具外部作者／權威來源及支持界線、產品選擇、適用性、正反例與未評估／限制；工具 Diagnostic 說明證據契約，不虛構方法論歸因 |
| AD-010 | 每個可獨立採用套件必須有完整使用說明 | P0 | 含安裝／認證前提、平台、最小範例、模式與設定、責任／相依、錯誤處理及限制；Analyzer-only／custom-contract 可按其真實能力使用 |
| AD-011 | 公開 API 必須提供對人與 AI 可讀且隨包交付的 XML 文件 | P0 | 有意義的 summary 及依簽章／行為適用的 typeparam、param、returns／value、exception、remarks；涵蓋不變條件、所有權／可變性、生命週期與限制；生成、nupkg 內容及 feed consumer 可讀分別留證 |

### 10.1 已選規則政策與文件邊界

規則細節集中於[規則設計依據](rule-design-rationale.md)、[方法論來源](methodology-references.md)及各 Diagnostic 文件；`rules/catalog.xml` 仍是唯一產品機器規則來源。保留的政策包括：Aggregate Repository 的 identity-load／save 與 read-side query 分工、純 Mapper 的 static-only 與無 I/O／編排角色、預設 Controller → UseCase、薄交付 Handler、專屬 Input／required CancellationToken，以及明選的純查詢例外。

這些是產品對責任邊界的選擇，不能宣稱方法論一律要求同樣形式；也不能只因更嚴格就自動視為方法論矛盾。自訂契約提供角色辨識能力，不表示任意契約形狀均符合所選政策。真實 symbols、有效契約、角色視圖與訊息範圍的精確化，必須保留已接受語意，並以對應證據驗收。

全面禁止 Aggregate static factory 及 MQ-only 尚缺完整設計理由與適用條件，保留為獨立待決議題；本文件不以缺證據撤銷既有選擇，也不擴張成新 Analyzer 行為。API XML 與使用文件的完整交付仍須另行實作及驗收。


### 10.2 文件治理與雙向追溯需求

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| DG-001 | 產品規劃書、PRD 與治理文件必須視為持續演進的受控基線 | P0 | 每份文件都有狀態、版本、Owner、最後審查日期與變更原則 |
| DG-002 | Repository 內的 Markdown 必須是正式來源，Word／PDF 只能是衍生輸出 | P0 | CI 或發布流程可由 Markdown 產生輸出；不存在兩份人工維護的正本 |
| DG-003 | 產品規劃書與 PRD 必須使用穩定路徑與檔名 | P0 | 正式路徑為 `docs/product/product-plan.md` 與 `docs/product/prd.md`；版本不寫入正本檔名 |
| DG-004 | `docs/`、`rules/` 與未來 `.dev/` 的責任必須固定且文件化 | P0 | `docs/` 保存產品文件、`rules/` 保存機器可讀規則、`.dev/` 保存需求拆解與執行證據 |
| DG-005 | 必須建立工程規範 → PRD Requirement → Rule ID → 執行機制 → 實作 → 測試／證據的正向追溯 | P0 | 每個公開 Rule 可列出完整鏈結或明確缺口狀態 |
| DG-006 | 必須建立 Diagnostic／Architecture Test／CLI Check → Rule ID → Requirement → 工程規範的反向追溯 | P0 | 所有公開執行機制都能反查規範來源；缺少來源會使文件驗證失敗 |
| DG-007 | 可機械化規範若尚無執行機制，必須被報告為 Coverage Gap | P0 | Gap Report 不得把缺少實作的規範視為已通過或不適用 |
| DG-008 | Diagnostic 若缺少 Rule ID、規範來源或測試映射，必須被視為發布阻擋 | P0 | CI 對公開 Diagnostic 的追溯完整性執行 Gate |
| DG-009 | 文件生命週期必須支援 `draft`、`in-review`、`approved`、`active`、`superseded`、`archived` | P1 | 文件 Metadata 與審查流程使用一致狀態定義 |
| DG-010 | 重大產品、需求與規則變更必須可追查 | P1 | 現行正文說明有效語意與理由；Git 保存差異與決策歷史；preview 不強制舊規則並存、replacement 或 migration 結構 |
| DG-011 | AI Collaboration Framework 導入必須先執行語意盤點與 Gap Analysis | P1 | 導入計畫能說明哪些內容保留於 `docs/`／`rules/`，哪些建立於 `.dev/`，且不以無差別搬移作為驗收 |
| DG-012 | 文件與追溯資產必須有固定 Owner 與審查節奏 | P1 | Release Candidate、重大規則變更與 Framework 升級前會執行一致性審查 |
| DG-013 | 重大產品邊界、Requirement 語意與文件 Ownership 變更須直接更新正式文件及可審查 Git 差異 | P1 | 現行理由、適用範圍與驗收一致；Git 可追查歷史，有獨立比較用途時才另建 ADR，不強制追加變更日誌 |
| DG-014 | Word／PDF 等衍生輸出必須記錄來源 Document ID、版本與 Commit／Tag | P0 | Export 可反查 Markdown 正式來源，且不得標示為 canonical |
| DG-015 | 規則與方法論或明選 Framework 知識有真正衝突時，只討論受影響範圍 | P0 | 列雙方精確來源／狀態、相同適用條件、互斥要求、consumer 影響及處置選項，由產品負責人決定；較嚴政策或方法論未規定形式不自動構成衝突 |

## 11. 功能需求：Build、Packaging 與 Release

| ID | 需求 | 優先 | 驗收標準 |
|---|---|---|---|
| RL-001 | Repository 預設 `IsPackable=false` | P0 | 未核准 Project 無法被一般 `dotnet pack` 意外發布 |
| RL-002 | `eng/package-inventory.yml` 是可發布 Project 的允許清單 | P0 | CI 拒絕 Inventory 外的 Package Artifact |
| RL-003 | Build 必須可重現且支援 Source Link／Symbols | P1 | Package Inspection 通過 |
| RL-004 | 每個 Candidate 必須從本機 Feed 安裝至 Consumer | P0 | 實際 `.nupkg` restore／build／test 成功 |
| RL-005 | Analyzer Package Layout 必須正確 | P0 | Analyzer 以 Analyzer Asset 載入，不成為 Runtime Reference |
| RL-006 | Release 必須附 Checksum、Inventory、當前使用與變更說明及平台／成品矩陣 | P1 | 發布證據可定位不可變成品與支援範圍；preview 不強制 migration scaffolding，正式版後依所選維護政策 |
| RL-007 | Validation、Feed Publication、Hosted Release 與下載後驗證是不同 Checkpoint | P1 | Release Workflow 有獨立狀態與失敗處理 |
| RL-008 | Provenance 與授權檢查未通過不得公開發布 | P0 | 發布 gate 有明確來源／權利阻擋條件；不以規則理由文字取代逐檔授權檢查 |
| RL-009 | 所有可建置、可發布的產品原始碼必須集中於 `src/`，並依 `Contracts`、`Analyzers`、`ArchitectureTesting`、`Conformance` 等能力分組 | P0 | Repository 結構檢查拒絕位於 `src/` 外的產品 Project；測試 Project 位於對應的 `tests/<Capability>/` 且不進入發布 Inventory |
| RL-010 | 產品 Project、預設 Assembly Name 與根 Namespace 必須使用 `ArchPrecept.<Capability>` 階層；測試 Project 加 `.Tests` | P0 | Repository 不含產品身分中的 `Lab`、`BuildingBlocks` 或 `Backend`；命名檢查不建立或推定 NuGet Package ID |
| RL-011 | 首個不帶 `-preview` 後綴的公開版本必須通過規則理由、完整使用與 API XML 交付 gate | P0 | AD-009～AD-011 逐項有實際證據；XML 內容、生成、nupkg 隨包、feed consumer 可讀與語意審查分別驗收，不以現有 preview pipeline 已通過推定完成 |

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

## 13. 核心資料模型

### 13.1 Rule Definition（概念）

```yaml
rule_id: ENG-USECASE-001
title: Use Case must expose one ExecuteAsync operation
category: application
applicability:
  profiles:
    - core
  roles:
    - use-case
enforcement:
  level: machine-enforced
  mechanisms:
    - type: roslyn-analyzer
      diagnostic_id: <diagnostic-id>
coverage: full
source:
  design_rationale: docs/product/rule-design-rationale.md
  external_references: docs/product/methodology-references.md
  policy_kind: product-strengthening
lifecycle:
  introduced: 0.1.0
  status: active
```

### 13.2 Role Mapping（概念）

```yaml
roles:
  aggregate-root:
    interfaces:
      - Contoso.Domain.IAggregateRoot
    base_types:
      - Contoso.Domain.AggregateBase

  use-case:
    interfaces:
      - Contoso.Application.IApplicationOperation
    suffixes:
      - UseCase
      - Operation

conventions:
  enabled: true
  default_confidence: convention
```

### 13.3 Evaluation Result（概念）

```yaml
rule_id: ENG-USECASE-001
status: failed
role:
  type: Contoso.Application.CreateOrderOperation
  role: use-case
  evidence: configured-contract
  confidence: explicit
mechanism:
  type: roslyn-analyzer
  diagnostic_id: <diagnostic-id>
message: ExecuteAsync must end with a non-optional CancellationToken
```

### 13.4 Traceability Record（概念）

```yaml
traceability_id: TRACE-ENG-USECASE-001
product_requirement:
  - RC-003
  - AN-010
normative_source:
  document: docs/architecture/use-case-boundaries.md
  section: use-case-contract
rule:
  rule_id: ENG-USECASE-001
  coverage: full
enforcement:
  - type: roslyn-analyzer
    diagnostic_id: <diagnostic-id>
implementation:
  - src/Analyzers/ArchPrecept.Analyzers.Core/Rules/Application/UseCaseContractAnalyzer.cs
tests:
  - tests/Analyzers/UseCaseContractAnalyzerTests.cs
  - tests/Consumers/AnalyzerOnly.CustomContracts/
status: active
```

此資料模型必須能表達：完整追溯、部分覆蓋、需人工審查、尚未實作及已淘汰等狀態。

### 13.5 Document Metadata（概念）

```yaml
title: ArchPrecept PRD
document_id: DOC-PRD
document_version: 0.4
status: draft
owners:
  - YuChia Wei
canonical_path: docs/product/prd.md
canonical: true
source_format: markdown
last_reviewed: 2026-10-18
supersedes: 0.3
related_documents:
  - DOC-PRODUCT-PLAN
  - DOC-GOV-TRACEABILITY
```

Document Metadata 必須符合以下規則：

- `document_id` 在檔名、章節或儲存位置調整後仍保持穩定；
- 同一 Document ID 不得同時存在兩份 active canonical source；
- `document_version` 是文件基線版本，不必與產品版本相同；
- Word／PDF 等 derived export 必須記錄來源 Document ID、版本與 Commit／Tag；
- `superseded`／`archived` 文件必須指出替代文件或保留原因。

## 14. 測試策略

### 14.1 Unit Tests

- Role Classifier；
- Symbol／Type Matching；
- Rule Descriptor；
- Configuration Merge；
- Report Status 與 Exit Code；
- Contract 行為與不變條件。

### 14.2 Analyzer Tests

- 合規／不合規 Source；
- Diagnostic Location、Message Argument 與 Severity；
- Built-in Contracts；
- Custom Contracts；
- Inherited Contracts；
- Convention-only；
- Unrelated Naming；
- Generated Code；
- Framework-specific Profile。

### 14.3 Consumer Tests

```text
AnalyzerOnly.CustomContracts
AnalyzerOnly.ConventionBased
AnalyzerWithReferenceContracts
ContractsOnly
UnrelatedNaming.FalsePositiveTests
FullAdoption
```

Consumer Test 必須對實際 Pack 後的 Artifact 執行，不可只使用 ProjectReference 模擬發布成品。

### 14.4 版本與當前契約檢查

- Public API Baseline；
- Diagnostic ID／Default Severity；
- Rule Catalog Schema；
- Config Schema；
- Report JSON Schema；
- 支援 Roslyn／SDK Matrix。

### 14.5 Performance Tests

- 小型、一般及大型 Solution；
- 角色分類 Cache 命中；
- 增加規則數量後的增量 Build 影響；
- Diagnostic 數量與記憶體使用。

### 14.6 文件治理與追溯測試

- 產品規劃書、PRD 與治理文件 Metadata／狀態檢查；
- Markdown 正本與 Word／PDF 衍生輸出的來源版本檢查；
- Rule Catalog、Diagnostic Descriptor、規範文件與測試映射完整性；
- 公開 Diagnostic 缺少 Rule ID、規範來源或測試時的失敗案例；
- 可機械化規範缺少執行機制時的 Coverage Gap 報告；
- Requirement／Rule 的當前語意、Git 差異與適用版本政策；
- AI Collaboration Framework 導入前後的責任與路徑映射驗證。

## 15. MVP 驗收條件

首個外部可用 Prerelease 必須同時符合：

1. Repository 名稱與產品定位已更新為 `ArchPrecept`。
2. Analyzer 不直接或間接相依任何 Reference Contract Library。
3. Custom Contracts Consumer 可透過明確 Mapping 觸發 Aggregate、Repository 與 Use Case 核心規則。
4. Reference Contracts Consumer 可在低設定下觸發相同規則。
5. 所有核心 Analyzer 使用共用 Role Classifier。
6. Role Match 包含 Evidence 與 Confidence。
7. Convention-only 規則不會對已知無關名稱產生誤報。
8. 未辨識角色時回報 `not-evaluated`。
9. 所有公開 Diagnostic 都有 Rule ID、Help Link、文件與測試。
10. Rule Catalog 通過 Schema Validation。
11. Domain／Application／Messaging Public API 已完成第一輪 Review。
12. Package Inventory、Deterministic Build、Package Inspection 與 Local Feed Consumer Test 通過。
13. 公開成品的來源／授權檢查與 Provenance 記錄均已完成。
14. Release Artifact 附 Checksum、Inventory、當前使用與變更說明，以及平台／成品矩陣。
15. NuGet Package ID 可仍維持待決策，但發布前必須正式核准。
16. `docs/product/product-plan.md`、`docs/product/prd.md` 與 `docs/governance/document-governance-and-traceability.md` 已成為穩定 Markdown 正本。
17. 所有公開 Diagnostic 均具備工程規範、Requirement、Rule ID、實作與測試的雙向追溯。
18. 可機械化但尚未實作的規範會被列入 Coverage Gap，不得顯示為 Passed。
19. Word／PDF 文件可由 Markdown 正本受控產生，且不作為並行人工正本。

## 16. Definition of Done

一項規則、契約或工具功能只有在以下條件完成後才視為 Done：

- 有明確 Requirement／Rule ID；
- 有實作與自動測試；
- 有正向、反向及誤報案例；
- 有文件、修正指引與例外說明；
- 有規範、Requirement、Rule、執行機制、實作與測試的追溯更新；
- 有當前契約與差異說明；正式版前不以維持 preview 相容為條件；
- 有 Provenance／Ownership 更新；
- 有 Consumer 或 Integration Evidence；
- 未造成 Analyzer 與 Contracts 之間的禁止相依；
- 有適用的完整使用說明、規則理由與有意義 API XML；XML 交付須有 nupkg 及 consumer 可讀證據；
- Markdown 正本與衍生輸出保持一致；
- CI、Package Validation、文件追溯驗證與 Repository 自檢通過。

# 附錄 A：目前程式碼基線

目前 Repository 程式碼基線包含：

```text
src/
├─ Contracts/
│  ├─ ArchPrecept.Contracts.Domain
│  ├─ ArchPrecept.Contracts.Application
│  ├─ ArchPrecept.Contracts.Messaging
│  ├─ ArchPrecept.Contracts.Application.Messaging
│  └─ ArchPrecept.Contracts.Domain.EventSourcing
└─ Analyzers/
   └─ ArchPrecept.Analyzers.Core

tests/
├─ Analyzers/
│  └─ ArchPrecept.Analyzers.Core.Tests
└─ Contracts/
   └─ ArchPrecept.Contracts.Domain.EventSourcing.Tests

docs/
samples/
```

現行已交付能力、Diagnostic 與套件狀態以 [README](../../README.md)、[診斷目錄](../diagnostics/README.md)、[設定與角色政策](analyzer-policy.md)及各版本發布紀錄為準。歷史測試結果不視為本文件本次驗收。

`src/ArchitectureTesting/` 與 `src/Conformance/` 仍是規劃能力，不以空專案預先佔位。Analyzer 與 Contracts 維持獨立；持續以共用 Classifier、明確 Mapping、唯一 Rule Catalog 及真實 package consumer 證據驗證其聲明範圍。

# 附錄 B：需求識別碼

| 前綴 | 需求領域 |
|---|---|
| `RM` | Rule Model |
| `RC` | Architecture Role Classification |
| `AN` | Roslyn Analyzer |
| `CT` | Reference Contract Libraries |
| `AT` | Architecture Testing |
| `CF` | Conformance Tool |
| `CFG` | Configuration／Profile |
| `AD` | Documentation、Adoption 與 Traceability |
| `DG` | Document Governance 與 Bidirectional Traceability |
| `RL` | Build、Packaging 與 Release |
| `NFR` | 非功能需求 |

需求 ID 不因文件重新排序、章節拆分或名稱調整而重編；需求語意與移除理由由當前文件及 Git 保存。Rule／Diagnostic 的 Preview 調整依版本政策，不因本段引入強制相容或替代結構。

# 附錄 C：詞彙表

| 詞彙 | 定義 |
|---|---|
| Guardrails | 透過規則、Analyzer、測試與 CI 限制已知不合規行為，但不宣稱取代所有工程判斷 |
| Rule Catalog | 本產品工程規則 ID、語意、適用性與執行映射的機器可讀正式來源 |
| Architecture Role | Aggregate Root、Domain Entity、Use Case、Repository、Handler 等被規則使用的角色 |
| Role Classifier | 根據設定、契約、繼承、結構或慣例判定 Architecture Role 的共用元件 |
| Evidence | Role Match 的判定來源，例如 explicit mapping、built-in contract 或 naming convention |
| Reference Contracts | 本產品提供、但非 Analyzer 必要相依的 Domain／Application／Messaging 契約與基底型別 |
| Architecture Testing | 透過測試驗證跨 Project、Assembly 或 Namespace 的依賴邊界 |
| Conformance | 彙整規則適用性、執行結果、覆蓋率、相容性與 CI 證據的符合性驗證能力 |
| Profile | 一組針對 Core、ASP.NET Core、Messaging、Event Sourcing 等情境的規則及設定集合 |
| `not-evaluated` | 規則未被可靠執行，不能視為通過 |
| Canonical | 在本產品範圍內作為正式來源的技術描述詞，不作為品牌名稱 |

# 附錄 D：PRD 維護方式

本 PRD 以《ArchPrecept 產品規劃書》作為上游產品決策基線，並以《文件治理與規則追溯規範》作為維護規則：

- 產品願景、能力邊界、Repository 策略、來源、授權與 Ownership 由產品規劃書管理；PRD 不重複維護相同決策內容。
- Repository 正本使用穩定路徑 `docs/product/prd.md`，Markdown 為正式來源；版本由文件控制與 Git 歷史管理。
- Requirement ID 不因章節重新排序而變更；新增、變更或移除直接更新理由及驗收，歷史由 Git 保存。
- PRD 的產品需求應進一步拆解為未來 `.dev/requirements/` 與 `.dev/specs/`，但 PRD 本身不作為單次工作項目或實作日誌。
- 每次重大需求或規則變更，必須同步追溯工程規範、Rule ID、Diagnostic／Architecture Test、實作與測試。
- NuGet Package Naming 決策完成後，應新增獨立 Naming／Identity 文件或附錄，不改寫不依賴名稱的需求語意。
