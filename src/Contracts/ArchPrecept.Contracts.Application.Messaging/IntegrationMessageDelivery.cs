namespace ArchPrecept.Contracts.Application.Messaging;

/// <summary>
/// 定義在重試傳遞時必須維持穩定的傳輸中立中繼資料。
/// </summary>
/// <param name="MessageId">同一邏輯訊息跨重試維持不變的識別碼；不在此處產生或驗證。</param>
/// <param name="PartitionKey">選用的分割鍵；null 表示未指定，由 adapter 決定路由。</param>
/// <remarks>不可變 record 的 value equality、複製及解構遵循兩個屬性值。
/// 本型別只攜帶中繼資料，不實作去重、排序、分割或交付保證。</remarks>
public sealed record IntegrationMessageDelivery(Guid MessageId, string? PartitionKey = null)
{
    /// <summary>取得或於初始化時指定跨重試維持穩定的訊息識別碼。</summary>
    /// <value>由呼叫端提供的 MessageId；不驗證 Guid.Empty，也不在複製時產生新值。</value>
    public Guid MessageId { get; init; } = MessageId;

    /// <summary>取得或於初始化時指定選用的分割鍵。</summary>
    /// <value>呼叫端提供的 PartitionKey；null 表示未指定。</value>
    public string? PartitionKey { get; init; } = PartitionKey;
}
