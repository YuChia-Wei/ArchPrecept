using System;
using System.Linq;
using System.Reflection;
using Xunit;

public sealed class AssemblyBoundaryTests
{
    [Fact]
    public void Source_must_not_reference_the_forbidden_assembly()
    {
        var source = typeof(global::BoundarySource.Type).Assembly;
        var forbidden = typeof(global::BoundaryTarget.Type).Assembly.GetName();
        Assert.False(MatchesTarget(source.GetName()), "Select types from two distinct assemblies.");

        var edge = source.GetReferencedAssemblies().FirstOrDefault(MatchesTarget);
        Assert.True(edge is null,
            $"{nameof(AssemblyBoundaryTests)}: {source.FullName} -> {edge?.FullName}. " +
            $"Remove the forbidden direct reference from {source.Location}.");

        bool MatchesTarget(AssemblyName reference) =>
            string.Equals(reference.Name, forbidden.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(reference.CultureName, forbidden.CultureName, StringComparison.OrdinalIgnoreCase) &&
            (reference.GetPublicKeyToken() ?? Array.Empty<byte>())
                .SequenceEqual(forbidden.GetPublicKeyToken() ?? Array.Empty<byte>());
    }
}
