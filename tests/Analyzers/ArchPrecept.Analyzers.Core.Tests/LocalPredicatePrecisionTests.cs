using Microsoft.CodeAnalysis;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class LocalPredicatePrecisionTests
{
    private const string EsConfiguration = """
        <ArchPrecept version="1" profiles="event-sourcing" namingConventions="false">
          <Role name="event-sourced-aggregate" metadataName="Root" assembly="AnalyzerTests" />
        </ArchPrecept>
        """;

    [Theory]
    [InlineData("protected override void When(object e) { State++; }", 0)]
    [InlineData("protected override void When(object e) { } private void When(string e) { State++; }", 1)]
    [InlineData("protected override void When(object e) { System.Action later = () => State++; }", 1)]
    [InlineData("protected override void When(object e) { void Later() { State++; } }", 1)]
    [InlineData("protected override void When(object e) { } public void Change() { State >>>= 1; }", 1)]
    public async Task Es_transition_requires_actual_override_and_execution_boundary(string members, int expected)
    {
        var source = "public abstract class Root { protected abstract void When(object e); } public class Order : Root { private int State; " + members + " }";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new EventSourcedAggregateMutationAnalyzer(), source, EsConfiguration);
        Assert.Equal(expected, diagnostics.Length);
        Assert.All(diagnostics, diagnostic => Assert.Equal("DBA1009", diagnostic.Id));
    }

    [Fact]
    public async Task Es_indirect_override_retains_mapped_contract_identity()
    {
        var source = """
            public abstract class Root { protected abstract void When(object e); }
            public abstract class Intermediate : Root { protected override void When(object e) { } }
            public sealed class Order : Intermediate { private int State; protected override void When(object e) { State++; } }
            """;
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new EventSourcedAggregateMutationAnalyzer(), source, EsConfiguration));
    }

    [Fact]
    public async Task Es_builtin_contract_supports_override_without_reference_contract_dependency()
    {
        var reference = AnalyzerTestHelper.ReferenceAssembly("""
            namespace ArchPrecept.Contracts.Domain.EventSourcing {
              public abstract class EsAggregateRoot<T> { protected abstract void When(object e); }
            }
            """, "ArchPrecept.Contracts.Domain.EventSourcing");
        var source = """
            public class Order : ArchPrecept.Contracts.Domain.EventSourcing.EsAggregateRoot<string> {
              private int state; protected override void When(object e) { state++; }
            }
            """;
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new EventSourcedAggregateMutationAnalyzer(), source,
            "<ArchPrecept version=\"1\" profiles=\"event-sourcing\" namingConventions=\"false\" />", references: new[] { reference }));
    }

    [Fact]
    public async Task Es_scope_excludes_static_other_receiver_constructor_and_collection_calls()
    {
        var source = """
            public abstract class Root { protected abstract void When(object e); }
            public class Order : Root {
              private int state = 1; private static int shared; private readonly System.Collections.Generic.List<int> items = new();
              public Order() { state = 2; }
              protected override void When(object e) { }
              public void Change(Order other) { shared++; other.state++; items.Add(1); }
            }
            """;
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new EventSourcedAggregateMutationAnalyzer(), source, EsConfiguration));
    }

    [Fact]
    public async Task Es_helper_assignment_is_reported_without_call_graph_exemption()
    {
        var source = """
            public abstract class Root { protected abstract void When(object e); }
            public class Order : Root {
              private int state; protected override void When(object e) => Change();
              private void Change() { state++; }
            }
            """;
        Assert.Equal("DBA1009", Assert.Single(await AnalyzerTestHelper.GetDiagnosticsAsync(new EventSourcedAggregateMutationAnalyzer(), source, EsConfiguration)).Id);
    }

    [Fact]
    public async Task Unresolved_es_mapping_fails_configuration_instead_of_claiming_applicability()
    {
        var source = "public class Order { private int state; public void Change() { state++; } }";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ConfigurationCoverageAnalyzer(), source, EsConfiguration);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "DBA0001");
    }

    private const string DomainConfiguration = """
        <ArchPrecept version="1" profiles="core,framework" namingConventions="false">
          <Role name="aggregate" metadataName="Order" assembly="AnalyzerTests" />
          <Role name="mapper" metadataName="Converter" assembly="AnalyzerTests" />
          <Role name="repository" metadataName="Store" assembly="AnalyzerTests" />
          <Role name="db-context" metadataName="Persistence" assembly="AnalyzerTests" />
        </ArchPrecept>
        """;

    [Fact]
    public async Task Nested_types_are_not_attributed_to_outer_domain_or_mapper()
    {
        var source = """
            public class Persistence { }
            public class Store { }
            public class Order { public class Adapter { private Persistence store; } }
            public static class Converter { public class Enricher { private Store store; } }
            """;
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new AggregateInfrastructureDependencyAnalyzer(), source, DomainConfiguration));
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new MapperComplianceAnalyzer(), source, DomainConfiguration));
    }

    [Fact]
    public async Task Domain_declared_field_property_and_parameter_are_checked()
    {
        var source = """
            public class Persistence { } public class Store { } public static class Converter { }
            public class Order { private Persistence field; public Persistence Property { get; set; } public void Work(Persistence input) { } }
            """;
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new AggregateInfrastructureDependencyAnalyzer(), source, DomainConfiguration);
        Assert.Equal(3, diagnostics.Length);
        Assert.All(diagnostics, diagnostic => Assert.Equal("DBA1003", diagnostic.Id));
    }

    [Fact]
    public async Task Static_mapper_does_not_gain_purity_from_form_and_dependency_still_reports()
    {
        var source = """
            public class Persistence { } public class Store { } public class Order { }
            public static class Converter { public static int count; public static int Convert(Store store) { count++; return count; } }
            """;
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new MapperComplianceAnalyzer(), source, DomainConfiguration);
        Assert.Equal("DBA1008", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task Instance_mapper_remains_disallowed_and_unmapped_enricher_is_outside_scope()
    {
        var source = """
            public class Persistence { } public class Store { } public class Order { }
            public class Converter { public int Convert(int value) => value; }
            public class Enricher { public Enricher(Store store) { } }
            """;
        Assert.Equal("DBA1007", Assert.Single(await AnalyzerTestHelper.GetDiagnosticsAsync(new MapperComplianceAnalyzer(), source, DomainConfiguration)).Id);
    }

    [Fact]
    public async Task Static_mutable_state_is_not_a_purity_pass_and_suffix_decoys_do_not_gain_roles()
    {
        var source = """
            public class Persistence { } public class Store { }
            public class DecoyDbContext { } public class DecoyRepository { }
            public class Order { public void Work(DecoyDbContext decoy) { } }
            public static class Converter { private static int counter; public static int Convert(DecoyRepository decoy) => ++counter; }
            """;
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new AggregateInfrastructureDependencyAnalyzer(), source, DomainConfiguration));
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new MapperComplianceAnalyzer(), source, DomainConfiguration));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Query_reader_uses_declared_persistence_api_and_ignores_new_same_name(bool explicitImplementation)
    {
        var source = """
            public interface Store { void SaveChanges(); }
            public class Adapter : Store { IMPLEMENT public void SaveChanges(int unrelated) { } }
            public class Reader { public void Read(Store port, Adapter adapter) { port.SaveChanges(); adapter.SaveChanges(1); } }
            """.Replace("IMPLEMENT", explicitImplementation ? "void Store.SaveChanges() { }" : "public void SaveChanges() { }");
        const string config = """
            <ArchPrecept version="1" profiles="core" namingConventions="false">
              <Role name="projection" metadataName="Reader" assembly="AnalyzerTests" />
              <Role name="db-context" metadataName="Store" assembly="AnalyzerTests" />
            </ArchPrecept>
            """;
        Assert.Equal("DBA1013", Assert.Single(await AnalyzerTestHelper.GetDiagnosticsAsync(new ProjectionReadOnlyAnalyzer(), source, config)).Id);
    }

    [Fact]
    public async Task Concrete_custom_port_implementation_and_override_are_checked()
    {
        var source = """
            public interface Store { void SaveChanges(); }
            public class Adapter : Store { public virtual void SaveChanges() { } }
            public class Derived : Adapter { public override void SaveChanges() { } }
            public class Reader { public void Read(Adapter adapter, Derived derived) { adapter.SaveChanges(); derived.SaveChanges(); } }
            """;
        const string config = """
            <ArchPrecept version="1" profiles="core" namingConventions="false">
              <Role name="projection" metadataName="Reader" assembly="AnalyzerTests" />
              <Role name="db-context" metadataName="Store" assembly="AnalyzerTests" />
            </ArchPrecept>
            """;
        Assert.Equal(2, (await AnalyzerTestHelper.GetDiagnosticsAsync(new ProjectionReadOnlyAnalyzer(), source, config)).Length);
    }

    [Fact]
    public async Task Framework_namespace_in_wrong_assembly_is_not_persistence_identity()
    {
        var fake = AnalyzerTestHelper.ReferenceAssembly("namespace Microsoft.EntityFrameworkCore { public class DbContext { public void SaveChanges() {} } }", "Unrelated.Persistence");
        var source = "public class Reader { public void Read(Microsoft.EntityFrameworkCore.DbContext context) { context.SaveChanges(); } }";
        const string config = """
            <ArchPrecept version="1" profiles="core" namingConventions="false">
              <Role name="projection" metadataName="Reader" assembly="AnalyzerTests" />
            </ArchPrecept>
            """;
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new ProjectionReadOnlyAnalyzer(), source, config, references: new[] { fake }));
    }

    [Fact]
    public async Task Projector_write_is_not_query_reader_but_mixed_query_role_is_checked()
    {
        var source = """
            public class Store { public void SaveChanges() { } }
            public class Projector { public void Apply(Store readStore, Store commandStore) { readStore.SaveChanges(); commandStore.SaveChanges(); } }
            public class Reader { public void Read(Store commandStore) { commandStore.SaveChanges(); } }
            """;
        const string config = """
            <ArchPrecept version="1" profiles="core" namingConventions="false">
              <Role name="projection" metadataName="Reader" assembly="AnalyzerTests" />
              <Role name="db-context" metadataName="Store" assembly="AnalyzerTests" />
            </ArchPrecept>
            """;
        Assert.Equal("DBA1013", Assert.Single(await AnalyzerTestHelper.GetDiagnosticsAsync(new ProjectionReadOnlyAnalyzer(), source, config)).Id);
    }
}
