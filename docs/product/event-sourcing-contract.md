# CT-005：Event Sourcing 核心契約與套件邊界

2026-10-21。依 [PRD CT-005](prd.md) 與負責人同意的評估方向實作。
此項以 CT-005 追溯，不新增 Issue。新能力採同 repository、獨立 `.csproj`／NuGet 套件；
已隨 `0.0.2-preview` 公開發布，見[發布證據](../releases/0.0.2-preview.md)；不表示已完成下游專案遷移。

## 責任與相依

`ArchPrecept.Contracts.Domain.EventSourcing` → `ArchPrecept.Contracts.Domain`。
Domain、Application、Messaging、Analyzer 不反向參考 EventSourcing。
Analyzer 可獨立安裝，ES 套件也可不搭配 Analyzer 使用。

- Domain 保留一般 Entity、Aggregate、Domain Event、Value Object。
- EventSourcing 提供 `EsAggregateRoot<TId>` 的記憶體內生命週期。
- ES 基底繼承 `AggregateRoot<TId>`，可滿足既有 `IAggregateRepository<TAggregate, TId>` 約束。
- DBA1009 留在 `ArchPrecept.Analyzers.Core`，ID、預設 severity、辨識行為不變。
- Event Store、snapshot、upcasting、projection runner、checkpoint、外部事件發布均不在此次範圍。
  完整 runtime 的獨立產品／repository 決策留待實際需求確立。

現行 DBA1013 的 `IProjection` 是查詢端 Port；不可將更新 read model 的 ES projection handler
直接視為同一角色。此次不改變 DBA1013。

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

## 一般 Domain API 的相容性界線

為避免經由 `DomainEntity`／`IAggregateRoot`／`IDomainEntity` 呼叫繞過 ES 生命週期，
Domain 的三個事件 mutator 改為 virtual，ES 以 sealed override 拒絕
`AddDomainEvent`、`RemoveDomainEvent`、`ClearDomainEvents`，擲回 `NotSupportedException`。
一般 Domain Entity／Aggregate 的原有行為不變。

這表示泛型約束仍相容，但「對所有 Aggregate 清除事件」的通用 dispatcher／repository
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

## 從 0.0.1-preview 遷移

1. 將所用 ArchPrecept 套件升到同一個候選版本，加入 `ArchPrecept.Contracts.Domain.EventSourcing`。
2. ES 基底改用 `ArchPrecept.Contracts.Domain.EventSourcing.EsAggregateRoot<TId>`；Domain Event 等型別仍在 Domain。
3. 移除 `base(events)`；改用 factory 建立已初始化的物件，再呼叫受保護的 `LoadFromHistory(events)`。
4. 以 `Apply` 產生事件，不直接呼叫一般事件 mutator。
5. Repository 以 expected revision 原子 append，成功後以事件總數呼叫 `MarkChangesAsCommitted`。
6. 重新編譯並重驗 replay、初始化、失敗提交及 dispatcher 路徑。

使用 Central Package Management 時，也須在 `Directory.Packages.props` 加入新 PackageVersion；
精確列舉 Package ID 的 NuGet source mapping 應涵蓋新的
`ArchPrecept.Contracts.Domain.EventSourcing` 及其 Domain 相依，並更新 lock file 與容器還原設定。
一般 `ArchPrecept.*` mapping 不需因本次更名新增 pattern。
套件相依宣告的是候選版的最低版本，不是精確鎖版；共同 release train 的使用端應自行統一引用版本，
本輪驗證的是六套件同版組合，不代表任意混搭未來版本都相容。

若曾使用尚未發布的 `ArchPrecept.Contracts.EventSourcing` 本機候選，移除其引用並改用
`ArchPrecept.Contracts.Domain.EventSourcing`；使用全新 cache 驗證，避免同版號的舊本機候選遮蔽新成品。
若持久化內容或 serializer 設定記錄舊基底的完整型別／assembly 名稱，須更新 binder／型別映射。
本次沒有改動消費端具體事件的 schema，也沒有執行既有儲存資料轉換。

舊 Domain assembly 不保留 ES 型別，也不加入 type forwarder，避免 Domain 反向依賴 ES。
因此這是來源與二進位不相容變更；舊使用端不能只更換 DLL。
採用自有 ES 基底的消費端可以繼續只用 Domain，不強制改用新的參考實作。
