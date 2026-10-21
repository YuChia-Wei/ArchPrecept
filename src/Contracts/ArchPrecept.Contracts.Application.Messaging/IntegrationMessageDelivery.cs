namespace ArchPrecept.Contracts.Application.Messaging;

/// <summary>
/// 定義在重試傳遞時必須維持穩定的傳輸中立中繼資料。
/// </summary>
public sealed record IntegrationMessageDelivery(Guid MessageId, string? PartitionKey = null);
