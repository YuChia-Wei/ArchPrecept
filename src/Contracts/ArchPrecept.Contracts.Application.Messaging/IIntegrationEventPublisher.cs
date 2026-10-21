using ArchPrecept.Contracts.Messaging;

namespace ArchPrecept.Contracts.Application.Messaging;

/// <summary>
/// 定義應用程式向外發布整合事件的連接埠，由外層轉接器實作。
/// </summary>
/// <remarks>
/// 此契約不依賴特定 broker、組態來源或傳輸實作。
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
    /// <remarks>
    /// 預設實作委派至單參數版本，不處理中繼資料，以保留既有實作相容性。
    /// 需要去重或分割語意的轉接器必須自行實作此多載；呼叫成功不代表已提供這些保證。
    /// </remarks>
    Task PublishAsync(IIntegrationEvent integrationEvent, IntegrationMessageDelivery delivery)
    {
        return PublishAsync(integrationEvent);
    }
}
