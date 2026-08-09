namespace EngineeringGuardrails.Contracts.Domain;

public abstract class DomainEntity<TId>
    : IDomainEntity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected DomainEntity(TId id)
    {
        Id = id;
    }

    protected DomainEntity()
    {
    }

    public TId Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
