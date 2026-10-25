---
title: "ArchPrecept"
subtitle: "文件治理與規則追溯規範"
author: "產品負責人：YuChia Wei"
date: "2026-10-18"
lang: "zh-TW"
document_id: "DOC-GOV-TRACEABILITY"
document_version: "0.3"
status: "draft"
last_reviewed: "2026-10-25"
canonical_path: "docs/governance/document-governance-and-traceability.md"
canonical: true
source_format: "markdown"
owners:
  - "YuChia Wei"
related_documents:
  - "DOC-PRODUCT-PLAN"
  - "DOC-PRD"
---

# 目錄

```text
文件控制
執行摘要

文件治理與規則追溯規範
├─ 1. 文件目的與適用範圍
├─ 2. 治理目標與核心原則
├─ 3. 文件與產品資產分類
├─ 4. Repository 擺放規則
├─ 5. 正式來源與衍生輸出
├─ 6. 文件生命週期與狀態
├─ 7. 產品規劃書與 PRD 的演進方式
├─ 8. 雙向追溯模型
├─ 9. 規則覆蓋與缺口分類
├─ 10. AI Collaboration Framework 導入策略
├─ 11. Gap Analysis 執行流程
├─ 12. 自動化驗證與 CI Gate
├─ 13. Ownership 與審查節奏
├─ 14. 現行文件維護方式
└─ 15. 驗收條件

附錄
├─ A. 建議 Repository 結構
├─ B. Traceability Record 範例
├─ C. 文件 Metadata 範例
└─ D. 決策摘要
```

# 文件控制

> **文件狀態：治理基線草案（Governance Baseline Draft）**<br>
> **目標 GitHub Repository：** `YuChia-Wei/ArchPrecept`<br>
> **版本：** `0.3`<br>
> **上游文件：** 《ArchPrecept 產品規劃書》與《產品需求文件（PRD）》<br>
> **Repository 正式路徑：** `docs/governance/document-governance-and-traceability.md`

全文件仍為 draft；明定的產品文件責任、Preview 政策與逐規則說明要求在各自範圍內適用，不表示整份治理規劃已落實或全部 gate 已通過。

| 欄位 | 內容 |
|---|---|
| 文件 ID | `DOC-GOV-TRACEABILITY` |
| 建議正式路徑 | `docs/governance/document-governance-and-traceability.md` |
| 正式來源 | Markdown；Word／PDF 僅為衍生輸出 |
| 文件目的 | 定義產品文件如何持續演進、放置、審查、輸出與追溯，並規劃未來 AI Collaboration Framework 的導入方式 |
| 適用對象 | 產品負責人、Repository 維護者、Analyzer／Contracts／Conformance 開發者、Reviewer、Release 維護者與 AI Agent |
| 治理範圍 | 產品規劃書、PRD、工程規範、Rule Catalog、Diagnostic 文件、Traceability Mapping、`.dev` 開發治理與衍生文件輸出 |
| 不包含 | 單一功能的完整技術規格、NuGet Package 命名、日常 Sprint 排程與單次 PR 實作細節 |
| 正式來源 | Repository 中的 Markdown 與機器可讀 Catalog／Mapping |
| 衍生輸出 | Word、PDF、GitHub Release 附件及其他閱讀格式 |

# 執行摘要

`ArchPrecept` 的產品價值建立在「規範、規則與驗證證據可被一致維護」之上。若產品規劃書、PRD、工程規範、Rule Catalog、Diagnostic 與測試各自演進，即使 Analyzer 技術上正確，產品仍可能出現兩類風險：一是工程規範已改變但驗證工具未跟上；二是 Analyzer 已新增限制，卻缺少可辯護的規範依據。

因此，本文件將產品文件視為**持續演進的受控產品資產（governed living product assets）**，而不是一次性提案、會議紀錄或工作日誌。產品規劃書與 PRD 放在 `docs/product/`，Markdown 為單一正式來源；Word／PDF 為衍生輸出。`rules/catalog.xml` 為唯一產品機器規則來源，結構由 `rules/catalog.xsd` 定義；consumer 設定結構為 `rules/configuration.xsd`。`.dev/` 承接開發治理與工作證據，不取代產品文件。

