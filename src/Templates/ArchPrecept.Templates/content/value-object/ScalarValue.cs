namespace ItemNamespace;

#if (CustomContracts)
public sealed record ScalarValue : CustomValueMarker
#else
public sealed record ScalarValue : global::ArchPrecept.Contracts.Domain.IValueObject
#endif
{
    public ScalarValue(ScalarType value) => Value = value;

    public ScalarType Value { get; }
}
