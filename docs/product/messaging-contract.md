# CT-004：Messaging 責任、相依與遷移

2026-10-21，依負責人同意的架構責任與命名方向調整。
對應 [PRD CT-004](prd.md)，已隨 `0.0.2-preview` 公開發布；見[發布證據](../releases/0.0.2-preview.md)。

## 套件與型別歸屬

| 套件／來源 | 型別 | 責任 |
|---|---|---|
| `ArchPrecept.Contracts.Messaging` | `IIntegrationEvent`、`IRequestContract` | 生產端與消費端共用的訊息契約 |
| `ArchPrecept.Contracts.Application.Messaging` | `IIntegrationEventPublisher`、`IntegrationMessageDelivery` | Application 向外發布訊息的 port 與其傳遞參數 |
| `samples/Messaging/Host`，不發布 | `MessagingTransportOptions`、`MessagingTransportProfile` | 由實際 host／adapter 擁有的組態讀取與傳輸選擇範例 |

套件相依只有 `Contracts.Application.Messaging → Contracts.Messaging`。
前者不因命名階層而依賴 `Contracts.Application`；訊息接收端也不需要安裝發布 port。
兩者不依賴 Domain、EventSourcing、Analyzer、Configuration 或 broker SDK。
它們提供參考契約，不實作 broker、Outbox、DI 註冊、重試或交付保證。

`IIntegrationEvent` 表示可供外部交換的事件，不等同 Domain Event；仍保留 `OccurredOn`。
`IRequestContract` 是跨系統請求訊息的 marker，不代表程序內的 Use Case 或 Command Handler。
具體訊息 schema 與相容性仍由擁有該訊息的產品維護。

## 發布 port 的語意

兩個 `PublishAsync` overload 的參數與委派行為保持原樣：

- 單參數版本交付 `IIntegrationEvent` 給 adapter。
- 帶 `IntegrationMessageDelivery` 的版本可傳遞穩定 `MessageId` 與可選 `PartitionKey`。
- 預設實作只呼叫單參數版本，**不處理 metadata**。需要去重或分割語意的 adapter
  必須自行實作此 overload；呼叫成功不能視為去重、持久化或 exactly-once 的證據。
- 例外與失敗的 Task 回傳給呼叫者，契約不吞掉錯誤或自行重試。

`PartitionKey` 是供應商中立的分組提示；實際 broker 映射由 adapter 定義。
它不是 Kafka client 的 partition 物件，也不應讓 Application 決定特定 broker API。

## 傳輸設定去向

原 `MessagingTransportOptions` 與 `MessagingTransportProfile` 從產品套件移出，
保留於 [`samples/Messaging/Host/MessagingTransportOptions.cs`](../../samples/Messaging/Host/MessagingTransportOptions.cs)，
namespace 為 `MessagingSample.Host`。這是遷移參考來源，不是新的共用套件或可直接取代的 assembly。

Host 範例維持原先的設定鍵與驗證行為：`Messaging:Profile`、
`Messaging:Kafka:ConnectionString`、`Messaging:RabbitMq:ConnectionString`。
在該 profile 已通過驗證時才呼叫相應的 connection getter；範例不承諾跨 profile 取值有效。
執行範例只讀取合成的記憶體設定，不連線 broker。

本 repo 沒有實際訊息 host／adapter，因此沒有新增 Infrastructure 或 Adapter 發布專案，
也沒有修改下游 Lab。消費端應把必要的設定類別放進自身 host／adapter，
採用自己的 namespace，並由該 host 參考 Configuration 套件。

## 從 0.0.1-preview 遷移

1. 移除 `ArchPrecept.Contracts.Messaging.Abstractions` 的 PackageReference。
2. 只有訊息契約需求的專案改用 `ArchPrecept.Contracts.Messaging`。
3. 需要 Publisher 或 Delivery 的專案改用 `ArchPrecept.Contracts.Application.Messaging`，
   它會引入相同候選版本的 Messaging。
4. 將 using／完整型別名稱分別改為上述 namespace；adapter 與訊息生產／接收端都重新編譯。
5. 若使用舊傳輸設定 API，先在自身 host／adapter 接管，再移除舊套件；可參考上述來源範例。
6. 若序列化器儲存 assembly-qualified type name，檢查其型別映射與歷史資料相容性；
   本次沒有執行下游資料遷移，也不以 JSON 範例通過推定所有序列化器相容。

套件設定也要隨引用更新：若使用 Central Package Management，將
`Directory.Packages.props` 的舊 `PackageVersion` 拆成實際採用的新 ID；
同步檢查共用 `.props`／`.targets`、`GlobalUsings.cs` 與產生程式碼的模板。
若 `NuGet.Config` 的 `packageSourceMapping` 精確列舉 ID，須加入
`ArchPrecept.Contracts.Messaging` 與 `ArchPrecept.Contracts.Application.Messaging`，
並涵蓋傳遞相依的 Messaging；既有 `ArchPrecept.*` mapping 已可涵蓋兩者。
根目錄、子目錄及容器建置所複製的 NuGet 設定均須檢查，不能只修改開發機設定。
重新 restore 並按專案政策更新 lock file，再以乾淨 cache 驗證；不可把舊 cache 的成功當成新 mapping 正確。
參考 [NuGet Package Source Mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)。

舊 Messaging 套件曾傳遞引入 `Microsoft.Extensions.Configuration.Abstractions`；
新契約套件不再提供它。接管設定來源的 host／adapter 應直接宣告
`Microsoft.Extensions.Configuration.Abstractions`（範例基線為 8.0.0），
若使用 `ConfigurationBuilder` 與 `AddInMemoryCollection`，則如 host 範例參考
`Microsoft.Extensions.Configuration` 8.0.0。其他依賴舊傳遞相依的程式也須自行補上必要引用。
完成後檢查 `project.assets.json`，避免舊 Messaging.Abstractions 經其他 library 留在相依圖中；
跨版本編譯的 adapter 必須一起重編，不能以同時安裝新舊套件充當型別轉接。

舊套件沒有轉接相依或 type forwarder；拆分涉及 assembly／namespace 身分變動，
不宣稱來源或二進位相容。已發布的 `0.0.1-preview` 套件保持不變。

## 驗收與證據邊界

`eng/Test-PackageConsumer.ps1` 在 SDK 8／10 用本機 nupkg 與獨立 cache 執行：

| 情境 | 可觀察結果 |
|---|---|
| 僅安裝 Messaging | 自訂事件與請求可 JSON 往返；不引入 Publisher、Domain、Analyzer 或 Configuration |
| 僅安裝 Application.Messaging | 自動引入 Messaging；metadata-aware adapter 收到原事件與 metadata |
| 僅實作舊單參數 overload | 預設委派一次；維持舊行為，不宣稱 metadata 被使用 |
| Adapter 回傳失敗 Task | 呼叫端觀察到原始例外 |
| Host 接管設定來源 | InMemory、Kafka、AMQP／AMQPS 正例及未知／缺漏／錯誤設定反例通過 |

六套件的 nuspec 相依也由既有封裝檢查驗證。範例使用本機替身，
不代表任何真實 broker 的交付、去重或連線驗證。
