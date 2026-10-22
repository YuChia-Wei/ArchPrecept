using Microsoft.CodeAnalysis;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class RoleClassifierTests
{
    [Theory]
    [InlineData("class Subject : IRole { }", "Subject")]
    [InlineData("record Subject : IRole;", "Subject")]
    [InlineData("struct Subject : IRole { }", "Subject")]
    [InlineData("interface Subject : IRole { }", "Subject")]
    [InlineData("abstract class Parent : IRole { } class Subject : Parent { }", "Subject")]
    [InlineData("class Subject<T> where T : IRole { }", "Subject`1")]
    public void Custom_mapping_follows_types_inheritance_and_constraints(string declaration, string metadataName)
    {
        var compilation = AnalyzerTestHelper.CreateCompilation("public interface IRole {} " + declaration);
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options("""
            <ArchPrecept version="1" namingConventions="false"><Role name="aggregate" metadataName="IRole" assembly="AnalyzerTests" /></ArchPrecept>
            """));
        var type = compilation.GetTypeByMetadataName(metadataName)!;
        var match = classifier.Match(type.IsGenericType ? type.TypeParameters[0] : type, "aggregate");
        Assert.True(match.Matched);
        Assert.Equal("configured-contract", match.Evidence);
        Assert.Equal("high", match.Confidence);
        Assert.Same(match, classifier.Match(type.IsGenericType ? type.TypeParameters[0] : type, "aggregate"));
    }

    [Theory]
    [InlineData("ArchPrecept.Contracts.Domain", "0.0.2.0", true)]
    [InlineData("Unrelated.Assembly", "0.0.2.0", false)]
    [InlineData("ArchPrecept.Contracts.Domain", "1.0.0.0", false)]
    public void Builtin_requires_full_metadata_and_compatible_assembly_identity(string assembly, string version, bool expected)
    {
        var reference = AnalyzerTestHelper.ReferenceAssembly($$"""
            [assembly: System.Reflection.AssemblyVersion("{{version}}")]
            namespace ArchPrecept.Contracts.Domain { public interface IValueObject {} }
            """, assembly);
        var compilation = AnalyzerTestHelper.CreateCompilation("class Price : ArchPrecept.Contracts.Domain.IValueObject {}", new[] { reference });
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options("<ArchPrecept version=\"1\" profiles=\"ddd\" namingConventions=\"false\" />"));
        Assert.Equal(expected, classifier.Is(compilation.GetTypeByMetadataName("Price"), "value-object"));
    }

    [Fact]
    public void Source_lookalike_and_namespace_prefix_do_not_gain_builtin_identity()
    {
        var compilation = AnalyzerTestHelper.CreateCompilation("""
            namespace ArchPrecept.Contracts.Domain { public interface IValueObject {} }
            namespace Other { public class Entity {} }
            namespace Acme.Domainish { public class PriceRecord {} }
            """);
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options("""
            <ArchPrecept version="1" namingConventions="false">
              <Convention role="entity" suffix="Record" namespace="Acme.Domain" />
            </ArchPrecept>
            """));
        Assert.False(classifier.Is(compilation.GetTypeByMetadataName("ArchPrecept.Contracts.Domain.IValueObject"), "value-object"));
        Assert.False(classifier.Is(compilation.GetTypeByMetadataName("Other.Entity"), "entity"));
        Assert.False(classifier.Is(compilation.GetTypeByMetadataName("Acme.Domainish.PriceRecord"), "entity"));
    }

    [Fact]
    public void Explicit_mapping_precedes_convention_and_naming_evidence_is_low_confidence()
    {
        var compilation = AnalyzerTestHelper.CreateCompilation("interface IOperation {} class CheckoutUseCase : IOperation {} class OtherUseCase {} ");
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options("""
            <ArchPrecept version="1"><Role name="use-case" metadataName="IOperation" assembly="AnalyzerTests" /></ArchPrecept>
            """));
        Assert.Equal("configured-contract", classifier.Match(compilation.GetTypeByMetadataName("CheckoutUseCase"), "use-case").Evidence);
        var convention = classifier.Match(compilation.GetTypeByMetadataName("OtherUseCase"), "use-case");
        Assert.Equal("naming-convention", convention.Evidence);
        Assert.Equal("low", convention.Confidence);
    }

    [Theory]
    [InlineData("ILocal", false)]
    [InlineData("IDerived", false)]
    [InlineData("IConflicting", true)]
    [InlineData("Other.IDomainEventDispatcher", true)]
    public async Task Local_domain_port_does_not_exempt_a_bus_mix_in(string dependency, bool forbidden)
    {
        var source = $$"""
            using System.Threading; using System.Threading.Tasks;
            public interface ILocal {}
            public interface IDerived : ILocal {}
            public interface ICustomBus {}
            public interface IConflicting : ILocal, ICustomBus {}
            namespace Other { public interface IDomainEventDispatcher {} }
            public class SaveUseCase
            {
                public SaveUseCase({{dependency}} port) {}
                public Task ExecuteAsync(CancellationToken ct) => Task.CompletedTask;
            }
            """;
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new UseCaseServiceProviderInjectionAnalyzer(), source, """
            <ArchPrecept version="1" profiles="framework">
              <Role name="domain-event-dispatcher" metadataName="ILocal" assembly="AnalyzerTests" />
              <Role name="bus" metadataName="ICustomBus" assembly="AnalyzerTests" />
            </ArchPrecept>
            """);
        Assert.Equal(forbidden ? 1 : 0, diagnostics.Count(diagnostic => diagnostic.Id == "DBA1016"));
    }

    [Fact]
    public async Task Builtin_port_and_derived_port_allow_use_case_injection_but_real_bus_still_fails()
    {
        var reference = AnalyzerTestHelper.ReferenceAssembly("namespace ArchPrecept.Contracts.Application { public interface IDomainEventDispatcher {} }", "ArchPrecept.Contracts.Application");
        var source = """
            using System.Threading; using System.Threading.Tasks;
            using ArchPrecept.Contracts.Application;
            interface IDerivedDispatcher : IDomainEventDispatcher {}
            interface IMessageBus {}
            class GoodUseCase(IDomainEventDispatcher first, IDerivedDispatcher second)
            { public Task ExecuteAsync(CancellationToken ct) => Task.CompletedTask; }
            class BadUseCase(IMessageBus bus)
            { public Task ExecuteAsync(CancellationToken ct) => Task.CompletedTask; }
            """;
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new UseCaseServiceProviderInjectionAnalyzer(), source, references: new[] { reference });
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal("DBA1016", diagnostic.Id);
        Assert.Contains("BadUseCase", diagnostic.GetMessage());
        Assert.Equal("AP-USECASE-002", diagnostic.Properties["ruleId"]);
        Assert.Equal("naming-convention", diagnostic.Properties["roleEvidence"]);
    }
}
