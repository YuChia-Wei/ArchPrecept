using SampleShop.Domain;

namespace SampleShop.Application;

public sealed record CreateProductInput(string Name);
public sealed record CreateProductOutput(Guid Id, string Name);

public interface ICreateProductUseCase : IUseCase<CreateProductInput, CreateProductOutput>
{
}

public sealed class CreateProductUseCase : ICreateProductUseCase
{
    public Task<CreateProductOutput> ExecuteAsync(CreateProductInput input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var name = new ProductName(input.Name);
        return Task.FromResult(new CreateProductOutput(Guid.NewGuid(), name.Value));
    }
}