Framework 導入時，不以「把既有文件搬到新目錄」作為完成條件，而是透過 Gap Analysis 確認：每條 Diagnostic 是否有工程規範、Requirement、Rule ID 與測試；每條可機械化規範是否有對應驗證機制；未機械化的部分是否明確標記為人工／AI 審查或產品缺口。

## 1. 文件目的與適用範圍

本文件回答以下問題：

- 產品規劃書與 PRD 是否需要持續更新，以及何時更新；
- 文件在目前 Repository 中應如何擺放；
- Markdown、Word 與 PDF 何者是正式來源；
- 產品規劃、需求、工程規範、Rule、Diagnostic、實作與測試如何建立雙向追溯；
- AI Collaboration Framework 導入後，`docs/`、`rules/` 與 `.dev/` 如何分工；
- 如何用自動化檢查發現文件漂移、規則缺口與無規範依據的 Analyzer 行為。

本文件不取代產品規劃書或 PRD，也不定義單一 Rule 的完整工程內容。它定義的是上述資產的治理方式與相互關係。

## 2. 治理目標與核心原則

### 2.1 治理目標

1. 建立單一、可版控且可審查的產品正式來源。
2. 讓產品邊界、需求、工程規範、規則與實作不會無聲漂移。
3. 讓人、AI Agent 與 CI 都能判斷某項規則的來源、覆蓋程度及驗證證據。
4. 用可審查 Git 差異保存歷史；現行正文直接說明有效理由與範圍。
5. 在 AI Collaboration Framework 導入前後維持清楚 Ownership。

### 2.2 核心原則

- **產品文件不是工作日誌。** 直接更新長期有效的產品決策、理由、需求與驗收；歷史由 Git 保存，不逐次追加研究／批准／執行經過。
- **Markdown 是正式來源。** Word／PDF 不與 Markdown 並行人工維護。
- **穩定路徑優先。** Repository 正本不把版本號放進檔名。
- **目錄表示責任，不只是格式。** `docs/`、`rules/`、`.dev/` 不可互相取代。
- **追溯必須雙向。** 規範可以找到實作，實作也必須能反查規範。
- **未評估不等於通過。** 文件與規則缺口必須明確呈現。
- **導入 Framework 前先做 Gap Analysis。** 不以搬檔或產生模板檔案代表治理完成。

## 3. 文件與產品資產分類

| 資產類型 | 主要問題 | 正式位置 | 更新頻率 |
|---|---|---|---|
| 產品規劃書 | 為何做、產品邊界、能力與 Roadmap | `docs/product/product-plan.md` | 低至中 |
| PRD | 產品必須提供什麼、如何驗收 | `docs/product/prd.md` | 中 |
| 文件治理規劃 | 文件、追溯與 Framework 導入如何治理 | `docs/governance/document-governance-and-traceability.md` | 低至中 |
| 產品架構文件 | Rule Model、Role Classification、相依與執行模型 | `docs/architecture/` | 中 |
| 規則設計說明 | 外部方法論依據、產品選擇、理由、代價與支持界線 | `docs/product/rule-design-rationale.md`、`docs/product/methodology-references.md` | 與規則同步 |
| Diagnostic 文件 | Diagnostic 行為、修正、Severity、Help Link | `docs/diagnostics/` | 與 Diagnostic 同步 |
| Rule Catalog | 現行 Rule ID、適用性、機械化程度與 Diagnostic 映射 | `rules/catalog.xml`；設定結構為 `rules/configuration.xsd` | 與規則同步 |
| Traceability Mapping | Requirement、Rule、理由、實作與測試映射 | 依現行 Catalog／文件保存；未實作的映射資產標為規劃 | 與規則／實作同步 |
| 產品原始碼 | Contracts、Analyzer、Architecture Testing 與 Conformance 實作 | `src/<Capability>/` | 與需求／規則實作同步 |
| 自動驗證專案 | 各產品能力與 Consumer 的測試證據 | `tests/<Capability>/` | 與對應實作同步 |
| 平台／成品矩陣 | 支援 SDK／IDE、成品及實際測試範圍 | 產品使用與發布文件；正式版相容承諾另行選定 | 每次 Candidate／Release |
| 開發 Requirement | 從 PRD 拆出的可排程需求 | `.dev/requirements/` | 高 |
| 技術 Spec | 單一能力的設計、介面與驗收 | `.dev/specs/` | 高 |
| Workflow Evidence | 單次變更計畫、結果、Review 與證據 | `.dev/workflows/` | 高 |
| Release Evidence | Pack、測試、Checksum、相容性與發布結果 | `.dev/releases/` | 每次 Candidate／Release |

