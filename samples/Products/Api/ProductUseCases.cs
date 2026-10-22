namespace Products.Sample;

public sealed record CreateProductInput(string Name, decimal Price);
public sealed record ProductOutput(Guid Id, string Name, decimal Price);

public interface ICreateProductUseCase
{
    Task<ProductOutput> ExecuteAsync(CreateProductInput input, CancellationToken cancellationToken);
}

public sealed class CreateProductUseCase(IProductRepository repository) : ICreateProductUseCase
{
    public async Task<ProductOutput> ExecuteAsync(CreateProductInput input, CancellationToken cancellationToken)
    {
        var product = new Product(input.Name, input.Price);
        await repository.SaveAsync(product, cancellationToken);
        return new ProductOutput(product.Id, product.Name, product.Price.Value);
    }
}

public interface IGetProductUseCase
{
    Task<ProductOutput?> ExecuteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class GetProductUseCase(IProductRepository repository) : IGetProductUseCase
{
    public async Task<ProductOutput?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await repository.FindByIdAsync(id, cancellationToken);
        return product is null ? null : new ProductOutput(product.Id, product.Name, product.Price.Value);
    }
}
