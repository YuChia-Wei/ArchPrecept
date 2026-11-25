# CT-005：Event Sourcing 核心契約與套件邊界

本文件定義 [PRD CT-005](prd.md) 的記憶體內事件生命週期及 0.0.6-preview 開發版 API。已發布 0.0.5-preview 的契約見[該版來源](https://github.com/YuChia-Wei/ArchPrecept/blob/v0.0.5-preview/docs/product/event-sourcing-contract.md)。事件重播與副作用隔離的設計依據見[規則設計理由](rule-design-rationale.md)。

## 責任與相依

`ArchPrecept.Contracts.Domain.EventSourcing` → `ArchPrecept.Contracts.Domain`。
Domain、Application、Messaging、Analyzer 不反向參考 EventSourcing。
Analyzer 可獨立安裝，ES 套件也可不搭配 Analyzer 使用。

- Domain 保留一般 Entity、Aggregate、Domain Event、Value Object。
- EventSourcing 定義 Domain 聚合的擴充介面 `IESAggregateRoot<TId>`，並提供記憶體內參考實作 `EsAggregateRoot<TId>`。
- 一般及 ES 聚合均實作 `IAggregateRoot<TId>`，可作為 `IAggregateRepository<TAggregate, TId>` 的型別引數。
- DBA1009 由 `ArchPrecept.Analyzers.Core` 提供；實際判定與限制見[規則說明](../diagnostics/DBA1009.md)。
- Event Store、snapshot、upcasting、projection runner、checkpoint、外部事件發布均不在此契約範圍。
  完整 runtime 的獨立產品／repository 決策留待實際需求確立。

現行 DBA1013 的 `IProjection` 是查詢端 Port；不可將更新 read model 的 ES projection handler
直接視為同一角色。

## 共同語言與不變條件

| 名稱 | 本套件語意 | 責任 |
|---|---|---|
| History | 同一聚合事件流從起點開始的完整、有序歷史 | Repository 提供；不以時間戳重新排序、不自行去重 |
| Replay | 在衍生型別初始化完成後呼叫 `LoadFromHistory`，依序執行 `When` | 每個實例最多一次，且必須早於任何新事件 |
| Pending events | 已透過 `Apply` 成功套用、尚未確認持久化的事件；由 `DomainEvents` 唯讀檢視取得 | `Apply` 加入，成功 commit 確認才清除 |
| Version | 已重播或確認提交的事件數；空流是 0；新事件尚未提交時不增加 | `int`，加法檢查溢位；不同儲存供應商的 revision 由 adapter 轉換 |
| Expected revision | Append 前所讀到的 Version | Event Store 必須原子檢查；本機版本算術不代表並行控制 |
| Commit confirmation | 呼叫 `MarkChangesAsCommitted`，版本必須等於 Version + pending count | 只在所有 pending events 已成功 append 後執行 |

Version 不是 event schema version、projection definition version、projection checkpoint、
snapshot schema version 或 integration contract version。本版不實作這些額外版本機制。

`When` 應只轉換聚合狀態，不呼叫儲存、訊息發布或其他外部副作用，也不再呼叫 `Apply`。
無法識別的事件由衍生型別明確擲回例外；基底無法判斷一個事件是否已被完整處理。
事件 payload 必須由消費端維持不可變；唯讀集合不代表深層複製事件。

重播或轉換失敗後，衍生型別的狀態可能已部分變更；基底不嘗試回滾。
它會拒絕後續 Apply、Replay、Commit，呼叫端必須丟棄實例並重新載入。
單純 commit 版本不符則保持原狀，可在釐清結果後使用正確版本確認。

此基底不支援多執行緒共用。同一實例在 append 等待期間不得再執行命令；
應先取得 pending events 陣列與 expected revision，再將整批送交 adapter。
若 append 結果未知，先依儲存端的冪等／查詢策略確認，不可盲目重送或直接清除 pending events。

## 一般 Domain 事件能力與 ES pending 的界線

| 呼叫端 | 接受的型別 | 可用操作與條件 |
|---|---|---|
| 只需識別 Entity | `IDomainEntity<out TId>` | 讀已指派的 `Id`。參考實作在尚未指派時拒絕讀取。 |
| Aggregate repository | `IAggregateRoot<out TId>` | 讀 `Id` 與 `Version`。具體 adapter 依聚合模型選擇儲存策略。 |
| 批次事件讀者 | `IDomainEventSource` | 讀 `DomainEvents`；儲存前可複製當批快照。事件物件仍由領域擁有。 |
| 一般事件緩衝的擁有者 | `IDomainEventBuffer` | 依序 Add、按相等性 Remove 第一個相符項目、處理成功後 Clear。操作只修改本機集合。 |
| ES 儲存 adapter | `IESAggregateRoot<out TId>` | 讀 identity、已提交版本及有序 pending。以該版本原子 append，成功後才呼叫 `MarkChangesAsCommitted`。版本不符時保留 pending。 |
| 實作 ES 領域命令及重建 | `EsAggregateRoot<TId>` | 子類別依 CT-005 呼叫 protected Apply、LoadFromHistory 並實作 When；失敗時依上節丟棄已失效實例。 |

這套能力分配是 ArchPrecept 對 consumer 使用方式的選擇。一般事件緩衝提供可編輯集合；ES pending 必須與已套用的狀態一致。只有一般緩衝的擁有者取得清除操作。

### 從 0.0.5-preview 升級

- `IDomainEntity<TId>` 只公開 Id。`DomainEntity<TId>` 保留兩種建構及 protected Id setter。先前靠它緩衝事件的非聚合實體，改繼承 `BufferedDomainEntity<TId>`；一般 `AggregateRoot<TId>` 已繼承這個基底。新基底的 Add／Remove／Clear 為非 virtual；原先 override 這些方法而有不同緩衝策略的模型，可直接實作 `IDomainEventBuffer`。
- 依賴 `IDomainEntity<TId>` 或 `IAggregateRoot<TId>` 的泛型 helper 原先若要讀事件，改要求 `IDomainEventSource`；需要編輯一般事件才要求 `IDomainEventBuffer`。`TId : notnull`、identity 介面協變，以及 Application repository 的 `TAggregate : IAggregateRoot<TId>`、`in TId` 維持原有語意。
- `EsAggregateRoot<TId>` 的直接基底改為 `DomainEntity<TId>`，透過 ES 介面延伸 Domain 聚合契約。ES 沒有 Add／Remove／Clear 方法；Version 由 ES 基底自行推進，衍生類別只能讀取。原先把兩種聚合轉型為 `AggregateRoot<TId>` 的程式，改依所需能力接收介面。
- 使用 reflection 或 XML member ID 的工具應重新取得 declaring type。例如原 ``P:ArchPrecept.Contracts.Domain.IDomainEntity`1.DomainEvents`` 移至 `P:ArchPrecept.Contracts.Domain.IDomainEventSource.DomainEvents`，`ClearDomainEvents()` 的介面文件位於 `M:ArchPrecept.Contracts.Domain.IDomainEventBuffer.ClearDomainEvents`。ES 的 Version 與 DomainEvents 由 `EsAggregateRoot<TId>` 自行宣告。依舊 API 編譯的 consumer 應重新建置。

DBA1009 辨識提供 When 的 ES 參考基底或精確映射的自有根。單獨實作讀取或儲存端介面並未宣告 canonical When。Analyzer-only 的自有模型仍可直接設定角色。詳見 [DBA1009](../diagnostics/DBA1009.md)。

## 可驗證行為

以下 Given／When／Then 對應
[`EsAggregateRootTests`](../../tests/Contracts/ArchPrecept.Contracts.Domain.EventSourcing.Tests/EsAggregateRootTests.cs)，
同一組案例在 .NET 8 與 .NET 10 執行。

| Given | When | Then |
|---|---|---|
| 歷史數量變更 +2、+3 | 初始化衍生物件後重播 | 狀態為 5、Version 為 2、pending 為空 |
| 已重播 +2 | 命令新增 +3、-1 | 狀態為 4、Version 仍為 1，pending 依序為 +3、-1 |
| Version 1、pending +3 | 成功 append 並確認 Version 2 | pending 清空；完整歷史重播與目前狀態相同 |
| Version 1、一個 pending event | 確認不符的版本 | 拒絕，版本與 pending 不變 |
| 兩個物件讀取相同 revision | 第二次 append 被儲存端拒絕 | 第二個物件不確認 commit，pending 保留 |
| 已重播或已套用新事件 | 再次載入歷史 | 拒絕，避免重複計算；空歷史也只能載入一次 |
| ES 有 pending events | 讀者取得唯讀事件檢視，儲存端確認成功 append | 讀者只能觀察；儲存端用符合的版本確認後 pending 才會清空 |
| 一般 Aggregate | 使用原有事件 mutator | 行為維持不變 |
| 未知事件、讀取歷史失敗或 When 重入 | 重播或 Apply 失敗 | 不記錄失敗事件，實例不能繼續使用 |

並行衝突案例使用明確的記憶體測試替身，只驗證呼叫者與聚合契約，不宣稱真實資料庫的交易保證。
[`samples/EventSourcing/Program.cs`](../../samples/EventSourcing/Program.cs) 由 package consumer
腳本以真實本機 nupkg 執行，並驗證 DBA1009 仍能辨識新 assembly 的 ES 基底。

## 採用方式

1. 加入 `ArchPrecept.Contracts.Domain.EventSourcing`，依需要選用 Analyzer；採用自有 ES 基底時不必改用此參考實作。
2. 由 `EsAggregateRoot<TId>` 衍生領域聚合。重建入口先建立已初始化物件，再呼叫受保護的 `LoadFromHistory(events)`，避免在基底建構時呼叫衍生行為。
3. 命令以 `Apply` 套用新事件，`When` 只轉換狀態；不直接呼叫一般事件 mutator。
4. Repository 以 expected revision 原子 append，確認全部 pending events 已成功持久化後，才以事件總數呼叫 `MarkChangesAsCommitted`。
5. 驗證自己的 replay、初始化、append 失敗／未知結果、並行衝突與 dispatcher 路徑；記憶體測試替身不取代真實 Event Store 驗證。

使用 Central Package Management 時，在 `Directory.Packages.props` 設定所選版本；精確列舉 Package ID 的 NuGet source mapping 須涵蓋 EventSourcing 與 Domain。套件相依宣告最低版本，未保證任意版本混搭；使用端應明確統一並驗證所選套件組合。

事件 payload、serializer 型別映射、儲存端 schema 與資料演進由 consumer 擁有。此契約不提供資料轉換、type forwarder、snapshot 或 upcasting。首個 non-preview 公開版本前的套件政策見[版本政策](../releases/version-policy-proposal.md)。