## 4. Repository 擺放規則

### 4.1 產品正式來源

```text
docs/
├─ product/       # Product Plan、PRD、規則理由與外部來源
├─ governance/    # 文件責任、狀態、追溯與衝突處理
├─ diagnostics/   # 現行 Diagnostic、修正與已知限制
├─ releases/      # 發布政策與各版本歷史證據
└─ ...

rules/
├─ catalog.xml           # 唯一產品機器 Rule Catalog
├─ catalog.xsd           # Catalog 結構
└─ configuration.xsd     # 現行設定 schema
```

文件可以引用機器資料，不另建第二份 Catalog。未來 Mapping／Profile／Schema 的規劃不得被描述成現行已交付檔案。

### 4.2 開發治理與 Framework

`.dev/` 保存專案開發治理、需求／規格拆解與工作證據；Framework selection、安裝與可讀知識的精確狀態以[採納文件](../../.dev/standards/framework-adoption.md)及實際 inspect 為準。selection 或檔案存在不等於 verified reader、規範採納或執行驗證。

`docs/product/` 保持產品正本，`.dev/` 不取代對外產品理由與使用文件。Framework 安裝與升級按具體範圍盤點，不以套用 Backend Application 假設或搬移所有文件代表完成。

### 4.3 不建議的做法

- 在 `docs/` 根目錄長期累積 `prd-v0.1.md`、`prd-v0.2.md`、`prd-final.md`、`prd-final-2.md`。
- 讓 Word 與 Markdown 各自由不同人修改。
- 把 Rule Catalog 只寫在 Markdown 表格中，讓工具無法驗證。
- Framework 安裝後把所有文件搬進 `.dev/`，導致對外產品文件與單次工作證據混在一起。
- 把 GitHub Issue、PR 說明或聊天紀錄當成唯一需求來源。

## 5. 正式來源與衍生輸出

### 5.1 正式來源

Repository 中的正式文件使用穩定檔名：

```text
docs/product/product-plan.md
docs/product/prd.md
docs/governance/document-governance-and-traceability.md
```

文件版本寫入控制 Metadata，歷史內容由 Git commit 與既有發布紀錄保存。現行正文直接呈現有效理由和契約；ADR／替代映射只在有獨立用途時建立，Preview 不強制舊政策並存或 migration scaffolding。

### 5.2 衍生輸出

Word／PDF 可用於主管審閱、外部溝通、課程成果或正式會議，但需符合：

- 從目前 Markdown 正本產生；
- 標示來源 Commit 或文件版本；
- 不直接回寫修改；
- 必要時放在 `docs/exports/`，或附於 GitHub Release，而非視為另一份正本。

### 5.3 輸出一致性

理想流程：

```text
Markdown 正本
  ├─ Review / Diff
  ├─ CI 內容與連結驗證
  └─ 產生 DOCX / PDF
          └─ Render / Visual QA
```

## 6. 文件生命週期與狀態

| 狀態 | 定義 | 可作為實作依據 |
|---|---|---|
| `draft` | 全文件尚未完成必要審查；不自動撤銷已明確確認的個別決策 | 僅限另有明確確認且適用的選定範圍；不得聲稱整份文件已核准 |
| `in-review` | 正在接受產品、架構或技術審查 | 有條件 |
| `approved` | 已核准，但尚未進入實際採用或發布 | 是 |
| `active` | 目前正式有效的產品／需求基線 | 是 |
| `superseded` | 已由另一份文件或版本取代 | 否，僅供歷史查詢 |
| `archived` | 不再維護，且沒有現行效力 | 否 |

