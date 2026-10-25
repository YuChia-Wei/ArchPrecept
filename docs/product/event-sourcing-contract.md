# CT-005：Event Sourcing 核心契約與套件邊界

本文件定義 [PRD CT-005](prd.md) 的記憶體內事件生命週期及採用方式。套件可獨立選用；已發布版本與驗證範圍見[發布紀錄](../releases/0.0.3-preview.md)。事件重播與副作用隔離的設計依據見[規則設計理由](rule-design-rationale.md)。

## 責任與相依

`ArchPrecept.Contracts.Domain.EventSourcing` → `ArchPrecept.Contracts.Domain`。
Domain、Application、Messaging、Analyzer 不反向參考 EventSourcing。
Analyzer 可獨立安裝，ES 套件也可不搭配 Analyzer 使用。

- Domain 保留一般 Entity、Aggregate、Domain Event、Value Object。
- EventSourcing 提供 `EsAggregateRoot<TId>` 的記憶體內生命週期。
- ES 基底繼承 `AggregateRoot<TId>`，可滿足既有 `IAggregateRepository<TAggregate, TId>` 約束。
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

為避免經由 `DomainEntity`／`IAggregateRoot`／`IDomainEntity` 呼叫繞過 ES 生命週期，
Domain 的三個事件 mutator 為 virtual，ES 以 sealed override 拒絕
`AddDomainEvent`、`RemoveDomainEvent`、`ClearDomainEvents`，擲回 `NotSupportedException`。
一般 Domain Entity／Aggregate 允許這些事件集合操作。

符合泛型約束不等於所有操作可互換。「對所有 Aggregate 清除事件」的通用 dispatcher／repository
流程不適用於 ES；它必須走 ES append／commit 協定。這是有意的行為限制，並非可無修改替換的承諾。
衍生型別仍須遵守領域不變條件，不能任意寫入受保護的 Version 或直接變更狀態。

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
| ES 有 pending events | 由基底類別或介面加入、移除、清除 | 全部拒絕，不能繞過 Apply／commit |
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
