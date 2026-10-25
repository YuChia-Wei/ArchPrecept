---
title: "ArchPrecept"
subtitle: "產品規劃書"
author: "產品負責人：YuChia Wei"
date: "2026-10-21"
lang: "zh-TW"
document_id: "DOC-PRODUCT-PLAN"
document_version: "0.9"
status: "draft"
last_reviewed: "2026-10-25"
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
> **版本：** `0.9`<br>
> **適用狀態：** 文件整體仍為 draft；第 3 節已確認決策及本文件明定的 Preview 政策可作為其範圍內的產品基線。本文不構成任何一次發布授權或商標權利判斷。

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
| 本版排除 | 具體版本發布授權、商標權利確認、最終定價與商業化策略 |

# 執行摘要

`ArchPrecept` 的目的，是解決「軟體工程規範寫得很清楚，但缺乏可信、可執行、可在編譯與 CI 階段產生證據的驗證機制」這個問題。

產品不只包含 Roslyn 分析器（Roslyn Analyzer），也包含支援規則執行所需的規則模型、架構角色辨識、架構測試（Architecture Testing）、符合性驗證工具（Conformance Tool）、設定與導入資產，以及一組**選配的參考契約類別庫（Reference Contract Libraries）**。參考契約類別庫預計提供 `IAggregateRoot`、Event-Sourced Aggregate、`IUseCase`、Repository Port、Messaging Abstraction 等正式維護的介面與基底型別，但採用這些型別**不得成為使用 Analyzer 的必要條件**。

產品本身負責解釋每條規則的設計理由、取捨、適用性與可驗證邊界；相關方法論的歸因以外部作者或權威來源為準。AI Collaboration Framework 可提供協作與審查知識，採用端則保存自身角色映射、啟用 Profile、例外及驗證證據。Framework 的存在不取代本產品文件，也不自動授予規則規範地位。

目前建議維持**單一 repo、多個成品輸出**。Analyzer 與參考契約類別庫在套件相依上完全解耦，但放在同一 repo 共同維護，以便在同一個 Pull Request 中原子性更新規則、契約、測試、文件與相容性宣告。只有在參考契約類別庫形成獨立產品生命週期、維護責任或授權邊界時，才考慮拆分新的 repo。

首要工程優先事項不是繼續新增規則，而是建立一套共用、可設定、可解釋的**架構角色分類器（Architecture Role Classifier）**，讓 Analyzer 同時支援：

1. 使用本產品參考契約的專案；
2. 使用自行定義契約的既有專案；
3. 只採用命名與結構慣例的低設定模式；
4. 無法可靠判定時回報 `not-evaluated`，而不是錯誤宣稱通過。

產品規劃書與 PRD 是持續演進的受控產品文件，而不是一次性提案或工作日誌。Repository 內以 Markdown 作為可審查、可版控的正式來源；Word 等格式視為衍生輸出。未來導入 AI Collaboration Framework 時，`docs/` 仍保存產品層級與對外有效的文件，`rules/` 保存機器可讀規則，新增的 `.dev/` 則承接需求拆解、技術規格、工作流程與發布證據，不以無差別搬移檔案取代語意治理。

## 1. 產品背景與問題陳述

### 1.1 背景

DDD、Clean Architecture 與 Hexagonal Architecture 提供責任、依賴與邊界的設計原則，但同一原則通常容許多種實作形式。團隊仍需要說明自己選擇的形式、代價及適用條件，並區分哪些條件可由 C# 語意分析可靠判斷。

