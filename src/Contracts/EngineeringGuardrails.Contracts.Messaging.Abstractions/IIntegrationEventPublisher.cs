namespace EngineeringGuardrails.Contracts.Messaging.Abstractions;

/// <summary>
/// 整合事件發布器
/// </summary>
/// <remarks>
/// 整合事件也可以稱為 application event
/// </remarks>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// 發布整合事件
    /// </summary>
    /// <param name="integrationEvent">欲發布的訊息內容</param>
    /// <returns>代表非同步操作的工作物件</returns>
    Task PublishAsync(IIntegrationEvent integrationEvent);

    /// <summary>
    /// 使用穩定的傳遞中繼資料發布整合事件。
    /// </summary>
    /// <remarks>支援去重或分割的轉接器應保留這些中繼資料。</remarks>
    Task PublishAsync(IIntegrationEvent integrationEvent, IntegrationMessageDelivery delivery)
    {
        return PublishAsync(integrationEvent);
    }
}
