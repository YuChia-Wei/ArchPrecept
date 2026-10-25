# CT-004：Messaging 責任、相依與使用

本文件定義 [PRD CT-004](prd.md) 的訊息契約與 Application 發布 port。將訊息資料、應用需求及傳輸機制分開，可讓接收端只依賴資料契約，並讓 Application 不依賴特定 broker。設計理由見[規則設計依據](rule-design-rationale.md)，已發布版本見[發布紀錄](../releases/0.0.3-preview.md)。

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

兩個 `PublishAsync` overload 的責任如下：

- 單參數版本交付 `IIntegrationEvent` 給 adapter。
- 帶 `IntegrationMessageDelivery` 的版本可傳遞穩定 `MessageId` 與可選 `PartitionKey`。
- 預設實作只呼叫單參數版本，**不處理 metadata**。需要去重或分割語意的 adapter
  必須自行實作此 overload；呼叫成功不能視為去重、持久化或 exactly-once 的證據。
- 例外與失敗的 Task 回傳給呼叫者，契約不吞掉錯誤或自行重試。

`PartitionKey` 是供應商中立的分組提示；實際 broker 映射由 adapter 定義。
它不是 Kafka client 的 partition 物件，也不應讓 Application 決定特定 broker API。

## 傳輸設定的責任

`MessagingTransportOptions` 與 `MessagingTransportProfile` 範例位於 [`samples/Messaging/Host/MessagingTransportOptions.cs`](../../samples/Messaging/Host/MessagingTransportOptions.cs)，
namespace 為 `MessagingSample.Host`。這是 host 組態範例，不是共用套件或交付保證。

Host 範例使用以下設定鍵：`Messaging:Profile`、
`Messaging:Kafka:ConnectionString`、`Messaging:RabbitMq:ConnectionString`。
在該 profile 已通過驗證時才呼叫相應的 connection getter；範例不承諾跨 profile 取值有效。
執行範例只讀取合成的記憶體設定，不連線 broker。

本產品不提供訊息 host／adapter 的發布專案。消費端應把必要的設定類別放進自身 host／adapter，
採用自己的 namespace，並由該 host 參考 Configuration 套件。

## 採用方式

1. 只有訊息契約需求的專案安裝 `ArchPrecept.Contracts.Messaging`。
2. 需要發布 port 的專案安裝 `ArchPrecept.Contracts.Application.Messaging`；它會引入 Messaging 相依。
3. Adapter 實作實際傳送、metadata 映射、錯誤處理與重試政策；若需要交易型 Outbox 或去重，另定持久化與交付協定。
4. Host 擁有傳輸設定、DI 與 Configuration 套件相依；Contracts 不傳遞提供這些機制。
5. 以實際 broker 驗證連線、持久化、重試、去重及分區等有宣告的行為，不能以介面呼叫成功替代。

Central Package Management、NuGet source mapping、lock file 與容器還原設定應涵蓋實際採用的 Package ID 及傳遞相依。Host 範例直接參考 `Microsoft.Extensions.Configuration` 與 `Microsoft.Extensions.Configuration.Abstractions` 8.0.0；使用者應按自己的 host 版本選擇相依。設定詳見 [NuGet Package Source Mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)。

具體訊息 schema、serializer 的 assembly-qualified type mapping 及歷史資料由訊息產品維護；ArchPrecept 不提供資料遷移或型別轉接層。首個 non-preview 公開版本前的套件政策見[版本政策](../releases/version-policy-proposal.md)。

## 驗收與證據邊界

`eng/Test-PackageConsumer.ps1` 在 SDK 8／10 用本機 nupkg 與獨立 cache 執行：

| 情境 | 可觀察結果 |
|---|---|
| 僅安裝 Messaging | 自訂事件與請求可 JSON 往返；不引入 Publisher、Domain、Analyzer 或 Configuration |
| 僅安裝 Application.Messaging | 自動引入 Messaging；metadata-aware adapter 收到原事件與 metadata |
| 僅實作單參數 overload | 預設委派一次；不宣稱 metadata 被使用 |
| Adapter 回傳失敗 Task | 呼叫端觀察到原始例外 |
| Host 接管設定來源 | InMemory、Kafka、AMQP／AMQPS 正例及未知／缺漏／錯誤設定反例通過 |

六套件的 nuspec 相依也由既有封裝檢查驗證。範例使用本機替身，
不代表任何真實 broker 的交付、去重或連線驗證。
