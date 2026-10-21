using ArchPrecept.Contracts.Application;
using ArchPrecept.Contracts.Domain;
using ArchPrecept.Contracts.Domain.EventSourcing;
using Xunit;

namespace ArchPrecept.Contracts.Domain.EventSourcing.Tests;

public sealed class EsAggregateRootTests
{
    [Fact]
    public void Given_history_when_rehydrated_then_initialized_state_is_rebuilt_without_pending_events()
    {
        var aggregate = Basket.Rehydrate(new Changed(2), new Changed(3));

        Assert.Equal("basket-1", aggregate.Id);
        Assert.Equal(5, aggregate.Total);
        Assert.Equal(new[] { 2, 3 }, aggregate.Changes);
        Assert.Equal(2, aggregate.Version);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Given_empty_history_when_rehydrated_then_version_is_zero()
    {
        var aggregate = Basket.Rehydrate();
        Assert.Equal(0, aggregate.Version);
        Assert.Empty(aggregate.DomainEvents);
        Assert.Throws<InvalidOperationException>(() => aggregate.Reload(Array.Empty<IDomainEvent>()));
    }

    [Fact]
    public void Given_history_when_a_command_runs_then_only_new_events_are_pending_in_order()
    {
        var aggregate = Basket.Rehydrate(new Changed(2));
        aggregate.Change(3);
        aggregate.Change(-1);

        Assert.Equal(4, aggregate.Total);
        Assert.Equal(1, aggregate.Version);
        Assert.Equal(new[] { 3, -1 }, aggregate.DomainEvents.Cast<Changed>().Select(e => e.Amount));
        Assert.Same(aggregate.DomainEvents.Last(), aggregate.LastEvent);
    }

    [Fact]
    public void Given_pending_events_when_appended_and_confirmed_then_version_advances_and_pending_events_clear()
    {
        var history = new IDomainEvent[] { new Changed(2) };
        var aggregate = Basket.Rehydrate(history);
        aggregate.Change(3);
        var appended = history.Concat(aggregate.DomainEvents).ToArray();

        aggregate.MarkChangesAsCommitted(2);
        var replayed = Basket.Rehydrate(appended);

        Assert.Equal(2, aggregate.Version);
        Assert.Empty(aggregate.DomainEvents);
        Assert.Null(aggregate.LastEvent);
        Assert.Equal(aggregate.Total, replayed.Total);
        Assert.Equal(aggregate.Changes, replayed.Changes);
        Assert.Equal(aggregate.Version, replayed.Version);
        aggregate.Change(4);
        Assert.Equal(2, aggregate.Version);
        Assert.Single(aggregate.DomainEvents);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void Given_pending_events_when_commit_version_mismatches_then_events_and_version_are_preserved(int version)
    {
        var aggregate = Basket.Rehydrate(new Changed(2));
        aggregate.Change(3);
        var pending = aggregate.DomainEvents.Single();

        Assert.Throws<InvalidOperationException>(() => aggregate.MarkChangesAsCommitted(version));

        Assert.Equal(1, aggregate.Version);
        Assert.Same(pending, aggregate.DomainEvents.Single());
        Assert.Equal(5, aggregate.Total);
        aggregate.MarkChangesAsCommitted(2);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Given_a_concurrent_append_when_store_rejects_expected_revision_then_losing_aggregate_keeps_pending_events()
    {
        // A deterministic port stand-in demonstrates caller responsibility, not a real storage guarantee.
        var store = new InMemoryStream();
        var winner = Basket.Rehydrate();
        var loser = Basket.Rehydrate();
        winner.Change(1);
        loser.Change(2);

        winner.MarkChangesAsCommitted(store.Append(winner.Version, winner.DomainEvents));
        Assert.Throws<InvalidOperationException>(() =>
            loser.MarkChangesAsCommitted(store.Append(loser.Version, loser.DomainEvents)));

        Assert.Equal(0, loser.Version);
        Assert.Single(loser.DomainEvents);
        Assert.Equal(1, Basket.Rehydrate(store.Events.ToArray()).Total);
    }

    [Fact]
    public void Given_an_existing_instance_when_history_is_loaded_again_then_it_is_rejected_without_state_change()
    {
        var aggregate = Basket.Rehydrate(new Changed(2));
        Assert.Throws<InvalidOperationException>(() => aggregate.Reload(new[] { new Changed(3) }));
        Assert.Equal(2, aggregate.Total);
        Assert.Equal(1, aggregate.Version);
    }

    [Fact]
    public void Given_a_new_event_when_history_is_loaded_then_it_is_rejected_even_after_commit()
    {
        var aggregate = new Basket();
        aggregate.Change(2);
        Assert.Throws<InvalidOperationException>(() => aggregate.Reload(Array.Empty<IDomainEvent>()));
        aggregate.MarkChangesAsCommitted(1);
        Assert.Throws<InvalidOperationException>(() => aggregate.Reload(Array.Empty<IDomainEvent>()));
    }

    [Fact]
    public void Given_pending_events_when_generic_mutators_are_called_then_all_dispatch_paths_reject_changes()
    {
        var aggregate = new Basket();
        aggregate.Change(2);
        var pending = aggregate.DomainEvents.Single();

        foreach (var entity in new IDomainEntity<string>[] { aggregate, (AggregateRoot<string>)aggregate })
        {
            Assert.Throws<NotSupportedException>(() => entity.AddDomainEvent(new Changed(9)));
            Assert.Throws<NotSupportedException>(() => entity.RemoveDomainEvent(pending));
            Assert.Throws<NotSupportedException>(() => entity.ClearDomainEvents());
        }
        Assert.Throws<NotSupportedException>(() => aggregate.ClearDomainEvents());
        Assert.Throws<NotSupportedException>(() => ((DomainEntity<string>)aggregate).AddDomainEvent(new Changed(9)));
        Assert.Throws<NotSupportedException>(() => ((ICollection<IDomainEvent>)aggregate.DomainEvents).Clear());
        Assert.Same(pending, aggregate.DomainEvents.Single());
        Assert.Equal(2, aggregate.Total);
    }

    [Fact]
    public void Given_a_normal_aggregate_when_generic_mutators_are_called_then_existing_behavior_is_preserved()
    {
        var aggregate = new NormalAggregate();
        var first = new Changed(1);
        var second = new Changed(2);
        aggregate.AddDomainEvent(first);
        aggregate.AddDomainEvent(second);
        aggregate.RemoveDomainEvent(first);
        Assert.Same(second, aggregate.DomainEvents.Single());
        ((IDomainEntity<string>)aggregate).ClearDomainEvents();
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Given_es_aggregate_when_used_with_application_repository_then_generic_constraint_is_satisfied()
    {
        Assert.NotNull(typeof(IAggregateRepository<Basket, string>));
        Assert.IsAssignableFrom<IAggregateRoot<string>>(new Basket());
    }

    [Fact]
    public void Given_invalid_input_when_null_is_supplied_then_it_is_rejected_before_state_changes()
    {
        var aggregate = new Basket();
        Assert.Throws<ArgumentNullException>(() => aggregate.Reload(null!));
        Assert.Throws<ArgumentNullException>(() => aggregate.Emit(null!));
        aggregate.Change(1);
        Assert.Equal(1, aggregate.Total);
        Assert.Single(aggregate.DomainEvents);
    }

    [Fact]
    public void Given_unknown_history_when_replay_fails_then_the_instance_cannot_be_used_or_committed()
    {
        var aggregate = new Basket();
        Assert.Throws<NotSupportedException>(() => aggregate.Reload(new IDomainEvent[] { new Changed(1), new Unknown() }));
        Assert.Empty(aggregate.DomainEvents);
        Assert.Throws<InvalidOperationException>(() => aggregate.Change(2));
        Assert.Throws<InvalidOperationException>(() => aggregate.MarkChangesAsCommitted(1));
        Assert.Throws<InvalidOperationException>(() => aggregate.Reload(Array.Empty<IDomainEvent>()));
    }

    [Fact]
    public void Given_a_failing_history_enumerator_when_replaying_then_the_instance_cannot_continue()
    {
        var aggregate = new Basket();
        Assert.Throws<IOException>(() => aggregate.Reload(BrokenHistory()));
        Assert.Throws<InvalidOperationException>(() => aggregate.Change(2));
        Assert.Empty(aggregate.DomainEvents);

        static IEnumerable<IDomainEvent> BrokenHistory()
        {
            yield return new Changed(1);
            throw new IOException("History read failed.");
        }
    }

    [Fact]
    public void Given_null_in_history_when_replaying_then_the_instance_is_rejected()
    {
        var aggregate = new Basket();
        Assert.Throws<ArgumentNullException>(() => aggregate.Reload(new IDomainEvent[] { null! }));
        Assert.Throws<InvalidOperationException>(() => aggregate.Change(2));
    }

    [Fact]
    public void Given_a_failed_transition_when_applying_then_no_event_is_recorded_and_instance_must_be_discarded()
    {
        var aggregate = new Basket();
        Assert.Throws<NotSupportedException>(() => aggregate.Emit(new Unknown()));
        Assert.Empty(aggregate.DomainEvents);
        Assert.Equal(0, aggregate.Version);
        Assert.Throws<InvalidOperationException>(() => aggregate.Change(2));
        Assert.Throws<InvalidOperationException>(() => aggregate.MarkChangesAsCommitted(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_a_reentrant_transition_when_applying_or_replaying_then_nested_apply_is_rejected(bool replay)
    {
        var aggregate = new Basket();
        if (replay)
            Assert.Throws<InvalidOperationException>(() => aggregate.Reload(new[] { new Reentrant() }));
        else
            Assert.Throws<InvalidOperationException>(() => aggregate.Emit(new Reentrant()));
        Assert.Empty(aggregate.DomainEvents);
    }

    private sealed record Changed(int Amount) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    private sealed record Unknown : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    private sealed record Reentrant : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

    private sealed class Basket : EsAggregateRoot<string>
    {
        // Deliberately initialized in the derived constructor body: replay must happen later.
        public Basket() : base("basket-1") => Changes = new List<int>();
        public List<int> Changes { get; }
        public int Total { get; private set; }
        public IDomainEvent? LastEvent => GetLastDomainEvent();
        public static Basket Rehydrate(params IDomainEvent[] history)
        {
            var basket = new Basket();
            basket.LoadFromHistory(history);
            return basket;
        }
        public void Reload(IEnumerable<IDomainEvent> history) => LoadFromHistory(history);
        public void Change(int amount) => Apply(new Changed(amount));
        public void Emit(IDomainEvent @event) => Apply(@event);
        protected override void When(IDomainEvent @event)
        {
            if (@event is Reentrant) { Apply(new Changed(1)); return; }
            if (@event is not Changed changed) throw new NotSupportedException("Unknown event.");
            Changes.Add(changed.Amount);
            Total += changed.Amount;
        }
    }

    private sealed class NormalAggregate : AggregateRoot<string>
    {
        public NormalAggregate() : base("normal-1") { }
    }

    private sealed class InMemoryStream
    {
        public List<IDomainEvent> Events { get; } = new();
        public int Append(int expectedVersion, IReadOnlyCollection<IDomainEvent> events)
        {
            if (expectedVersion != Events.Count) throw new InvalidOperationException("Revision conflict.");
            Events.AddRange(events);
            return Events.Count;
        }
    }
}