每份受治理文件至少記錄：

- `document_version`
- `status`
- `owner`
- `last_reviewed`
- `canonical_path`
- `supersedes`／`superseded_by`（適用時）

## 7. 產品規劃書與 PRD 的演進方式

### 7.1 產品規劃書

應在以下情況更新：

- 產品定位、能力邊界或非目標改變；
- Reference Contracts 是否拆 repo 等 Repository 策略改變；
- Ownership、授權、發布或相容性治理改變；
- 新增或移除重大產品能力；
- Roadmap 或成功指標出現產品層級調整。

不應放入：單一 Issue、單一 Diagnostic Bug、Sprint 進度、臨時工作分派及一次性 CI 失敗紀錄。

### 7.2 PRD

應在以下情況更新：

- 新增、修改或移除產品需求；
- Priority、驗收標準、資料模型或非功能需求改變；
- MVP／Release Gate 改變；
- Requirement 與 Rule／Diagnostic 的追溯關係改變。

Requirement ID 不因章節重排而更改；現行需求直接更新語意、理由與驗收，歷史由 Git 保留。首個不帶 `-preview` 公開版本之前不考慮向後相容，Rule／Diagnostic ID、schema、defaults、API 可直接調整，不強制 replacement、legacy 並存或 migration scaffolding。版本內唯一與已發布成品不可覆寫仍須遵守，詳見[版本政策](../releases/version-policy-proposal.md)。

### 7.3 與 `.dev` 的關係

未來：

```text
Product Plan
    ↓
PRD Requirement
    ↓
.dev/requirements/ 可排程需求
    ↓
.dev/specs/ 技術規格
    ↓
Source / Analyzer / Tests
    ↓
.dev/workflows/ 與 .dev/releases/ 證據
```

PRD 不取代 Feature Spec；Feature Spec 也不得無聲改變 PRD 的產品語意。

## 8. 雙向追溯模型

### 8.1 正向追溯

```text
產品目標
  → PRD Requirement ID
  → 工程規範／Normative Source
  → Rule ID
  → Enforcement Mechanism
  → Implementation
  → Automated Test / Consumer Evidence
  → Release Evidence
```

### 8.2 反向追溯

```text
Diagnostic / Architecture Test / CLI Check
  → Rule ID
  → PRD Requirement
  → 工程規範
  → 產品能力與目標
```

### 8.3 為何必須雙向

正向追溯能發現：

- 規範寫得很清楚，但沒有 Analyzer／Architecture Test；
- PRD 要求存在，但沒有實作或測試；
- 某條規則只完成部分覆蓋。

反向追溯能發現：

- Analyzer 只是因為技術上可檢查而新增，沒有產品需求或規範依據；
- Diagnostic 行為已擴張，但文件沒有更新；
- 測試只驗證實作細節，沒有驗證真正產品語意。

### 8.4 Traceability 的最小欄位

- Traceability ID
- Product Requirement ID
- 產品理由所在文件／Section
- 外部方法論／作者來源、對應段落與支持界線
- 產品選擇及對齊／加強說明
- Rule ID
- Applicability／Profile／Architecture Role
- Enforcement Level
- Diagnostic ID／Architecture Test ID／CLI Check ID
- Implementation Path
- Test／Consumer Evidence Path
- Coverage：`full`、`partial`、`assisted`、`review-required`、`missing`
- Lifecycle Status

## 9. 規則覆蓋與缺口分類

