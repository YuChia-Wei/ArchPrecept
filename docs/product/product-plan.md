---
title: "ArchPrecept"
subtitle: "產品規劃書"
author: "產品負責人：YuChia Wei"
date: "2026-10-21"
lang: "zh-TW"
document_id: "DOC-PRODUCT-PLAN"
document_version: "0.7"
status: "draft"
last_reviewed: "2026-10-21"
canonical_path: "docs/product/product-plan.md"
canonical: true
source_format: "markdown"
owners:
  - "YuChia Wei"
related_documents:
  - "DOC-PRD"
  - "DOC-GOV-TRACEABILITY"
---

# 目錄

```text
文件控制
執行摘要

產品規劃書
├─ 1. 產品背景與問題陳述
├─ 2. 產品願景、使命與定位
├─ 3. 已確認的關鍵決策
├─ 4. 產品設計原則
├─ 5. 目標使用者與待完成工作
├─ 6. 採用模式
├─ 7. 產品能力架構
├─ 8. 與 AI Collaboration Framework 的責任邊界
├─ 9. 相依與分層原則
├─ 10. Git Repository 策略
├─ 11. 預期 Git repo 結構
├─ 12. 預期發布成品
├─ 13. 版本與相容性治理
├─ 14. 來源、授權與 Ownership 規劃
├─ 15. 成功衡量指標
├─ 16. Roadmap
├─ 17. 主要風險與緩解措施
├─ 18. 非目標
└─ 19. 待決策事項

附錄
├─ A. 目前程式碼基線
├─ B. 詞彙表
├─ C. 主要來源與參考
└─ D. 文件治理與持續演進
```

# 文件控制

> **文件狀態：決策基線草案（Decision Baseline Draft）**<br>
> **目標 GitHub Repository：** `YuChia-Wei/ArchPrecept`<br>
> **版本：** `0.7`<br>
> **重要排除事項：** 產品品牌與 Project／Assembly／Namespace 前綴已核准為 ArchPrecept；本文件不核准 NuGet Package ID、套件發布與商標權利。

| 欄位 | 內容 |
|---|---|
| 文件 ID | `DOC-PRODUCT-PLAN` |
| 建議正式路徑 | `docs/product/product-plan.md` |
| 正式來源 | Markdown；Word／PDF 僅為衍生輸出 |
| 文件目的 | 匯總產品定位、能力邊界、Git repo 策略與結構、採用模式、發布成品、治理原則及演進路線 |
| 適用對象 | 產品負責人、維護者、貢獻者、AI Collaboration Framework 維護者、試用團隊及潛在採用者 |
| 工作名稱 | `ArchPrecept` |
| Repository 名稱 | `YuChia-Wei/ArchPrecept` |
| Repository 模式 | 單一儲存庫（monorepo），輸出多個可獨立採用的成品 |
| 目前階段 | 初始抽取、產品邊界定義、公開契約硬化與 Analyzer 架構重整前 |
| 本版納入 | 產品背景、定位、決策、設計原則、使用者、能力架構、責任邊界、repo 結構、文件治理與追溯、發布治理、來源與 Ownership、Roadmap、風險及待決策事項 |
| 關聯治理文件 | 《ArchPrecept 文件治理與規則追溯規範》 |
| 本版排除 | NuGet Package ID 核准、商標權利確認、最終定價與商業化策略 |

# 執行摘要

`ArchPrecept` 的目的，是解決「軟體工程規範寫得很清楚，但缺乏可信、可執行、可在編譯與 CI 階段產生證據的驗證機制」這個問題。

產品不只包含 Roslyn 分析器（Roslyn Analyzer），也包含支援規則執行所需的規則模型、架構角色辨識、架構測試（Architecture Testing）、符合性驗證工具（Conformance Tool）、設定與導入資產，以及一組**選配的參考契約類別庫（Reference Contract Libraries）**。參考契約類別庫預計提供 `IAggregateRoot`、Event-Sourced Aggregate、`IUseCase`、Repository Port、Messaging Abstraction 等正式維護的介面與基底型別，但採用這些型別**不得成為使用 Analyzer 的必要條件**。

本產品與 AI Collaboration Framework 的責任分工如下：

- AI Collaboration Framework 保存工程原則、理由、取捨、例外情境、AI Skill 與人工審查方式。
- `ArchPrecept` 實作可被確定判定的規則、標準化角色模型、參考契約及機械化證據。
- 採用端 Repository 保存自己的真實架構、角色映射、啟用 Profile、例外與抑制證據。

目前建議維持**單一 repo、多個成品輸出**。Analyzer 與參考契約類別庫在套件相依上完全解耦，但放在同一 repo 共同維護，以便在同一個 Pull Request 中原子性更新規則、契約、測試、文件與相容性宣告。只有在參考契約類別庫形成獨立產品生命週期、維護責任或授權邊界時，才考慮拆分新的 repo。

首要工程優先事項不是繼續新增規則，而是建立一套共用、可設定、可解釋的**架構角色分類器（Architecture Role Classifier）**，讓 Analyzer 同時支援：

1. 使用本產品參考契約的專案；
2. 使用自行定義契約的既有專案；
3. 只採用命名與結構慣例的低設定模式；
4. 無法可靠判定時回報 `not-evaluated`，而不是錯誤宣稱通過。

產品規劃書與 PRD 是持續演進的受控產品文件，而不是一次性提案或工作日誌。Repository 內以 Markdown 作為可審查、可版控的正式來源；Word 等格式視為衍生輸出。未來導入 AI Collaboration Framework 時，`docs/` 仍保存產品層級與對外有效的文件，`rules/` 保存機器可讀規則，新增的 `.dev/` 則承接需求拆解、技術規格、工作流程與發布證據，不以無差別搬移檔案取代語意治理。

## 1. 產品背景與問題陳述

### 1.1 背景

本產品由三條實務演進路徑匯流而成：

