using ArchPrecept.Contracts.Domain;

namespace ArchPrecept.Contracts.Application;

/// <summary>
/// 領域事件發布者
/// </summary>
/// <remarks>
/// Dispatch 強調「路由 / 分派」到同步或極低延遲的本地 Handler；多半走 Mediator 或類似模式，行為接近函式呼叫。
/// </remarks>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// 將領域事件分派給目前程序內可處理的 Handler。
    /// </summary>
    /// <param name="events">要分派的領域事件集合。</param>
    /// <param name="ct">取消非同步作業的權杖。</param>
    /// <returns>代表非同步分派作業的工作。</returns>
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