| 分類 | 說明 | 報告行為 |
|---|---|---|
| `machine-enforced` | 可由工具可靠判定 | 必須有執行機制與測試 |
| `machine-assisted` | 工具可檢查部分條件 | 顯示覆蓋範圍與人工判斷項目 |
| `review-required` | 需人或 AI 進行工程判斷 | 不得假裝自動通過 |
| `missing-enforcement` | 規範可機械化，但尚無工具 | 列入產品 Backlog／Gap Report |
| `missing-normative-source` | 公開規則缺少可查證依據或產品理由；工具 Diagnostic 缺少證據契約 | 明列缺口，按適用的發布 gate 阻擋；不得冒稱已補齊 |
| `not-applicable` | Profile 或產品情境不適用 | 記錄理由 |
| `not-evaluated` | 缺少角色映射、環境或執行條件 | 不得計入 Passed |
| `superseded` | 已由新規則或需求取代 | 歷史由 Git 保存；替代／相容處置依版本政策 |

## 10. AI Collaboration Framework 導入策略

### 10.1 導入前提

Framework 應先具備或允許建立：

- 適合 Analyzer／NuGet 套件產品的 Target Profile；
- Requirement、Spec、Standard、Workflow、Release Evidence 的清楚責任；
- 不會複製 Analyzer 原始碼或 Reference Contracts；
- 能保存 Target Repository 自身產品事實，而不是套用 Backend Application 假設；
- 能與既有 Rule Catalog 及 Traceability Mapping 共存。

### 10.2 導入原則

1. 先盤點，再安裝。
2. 先做 Dry Run 與 File Disposition Plan。
3. 保留 `docs/` 的產品文件 Ownership。
4. 保留 `rules/` 的機器可讀產品資料 Ownership。
5. 以 `.dev/` 新增開發治理與證據，不直接覆蓋既有產品真實內容。
6. 建立 Compatibility／Provenance，而不是把複製檔案宣稱為已安裝版本。

### 10.3 導入後責任

- `docs/`：產品定位、PRD、產品架構、對外規則文件。
- `rules/`：Rule Catalog、Profile、Schema、Mapping。
- `.dev/`：Target-owned Requirements、Specs、Standards、Workflows、Release Evidence。
- `.ai/`／Skills：AI Agent 執行與審查方式。

## 11. Gap Analysis 執行流程

### 11.1 規則理由與來源盤點

每條規則分開記錄：外部方法論／作者的原始主張及其支持界線、產品選擇、為何選它、保護的責任、代價與限制、如何對齊或加強相關方法論。入口為[規則設計依據](../product/rule-design-rationale.md)與[方法論來源](../product/methodology-references.md)。設定／範圍 Diagnostic 說明自身契約，不強配方法論出處。

正式產品文件只引用可查證的外部方法論／作者作為設計依據。私人討論、其他專案歷史及內部研究紀錄不作為設計權威，也不要求讀者重建研究過程。程式碼實際來源／授權資料另有保存用途，不能替代上述設計說明。

### 11.2 Diagnostic 與 Architecture Test 盤點

列出：

- Diagnostic／Check ID；
- Rule Title；
- Default Severity；
- 目前角色辨識方式；
- 實作路徑；
- 測試與 Consumer Evidence。

### 11.3 Reference Contracts 盤點

列出 `IAggregateRoot`、Domain Entity、`IUseCase`、Repository Port、Messaging Contract 等型別，以及它們對應的 Architecture Role 與 Rule。

### 11.4 雙向映射

產生：

```text
Standard → Requirement → Rule → Mechanism → Test
Mechanism → Rule → Requirement → Standard
Contract → Role → Applicable Rules
```

### 11.5 缺口分類與 Backlog

建立下列工作項目：

- 缺少規範文件；
- 缺少 Rule ID；
- 缺少機械化執行；
- Analyzer 只有不可靠名稱判斷；
- 缺少 Custom Contracts Consumer Test；
- 缺少 False-positive Test；
- 缺少 Help Link 或 Diagnostic 文件；
- 規則與 AI Collaboration Framework 語意衝突。

### 11.6 真正衝突與來源缺口

當規則與方法論或明選 Framework 知識在相同適用範圍有互斥要求時：

