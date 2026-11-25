namespace ItemNamespace;

public sealed class ModelName : AggregateBase<IdentityType>
{
    public ModelName(IdentityType id) : base(id)
    {
        // Validate the identity and initial state required by your domain.
    }
}
