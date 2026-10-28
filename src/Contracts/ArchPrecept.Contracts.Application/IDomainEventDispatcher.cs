using ArchPrecept.Contracts.Domain;

namespace ArchPrecept.Contracts.Application;

/// <summary>
/// 領域事件發布者
/// </summary>
/// <remarks>
/// Dispatch 強調「路由 / 分派」到同步或極低延遲的本地 Handler；多半走 Mediator 或類似模式，行為接近函式呼叫。
/// 不代表 broker、Outbox、exactly-once 或資料庫交易；事件的清除、重試與失敗處理由應用及 adapter 定義。
/// </remarks>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// 將領域事件分派給目前程序內可處理的 Handler。
    /// </summary>
    /// <param name="events">要分派的領域事件集合。</param>
    /// <param name="ct">取消非同步作業的權杖。</param>
    /// <returns>代表非同步分派作業的工作。</returns>
    /// <remarks>呼叫端在分派期間不得修改傳入序列或事件；介面不複製事件，也不規定處理順序。
    /// 取消及 Handler 例外的傳遞由實作定義；部分 Handler 可能已執行，取消不是 rollback。</remarks>
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