1. 固定受影響 Rule／Diagnostic、角色、Profile 與場景。
2. 列雙方精確來源、狀態、原始主張及互斥條件；未讀或未取得的來源明列缺口。
3. 說明 consumer 影響，提出保留並補理由、修正規則或明選另一政策等具體選項與代價。
4. 交由產品負責人決定，並直接更新當前規則理由、要求與驗收；只暫停依賴該衝突的判斷。

較嚴政策、方法論未強制某個名稱／形式或另一合法風格存在，均不自動構成矛盾。來源未完整也不自動撤銷已接受選擇；全面禁止 Aggregate static factory 及 MQ-only 尚缺完整設計理由與適用條件，保留為獨立待決議題，不作新增行為依據。審查核對理由、有效契約與實作範圍，不能只靠格式完整判定已解決。

## 12. 自動化驗證與 CI Gate

建議逐步建立：

- 文件 Metadata 與狀態驗證；
- Markdown Link 與 Path 驗證；
- Rule Catalog Schema Validation；
- Rule ID／Diagnostic ID 唯一性；
- Traceability Mapping 完整性；
- 公開 Diagnostic 必須有規範來源、Rule ID、Help Link 與測試；
- `machine-enforced` 規則必須有執行機制；
- Word／PDF 輸出的來源 Commit／文件版本一致性；
- Release Candidate 前的整體 Gap Report。

首個不帶 `-preview` 的公開版本必須另外驗收逐規則理由、完整套件使用說明與對人／AI 可讀的公開 API XML。XML 要說明責任、不變條件、所有權／可變性、生命週期、適用參數／回傳／例外與限制；原始註解語意、生成檔、nupkg 隨包內容與 feed consumer 可讀分別留證。詳見 PRD AD-009～AD-011／RL-011 及[版本政策](../releases/version-policy-proposal.md)。

這些是產品要求；現有 preview pipeline 是否實際檢查、每條規則是否補全、XML 是否交付，均須以實際證據回答，不因本文件新增文字而視為通過。

初期可先以 Script 或 Conformance Tool 執行；成熟後由 Conformance 成品提供統一報告。

## 13. Ownership 與審查節奏

### 13.1 Ownership

| 資產 | Owner |
|---|---|
| 產品規劃書 | 產品負責人 |
| PRD | 產品負責人，技術負責人共同審查 |
| 文件治理規劃 | 產品負責人／Repository Maintainer |
| 規則理由／方法論歸因 | ArchPrecept 產品 Owner；外部作者主張依原來源呈現 |
| Rule Catalog／Mapping | Guardrails Maintainer |
| Diagnostic／Architecture Test | 對應元件 Maintainer |
| Consumer／Compatibility Test | Release／品質維護責任人 |
| `.dev` Workflow／Release Evidence | 該次工作與發布 Owner |

### 13.2 審查時機

- 重大產品能力或 Repository 邊界變更前；
- 新增、擴張或淘汰公開 Rule／Diagnostic 時；
- 建立 Release Candidate 前；
- AI Collaboration Framework 安裝或升級後；
- 發現規則誤報、漏報或規範衝突時；
- 固定週期性審查，例如每季或每個 Major Milestone。

## 14. 現行文件維護方式

Product Plan、PRD 與本文件維持各自穩定路徑，Markdown 只有一份正本。每次在授權範圍內直接修正文義、依據、適用條件與驗收，並檢查連結與相關文件的一致性；Git 保存歷史，不在正文追加逐次研究、交接或批准日誌。

規則文件、套件使用說明與 API XML 依對應產品變更同步維護。現行 `rules/catalog.xml` 不因文件盤點而被複製或改名；新機器資料與自動 gate 只有在另行實作並驗證後，才能稱為已交付。

來源與理由缺口、未交付能力、未執行檢查分別列明。文件正規化不授予實作、套件發布或 Framework 安裝權限，也不將歷史測試轉成當前驗收。

## 15. 驗收條件

本治理規劃落地的最低驗收條件：

