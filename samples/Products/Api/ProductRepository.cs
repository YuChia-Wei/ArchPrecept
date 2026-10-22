using System.Collections.Concurrent;
using ArchPrecept.Contracts.Application;

namespace Products.Sample;

public interface IProductRepository : IAggregateRepository<Product, Guid> { }

// Sample only: process-local storage, no durability, cross-request transaction, or broker guarantee.
public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> products = new();

    public Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(products.TryGetValue(id, out var product) ? product : null);
    }

    public Task SaveAsync(Product aggregate, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        products[aggregate.Id] = aggregate;
        return Task.CompletedTask;
    }
}
