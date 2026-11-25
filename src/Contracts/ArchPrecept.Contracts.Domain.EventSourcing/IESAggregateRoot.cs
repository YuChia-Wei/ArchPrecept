using ArchPrecept.Contracts.Domain;

namespace ArchPrecept.Contracts.Domain.EventSourcing;

/// <summary>提供儲存已套用事件所需的聚合識別、版本、待提交事件及提交確認。</summary>
/// <typeparam name="TId">聚合根的非 null 識別碼型別。</typeparam>
/// <remarks>DomainEvents 依套用順序列出尚待附加的事件。Version 計算已重播或確認的事件。
/// 同一實例在儲存期間應由單一呼叫端使用；重播及狀態轉換由聚合擁有。</remarks>
public interface IESAggregateRoot<out TId> : IAggregateRoot<TId>, IDomainEventSource where TId : notnull
{
    /// <summary>確認整批事件已成功 append，推進本機版本並清空 pending。</summary>
    /// <param name="committedVersion">儲存成功後的事件總數，應等於原 Version 加上待提交事件數。</param>
    /// <exception cref="InvalidOperationException">版本不符、正在轉換或實例已失效。</exception>
    /// <exception cref="OverflowException">事件總數超出 Int32 範圍。</exception>
    /// <remarks>儲存端負責原子 append 及 expected revision 核對。結果未知時應先查證。</remarks>
    void MarkChangesAsCommitted(int committedVersion);
}
