---
title: ArchPrecept 產品範圍與方向
document_id: DOC-PRODUCT-PLAN
lang: zh-TW
---

# ArchPrecept 產品範圍與方向

ArchPrecept 讓 .NET 團隊把明確的架構規則轉成可在編譯與 CI 執行、且能解釋結果的檢查。產品維護規則的適用範圍、設計理由與取捨。Aggregate 邊界、業務不變條件、是否採用 Event Sourcing 等設計判斷仍由採用團隊負責。[規則設計理由](rule-design-rationale.md) · [方法論與平台來源](methodology-references.md)

## 產品範圍

| 能力 | 現行範圍 |
|---|---|
| Rule Catalog 與 Analyzer | 共用角色辨識、可組合的 Profile 及 Roslyn 語意檢查。適用的角色、Diagnostic 與 severity 見 [Rule Catalog](../../rules/catalog.xml)，用法與已知限制見 [Analyzer 設定](analyzer-policy.md)和[診斷目錄](../diagnostics/README.md)。 |
| Reference Contracts | 選配的 Domain、Application、Event Sourcing、Messaging 與 Application Messaging。各套件的 API、最低框架及相依見[套件使用說明](../package-usage.md)。 |
| Templates 與架構測試 | `dotnet new` starter、項目範本，以及用 xUnit 檢查指定組件間的直接相依。[範本指南](../templates.md) · [架構測試](../architecture-testing.md) |
| Repository 符合性工具 | 規劃中的 CI 結果彙整與覆蓋報告。目前可用能力與後續目標見[下節](#後續目標)。 |

現行版本與可取得成品見[套件使用手冊](../README.md)的版本索引。五個契約套件、核心 Analyzer 與 Templates 各有獨立 Package ID，共用產品版號。[版本與支援政策](../releases/version-policy.md)

## 已確認的產品決策

- **產品身分與程式碼組織。** 產品與 GitHub repository 均使用 ArchPrecept 名稱。可發布程式碼依能力放在 `src/`，Project、預設組件名稱與根命名空間採 `ArchPrecept.<Capability>`。相應測試放在 `tests/`，採用範例放在 `samples/`。單一 repository 共同維護規則、契約、測試與文件，套件依各自責任安裝。
- **獨立採用。** Analyzer 透過共用角色辨識支援自訂及內建契約，套件相依中不含參考契約。Core 設計保持宿主中立，Web API、Messaging 等技術條件由 Profile 管理。使用者可單用 Analyzer、單用契約，或組合採用。[Analyzer 設定](analyzer-policy.md)
- **規則與評估範圍。** Rule Catalog 保存機器可讀規則，Diagnostic 手冊保存判定與修正方法。明確角色映射能支援既有契約，仍依所選政策判定。判讀結果須考量選用範圍、角色證據及當次編譯的診斷。[設定與評估範圍](analyzer-policy.md#宣告的評估範圍)
- **應用責任與設計取捨。** Aggregate Repository 專注於依身分載入及儲存，查詢由讀取端負責。純 Mapper 使用 `static`，讓轉換責任維持於資料映射。薄 Handler 將請求交給 Use Case，讓 HTTP、訊息等入口共用同一應用流程。[規則設計理由](rule-design-rationale.md)
- **契約的執行責任。** Event Sourcing 核心可獨立採用，提供記憶體內的重播、待提交事件與版本語意，其 Analyzer 規則仍由核心套件提供。Messaging 分為共用訊息契約與 Application 發布 port。採用端擁有儲存交易、序列化、傳輸、重試及其設定。[Event Sourcing 契約](event-sourcing-contract.md) · [Messaging 契約](messaging-contract.md)
- **文件與規則來源。** `docs/` 保存對外產品語意，`rules/` 保存機器規則與設定結構，`.dev/` 保存需求與開發治理。Markdown 是正式文件來源，Word 與 PDF 由正式文件衍生。規則、需求、執行機制、實作與驗證應可雙向追溯。
- **發布與相容。** 套件依版本 tag 各自發布到支援的 registry，說明網站依網站內容部署。首個無後綴公開版本之前允許破壞性變更，已發布成品內容固定。首個正式版須具備逐規則設計依據、完整套件使用說明及隨包可讀的公開 API XML。[版本與支援政策](../releases/version-policy.md)

規則的設計與交付由 ArchPrecept 維護。採用團隊管理自己的 role mapping、Profile、例外與執行結果，也可以獨立使用 AI Collaboration Framework 的工程知識與審查方法。ArchPrecept 的執行與設定無須安裝該 Framework。

## 後續目標

| 方向 | 目標與狀態 |
|---|---|
| 0.1.0 正式版 | 交付可安裝的套件、完整使用與公開 API 說明，以及支援平台上的實際成品驗收。各預覽版本的發布狀態各見當版說明。[版本與支援政策](../releases/version-policy.md) |
| 判定精確度與效能 | 持續改進角色及語意證據、評估範圍與誤判，並為大型 solution 建立可重複的效能基線。尚待定義理由及適用條件的規則見[規則設計理由](rule-design-rationale.md#尚未定義的政策)。 |
| 架構測試 | 現行範本支援直接組件相依。獨立測試類別庫、傳遞相依圖、Project 或 namespace 粒度留待評估。[目前的判定範圍](../architecture-testing.md) |
| Repository Conformance | 規劃使用既有建置、Analyzer 與測試結果，彙整適用範圍、覆蓋、未評估原因及機器可讀結果。CLI、報告格式與套件身分尚待交付。 |
| 參考契約演進 | 參考契約暫與 Analyzer 同 repository 維護。形成獨立生命週期、發布或維護責任，或授權需要隔離時，再評估拆分。額外的 batch／purge port、ES 儲存及 projection runtime 依具體場景另行評估。 |
| 交付能力 | 依採用需求評估低風險 Code Fix、相容性清單、SBOM、簽章與 build attestation。 |
