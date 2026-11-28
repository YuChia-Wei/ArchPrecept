# 各套件的最小使用與責任

本頁說明 **0.1.0-preview.1**，套件取得方式見[當版紀錄](releases/0.1.0-preview.1.md)。上一版使用 [0.0.6-preview 固定手冊](https://github.com/YuChia-Wei/ArchPrecept/blob/v0.0.6-preview/docs/package-usage.md)。安裝來源、SDK 8／10 與 AdditionalFiles 接線見[安裝指南](getting-started.md)。下列 C# 範例使用一般 SDK implicit usings。

## ArchPrecept.Templates

以 `dotnet new install` 安裝至 SDK。建立專案、逐項新增 Domain／Application 型別與入口的命令、參數及適用契約見[範本使用指南](templates.md)。測試專案可另外產生[直接組件依賴測試](architecture-testing.md)。

## Analyzer-only 與自有契約

只加入 `ArchPrecept.Analyzers.Core`（`PrivateAssets="all"`）即可檢查自有模型，不加入 Contracts runtime 相依。完整 metadata name＋assembly 的 `Role` mapping 與 Profile 範例見[設定契約](product/analyzer-policy.md)。角色可沿介面、基底與 generic constraint 傳遞；未辨識不等於通過，需用 `Expected` 宣告必要覆蓋。

建置後先處理 DBA0001、CS8032／CS9057／AD0001，再判讀實際 Diagnostic 與 DBA0002 scope。`applicable` 只表示宣告適用；`not-evaluated`、停用及零 Diagnostic 都不是完整合規證明。每條規則的理由、正反例、修正及 suppression 見[手冊](diagnostics/README.md)。

## Domain

加入 `ArchPrecept.Contracts.Domain`，使用 identity-bearing Entity／Aggregate、Domain Event 或選配 ValueObject 基底：

```csharp
using ArchPrecept.Contracts.Domain;

public sealed record OrderConfirmed(Guid EventId, DateTime OccurredOn) : IDomainEvent;

public sealed class Order : AggregateRoot<Guid>
{
    public Order(Guid id) : base(id) { }
    public void Confirm() => AddDomainEvent(new OrderConfirmed(Guid.NewGuid(), DateTime.UtcNow));
}
```

`DomainEntity<TId>` 的一般建構路徑要求非 null identity。保留 protected 無參數建構子供 ORM／序列化／ES 重建：指派前讀 `Id` 擲 `InvalidOperationException`，指派 null 擲 `ArgumentNullException`。零、`Guid.Empty` 或空字串是否合法由你的 Domain invariant 決定；不拿 `default(TId)` 代替初始化狀態，也不新增「Id 永不再指派」政策。ORM 若繞過 setter 直接寫私人欄位，必須由 adapter 驗證其 materialization 接線；本候選沒有宣稱任何特定 ORM 整合已驗收。

若儲存端產生正整數 Id，而且模型用零表示尚未持久化，建立物件時請明確傳入 `base(0)`。首次儲存由 repository 取得產生的正數並交給物件。重建時先經相同建構路徑，再指派儲存所得的 Id。已取得正數 Id 的物件只能再次確認相同識別碼。以下是此種領域模型的選擇，採用 UUID 或合法零值的模型可以定義自己的生命週期。

```csharp
public sealed class Document : AggregateRoot<int>
{
    public Document() : base(0) { }

    public void AssignIdentity(int id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (Id != 0 && Id != id) throw new InvalidOperationException("Document identity cannot change.");
        Id = id;
    }
}
```

僅讀取 identity 的程式使用 `IDomainEntity<TId>`。需要觀察事件時，接受 `IDomainEventSource` 並讀取 `DomainEvents`。採用可編輯事件集合的普通 Entity 可繼承 `BufferedDomainEntity<TId>`，一般 `AggregateRoot<TId>` 已提供這項能力。`IDomainEventBuffer` 的 Add／Remove／Clear 只修改本機集合；dispatcher、交易與何時清除由 consumer 擁有。唯讀檢視保留事件物件的原參考。

`IValueObject` 是選定政策 marker，`ValueObject` 提供依相等性元件比較的基底。領域模型負責維持不可變狀態，元件序列須穩定且符合 XML 所列非空／非 null 條件。

## Application

加入 `ArchPrecept.Contracts.Application`，NuGet 會帶入 Domain。`Result<T>`／`PageResult<T>` 為傳輸中立結果；Repository 與 dispatcher 是 port，不附資料庫或 DI adapter。

```csharp
using ArchPrecept.Contracts.Application;
using ArchPrecept.Contracts.Domain;

public sealed class Order : AggregateRoot<Guid>
{
    public Order(Guid id) : base(id) { }
}
public interface IOrders : IAggregateRepository<Order, Guid> { }
public sealed record OrderRow(Guid Id, string Status);
public interface IOrderReader : IQueryRepository
{
    Task<IReadOnlyList<OrderRow>> FindByStatusAsync(string status, CancellationToken cancellationToken);
}
```

Aggregate port 保留 `FindByIdAsync`／`SaveAsync`。不存在的 identity 以 null 表達；Save 的 optimistic concurrency、錯誤、取消與交易由 adapter 實作。Query port 可查 criteria／分頁並回傳 read model；查得 ID 後的 command 必須重新載入並驗證目前 Aggregate 狀態。`IDomainEventDispatcher` 是程序內 port，不承諾 Outbox 或可靠交付。各 API 的所有權、缺值與失敗語意由隨包 XML 說明；Result 不是 exception 或交易機制。

## Domain.EventSourcing

加入 `ArchPrecept.Contracts.Domain.EventSourcing`，NuGet 會帶入 Domain。從 `EsAggregateRoot<TId>` 衍生，在初始化完成後用 `Apply` 處理新事件，或以 `LoadFromHistory` 重播完整有序歷史；覆寫 `When(IDomainEvent)` 處理狀態。完整可用範例及失敗情境見 [CT-005](product/event-sourcing-contract.md)。

儲存端接受 `IESAggregateRoot<TId>`，可讀取 Id、已提交版本及依套用順序排列的 pending events。重播只重建狀態。成功 append 本批全部事件後，才以 `MarkChangesAsCommitted` 確認版本並清除 pending。append 的原子性及結果由儲存 adapter 驗證；轉換失敗的實例須丟棄。Event Store、snapshot、upcasting、跨程序並行與 retry 亦由 adapter 定義。DBA1009 檢查宣告的局部 state mutation。

## Messaging

只加入 `ArchPrecept.Contracts.Messaging` 即可定義對外事件／request，不依賴 Domain 或 Application：

```csharp
using ArchPrecept.Contracts.Messaging;
public sealed record OrderAccepted(Guid OrderId, DateTime OccurredOn) : IIntegrationEvent;
```

事件代表已發生的事實；request 表達請求。marker 不提供序列化、schema registry、deep immutability、去重或 transport。宿主負責 broker、認證、組態與契約演進；完整責任見 [CT-004](product/messaging-contract.md)。

## Application.Messaging

加入 `ArchPrecept.Contracts.Application.Messaging`，NuGet 只額外帶入 Messaging。Application 依賴 `IIntegrationEventPublisher`，外層 adapter 實作：

```csharp
using ArchPrecept.Contracts.Application.Messaging;
using ArchPrecept.Contracts.Messaging;

public sealed record OrderAccepted(Guid OrderId, DateTime OccurredOn) : IIntegrationEvent;
public static class Publication
{
    public static Task PublishAsync(IIntegrationEventPublisher publisher, Guid orderId, Guid stableMessageId)
        => publisher.PublishAsync(new OrderAccepted(orderId, DateTime.UtcNow), new IntegrationMessageDelivery(stableMessageId));
}
```

同一邏輯訊息的 MessageId 應跨 retry 穩定。兩參數 overload 的預設實作委派單參數方法，**不處理 delivery metadata**；需去重／partition 語意時 adapter 必須實作它。介面沒有 CancellationToken，完成與錯誤依 adapter 契約，不能自行推定 broker acknowledgment、Outbox／exactly-once 或取消支援。

## 隨包文件與驗證

Contracts 的 DLL/XML 位於 `lib/net8.0/`；Analyzer 位於 `analyzers/dotnet/cs/`。每包攜帶同一來源的 `docs/` 與 `rules/`，Analyzer 另在 `schemas/` 提供設定 schema。API 說明使用原生 C# XML 文件註解，由編譯器生成 DLL 旁的 XML，供 IDE 讀取；Analyzer 以規則 HelpLink 與隨包 Markdown 解說，不要求將 Analyzer 當 runtime reference。

工程入口為 `eng/Build-Packages.ps1` 與 `eng/Test-PackageConsumer.ps1`。封裝檢查確認 XML、Catalog、schema 與規則說明確實隨包提供；文件內容依變更範圍審查。這兩個入口不執行 NuGet 上傳，本機候選的檢查結果也不代表未來 feed 成品已完成驗收。
