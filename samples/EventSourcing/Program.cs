using ArchPrecept.Contracts.Domain;
using ArchPrecept.Contracts.Domain.EventSourcing;

// Executed against packed NuGet assets by eng/Test-PackageConsumer.ps1 on SDK 8 and 10.
var history = new IDomainEvent[] { new QuantityChanged(2) };
var basket = Basket.Rehydrate("basket-1", history);
if (basket.Quantity != 2 || basket.Version != 1 || basket.DomainEvents.Count != 0)
    throw new Exception("Replay contract failed.");

basket.ChangeQuantity(3);
var pending = basket.DomainEvents.ToArray();
if (basket.Quantity != 5 || basket.Version != 1 || pending.Length != 1)
    throw new Exception("Pending-event contract failed.");

try
{
    ((IDomainEntity<string>)basket).ClearDomainEvents();
    throw new Exception("Uncommitted events were cleared.");
}
catch (NotSupportedException) { }

try
{
    basket.MarkChangesAsCommitted(99);
    throw new Exception("Mismatched commit version was accepted.");
}
catch (InvalidOperationException) { }
if (basket.Version != 1 || basket.DomainEvents.Count != 1)
    throw new Exception("Failed commit changed pending events.");

// Simulate successful append. A real adapter must atomically enforce expected revision = 1.
var stored = history.Concat(pending).ToArray();
basket.MarkChangesAsCommitted(stored.Length);
var replayed = Basket.Rehydrate("basket-1", stored);
if (basket.Version != 2 || basket.DomainEvents.Count != 0 || replayed.Quantity != basket.Quantity)
    throw new Exception("Commit/replay equivalence failed.");
Console.WriteLine("EventSourcing replay, pending events and commit contracts passed.");

public sealed record QuantityChanged(int Delta) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed class Basket : EsAggregateRoot<string>
{
    private readonly List<int> _changes;
    private Basket(string id) : base(id)
    {
        // Initialize before Rehydrate invokes LoadFromHistory.
        _changes = new List<int>();
    }

    public int Quantity { get; private set; }

    public static Basket Rehydrate(string id, IEnumerable<IDomainEvent> history)
    {
        var basket = new Basket(id);
        basket.LoadFromHistory(history);
        return basket;
    }

    public void ChangeQuantity(int delta) => Apply(new QuantityChanged(delta));

    protected override void When(IDomainEvent @event)
    {
        switch (@event)
        {
            case QuantityChanged changed:
                _changes.Add(changed.Delta);
                Quantity += changed.Delta;
                break;
            default:
                throw new NotSupportedException($"Unknown event: {@event.GetType().Name}");
        }
    }
}