1. Product Plan、PRD 與本文件在 Repository 中只有一份 Markdown 正本。
2. 正本使用穩定路徑，版本由 Metadata 與 Git 保存。
3. Word／PDF 明確標示為衍生輸出。
4. `docs/`、`rules/` 與`.dev/` 的責任文件化。
5. 每個公開 Diagnostic 都有 Rule ID、規範來源、Help Link 與測試映射。
6. 每條 `machine-enforced` Rule 都有執行機制；缺少時會顯示 Gap。
7. `not-evaluated`、`review-required` 與 `missing-enforcement` 不被計入 Passed。
8. Framework 導入計畫包含 Dry Run、File Disposition、Gap Analysis 與獨立審查。
9. Release Candidate 前會執行文件與追溯一致性檢查。
10. 文件 Owner 與審查節奏已被指定。
11. 真正衝突按第 11.6 節窄範圍處理，較嚴政策不自動視為矛盾。
12. 首個不帶 `-preview` 公開版本前，逐規則依據、完整使用說明、API XML 隨包及 consumer 可讀均有實際驗收證據；未完成前保持明確缺口。

# 附錄 A：建議 Repository 結構

```text
ArchPrecept/
├─ src/
│  ├─ Contracts/
│  └─ Analyzers/
├─ tests/
├─ samples/
├─ docs/
│  ├─ product/
│  ├─ governance/
│  ├─ diagnostics/
│  └─ releases/
├─ rules/
│  ├─ catalog.xml
│  ├─ catalog.xsd
│  └─ configuration.xsd
├─ eng/
└─ .dev/                    # 開發治理與工作證據
```

ArchitectureTesting／Conformance 屬規劃能力，不以空專案預佔。`src/` 第一層是產品能力邊界；可建置、可發布的產品 Project 不得回到 Repository 根目錄的 `analyzers/`、`tools/` 或其他平行原始碼樹。`tests/` 使用相同能力名稱保存不發布的驗證 Project，Traceability Record 必須引用搬移後的正式路徑。

# 附錄 B：Traceability Record 範例

以下是說明追溯關係的文件範例，不是現行設定 schema 或第二份 Rule Catalog；實際 metadata 仍由 `rules/catalog.xml` 維護。

```yaml
traceability_id: TRACE-AP-REPOSITORY-001
status: active
product_requirements:
  - RM-004
  - RC-003
  - AN-004
normative_source:
  document: docs/product/rule-design-rationale.md
  section: repository
rule:
  rule_id: AP-REPOSITORY-001
  coverage: partial
applicability:
  profiles:
    - core
  roles:
    - aggregate-repository
enforcement:
  - type: roslyn-analyzer
    diagnostic_id: DBA1001
implementation:
  - src/Analyzers/ArchPrecept.Analyzers.Core/RepositoryQueryMethodAnalyzer.cs
tests:
  - tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/RepositoryQueryMethodAnalyzerTests.cs
  - eng/Test-PackageConsumer.ps1
release_evidence: null
```

# 附錄 C：文件 Metadata 範例

```yaml
title: ArchPrecept Product Plan
document_version: 0.3
status: active
owner: YuChia Wei
last_reviewed: 2026-08-09
canonical_path: docs/product/product-plan.md
supersedes: 0.2
approved_for:
  - prerelease-planning
not_approved_for:
  - public-package-publication
```

# 附錄 D：決策摘要

| 編號 | 決策 |
|---|---|
| DOC-001 | Product Plan 與 PRD 持續演進，但採受控基線而非工作日誌模式 |
| DOC-002 | Markdown 是 Repository 正式來源；Word／PDF 是衍生輸出 |
| DOC-003 | 正式文件使用穩定路徑與檔名，版本由 Metadata 與 Git 管理 |
| DOC-004 | `docs/` 保存產品文件，`rules/` 保存機器可讀規則，`.dev/` 保存未來開發治理與證據 |
| DOC-005 | Framework 安裝採 Gap Analysis 與語意映射，不以無差別搬檔作為完成條件 |
| DOC-006 | 工程規範、Requirement、Rule、執行機制、實作與測試必須雙向追溯 |
| DOC-007 | 未機械化規範與無規範依據的 Diagnostic 都必須形成可見缺口 |