ArchPrecept 將明確的產品政策轉成可執行檢查，同時保留人工與 AI 判斷的範圍。規則的外部依據與產品加強處分別寫入[規則設計依據](rule-design-rationale.md)，來源資格及支持界線見[方法論來源](methodology-references.md)。個人研究、私人討論及其他專案的開發歷史不作為公開規則的設計權威。

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
| DEC-001 | GitHub repository 使用 `YuChia-Wei/ArchPrecept` | 已確認 | 對外產品與 repository 身分一致；本機目錄保留 `dotnet-architecture-kit` |
| DEC-002 | 對外產品名稱由 DEC-015 定義 | 已取代 | 現行名稱集中於 DEC-015；歷史內容由 Git 保存 |
| DEC-003 | 單一 repo、多成品輸出 | 已確認 | 規則、契約、測試與文件可共同演進 |
| DEC-004 | Analyzer 不得依賴 Reference Contract Libraries | 已確認 | Analyzer-only 是一等採用模式 |
| DEC-005 | 自訂契約也能執行核心規則 | 已確認 | 以角色映射支援既有系統；自訂辨識不等於放寬已選政策 |
| DEC-006 | Reference Contracts 留在同一 repo，定位為選配 | 暫定 | 依獨立產品生命週期與 ownership 再評估是否拆 repo |
| DEC-007 | Core 宿主中立；宿主／技術規則以 Profile 區分 | 已確認 | 支援 Web API、桌面、Console、Worker 與其他宿主 |
| DEC-008 | `rules/catalog.xml` 是機器規則正式來源 | 已確認 | 文件說明理由與限制，不建立第二份產品 Catalog |
| DEC-009 | 零 Diagnostic 不等於整體合規 | 已確認 | 未辨識、未啟用與未執行的檢查需分別揭露 |
| DEC-010 | Package ID 的現行決策見第 12.6 節 | 已確認 | 未來成品名稱仍需獨立選定，不由 Project 名稱自動推定 |
| DEC-011 | Markdown 正本持續演進，Word／PDF 為衍生輸出 | 已確認 | 以 Git 保存歷史，現行本文維持可直接閱讀的產品語意 |
| DEC-012 | `docs/`、`rules/` 與 `.dev/` 各自維持責任 | 已確認 | Framework 安裝不搬移或取代產品來源 |
| DEC-013 | 規範、Requirement、Rule、實作、測試與證據雙向追溯 | 已確認 | 讓缺少設計依據與缺少執行機制都可見 |
| DEC-014 | 程式碼命名前綴由 DEC-015 定義 | 已取代 | 能力分層繼續適用；歷史內容由 Git 保存 |
| DEC-015 | 產品名稱為 ArchPrecept；Solution 為 `ArchPrecept.slnx`；Project／Assembly／Namespace 為 `ArchPrecept.<Capability>` | 已確認 | 產品身分及能力邊界一致 |
| DEC-016 | 本機目錄保留 `dotnet-architecture-kit` | 已確認 | 本機路徑無須與遠端名稱一致 |
| DEC-017 | ES 核心採獨立 `Contracts.Domain.EventSourcing` 套件；ES Analyzer 留在 Core | 已確認 | ES 能力選配；完整 storage／projection runtime 另案評估 |
| DEC-018 | Messaging 分為訊息契約與 Application publisher port | 已確認 | 依責任解耦；host／adapter 擁有組態與具體傳輸機制 |
| DEC-019 | 六套件 public；review 與驗證後 ff-only 回 main，再由 main push 發布 preview | 已確認 | 現行流程見[CI/CD](../releases/cicd-plan.md)，不新增 PR／reviewer 必經門檻 |
| DEC-020 | 核心 Analyzer 採共用分類與明確設定／範圍證據 | 已確認 | 現行交付見[設定與角色政策](analyzer-policy.md)；目標語意與已交付範圍分開 |
| DEC-021 | 保留嚴格 Repository、static Mapper 及 UseCase／薄 Handler 政策，優先修正判定精確度 | 已確認 | 理由、方法論支持界線與代價見[規則設計依據](rule-design-rationale.md)；來源未完整處仍明列待補 |
| DEC-022 | 首個不帶 `-preview` 公開版本之前不考慮向後相容 | 已確認 | 可直接整理 ID、schema、defaults、API，不強制 legacy／replacement／migration scaffolding；已發布 bytes 不可覆寫 |
| DEC-023 | 首個不帶 `-preview` 公開版本須具備逐規則依據、完整使用說明與隨包可讀 API XML | 已確認 | 人與 AI 均須能理解產品契約；交付要求不等於檢查已通過 |

## 4. 產品設計原則

### 4.1 規範推理與機械化執行分離

產品文件直接回答「為什麼、何時、如何取捨」，以外部方法論／作者來源說明支持界線；Analyzer 等機制回答「哪些條件已被實際判定，以及結果證據為何」。協作知識可協助審查，不取代產品自行說明設計理由的責任。

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

### 4.8 版本政策明示

