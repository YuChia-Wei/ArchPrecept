namespace SampleShop.Domain;

#if (CustomContracts)
public sealed record ProductName : IDomainValue
#else
public sealed record ProductName : global::ArchPrecept.Contracts.Domain.IValueObject
#endif
{
    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Product name is required.");

        Value = value.Trim();
    }

    public string Value { get; }
}