1. **課程學習與靜態分析器演進**<br>
   完成 [馴服 AI 寫出可維護的系統：模式語言驅動開發工作坊](https://teddysoft.tw/courses/ai-coding-pl/) 課程後，依據課程中學習到的與 AI 協作的做法，配合老師提供的資源與自己的軟體工程知識、實際工作需求，進行 AI Collaboration Framework 的整理與開發。
   之後，由於 dotnet 本身就有 Roslyn Analyzer 的靜態檢查工具，所以將 dotnet 開發專用的規則檢核特別抽出來做專用的 Roslyn Analyzer。

2. **DDD 與應用程式架構契約的累積**<br>
   `IAggregateRoot`、`IDomainEntity`、Domain Event、Repository Port、Result、Event Sourcing Aggregate 等型別，來自多個 Lab Repository 中反覆實作與修正後的經驗。

3. **AI 協作規範的機械化需求**<br>
   AI Collaboration Framework 已能詳細描述 DDD、Clean Architecture、Hexagonal Architecture、Use Case、Handler、Repository、Controller 與 Messaging 邊界，但單靠文件與 Prompt，仍無法保證人員或 AI 每次實作都遵循規範。

### 1.2 核心問題

目前需要解決的問題包括：

- 規範存在，但未被建置流程強制執行。
- 純文字、檔名或 `grep` 型檢查無法正確理解 C# 型別、繼承與語意。
- 每個產品各自複製 `IAggregateRoot`、`IUseCase` 等基礎契約，造成語意與版本漂移。
- 既有專案不應為了採用 Analyzer，被迫全面改用特定 Framework 或 Base Class。
- Analyzer 若只靠名稱猜測角色，容易產生誤報（false positive）與漏報（false negative）。
- 沒有 Diagnostic 不代表規則已被評估；目前工具容易產生「零錯誤等於全部合規」的錯誤安全感。
- 規範、Diagnostic、設定、文件與 AI Context 之間缺乏穩定的可追溯關係。
- Analyzer、架構測試、Repository 檢查及 CI 結果尚無統一的符合性報告。

### 1.3 機會

透過統一的 Rule Catalog、Role Classifier、Roslyn Analyzer、架構測試與 Conformance CLI，可將工程規範轉換成：

- 可重複執行的編譯期檢查；
- 可在 CI 阻擋的品質閘門；
- 可解釋角色判定來源的 Diagnostic；
- 可追蹤規則覆蓋率與未評估範圍的報告；
- 可供新舊專案漸進採用的模組化產品；
- AI 與人工審查可共同引用的客觀證據。

## 2. 產品願景、使命與定位

### 2.1 產品願景

> 讓 .NET 團隊能把具明確脈絡的軟體工程規範，轉換為可解釋、可組合、可在編譯與 CI 執行的工程護欄，同時保留需要工程判斷的部分給人與 AI。

### 2.2 產品使命

- 將可確定判定的規範實作為可靠的 Roslyn Diagnostic、架構規則及 Repository 驗證。
- 建立規範、角色、Diagnostic、設定與文件之間的穩定追溯鏈。
- 支援既有專案自訂契約，不以採用本產品介面作為 Analyzer 使用門檻。
- 提供一組經實務驗證、但明確標示為 opinionated 的參考契約。
- 避免工具宣稱超過其實際可判定範圍。

### 2.3 產品定位

`ArchPrecept` 是：

- .NET 軟體工程規則的機械化執行產品線；
- 可獨立採用的 Roslyn Analyzer、Architecture Testing 與 Conformance Tool；
- 與 AI Collaboration Framework 配套、但可脫離 AI 工具獨立使用的開發者工具；
- 適用於 Web API、桌面應用程式、Worker、Console Job、批次工具、Message Consumer 與類別庫的宿主中立核心；
- 提供選配 Reference Contract Libraries 的單一 repo、多成品產品。

`ArchPrecept` 不是：

- DDD、Clean Architecture 或 Hexagonal Architecture 的唯一官方實作；
- 必須全面繼承特定 Base Class 才能使用的 Application Framework；
- 可自動判斷所有 Aggregate 邊界與架構取捨的「全知」驗證器；
- 專門限於 ASP.NET Core 或網站後端的工具；
- AI Prompt、Agent Skill 或 AI Context 發布 Repository 的替代品。

## 3. 已確認的關鍵決策

| 編號 | 決策 | 狀態 | 理由 |
|---|---|---|---|
| DEC-001 | GitHub repo 使用 `dotnet-engineering-guardrails` | 已確認 | 目前產品主軸已收斂為規則模型、機械化執行與符合性證據 |
| DEC-002 | 對外工作名稱使用 `.NET Engineering Guardrails` | 已由 DEC-015 取代 | 保留原工作名稱的歷史；自 2026-10-18 起採 ArchPrecept |
| DEC-003 | 維持單一 repo、多成品輸出 | 已確認 | 契約、Analyzer、Rule Catalog、測試與相容性需原子性共同演進 |
| DEC-004 | Analyzer 不得依賴 Reference Contract Libraries | 已確認 | Analyzer-only 必須是一等採用模式 |
| DEC-005 | 未採用本產品介面的專案也必須能執行核心規則 | 已確認 | 既有系統不能被迫全面遷移才能獲得驗證能力 |
| DEC-006 | Reference Contract Libraries 保留在同一 repo，定位為選配 | 暫定 | 共同演進效益大於目前拆 repo 的治理收益 |
| DEC-007 | Core 規則宿主中立，ASP.NET Core／Messaging 規則以 Profile 區分 | 已確認 | 支援桌面、Console、Worker、批次與其他宿主 |
| DEC-008 | 規則目錄是機器可讀的正式來源 | 已確認 | 避免 Diagnostic、Markdown 與設定各自漂移 |
| DEC-009 | 零 Diagnostic 不得自動等同整體合規 | 已確認 | 無法辨識或未啟用的規則應標示 `not-evaluated` |
| DEC-010 | NuGet Package ID 與 Package Prefix 延後決策 | 已確認 | Project、Assembly 與 Namespace 可先穩定能力身分，但不據此推定發布識別 |
| DEC-011 | 產品規劃書與 PRD 採受控持續演進；Markdown 為正式來源，Word 為衍生輸出 | 已確認 | 讓 Git Diff、Review、歷史追蹤與自動驗證有單一來源，避免雙向人工維護造成漂移 |
| DEC-012 | 未來安裝 AI Collaboration Framework 時保留 `docs/` 產品文件，並以 `.dev/` 補上開發治理層 | 已確認 | Framework 導入應採語意映射與 Gap Analysis，不應把產品文件無差別搬入流程目錄 |
| DEC-014 | .NET Project、預設 Assembly Name 與 Namespace 採 `EngineeringGuardrails.<Capability>` 階層 | 已由 DEC-015 取代 | 保留原命名決策；能力分層與不推定 NuGet Package ID 的原則繼續適用 |
| DEC-013 | 工程規範、PRD Requirement、Rule、執行機制、實作、測試與證據必須雙向可追溯 | 已確認 | 可識別「規範沒有工具」與「工具缺少正式規範依據」兩種治理缺口 |
| DEC-015 | 產品採 ArchPrecept；Solution 採 `ArchPrecept.slnx`；Project、預設 Assembly Name 與 Namespace 採 `ArchPrecept.<Capability>` | 已確認，2026-10-18 生效 | 取代 DEC-002 與 DEC-014 的命名；保留能力分層、公開型別簡名、Diagnostic ID 與規則行為；不核准套件發布 |
| DEC-016 | GitHub Repository 改名另行安排，本機目錄保留 `dotnet-architecture-kit` | 已確認，2026-10-18 生效 | DEC-001 仍記錄目前遠端名稱；本機目錄保留以維持 chat/Codex session 連續性，不要求與線上名稱一致 |
| DEC-017 | CT-005 的 ES 核心採同 repository、獨立 `.csproj`／NuGet 套件，ES Analyzer 留在 Core | 負責人同意，2026-10-21 | [核心契約與遷移](event-sourcing-contract.md)；完整 ES runtime 是否獨立產品另案評估；本次沒有發布授權 |
| DEC-018 | 契約按責任與能力命名；ES 採 `Contracts.Domain.EventSourcing`，Messaging 拆成訊息契約與 `Contracts.Application.Messaging` port | 負責人同意，2026-10-21 | [Messaging 邊界與遷移](messaging-contract.md)；組態由 host／adapter 擁有，只保留不發布的範例 |
| DEC-019 | 六套件採 public、目前維持 `0.0.2-preview`；初期以 `--ff-only` 合併回 main 後直接推送，由 main push 建置與發布 | 負責人同意，2026-10-21 | [初期 CI/CD](../releases/cicd-plan.md)；tag 與其他觸發方式留待後期，不新增 PR／reviewer 門檻 |

### 3.1 改名與採用端遷移

本次將 `EngineeringGuardrails.*` 改為 `ArchPrecept.*`，並將 `EngineeringGuardrails.slnx` 改為 `ArchPrecept.slnx`。既有使用端需同步更新 ProjectReference、Assembly 引用、using 與完整型別名稱並重新建置；Assembly 與公開型別身分會改變，不能視為二進位相容的更名。既有 Diagnostic ID `DBA1001`～`DBA1017`、嚴重性與規則行為不變。

DEC-015 的改名階段只處理 Repository 內部與文件。2026-10-18 後續發布準備已確認 GitHub Repository 為 `YuChia-Wei/ArchPrecept`，本機目錄 `dotnet-architecture-kit` 保留。負責人允許準備 GitHub Packages preview、CI/CD 與 PR；四個既有產品專案已 opt in 封裝為 `0.0.1-preview`。2026-10-19 License／來源檢查已完成，四套件已發布 GitHub Packages 並通過 .NET 8／10 registry 消費驗證；實際 public 可見性與原先 private 預期不符，等待負責人決定公開範圍，後續發布已停用。詳見[發布紀錄](../releases/preview.md)。未來版本政策仍待審查。

## 4. 產品設計原則

### 4.1 規範推理與機械化執行分離

AI Collaboration Framework 或人類文件負責回答「為什麼、何時、如何取捨」；Guardrails 負責回答「哪些條件可被確定判定，以及結果證據為何」。

### 4.2 Analyzer 獨立採用

安裝 Analyzer 不應自動引入 Domain、Application、Messaging 或 Event Sourcing runtime library。官方契約是內建精準辨識 Profile，不是必要相依。

### 4.3 明確設定優先於魔法猜測

架構角色判定的優先順序應為：

1. 採用端明確角色映射；
2. 內建完整 Metadata Name 契約；
3. 已設定契約的介面／基底類別繼承；
4. 強結構特徵；
5. 可設定的命名與 Namespace 慣例；
6. 無法判定。

### 4.4 不製造錯誤安全感

工具必須區分：

- `passed`
- `failed`
- `not-evaluated`
- `not-applicable`
- `environment-blocked`
- `owner-deferred`

未發現可辨識的 Use Case，不得回報「Use Case Rules Passed」。

### 4.5 可解釋性

每次角色分類與 Diagnostic 都應能說明：

- 被判定為何種架構角色；
- 判定依據為何；
- 套用哪一條 Rule ID；
- 由哪個機制執行；
- 如何修正或配置例外。

### 4.6 漸進式採用

產品應支援 Analyzer-only、Contracts-only、Custom Contracts、Reference Contracts 與 Full Adoption，不要求一次完成架構遷移。

### 4.7 Host-agnostic Core

Domain、Application 與核心 Analyzer 不得預設 ASP.NET Core、Wolverine、MediatR、EF Core 或特定 UI Framework。宿主／技術專屬規則以 Profile 或獨立規則群處理。

### 4.8 相容性是產品功能

公開介面、Diagnostic ID、規則語意、預設 Severity、設定 Schema 及 CLI 報告格式都必須納入相容性治理。

### 4.9 發布採明確允許清單

Repository 預設不可打包；只有 package inventory 明確核准的專案能輸出發布成品。

### 4.10 來源可追溯

每條 Analyzer 規則與每組參考契約，都應記錄其設計來源、相關規範、實作 Ownership、來源 Commit（適用時）及後續演進責任。

### 4.11 產品文件採受控持續演進

產品規劃書與 PRD 應維持穩定文件身分、狀態與責任人。重大產品邊界、需求或驗收變更必須更新對應文件，但 Sprint 進度、單次 PR 細節與暫時性工作紀錄不應直接堆入產品基線。

### 4.12 正式來源、衍生輸出與雙向追溯分離

- Markdown 是 Repository 內的正式文件來源，檔名維持穩定；版本由文件 Metadata、Git Commit、Tag 與 Release 保存。
- Word／PDF 是供閱讀或對外溝通的衍生輸出，不接受與 Markdown 並行的人工編輯。
- 工程規範、PRD Requirement、Rule ID、Diagnostic／Architecture Test、實作與測試之間必須建立雙向追溯，讓「有規範但無驗證」與「有 Diagnostic 但無規範依據」都能被辨識。

## 5. 目標使用者與待完成工作

### 5.1 產品／Framework 維護者

**需求：** 將規範轉成可維護的 Rule Catalog、Diagnostic 與參考契約。<br>
**成功條件：** 規範更新可在單一 PR 同步實作、測試、文件及相容性資訊。

### 5.2 新專案開發團隊

**需求：** 快速取得一致的 Domain／Application 契約與嚴格驗證。<br>
**成功條件：** 安裝參考契約與 Analyzer 後，不需大量設定即可啟用 strict profile。

### 5.3 既有專案開發團隊

**需求：** 不全面改用新介面，也能導入核心規則。<br>
**成功條件：** 透過角色映射指定既有 `IAggregateRoot`、Application Operation、Repository 與 Handler，逐步提高規則覆蓋率。

### 5.4 架構師／技術負責人

**需求：** 知道哪些規則已被機械化、哪些仍需人工或 AI Review。<br>
**成功條件：** Conformance Report 清楚顯示執行機制、適用性、未評估範圍與例外證據。

### 5.5 CI／Release 維護者

**需求：** 將規則納入可靠、可重現的建置閘門。<br>
**成功條件：** 工具提供穩定 Exit Code、SARIF（Static Analysis Results Interchange Format）、JSON、Markdown 及可保存的 Artifact Inventory。

### 5.6 AI Agent 與人工 Reviewer

**需求：** 使用一致證據進行審查，而不是重新猜測整個 Codebase。<br>
**成功條件：** Diagnostic 與 Rule Catalog 能連回工程理由、例外及修正指引。

## 6. 採用模式

| 模式 | 安裝內容 | 角色辨識方式 | 建議用途 | 可信度 |
|---|---|---|---|---|
| A. Analyzer-only／Custom Contracts | Analyzer | 明確 Metadata Name、Base Type、Interface 或 Namespace Mapping | 既有專案、低侵入導入 | 高 |
| B. Analyzer-only／Convention-based | Analyzer | 名稱、方法形狀與 Namespace 慣例 | 初步盤點、試用 | 中或低 |
| C. Analyzer + Reference Contracts | Analyzer + 選配契約 | 內建完整契約與繼承鏈 | 新專案、嚴格 Profile | 最高 |
| D. Contracts-only | 選配契約 | 不執行規則 | 只希望重用介面與基底型別的專案 | 不適用 |
| E. Full Adoption | 契約 + Analyzer + Architecture Testing + Conformance | 完整角色模型與 Profile | 需要 CI 證據與治理的團隊 | 最高 |

### 6.1 模式 A 是 P0 驗收情境

首個公開 Prerelease 必須以一個完全未參考本產品 runtime contracts 的 Consumer Project，證明 Aggregate、Repository 與 Use Case 核心規則仍能透過明確角色映射生效。

### 6.2 模式 B 不得預設為強制 Error

純命名或結構推論具有不確定性。僅由 Convention 辨識的角色，預設應回報較低 Severity、要求 Profile 明確啟用，或在 Conformance Report 中標記判定可信度。

## 7. 產品能力架構

```text
ArchPrecept
│
├─ Rule Model
│  ├─ Canonical Rule Catalog
│  ├─ Architecture Role Definitions
│  ├─ Applicability Profiles
│  ├─ Diagnostic / AI Context Mappings
│  └─ Configuration Schemas
│
├─ Enforcement
│  ├─ Roslyn Analyzers
│  │  ├─ Core Architecture Rules
│  │  ├─ ASP.NET Core Rules
│  │  └─ Messaging Rules
│  ├─ Architecture Testing
│  └─ Severity / Enablement Profiles
│
├─ Reference Contract Libraries（選配）
│  ├─ Domain Model Contracts and Base Types
│  ├─ Application Ports and Contracts
│  ├─ Messaging Contracts 與 Application Messaging Ports
│  └─ Event Sourcing Support
│
├─ Conformance
│  ├─ Repository Validation CLI
│  ├─ Rule Applicability and Coverage
│  ├─ Compatibility Checks
│  └─ CI Evidence and Reports
│
└─ Adoption Assets
   ├─ Diagnostic Documentation
   ├─ Configuration Examples
   ├─ Compatibility Manifests
   ├─ Migration Guides
   ├─ Provenance Records
   └─ Consumer Samples
```

### 7.1 Rule Model

Rule Model 是產品真正的中心，負責定義：

- 穩定 Rule ID；
- 規則標題、理由與適用範圍；
- 可機械化程度；
- 角色需求；
- Diagnostic ID／Architecture Test／CLI Check 對照；
- 預設 Profile 與 Severity；
- AI Collaboration Framework 文件引用；
- 版本、淘汰與替代關係。

### 7.2 Enforcement

Enforcement 將規則實作為：

- Roslyn Symbol／Semantic Analysis；
- Cross-project、Assembly、Namespace 邊界測試；
- MSBuild、`.editorconfig` 及 AdditionalFiles 設定；
- CI 可執行的失敗條件。

### 7.3 Reference Contract Libraries

Reference Contracts 提供產品正式維護的 opinionated 實作契約，但：

- 不得是 Analyzer 的必要相依；
- 不得綁定特定 Web、ORM、Messaging Framework；
- 應維持低相依與明確責任；
- 必須完成 Public API Review 後才可發布；
- 只能提供有穩定跨專案價值的型別，避免成為 Common／Utility 雜物區。

預期能力包含：

- Domain Entity、Aggregate Root、Value Object、Domain Event；
- Event-Sourced Aggregate 基底與版本語意；
- Application Use Case 介面；
- Aggregate Repository、Query Repository、Domain Event Dispatcher 等 Port；
- Transport-neutral Messaging Contract 與 Publisher Port；
- 經審查後的 Result／Page Result 等共用型別。

### 7.4 Conformance

Conformance Tool 不重寫 Roslyn 語意判斷，而負責：

- 探索 Repository 與設定；
- 執行或編排 Build、Test、Analyzer、Architecture Test 與非 C# Repository Check；
- 彙整 Rule Status；
- 產出 Rule Coverage、Compatibility 與 CI 報告；
- 對未評估規則提出明確原因；
- 提供 `validate`、`explain`、`inventory`、`report` 等命令能力。

## 8. 與 AI Collaboration Framework 的責任邊界

```text
AI Collaboration Framework
  ├─ Engineering principles and rationale
  ├─ Trade-offs and exception guidance
  ├─ AI Skills and review workflows
  └─ Human-readable standards
                │
                │ 規則來源與相容版本引用
                ▼
ArchPrecept
  ├─ Rule Catalog
  ├─ Role Classification
  ├─ Analyzer / Architecture Tests
  ├─ Optional Reference Contracts
  └─ Conformance Evidence
                │
                │ 採用、設定與例外
                ▼
Target Repository
  ├─ Actual architecture facts
  ├─ Role mappings
  ├─ Enabled profiles and severities
  ├─ Waivers / suppressions
  └─ CI evidence
```

### 8.1 Ownership 原則

- AI Collaboration Framework 不再保存 Analyzer 實作原始碼。
- Guardrails repo 不複製完整 AI Context、Skill 或專案特定規格。
- Target Repository 的業務、技術棧、例外及發布狀態，不得回寫成通用產品事實。
- 兩個產品透過 Compatibility Manifest 與 Rule Mapping 連結，不強制共用相同版本號。

### 8.2 未來 Framework 導入原則

在 AI Collaboration Framework 尚未具備適合 Analyzer／套件產品開發的 Profile 前，產品層級文件先由 `docs/` 管理。未來安裝 Framework 時，應先執行語意盤點與 Gap Analysis，確認：

- 哪些內容屬於長期產品文件，應繼續留在 `docs/`；
- 哪些是機器可讀規則，應由 `rules/` 維護；
- 哪些應拆解為 `.dev/requirements/`、`.dev/specs/`、`.dev/standards/`、`.dev/workflows/` 或 `.dev/releases/`；
- 每條現行 Diagnostic 是否有配套工程規範、Rule ID、測試與適用性說明；
- 每條可機械化規範是否已有對應執行機制，或應明確標記為缺口。

Framework 導入不得把移動路徑視為治理完成；真正的驗收標準是 Ownership、正式來源與追溯關係清楚，且沒有重複維護同一產品事實。

## 9. 相依與分層原則

```text
Canonical Rule Catalog
  ├─> Roslyn Analyzers
  ├─> Architecture Testing
  ├─> Conformance Tool
  └─> Documentation validation

Reference Contract Libraries
  ├─ no dependency on Analyzers
  ├─ no dependency on Conformance
  └─ no framework-specific dependency in core contracts

Roslyn Analyzers
  ├─ no PackageReference to Reference Contract Libraries
  ├─ recognize built-in contracts by metadata name
  ├─ support custom role mappings
  └─ support configurable convention fallbacks

Conformance Tool
  ├─ orchestrates existing mechanisms
  └─ does not duplicate semantic analyzer implementations
```

## 10. Git Repository 策略

### 10.1 為何維持單一 repo

單一 repo 能讓以下變更在同一 PR 內完成：

- Contract shape 變更；
- Built-in role mapping 更新；
- Analyzer 規則更新；
- Analyzer／Consumer 測試；
- Rule Catalog 與文件；
- Compatibility Manifest；
- Migration Note。

Repository 邊界不等於 NuGet 相依邊界。同一 repo 仍可讓 Analyzer-only、Contracts-only 與 Full Adoption 完全獨立安裝及版本化。

### 10.2 未來拆分 Reference Contracts repo 的觸發條件

只有在下列情況實際發生時才拆分：

- Reference Contracts 形成獨立 Roadmap、文件網站或使用者社群；
- Analyzer 與 Contracts 的 Release Cadence 長期衝突；
- 維護團隊、權限或發布 Ownership 分離；
- 授權或來源治理需要隔離；
- Contracts 擴張為完整 Runtime Framework；
- 大量使用者只採用 Contracts，且 Guardrails 已不再是主要產品關聯。

## 11. 預期 Git repo 結構

> 下列 `ArchPrecept.*` 名稱是已核准的 .NET Project、預設 Assembly Name 與 Namespace 工作身分；不代表或推定最終 NuGet Package ID。其他目錄名稱描述 Repository 責任。

```text
dotnet-engineering-guardrails/
│
├─ .github/
│  ├─ ISSUE_TEMPLATE/
│  └─ workflows/
│     ├─ ci.yml
│     ├─ package-validation.yml
│     └─ release.yml
│
├─ .dev/                            # 未來 Framework 導入後建立
│  ├─ requirements/
│  ├─ specs/
│  ├─ standards/
│  ├─ workflows/
│  └─ releases/
│
├─ rules/
│  ├─ catalog/
│  │  ├─ domain.yml
│  │  ├─ application.yml
│  │  ├─ messaging.yml
│  │  └─ aspnetcore.yml
│  ├─ profiles/
│  ├─ mappings/
│  └─ schemas/
│
├─ src/
│  ├─ Contracts/
│  │  ├─ ArchPrecept.Contracts.Domain/
│  │  ├─ ArchPrecept.Contracts.Application/
│  │  ├─ ArchPrecept.Contracts.Messaging/
│  │  ├─ ArchPrecept.Contracts.Application.Messaging/
│  │  └─ ArchPrecept.Contracts.Domain.EventSourcing/
│  ├─ Analyzers/
│  │  └─ ArchPrecept.Analyzers.Core/
│  │     ├─ Classification/
│  │     │  ├─ Roles/
│  │     │  ├─ Evidence/
│  │     │  ├─ BuiltInContracts/
│  │     │  └─ Configuration/
│  │     ├─ Rules/
│  │     │  ├─ Domain/
│  │     │  ├─ Application/
│  │     │  ├─ Messaging/
│  │     │  └─ AspNetCore/
│  │     ├─ Diagnostics/
│  │     ├─ Infrastructure/
│  │     └─ CodeFixes/              # 僅限安全修正
│  ├─ ArchitectureTesting/
│  └─ Conformance/
│     ├─ Commands/
│     ├─ Discovery/
│     ├─ Execution/
│     ├─ Compatibility/
│     └─ Reporting/
│
├─ config/
│  ├─ editorconfig/
│  ├─ msbuild/
│  └─ conformance/
│
├─ tests/
│  ├─ Analyzers/
│  ├─ Contracts/
│  ├─ ArchitectureTesting/
│  ├─ Conformance/
│  └─ Consumers/
│     ├─ AnalyzerOnly.CustomContracts/
│     ├─ AnalyzerOnly.ConventionBased/
│     ├─ AnalyzerWithReferenceContracts/
│     ├─ ContractsOnly/
│     ├─ UnrelatedNaming.FalsePositiveTests/
│     └─ FullAdoption/
│
├─ samples/
│  ├─ CustomContracts/
│  ├─ ReferenceContracts/
│  ├─ AspNetCore/
│  ├─ Worker/
│  └─ ConsoleJob/
│
├─ docs/
│  ├─ product/
│  │  ├─ product-plan.md
│  │  └─ prd.md
│  ├─ governance/
│  │  └─ document-governance-and-traceability.md
│  ├─ architecture/
│  ├─ rules/
│  ├─ diagnostics/
│  ├─ contracts/
│  ├─ compatibility/
│  ├─ migration/
│  ├─ provenance/
│  ├─ adoption/
│  │  └─ ai-collaboration-framework.md
│  └─ exports/                      # 選配；由 Markdown 產生
│
├─ eng/
│  ├─ package-inventory.yml
│  ├─ build/
│  ├─ packaging/
│  ├─ compatibility/
│  └─ scripts/
│
├─ Directory.Build.props
├─ Directory.Build.targets
├─ Directory.Packages.props
├─ global.json
├─ NuGet.Config
├─ ArchPrecept.slnx
├─ README.md
├─ LICENSE
├─ NOTICE
└─ THIRD-PARTY-NOTICES
```

### 11.1 實體結構原則

- `rules/`：機器可讀規則與 Schema，不是一般說明文件。
- `docs/rules/`：人類可讀的理由、範例、例外與遷移說明。
- `src/`：所有可建置、可發布的產品原始碼；第一層依產品能力分組，不在 Repository 根目錄另設 `analyzers/` 或 `tools/` 原始碼樹。
- 產品 `.csproj` 檔名、預設 Assembly Name 與根 Namespace 使用 `ArchPrecept.<Capability>`；測試專案使用對應產品名稱加 `.Tests`。
- `ArchPrecept.*` 工作身分不得自動複用為 NuGet Package ID；發布識別仍需獨立決策與核准。
- `src/Contracts/`：選配 runtime contracts；目錄位置不代表 Analyzer 相依。
- `src/Analyzers/`：Roslyn Analyzer 與僅限安全修正的 Code Fix 原始碼。
- `src/ArchitectureTesting/`：可由採用端測試專案引用的架構測試程式庫。
- `src/Conformance/`：可執行的 .NET Tool／CLI 與報告編排能力。
- `tests/`：不發布的自動驗證專案，第二層沿用 `src/` 的能力名稱；測試原始碼不與產品成品混放。
- `tests/Consumers/`：以實際打包成品驗證 restore、build、Diagnostic 與執行行為。
- `samples/`：供人閱讀的完整案例，不取代 Consumer Smoke Test。
- `docs/product/`：產品規劃書與 PRD 的穩定正式路徑；檔名不隨版本改變。
- `docs/governance/`：文件生命週期、追溯模型與 Framework 導入規劃。
- `docs/exports/`：選配的 Word／PDF 衍生輸出，不是人工維護的正式來源。
- `.dev/`：未來 Framework 導入後的需求拆解、技術規格、工作流程與證據層；不取代 `docs/` 或 `rules/`。
- `eng/`：建置、打包、相容性與 Artifact Inventory 的產品工程資產。

### 11.2 文件與治理結構

目前 Repository 先採下列責任分工：

```text
docs/
  ├─ product/       # 產品規劃與需求基線
  ├─ governance/    # 文件治理與追溯設計
  ├─ architecture/  # 對外可閱讀的產品架構
  └─ ...

rules/
  ├─ catalog/       # 機器可讀 Rule ID 與語意
  ├─ mappings/      # Rule、Diagnostic、規範與測試映射
  └─ schemas/
```

未來安裝 AI Collaboration Framework 後，增加：

```text
.dev/
  ├─ requirements/  # 從 PRD 拆出的可排程需求
  ├─ specs/         # 技術設計與介面規格
  ├─ standards/     # Repository 自身工程規範
  ├─ workflows/     # 單次變更的計畫、結果與證據
  └─ releases/      # Candidate 與正式發布證據
```

產品規劃書與 PRD 不因 Framework 導入而預設搬離 `docs/product/`。是否移動任何文件，必須由語意責任與 Ownership 決定，而不是套用目錄範本。

## 12. 預期發布成品

> 本節定義成品類型，並列出尚未核准的 Package ID 命名候選。

### 12.1 Reference Contract Libraries

- Domain Model Contracts and Base Types
- Application Ports and Contracts
- Messaging Contracts 與 Application Messaging Ports
- Event Sourcing Support（依 DEC-017 拆為同 repository 的獨立選配套件）

### 12.2 Guardrails

- Core Roslyn Analyzer 成品
- ASP.NET Core／Messaging Profile 規則（初期可放在同一 Analyzer 成品）
- Architecture Testing 類別庫
- `.editorconfig`、MSBuild 與角色映射設定範例

### 12.3 Conformance

- .NET Tool／CLI
- Console、JSON、Markdown、SARIF 報告
- Rule Coverage、Compatibility 與 Artifact Inventory

### 12.4 Adoption Assets

- Rule Catalog 與 JSON Schema
- Diagnostic 文件
- Profile 與 Severity 範本
- Migration Guide
- Compatibility Manifest
- Provenance／Lineage 文件
- 文件治理與規則追溯規範
- Consumer Samples

### 12.5 Release Artifacts

- `.nupkg`
- `.snupkg`
- SHA-256 Checksum
- Artifact Inventory
- Release Notes
- Migration Notes
- Compatibility Manifest
- Rule Catalog Snapshot
- Software Bill of Materials（SBOM，後續成熟階段）

### 12.6 Package ID 命名候選

下表區分現行實作與未來能力。初始四個 ID 已發布 `0.0.1-preview`，詳見[歷史套件 inventory](../releases/preview.md#套件-inventory)。2026-10-21 依 CT-004／CT-005 拆分 Messaging 並新增 Domain.EventSourcing，現行六套件以 `0.0.2-preview` 作本機候選，尚未發布；舊 Messaging.Abstractions ID 不在此次成品中。nuget.org ID 可用性尚未確認，發布狀態與來源實作分開記錄。

| 能力 | Package ID 候選 | 實作狀態 |
|---|---|---|
| Domain Contracts | `ArchPrecept.Contracts.Domain` | 已有對應專案 |
| Application Contracts | `ArchPrecept.Contracts.Application` | 已有對應專案 |
| Messaging Contracts | `ArchPrecept.Contracts.Messaging` | 共用訊息契約，不依賴 port 或 Configuration |
| Application Messaging Ports | `ArchPrecept.Contracts.Application.Messaging` | 發布 port 與傳遞 metadata，只依賴 Messaging；見[責任邊界](messaging-contract.md) |
| 核心 Analyzer | `ArchPrecept.Analyzers.Core` | 已有對應專案 |
| Event Sourcing Contracts | `ArchPrecept.Contracts.Domain.EventSourcing` | CT-005 已有對應專案；`EsAggregateRoot` 已移入，待發布；見[行為與遷移契約](event-sourcing-contract.md) |
| Architecture Testing | `ArchPrecept.ArchitectureTesting` | 尚未建立專案 |
| Conformance CLI／.NET Tool | `ArchPrecept.Conformance` | 尚未建立專案，CLI 命令名稱另行決策 |

測試專案不發布套件；未來能力不以空專案預先佔位。目前不新增 `ArchPrecept` 或 `ArchPrecept.Analyzers` 聚合套件，待實際安裝組合需求確立後再評估。Analyzer 與 Contracts 繼續獨立採用。

## 13. 版本與相容性治理

### 13.1 相容性面向

| 面向 | 受治理內容 |
|---|---|
| Source Compatibility | 公開 Interface、Base Class、Generic Constraint、方法 Signature |
| Binary Compatibility | Assembly Public API、Type Forwarding、移除或修改 Member |
| Analyzer Compatibility | Diagnostic ID、觸發條件、訊息參數、預設 Severity、誤報修正 |
| Rule Compatibility | Rule ID、語意、適用 Profile、替代／淘汰關係 |
| Configuration Compatibility | `.editorconfig` Key、AdditionalFiles Schema、CLI Config Schema |
| Report Compatibility | JSON／SARIF／Markdown Schema、Exit Code |

### 13.2 初期版本策略

以下為規劃方向，未來維護規則仍待負責人決定。本次只固定 `0.0.1-preview`；平台 major 對齊與產品 SemVer 的取捨詳見[版本規則提案](../releases/version-policy-proposal.md)，不自動套用後續升版。

- 初期採 `0.x` Prerelease。
- 可先採共同 Release Train，但每個成品仍可獨立安裝。
- 是否採獨立版本號，待實際演進壓力再決定。
- Diagnostic ID 一旦公開，即視為穩定識別，不得任意重用。
- 規則觸發範圍擴張可能是 Breaking Change，不只 API 變更才算 Breaking。

### 13.3 發布閘門

公開 Candidate 必須完成：

1. Restore、Release Build、所有測試；
2. Analyzer-only／Custom Contracts Consumer Test；
3. Reference Contracts Consumer Test；
4. False-positive Regression Test；
5. Public API 與 Analyzer 行為相容性檢查；
6. Package Content、Source Link、Symbols 與 Metadata 檢查；
7. 本機 Feed 安裝驗證；
8. Rule Catalog／Diagnostic 文件完整性驗證；
9. Provenance 與 License Gate；
10. 由不可變 Commit 產出且不重新建置的發布 Artifact。

### 13.4 文件與規則基線治理

- Repository 內的產品規劃書與 PRD 使用穩定路徑：`docs/product/product-plan.md` 與 `docs/product/prd.md`。
- 文件版本記錄於文件控制區塊及 Git 歷史，不以 `-v0.x` 檔名在 Repository 內建立多份並行正本。
- Markdown 為可審查的正式來源；Word／PDF 僅由正式來源產生。
- Rule Catalog、Diagnostic 文件與 Traceability Mapping 應可由 CI 驗證一致性。
- 重大產品或需求變更應保留 Decision／Requirement 歷史，不得只覆寫結論而失去替代關係。

## 14. 來源、授權與 Ownership 規劃

### 14.1 AI 協作的課程啟發與既有 .NET 工具的產品化

依負責人於 2026-10-19 的補充，Teddysoft 課程主要採用 Java，啟發的是與 AI 協作的模式。ArchPrecept 的套件與分析器在此之前已在使用，負責人也曾研究這類工具；課程不是 .NET 套件與分析器技術的起點。課程網址：[Teddysoft AI Coding 課程頁面](https://teddysoft.tw/courses/ai-coding-pl/)。

為了與 AI 協作，並在看到課程以純文字比較處理這類需求後，負責人想到將既有 .NET 生態系的分析器做法搭配協作流程，讓規範能由編譯與 CI 階段的工具提供可重複、可追溯的檢查證據。這次的進一步投入，是把既有使用與研究累積發展成自己使用的套件與框架。

目前產品化結合課程帶來的 AI 協作方法啟發、既有 Roslyn Syntax／Semantic Model 使用經驗、AI Collaboration Framework 的規範，以及個人在 DDD、Clean Architecture、Hexagonal Architecture 與 .NET 專案中的實務經驗，定義規則邊界、Diagnostic 語意、角色辨識及測試策略。

### 14.2 來源揭露與授權處理原則

2026-10-19 決策：ArchPrecept 自有內容採 Apache-2.0。負責人確認 Framework、distributed architecture Lab 與 webapi Lab 為自有知識累積來源，並補充課程為 AI 協作方法啟發，套件與分析器來自既有 .NET 使用及研究累積。固定上游逐檔比對、依賴條款檢查及來源聲明已記錄於[來源檢查結果](../releases/licensing-assessment.md)，本次來源待確認項目已完成；四個 preview 套件已發布並通過 registry 消費測試，public 可見性與原先 private 預期不符，後續發布開關已關閉。此決策更新先前 License 未選定的狀態，不更動舊版快照的歷史記錄。

- 對外文件應清楚揭露課程對產品構想的啟發關係，避免把課程貢獻描述成不存在。
- 本 repo 應擁有後續自行設計與實作的 Roslyn Analyzer 原始碼、Rule Catalog、Diagnostic 定義、測試及文件。
- 公開發布前，應進行檔案層級的來源與授權檢查，確認發布成品未直接包含需另行授權的教材文字、程式碼或其他受保護內容。
- 若檢查發現存在直接引用或改作內容，應依授權條款處理、取得必要同意，或改以獨立實作取代。
- `NOTICE`、`THIRD-PARTY-NOTICES` 與 Provenance Record 應只記載可被證據支持的來源、授權與致謝資訊。

### 14.3 Reference Contracts 的來源與 Ownership

參考契約類別庫主要來自下列個人 Lab Repository 中反覆實作、驗證與演進的成果：

- `https://github.com/YuChia-Wei/dotnet-webapi-lab/tree/main/src/dotnetLab.BuildingBlocks.Domain`
- `https://github.com/YuChia-Wei/dotnet-distributed-architecture-lab/tree/main/src/BuildingBlocks`

抽取與 Public API 硬化完成後，本 repo 應成為這些參考契約的 Canonical Source Owner。原 Lab Repository 應改為發布成品的 Consumer 或實驗環境，不再各自維護具有相同責任的複本。

### 14.4 產品間 Ownership 邊界

- AI Collaboration Framework 擁有工程原則、理由、適用條件、例外、AI Skill 與人工審查方法。
- 本 repo 擁有 Rule Catalog、架構角色模型、Roslyn Analyzer、Architecture Testing、Conformance 能力，以及選配 Reference Contract Libraries。
- 採用端 Repository 擁有自身架構事實、Role Mapping、啟用 Profile、例外與 Waiver 證據。
- 規則內容可彼此引用，但不得在不同 Repository 重複維護相同的 Analyzer 原始碼或參考契約實作。

## 15. 成功衡量指標

### 15.1 MVP 工程指標

- Analyzer 成品對 Reference Contract Libraries 的 PackageReference 數量為 `0`。
- 所有公開 Diagnostic 都有對應 Rule ID、文件與測試。
- 自訂契約 Consumer 能觸發 Aggregate、Repository、Use Case 核心規則。
- 官方契約 Consumer 能在低設定下啟用相同規則。
- 未辨識角色時，報告為 `not-evaluated`，不回報 `passed`。
- Convention-only 與 Unrelated Naming 測試能防止已知誤報。
- 現有 Diagnostic Baseline 在重構後不發生未核准的行為退化。
- 所有正式發布專案都由 package inventory 明確允許。
- 所有公開 Diagnostic 都能追溯至工程規範、PRD Requirement、Rule ID、實作與自動測試。
- 可機械化但尚未有執行機制的規範會被列為明確缺口，而不是無聲忽略。
- 產品規劃書、PRD 與治理文件在 Repository 內只有一份 Markdown 正本。

### 15.2 品質指標

- 100% Rule Catalog 通過 Schema Validation。
- 100% Diagnostic ID 不重複且有相容性紀錄。
- 100% Release Artifact 可由 Checksum 驗證。
- 100% Package Candidate 經本機 Feed Consumer Test。
- Analyzer 對大型 Solution 的 Build Overhead 建立可重現基線，並設定後續效能預算。

### 15.3 採用指標（正式發布後）

- Analyzer-only 與 Full Adoption 的使用比例。
- Custom Role Mapping 的設定成功率。
- 使用者抑制／停用規則的主要原因。
- 誤報 Issue 比率與修正時間。
- 未評估規則比例是否隨導入成熟度下降。

## 16. Roadmap

### Phase 0：產品與治理基線

**目標：** 完成 repo 命名、產品定位、目錄策略、課程啟發揭露，以及來源與授權檢查基線。<br>
**主要工作：**

- 將 repo 定位改為 `ArchPrecept`；
- 建立 Product Plan、PRD、文件治理與規則追溯規範、Decision Log；
- 建立 `docs/product/`、`docs/governance/` 與 `rules/mappings/` 的正式來源結構；
- 建立來源清冊、課程啟發說明與 Provenance 記錄；
- 保持所有 Project `IsPackable=false`；
- 決定 Rule ID 與 Diagnostic ID 管理策略。

**退出條件：** 產品邊界、非目標與公開發布阻擋條件被正式記錄。

### Phase 1：角色分類與 Analyzer 解耦（最高優先）

**目標：** 建立可設定、可解釋的 Architecture Role Classifier。<br>
**主要工作：**

- 抽取共用 Aggregate、Entity、Use Case、Repository、Handler 分類器；
- 支援 Built-in Metadata Name；
- 支援 Custom Interface／Base Type Mapping；
- 支援 Evidence／Confidence；
- 讓現有 Analyzer 全部改用共用分類層；
- 建立 Custom Contracts、Convention-only、False-positive Consumer Matrix。

**退出條件：** 完全不引用 Reference Contracts 的 Consumer 能通過核心規則驗證。

### Phase 2：Rule Catalog 與 Reference Contracts 硬化

**目標：** 穩定規則正式來源及選配 Public API。<br>
**主要工作：**

- 建立機器可讀 Rule Catalog 與 Schema；
- 將現有 Diagnostic 映射至 Rule ID、工程規範、PRD Requirement 與測試；
- 產生「規範存在但缺少驗證」及「Diagnostic 存在但缺少規範依據」的 Gap Report；
- 審查 Domain／Application／Messaging／Event Sourcing Public API；
- 移除 Lab 命名與 Backend 假設；
- 建立 Public API Baseline；
- 完成契約測試及文件。

**退出條件：** Rule Catalog、Diagnostic、Reference Contracts 與 AI Context 引用一致。

### Phase 3：首次 Prerelease

**目標：** 發布可由外部 Repository 安裝驗證的候選版本。<br>
**主要工作：**

- 啟用核准專案打包；
- 建立 CI、Package Validation 與 Release Workflow；
- 建立 Consumer Smoke Tests；
- 完成 Source Link、Symbols、Checksum、Artifact Inventory；
- 發布 Diagnostic 文件、Migration Guide 與 Compatibility Manifest。

**退出條件：** Analyzer-only、Contracts-only 與 Full Adoption 均能從實際 Feed 安裝。

### Phase 4：Architecture Testing 與 Conformance

**目標：** 從單一 Diagnostic 擴展為 Repository-level Evidence。<br>
**主要工作：**

- 提供 Assembly／Namespace／Layer Dependency Assertion；
- 建立 Conformance CLI；
- 產出 Status、Coverage、Compatibility 與 CI Report；
- 支援 `validate`、`explain`、`inventory`、`report`。

**退出條件：** CI 能保存完整 Rule Evaluation Evidence，並區分 Passed、Failed 與 Not Evaluated。

### Phase 5：穩定版與生態成熟

**目標：** 建立穩定相容性政策與外部採用流程。<br>
**主要工作：**

- 穩定 Public API、Diagnostic 與 Config Schema；
- 建立效能預算與大型 Codebase 基準；
- 評估安全 Code Fix；
- 建立 SBOM、簽章或 Build Attestation；
- 依實際生命週期評估是否拆分 Contracts repo。

## 17. 主要風險與緩解措施

| 風險 | 影響 | 緩解措施 |
|---|---|---|
| 過度依賴型別名稱 | 誤報、團隊停用 Analyzer | 建立 Role Classifier、明確 Mapping、Evidence 與 False-positive Suite |
| 強迫採用官方契約 | 既有專案導入成本過高 | Analyzer-only／Custom Contracts 列為 P0 |
| 規則與文件漂移 | Diagnostic 無法追溯 | Canonical Rule Catalog、雙向 Traceability Mapping、CI 一致性檢查 |
| Markdown 與 Word 同時人工維護 | 內容分叉、Review 失真 | Markdown 單一正式來源，Word／PDF 僅由自動化或受控流程產生 |
| Framework 導入時無差別搬移文件 | Ownership 模糊、產品文件與工作證據混雜 | 先做 Gap Analysis 與語意映射，保留 `docs/`、`rules/`、`.dev/` 的責任差異 |
| 契約成為共用雜物區 | API 膨脹、Breaking Change 增加 | 明確 Package Boundary、Public API Review、非目標清單 |
| Analyzer 規則過度 opinionated | 使用者不信任或大量抑制 | Profile、Severity、適用性與人工審查邊界 |
| 零 Diagnostic 被誤判為合規 | 錯誤安全感 | `not-evaluated` 狀態與 Coverage Report |
| Analyzer 效能下降 | Build 變慢 | 共用 Classification Cache、Concurrent Execution、Benchmark |
| Diagnostic ID 或行為不穩定 | Suppression 與 CI 破壞 | Analyzer Compatibility Policy、不可重用 ID |
| 來源揭露或授權檢查不完整 | 公開發布存在權利與信任風險 | 課程啟發揭露、檔案層級 Provenance Audit、必要授權確認或獨立重寫 |
| 單一 repo 範圍膨脹 | CI 與 Roadmap 複雜 | 僅保留 Guardrails 主軸，並設定明確的能力納入與拆分觸發條件 |
| 過早拆 repo | 跨 repo 發版與相容性成本 | 先採單一 repo、獨立成品與 package inventory |

## 18. 非目標

第一階段明確不處理：

- 自動決定 Aggregate 邊界是否符合業務不變條件；
- 自動判斷是否應採用 DDD、CQRS 或 Event Sourcing；
- 自動大規模重構 Controller、Use Case 或 Domain Model；
- 提供完整 ORM、Outbox、Message Bus 或 Transaction Runtime；
- 規定單一專案資料夾結構為唯一正確架構；
- 透過 Analyzer 取代人類或 AI 架構審查；
- 核准 NuGet Package ID、套件發布及商標權利；ArchPrecept 品牌與程式碼前綴依 DEC-015 已確認。

## 19. 待決策事項

| 優先 | 議題 | 決策時點 |
|---|---|---|
| P0 | Rule ID 與 Diagnostic ID 命名／保留政策 | Phase 0 |
| P0 | 課程啟發揭露、來源與授權檢查基線 | Phase 0 |
| P0 | Role Mapping 設定格式：`.editorconfig`、MSBuild、AdditionalFiles 或 YAML／JSON | Phase 1 |
| P0 | Supported Roslyn／Compiler Matrix | Phase 1 |
| P0 | Reference Contracts 的最低 Target Framework | Phase 2 |
| P1 | `IUseCase`、Aggregate Version、Result 與 Event Sourcing Public API | Phase 2 |
| P1 | 完整 Event Sourcing runtime 是否另立產品／repository；核心套件拆分已由 DEC-017 決定 | 實際 storage／projection 等需求確立後 |
| P1 | ASP.NET Core／Messaging Analyzer 是否初期拆分 | Phase 3 |
| P1 | 共同版本或獨立版本策略 | Phase 3 |
| P1 | GitHub Packages、nuget.org 或其他初始 Feed | Phase 3 |
| P2 | Conformance CLI 技術、AOT 與輸出 Schema | Phase 4 |
| P2 | 是否提供 Code Fix | Phase 5 |
| Deferred | NuGet Package ID、Package Prefix | API、產品邊界與發布治理穩定後 |

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

已知基線：

- Domain、Application 與 Messaging Contracts 與 Application Messaging Ports 抽取專案；
- Roslyn Analyzer 初始實作；
- 17 個 Diagnostic；本輪 51 個 Analyzer 測試、.NET 8／10 各 21 個 ES 測試與六種 consumer 模式，見[候選驗證](../releases/0.0.2-preview.md)；
- Analyzer 目前主要依簡單型別名稱、Suffix 與方法形狀判定角色；
- Project 仍為 `IsPackable=false`；
- Project、預設 Assembly Name 與根 Namespace 已核准採 `ArchPrecept.<Capability>`；公開 Package ID、License 與相容性策略尚未核准。

目前尚未建立 `src/ArchitectureTesting/` 與 `src/Conformance/` 專案；兩者是已規劃但尚未實作的能力邊界，不以空目錄或空專案預先佔位。

主要重構方向：

- 維持 Project、Assembly 與 Namespace 的 `ArchPrecept.<Capability>` 階層，禁止重新引入 `Backend`、`Lab` 與 `BuildingBlocks` 產品身分；
- 把分散於各 Analyzer 的角色判定抽成中央 Classifier；
- 把 Source-included Analyzer 轉為正式 Analyzer 成品；
- 將 Reference Contracts 從複製來源轉為 Canonical Ownership；
- 建立 Rule Catalog、Consumer Matrix 及 Release Governance。

# 附錄 B：詞彙表

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

# 附錄 C：主要來源與參考

- AI Collaboration Framework：`https://github.com/YuChia-Wei/ai-collaboration-prompts-dotnet-backend`
- 目前 Repository：[dotnet-engineering-guardrails](https://github.com/YuChia-Wei/dotnet-engineering-guardrails)；GitHub 改名另行安排，本機目錄保留 `dotnet-architecture-kit`。
- Analyzer 構想的課程啟發：[Teddysoft 課程](https://teddysoft.tw/courses/ai-coding-pl/)
- Domain Contract 來源之一：`https://github.com/YuChia-Wei/dotnet-webapi-lab/tree/main/src/dotnetLab.BuildingBlocks.Domain`
- 分散式架構 Contract 來源：`https://github.com/YuChia-Wei/dotnet-distributed-architecture-lab/tree/main/src/BuildingBlocks`

# 附錄 D：文件治理與持續演進

本產品以《ArchPrecept 文件治理與規則追溯規範》作為完整治理依據，摘要如下：

- 產品規劃書與 PRD 是持續演進的受控基線，不是 Sprint 日誌或單次交付紀錄。
- Repository 內以 `docs/product/product-plan.md`、`docs/product/prd.md` 作為穩定 Markdown 正本；版本由文件控制 Metadata 與 Git 歷史保存。
- Word／PDF 是衍生輸出，不得與 Markdown 並行人工修改。
- 重大產品邊界變更應建立 ADR（Architecture Decision Record，架構決策紀錄）或 Decision Log，而非只覆寫結論。
- PRD Requirement ID、Rule ID 與 Diagnostic ID 不因文件重新排序而改號；淘汰時保留狀態與替代關係。
- 工程規範、PRD、Rule Catalog、Diagnostic／Architecture Test、實作與測試必須可雙向追溯。
- 未來安裝 AI Collaboration Framework 時，先執行 Gap Analysis，再以 `.dev/` 補上 Requirements、Specs、Standards、Workflows 與 Release Evidence；產品文件不因安裝行為而預設搬家。
- NuGet Package Naming 決策完成後，應新增 Naming／Identity 文件或附錄，不改寫不依賴名稱的需求語意。
