---
document_id: "DOC-DEV-FRAMEWORK-ADOPTION"
status: "adopted"
---

# ArchPrecept 的 AI Collaboration Framework 採用

本專案使用 AI Collaboration Framework v0.23.1。可用能力、參考知識與讀取條件由[專案安裝設定](../../.ai/project/installation.json)定義。受管檔案與安裝一致性由 Framework installer 維護及查驗。檔案一致性、特定工作可讀的資源、用戶端實際發現與執行是不同的驗證事項。

## 適用範圍

- 開發 ArchPrecept 的 Analyzer、Contracts、套件、文件與測試時，依[產品決策](../../docs/product/README.md#已確認的產品決策)、[需求與驗收](../product/prd.md)及[規則目錄](../../rules/catalog.xml)。Analyzer 與參考契約保持解耦，支援獨立使用 Analyzer 以及 consumer 使用自有契約。Backend Application 的四層結構、broker、資料庫及 DI 應用慣例只在對應的下游情境適用。
- 定義或驗證下游應用規則時，依該工作明選的領域、架構角色、Profile 與例外判斷。只有明選 `dotnet-backend` 時才可依專案設定和 skill 讀取交集參考相關的 .NET 知識。Consumer 自行決定業務規則、角色映射和適用例外。
- Consumer 範例與測試按所選情境說明契約、host、相容性及預期 Diagnostic。反例應保留其預期行為；若缺少角色映射或執行條件，按該項能力標示未評估。
- 本專案的 Framework 知識選配供設計和審查參考。目前無 Framework normative-rule bindings。公開 Rule／Diagnostic 的適用性、覆蓋與執行契約由本產品維護。規範來源與實作不一致時，明確標出受影響的行為與規則，按[文件治理與規則追溯](../governance/document-governance-and-traceability.md#來源與衝突)處理。

## 下游應用慣例

| 主題 | 適用慣例 |
|---|---|
| DDD、CA 與 Hexagonal Architecture | 以共同語言、Bounded Context、Aggregate 不變條件、Entity 身分和 VO 值語意說明領域。Domain 維持對 transport、ORM、broker 的中立。Application 擁有 UseCase 與 port，adapter 承接外部機制。 |
| Controller、UseCase 與 Handler | 同步 API 預設由 Controller 呼叫 UseCase 介面。存在 dispatch 或 message delivery 才設 Handler，薄轉接至單一 UseCase。Controller 與 Handler 不編排多個 UseCase，也不預設增加 ApplicationService 中介層。僅讀查詢的 fast path 須有 consumer 明確採用的決策與限制。 |
| UseCase 契約及相依 | 採 `I…UseCase`／`…UseCase` 與 `ExecuteAsync`，使用非 optional 的 CancellationToken 與 transport-neutral Input／Output，無輸出可回傳 `Task`。無業務輸入可省略 Input，單一 BCL scalar 可直接傳入。Collection、tuple、自訂型別或多值使用專用 Input。以建構式注入 outbound port，避開 Service Locator、屬性注入、直接建立 Repository、transport framework 或其他 UseCase 相依。 |
| Repository、Mapper、Projection | 分開 Aggregate write port 與 read-only Query Port，以依身分讀取與 Save 表達 Aggregate 生命週期，依產品契約處理允許的形狀及別名。Mapper 負責純轉換。產品規則定義的查詢 Projection 保持唯讀。ES projector 更新 read model 時依其自身角色評估。 |
| 交易與訊息 | 預設每個 Command 變更一個 Aggregate，跨 Aggregate 使用事件及最終一致性。同 BC 強一致性例外須有具名 all-or-nothing 不變條件、中間狀態不可接受且不可補償，並先檢查 Aggregate 邊界。Domain Event、Integration Event、Reactor 與 Outbox 界線須清楚。事件 port 與 Unit of Work 介面各自仍需實際機制才能證明交付或交易保證。 |
| Aggregate、Value Object 與契約式設計 | 依選定的 Aggregate 生命週期採用 ES、Soft Delete 或 Purge。本產品的基底契約為選配。VO 建構後保持不可變並採值相等，可使用 record 或 readonly record struct。Entity 參考與不可變性分別依其診斷判斷，`init` 本身允許，唯讀集合外觀須依實際狀態判斷。適用的領域行為應列明前置、後置與不變條件。 |
| 跨 BC 與 DI | 在明選此 backend 慣例的應用中，跨 BC 維持選定的 MQ 路徑（RabbitMQ／Kafka）與直接 Web API 限制，composition root 明確註冊 `IServiceCollection`。這些應用條件依 consumer 的選用範圍判讀。 |
| BDD 與測試 | Discovery、Formulation、Automation 各有責任。Given／When／Then 描述可觀察行為。xUnit、mocking 和 BDDfy 按所選技術及場景使用。Analyzer 測試沿用適合 Roslyn 行為的 harness。 |

產品如何檢查角色、Repository、UseCase、VO 與其他機制，見 [Analyzer 政策](../../docs/product/analyzer-policy.md)、[診斷目錄](../../docs/diagnostics/README.md)、[Event Sourcing 契約](../../docs/product/event-sourcing-contract.md)及[Messaging 契約](../../docs/product/messaging-contract.md)。對應機械規則以 Rule Catalog 和公開 Diagnostic 契約為準。

## 維護與安裝

缺少協作入口或需要更新專案事實時使用初始化能力，既有政策由治理能力維護，只讀比較則使用稽核能力。Framework 安裝或升級前，按明確授權核對適用性、來源、相依及變更範圍。Installer 管理選配與受管檔，專案維護自己的規則與產品文件。新版本或知識不自動取代既有決策。

提交、合併與發布遵守[本專案的 CI/CD 規則](../releases/cicd-plan.md)及當次授權。選用 Framework skill 本身不建立額外的 PR、派工、模型、provider 或工作紀錄流程。安裝及升級所需的實際查驗按選定範圍判斷。