公開介面、Diagnostic ID、規則語意、預設 Severity、設定 Schema 與報告格式均須有明確當前契約。首個不帶 `-preview` 公開版本之前不考慮向後相容；正式版相容承諾另行選定，見[版本政策](../releases/version-policy-proposal.md)。

### 4.9 發布採明確允許清單

Repository 預設不可打包；只有 package inventory 明確核准的專案能輸出發布成品。

### 4.10 來源可追溯

每條規則直接說明外部設計依據、產品選擇、理由與取捨，並區分方法論原則與產品加強。程式碼來源、授權與 Ownership 以專門 provenance／license 資料保存，不把抽取歷史或私人討論當成方法論依據。

### 4.11 產品文件採受控持續演進

產品規劃書與 PRD 應維持穩定文件身分、狀態與責任人。重大產品邊界、需求或驗收變更必須更新對應文件，但 Sprint 進度、單次 PR 細節與暫時性工作紀錄不應直接堆入產品基線。

### 4.12 正式來源、衍生輸出與雙向追溯分離

- Markdown 是 Repository 內的正式文件來源，檔名維持穩定；版本由文件 Metadata、Git commit 與既有發布紀錄保存。
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
   ├─ Adoption and configuration guidance
   ├─ Provenance Records
   └─ Consumer Samples
