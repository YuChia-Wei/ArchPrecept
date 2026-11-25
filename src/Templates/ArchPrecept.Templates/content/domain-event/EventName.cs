namespace ItemNamespace;

// Add the business facts that consumers of this domain event need.
public sealed record EventName(global::System.Guid EventId, global::System.DateTime OccurredOn) : EventContract;
