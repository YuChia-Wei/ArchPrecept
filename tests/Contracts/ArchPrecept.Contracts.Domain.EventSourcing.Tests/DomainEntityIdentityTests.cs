using ArchPrecept.Contracts.Domain;
using Xunit;

namespace ArchPrecept.Contracts.Domain.EventSourcing.Tests;

public sealed class DomainEntityIdentityTests
{
    [Fact]
    public void Reference_identity_is_required_at_the_read_boundary()
    {
        var entity = new RestoredEntity<string>();
        Assert.Throws<InvalidOperationException>(() => entity.Id);
        entity.Restore("order-1");
        Assert.Equal("order-1", entity.Id);
    }

    [Fact]
    public void Null_identity_is_rejected_without_destroying_an_existing_identity()
    {
        Assert.Throws<ArgumentNullException>(() => new RestoredEntity<string>(null!));
        var entity = new RestoredEntity<string>("order-1");
        Assert.Throws<ArgumentNullException>(() => entity.Restore(null!));
        Assert.Equal("order-1", entity.Id);
    }

    [Fact]
    public void Value_identity_distinguishes_unassigned_from_explicit_default()
    {
        var entity = new RestoredEntity<int>();
        Assert.Throws<InvalidOperationException>(() => entity.Id);
        entity.Restore(0);
        Assert.Equal(0, entity.Id);
        Assert.Equal(Guid.Empty, new RestoredEntity<Guid>(Guid.Empty).Id);
    }

    [Fact]
    public void Rehydration_can_assign_identity_through_the_protected_property()
    {
        var entity = new RestoredEntity<string>();
        typeof(DomainEntity<string>).GetProperty(nameof(DomainEntity<string>.Id))!.SetValue(entity, "restored-1");
        Assert.Equal("restored-1", entity.Id);
    }

    [Fact]
    public void Creation_event_assigns_identity_during_replay_without_pending_events()
    {
        var entity = new EventCreatedEntity();
        Assert.Throws<InvalidOperationException>(() => entity.Id);
        entity.Replay(new Created("event-1"));
        Assert.Equal("event-1", entity.Id);
        Assert.Equal(1, entity.Version);
        Assert.Empty(entity.DomainEvents);
    }

    private sealed class RestoredEntity<TId> : DomainEntity<TId> where TId : notnull
    {
        public RestoredEntity() { }
        public RestoredEntity(TId id) : base(id) { }
        public void Restore(TId id) => Id = id;
    }

    private sealed record Created(string Id) : IDomainEvent
    {
        public Guid EventId => Guid.Empty;
        public DateTime OccurredOn => DateTime.UnixEpoch;
    }

    private sealed class EventCreatedEntity : EsAggregateRoot<string>
    {
        public void Replay(params IDomainEvent[] events) => LoadFromHistory(events);
        protected override void When(IDomainEvent @event) => Id = ((Created)@event).Id;
    }
}
