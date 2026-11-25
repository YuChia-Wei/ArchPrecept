# MQ consumer 的 Repository 採用方式

在 [dotnet-distributed-architecture-lab 的固定基線](https://github.com/YuChia-Wei/dotnet-distributed-architecture-lab/tree/7a0bff401bea86428b5c24eab6362ef2170d20d7) 上，以 Analyzer 和自有契約 mapping 重現了 [0.0.4-preview 試用](0.0.4-consumer-validation.md)的 11 個 DBA1001。11 項都符合 [DBA1001](../diagnostics/DBA1001.md) 對具體 Repository **額外公開方法**的現行限制。Procurement 的兩個 query 方法皆符合規則，四個診斷落在同一 adapter 對外公開的其他方法。

## 11 個診斷與實際依賴

| Adapter 與角色來源 | 公開能力與判讀 | 隔離 consumer 的處置 |
|---|---|---|
| Products 的 `ProductDomainRepository` 實作 `IDomainRepository<Product, Guid>`，其父介面映射為 aggregate-repository | `FindByIdsAsync`、`SaveAllAsync`、`DeleteAsync` 共三項。三者均超出依 ID 載入與保存單一 Aggregate 的 port。批次保存實際上逐筆開交易。 | 此基線的產品程式沒有呼叫三個方法，移除它們。原有命令透過 Aggregate 的刪除事件與 `SaveAsync` 執行軟刪除。 |
| Inventory 的 `InventoryItemDomainRepository` 經 `IInventoryItemDomainRepository` 繼承同一 aggregate-repository，另實作 query-repository | 同名三個方法也未對應任何已辨識 Repository port。兩個批次方法直接擲出 `NotImplementedException`；`DeleteAsync` 執行實體刪除。合法的查詢方法 `FindByProductIdAsync` 回傳讀取模型。 | 移除三個沒有產品呼叫端的方法，保留查詢投影、保存及事件派送。若有實際批次或清除需求，另訂專用 port 的重複 ID、部分失敗、重試與實體刪除政策。 |
| Orders 的 `OrderEventSourcingRepository` 經 `IOrderDomainRepository` 繼承 aggregate-repository，也實作 `IOrderEventCommitter` | 一項 `CommitAsync`。命令已透過專用 committer 呼叫，隱含介面實作使具體 Repository 同樣公開 commit。 | 改用 `IOrderEventCommitter.CommitAsync` 顯式實作。兩個介面仍解析至同一 scoped 實體，沿用原本的事件串流、outbox 與交易提交方法。 |
| Procurement 的 `PostgresPurchaseStore` 經 `IPurchaseOrderQueries : IQueryRepository` 取得 query-repository 角色 | 一項 `CreateOrGetAsync` 觸發寫入動詞；`FindByIdAsync`、`ApplyOutcomeAsync`、`MarkUnknownAsync` 三項回傳含 `PurchaseOrder` 的型別。此 Aggregate 的 `IProcurementAggregateRoot` 符合已啟用的命名慣例。四個方法都在具體型別上公開，分別屬 creation committer、命令側載入與 submission committer。 | 透過這三個應用程式介面顯式實作，收貨的 `IGoodsReceiptCommitter` 同樣只公開給該 port 的使用者。查詢仍回傳 `PurchaseOrderResponse` 或其清單。保留相同 DI 生命週期、SQL、交易與並行處理。 |

Procurement 的 `GetPurchaseUseCase` 與 `ListPurchasesUseCase` 依賴 `IPurchaseOrderQueries`；建立、對帳與收貨分別取得實際需要的寫入、載入或提交介面。DBA1001 對具體類別的回報不改變這些依賴視圖。將同一個 adapter 分割成不同實體或拆開同一筆交易，會增加採用成本。此案例只收窄了公開方法的可見範圍，直接使用具體寫入方法的測試改經相應 port。[Procurement ports 與 Use Cases](https://github.com/YuChia-Wei/dotnet-distributed-architecture-lab/blob/7a0bff401bea86428b5c24eab6362ef2170d20d7/src/Procurement/DomainCore/Procurement.Applications/ProcurementOperations.cs) · [Adapter 與交易](https://github.com/YuChia-Wei/dotnet-distributed-architecture-lab/blob/7a0bff401bea86428b5c24eab6362ef2170d20d7/src/Procurement/DomainCore/Procurement.Infrastructure/PostgresPurchaseStore.cs)

## 驗證範圍

2026-11-25 使用 SDK 11.0.100，從 [ArchPrecept 原始碼](https://github.com/YuChia-Wei/ArchPrecept/tree/dd87135810b96f5d45c0ebff55e7c7f55ee64899)建置 Analyzer 並直接載入 DLL。範圍為 Products／Orders／Inventory 的 Domains 和四個業務 context 的 Applications、Infrastructure、WebApi 共 15 個專案。套用 core、web-api、messaging、event-sourcing、framework、ddd profiles，啟用命名慣例，對可參考的 Lab 共用契約明確設定 Entity、Aggregate、VO、ES、Aggregate／Compatibility／Query Repository 與事件 dispatcher 角色。

在隔離副本建置完整 `MQArchLab.slnx`，原始程式重現 11 個 DBA1001；套用上表的 consumer 調整後，同一份 DLL 與設定下完整 solution 建置成功，零錯誤。過程未安裝 ArchPrecept runtime 套件，也未使用 suppression 或更改 severity。此驗證針對原始碼建置的 Analyzer 及隔離 consumer，正式 feed 安裝和下游合併須依各自交付紀錄確認。

對四個受影響的既有測試專案分別執行 `dotnet test --no-build --no-restore`，合計 107 通過、0 失敗、41 略過。

| 測試專案 | 通過 | 略過 |
|---|---:|---:|
| SaleProducts.Tests | 22 | 4 |
| InventoryControl.Tests | 37 | 27 |
| SaleOrders.Tests | 19 | 0 |
| Procurement.Tests | 29 | 10 |

外部 PostgreSQL、broker 及端對端交付未在本次驗證。Solution 建置涵蓋其餘專案的編譯，測試執行範圍限於上表。