```

### 7.1 Rule Model

Rule Model 是產品真正的中心，負責定義：

- 版本內唯一且可追溯的 Rule ID；
- 規則標題、理由與適用範圍；
- 可機械化程度；
- 角色需求；
- Diagnostic ID／Architecture Test／CLI Check 對照；
- 預設 Profile 與 Severity；
- 外部方法論／作者來源與支持界線；
- 版本與依生效政策所需的生命週期資訊。

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

- ArchPrecept 擁有規則理由、產品政策、Rule Catalog、角色模型、Analyzer、選配 Contracts、使用說明與交付證據。
- AI Collaboration Framework 提供工程與 AI 協作知識、skill 與審查方法；明選可讀知識不代表已採納 normative rule，也不等於產品驗證。
- 採用端擁有自身架構、role mapping、啟用 Profile、例外與執行結果。
- 規則對外歸因引用方法論作者或權威來源，Framework 研究工作流、私人對話及其他專案歷史不作為規則設計權威。

### 8.1 真正衝突的處理

當規則與其宣稱的方法論，或本專案明選的 Framework 知識，在同一角色、適用條件及責任邊界提出互不相容的要求時，只提出受影響規則的窄範圍討論：列出兩方精確來源及狀態、衝突條件、對 consumer 的影響、保留／修正／明選另一政策等選項，交由產品負責人決定後更新當前文件。討論期間只暫停依賴該衝突的結論。

比方法論更嚴格、方法論未規定某種命名或形式，均不自動構成矛盾。應先說明產品加強保護的責任、成本與限制。Framework 的缺失／未讀來源屬證據缺口，不可自行推翻已接受政策。全面禁止 Aggregate static factory 及 MQ-only 尚缺完整設計理由與適用條件，保留為獨立待決議題，不擴張為新的 Analyzer 行為。

### 8.2 開發治理

`docs/` 保存產品文件，`rules/` 保存機器規則，`.dev/` 保存專案開發治理與證據。Framework selection、安裝 bytes、verified reader、規範採納及實際執行分別讀回，不因目錄或文件存在就宣稱完整覆蓋；精確選配與缺口以專案入口及[採納文件](../../.dev/standards/framework-adoption.md)為準。

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
- 當前使用與變更說明。

Repository 邊界不等於 NuGet 相依邊界。同一 repo 仍可讓 Analyzer-only、Contracts-only 與 Full Adoption 完全獨立安裝及版本化。

### 10.2 未來拆分 Reference Contracts repo 的觸發條件

只有在下列情況實際發生時才拆分：

- Reference Contracts 形成獨立 Roadmap、文件網站或使用者社群；
- Analyzer 與 Contracts 的 Release Cadence 長期衝突；
- 維護團隊、權限或發布 Ownership 分離；
- 授權或來源治理需要隔離；
- Contracts 擴張為完整 Runtime Framework；
- 大量使用者只採用 Contracts，且 Guardrails 已不再是主要產品關聯。

## 11. Git repo 結構與責任

```text
ArchPrecept/
├─ src/
│  ├─ Contracts/                 # 五個選配契約專案
│  └─ Analyzers/                 # Core Analyzer
├─ tests/                       # 對應能力測試，不發布
├─ samples/                     # 採用範例
├─ rules/
│  ├─ catalog.xml               # 唯一產品機器規則來源
│  ├─ catalog.xsd               # Catalog 結構
│  └─ configuration.xsd         # Consumer 設定結構
├─ docs/
│  ├─ product/                  # 定位、PRD、規則理由、外部來源與使用政策
│  ├─ governance/               # 文件責任、狀態與追溯
│  ├─ diagnostics/              # 現行 Diagnostic 與限制
│  └─ releases/                 # 發布政策與歷史證據
├─ .dev/                        # 開發治理及工作證據
├─ eng/                         # 建置、打包與 consumer 驗證入口
└─ ArchPrecept.slnx
```

### 11.1 實體結構原則

- 可建置、可發布的產品原始碼集中在 `src/<Capability>/`，不在根目錄另建平行產品樹。
- 產品 Project、預設 Assembly Name 與根 Namespace 使用 `ArchPrecept.<Capability>`；測試專案使用對應產品名稱加 `.Tests`，不進入發布 inventory。
- Analyzer 與 Contracts 目錄相鄰不構成套件相依；五個契約與 Analyzer 各自可獨立採用。
- `rules/catalog.xml` 與 XSD 是機器資料；人類可讀理由、來源支持界線及修正方式集中於產品／Diagnostic 文件，不複製第二份 Rule Catalog。
- 規劃中的 ArchitectureTesting／Conformance 只有在實作後才新增相應專案，不以空專案代表能力已交付。
- `.dev/` 保存專案開發治理及證據；Framework 安裝不搬移或取代 `docs/` 的產品 Ownership。
- `samples/` 供人與 AI 閱讀；實際 pack 成品的 consumer 驗證仍須獨立執行。

## 12. 預期發布成品

> 本節區分現行套件與規劃能力；列出不代表某次發布已完成。

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

- XML Rule Catalog、Catalog XSD 與設定 XSD
- Diagnostic 文件
- Profile 與 Severity 範本
- 當前採用與角色映射指引
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
- 當前使用與變更說明
- Compatibility Manifest
- Rule Catalog Snapshot
- Software Bill of Materials（SBOM，後續成熟階段）

### 12.6 現行套件與未來能力

現行六個 Package ID 如下；實際來源與發布結果分別由目前 checkout 及各版本發布紀錄證明。nuget.org 尚未納入發布範圍，未來能力名稱不預先視為已發布 ID。

| 能力 | Package ID／未來候選 | 實作狀態 |
|---|---|---|
| Domain Contracts | `ArchPrecept.Contracts.Domain` | 已有對應專案 |
| Application Contracts | `ArchPrecept.Contracts.Application` | 已有對應專案 |
| Messaging Contracts | `ArchPrecept.Contracts.Messaging` | 共用訊息契約，不依賴 port 或 Configuration |
| Application Messaging Ports | `ArchPrecept.Contracts.Application.Messaging` | 發布 port 與傳遞 metadata，只依賴 Messaging；見[責任邊界](messaging-contract.md) |
| 核心 Analyzer | `ArchPrecept.Analyzers.Core` | 已有對應專案 |
| Event Sourcing Contracts | `ArchPrecept.Contracts.Domain.EventSourcing` | CT-005 已有對應專案；見[行為契約](event-sourcing-contract.md) |
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

### 13.2 Preview 與正式版政策

首個不帶 `-preview` 後綴的公開版本之前不考慮向後相容。可直接調整 ID、schema、defaults 與 API，不為舊 preview 強制保留 legacy 模式、replacement rule、雙軌 API 或 migration scaffolding。版本內識別仍須唯一、文件與實作一致；已發布 Package ID／version 的 bytes 不可覆寫。

六個現行套件暫採共同 release train，各自仍可獨立安裝。正式版的版號模型、維護線與相容承諾待選定；本文件不自動選定下一版本或授權發布。詳見[版本政策與首個正式版條件](../releases/version-policy-proposal.md)。

### 13.3 發布閘門

公開 Candidate 必須完成：

1. Restore、Release Build、所有測試；
2. Analyzer-only／Custom Contracts Consumer Test；
3. Reference Contracts Consumer Test；
4. False-positive Regression Test；
5. Public API 與 Analyzer 行為差異檢查；Preview 不以維持舊版相容為通過條件；
6. Package Content、Source Link、Symbols 與 Metadata 檢查；
7. 本機 Feed 安裝驗證；
8. Rule Catalog／Diagnostic 文件完整性驗證；
9. Provenance 與 License Gate；
10. 由不可變 Commit 產出且不重新建置的發布 Artifact。

首個不帶 `-preview` 的公開版本另須完成每條規則的外部依據、選擇與對齊／加強說明、各套件完整使用文件，以及公開 API 的有意義 XML（summary、適用參數／回傳／例外、生命週期與限制），並證明 XML 隨 nupkg 交付且 feed consumer 可讀。這是待交付 gate，不宣稱現有 pipeline 或本次文字修改已驗收通過。

### 13.4 文件與規則基線治理

- Repository 內的產品規劃書與 PRD 使用穩定路徑：`docs/product/product-plan.md` 與 `docs/product/prd.md`。
- 文件版本記錄於文件控制區塊及 Git 歷史，不以 `-v0.x` 檔名在 Repository 內建立多份並行正本。
- Markdown 為可審查的正式來源；Word／PDF 僅由正式來源產生。
- Rule Catalog、Diagnostic 文件與 Traceability Mapping 應可由 CI 驗證一致性。
- 重大產品或需求變更直接更新現行理由、語意及驗收，歷史由可審查 Git diff／commit 保存；只有有實際用途時才另建 ADR 或替代映射，不強制附加變更日誌。

## 14. 來源、授權與 Ownership 規劃

### 14.1 產品設計依據

規則設計以外部方法論／作者來源為依據，直接說明所解決的問題、選定政策、替代方案的取捨與檢查限制。入口為[規則設計依據](rule-design-rationale.md)與[方法論來源](methodology-references.md)。產品加強須明確標示，不把 opinionated 選擇歸因為方法論普遍禁令。

### 14.2 來源與授權

自有內容採 Apache-2.0；第三方內容維持其授權。程式碼及文件的來源清冊、逐檔檢查、第三方聲明與致謝由 [NOTICE](../../NOTICE)、[THIRD-PARTY-NOTICES](../../THIRD-PARTY-NOTICES.md) 及既有來源／授權資料維護。發布成品不得包含權利未處理的教材、文字、程式碼或其他受保護內容。

實際來源沿革與授權證據有其保存用途；它們不替代規則設計理由，也不要求在每條現行政策重述研究過程。

### 14.3 Ownership

- 本 repo 擁有後續自行維護的參考契約、Rule Catalog、角色模型、Analyzer、Architecture Testing、Conformance、測試與對外產品文件。
- Consumer／實驗專案可採用已發布成品；不重複維護同責任的產品實作。
- 協作 Framework 擁有其 skill、知識與工作方法，不因此擁有本產品的規則設計權威。
- 採用端保存自己的真實架構、設定、例外與驗證結果，不將其實驗結果直接推成普遍產品承諾。

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
- 100% Diagnostic ID 在版本內不重複，且與當前文件及實作一致。
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

**目標：** 完成 repo 命名、產品定位、目錄策略，以及來源與授權檢查基線。<br>
**主要工作：**

- 將 repo 定位改為 `ArchPrecept`；
- 建立 Product Plan、PRD、文件治理與規則追溯規範；
- 維護 `docs/product/`、`docs/governance/` 與 `rules/catalog.xml` 的正式來源責任；
- 建立來源清冊、權利證據與 Provenance 記錄；
- 維持預設不可打包、inventory 明選專案 opt in 的邊界；
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
- 發布 Diagnostic 文件、當前使用說明與平台／成品矩陣。

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

- 選定正式版相容政策，穩定 Public API、Diagnostic 與 Config Schema；
- 完成逐規則外部依據、選擇／取捨與方法論對齊／加強說明；
- 完成各套件使用文件及隨 nupkg 交付、consumer 可讀的有意義 API XML；
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
| ID／行為與當前文件不一致 | 採用者無法解釋結果 | 每版內唯一、一致性檢查及清楚的當前使用說明；正式版前不保留 preview 相容負擔 |
| 來源揭露或授權檢查不完整 | 公開發布存在權利與信任風險 | 檔案層級來源清冊、授權條款與權利證據、必要授權確認或獨立重寫 |
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
- 用產品文件授予某次套件發布權限或判斷商標權利；具體發布沿當次授權及 CI/CD 流程。

## 19. 待決策事項

| 議題 | 狀態／決策時點 |
|---|---|
| 正式版版號模型、維護線與相容承諾 | 首個不帶 `-preview` 公開版本前選定；不影響目前 Preview 直接整理契約 |
| 全面禁止 Aggregate static factory 及 MQ-only 的完整設計理由與適用條件 | 保留為獨立待決議題；不因缺證據自行撤銷政策或新增行為 |
| 真實 consumer 需要的 batch／purge 等額外 port | 有具體場景、責任與失敗語意後個別評估，不擴張 shared 最小 Repository |
| 完整 Event Sourcing runtime 是否另立產品／repository | 實際 storage／projection 需求確立後；選配核心契約邊界已確定 |
| Architecture Testing／跨專案依賴與 Conformance CLI | 各自保持獨立規劃，不作文件正規化的隱含前置 |
| Code Fix、SBOM、簽章／attestation 與 Contracts 拆 repo | 依產品成熟度及實際需求評估 |

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
- 0.0.3-preview 候選包含 19 個產品 Diagnostic 與 2 個設定／範圍 Diagnostic；驗收數量與成品證據見[版本交付](../releases/0.0.3-preview.md)；
- 所有既有 Analyzer 共用 Role Classifier：明確 mapping、內建／繼承契約與可選低信心命名慣例；
- 六個可封裝專案已建立 nupkg 版面、相依方向與真實 consumer 驗證；
- Project、Assembly、Namespace 與公開 Package ID 已核准採 `ArchPrecept.<Capability>`；Apache-2.0、public GitHub Packages 與 main-push 發布已由既有決策核准。

目前尚未建立 `src/ArchitectureTesting/` 與 `src/Conformance/` 專案；兩者是已規劃但尚未實作的能力邊界，不以空目錄或空專案預先佔位。

主要重構方向：

- 維持 Project、Assembly 與 Namespace 的 `ArchPrecept.<Capability>` 階層，禁止重新引入 `Backend`、`Lab` 與 `BuildingBlocks` 產品身分；
- 維持中央 Classifier 與明確 applicability／not-evaluated 狀態；
- 持續以正式 Analyzer 成品驗證實際 Compiler 行為；
- 將 Reference Contracts 從複製來源轉為 Canonical Ownership；
- 維護唯一 Rule Catalog、Consumer Matrix 與 Release Governance。

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

- [規則設計依據](rule-design-rationale.md)：產品選擇、理由、代價、方法論對齊／加強及已知缺口。
- [方法論來源](methodology-references.md)：外部作者／權威來源及支持界線。
- [產品需求](prd.md)：可觀察的能力與交付要求。
- [文件治理與追溯](../governance/document-governance-and-traceability.md)：正式來源、證據及衝突處理。

# 附錄 D：文件治理與持續演進

本產品以《ArchPrecept 文件治理與規則追溯規範》作為完整治理依據，摘要如下：

- 產品規劃書與 PRD 是持續演進的受控基線，不是 Sprint 日誌或單次交付紀錄。
- Repository 內以 `docs/product/product-plan.md`、`docs/product/prd.md` 作為穩定 Markdown 正本；版本由文件控制 Metadata 與 Git 歷史保存。
- Word／PDF 是衍生輸出，不得與 Markdown 並行人工修改。
- 重大產品邊界變更直接更新現行理由及驗收，歷史由 Git 保存；有獨立決策比較需要時才另建 ADR，不以追加日誌取代現行正文。
- 文件重排不改動 Requirement ID；Rule／Diagnostic 的 preview 調整依第 13 節，不為舊 preview 強制維持 replacement／migration。
- 工程規範、PRD、Rule Catalog、Diagnostic／Architecture Test、實作與測試必須可雙向追溯。
- 未來安裝 AI Collaboration Framework 時，先執行 Gap Analysis，再以 `.dev/` 補上 Requirements、Specs、Standards、Workflows 與 Release Evidence；產品文件不因安裝行為而預設搬家。
- NuGet Package Naming 決策完成後，應新增 Naming／Identity 文件或附錄，不改寫不依賴名稱的需求語意。
