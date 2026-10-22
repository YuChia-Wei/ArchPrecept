using ArchPrecept.Contracts.Domain;

namespace Products.Sample;

public readonly record struct ProductPrice : IValueObject
{
    public decimal Value { get; }

    public ProductPrice(decimal value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Price cannot be negative.");
        Value = value;
    }
}

public sealed class Product : AggregateRoot<Guid>
{
    public string Name { get; }
    public ProductPrice Price { get; }

    public Product(string name, decimal price) : base(Guid.NewGuid())
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Price = new ProductPrice(price);
    }
}
