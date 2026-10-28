using Microsoft.CodeAnalysis;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class RepositoryPortViewTests
{
    private const string Contracts = """
        using System.Threading;
        using System.Threading.Tasks;
        public interface IAggregateRoot<TId> { }
        public sealed class Order : IAggregateRoot<string> { }
        public sealed class Invoice : IAggregateRoot<int> { }
        public interface IAggregateRepository<TAggregate, TId> where TAggregate : IAggregateRoot<TId>
        {
            Task<TAggregate> FindByIdAsync(TId id, CancellationToken ct);
            Task SaveAsync(TAggregate aggregate, CancellationToken ct);
        }
        public interface IQueryRepository { }
        public interface IReadOrders : IQueryRepository { Task<string[]> FindIdsAsync(); }
        """;

    [Fact]
    public async Task Adapter_can_implement_separate_aggregate_and_query_ports()
    {
        var diagnostics = await Analyze(Contracts + """
            public sealed class Store : IAggregateRepository<Order, string>, IReadOrders
            {
                public Task<Order> FindByIdAsync(string id, CancellationToken ct) => Task.FromResult(new Order());
                public Task SaveAsync(Order aggregate, CancellationToken ct) => Task.CompletedTask;
                public Task<string[]> FindIdsAsync() => Task.FromResult(new string[0]);
            }
            """);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Private_default_interface_helpers_are_not_exposed_port_methods()
    {
        Assert.Empty(await Analyze(Contracts + """
            public interface IOrders : IAggregateRepository<Order, string>
            {
                private void InitializeCache() { }
            }
            """));
    }

    [Fact]
    public async Task Explicit_multi_aggregate_implementation_keeps_each_generic_pair()
    {
        var diagnostics = await Analyze(Contracts + """
            public sealed class Store : IAggregateRepository<Order, string>, IAggregateRepository<Invoice, int>, IReadOrders
            {
                Task<Order> IAggregateRepository<Order, string>.FindByIdAsync(string id, CancellationToken ct) => Task.FromResult(new Order());
                Task IAggregateRepository<Order, string>.SaveAsync(Order aggregate, CancellationToken ct) => Task.CompletedTask;
                Task<Invoice> IAggregateRepository<Invoice, int>.FindByIdAsync(int id, CancellationToken ct) => Task.FromResult(new Invoice());
                Task IAggregateRepository<Invoice, int>.SaveAsync(Invoice aggregate, CancellationToken ct) => Task.CompletedTask;
                Task<string[]> IReadOrders.FindIdsAsync() => Task.FromResult(new string[0]);
            }
            """);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task Inherited_generic_interface_and_base_implementation_are_substituted()
    {
        var diagnostics = await Analyze(Contracts + """
            public interface IIntermediate<T> : IAggregateRepository<T, string> where T : IAggregateRoot<string> { }
            public interface IOrders : IIntermediate<Order> { }
            public class StoreBase<T> where T : IAggregateRoot<string>, new()
            {
                public Task<T> FindByIdAsync(string id, CancellationToken ct) => Task.FromResult(new T());
                public Task SaveAsync(T aggregate, CancellationToken ct) => Task.CompletedTask;
            }
            public sealed class Store : StoreBase<Order>, IOrders { }
            """);
        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("Task<string[]> FindByStatusAsync(string status);", "outside")]
    [InlineData("Task<Order> FindByIdAsync(int id, CancellationToken ct);", "signature")]
    [InlineData("Task SaveAsync(Invoice value, CancellationToken ct);", "signature")]
    [InlineData("Task SaveAsync<T>(Order value, CancellationToken ct);", "signature")]
    [InlineData("Task SaveAsync(ref Order value, CancellationToken ct);", "signature")]
    public async Task Extra_members_on_aggregate_port_remain_restricted(string member, string reason)
    {
        var diagnostics = await Analyze(Contracts + "public interface IOrders : IAggregateRepository<Order, string> { " + member + " }");
        var diagnostic = Assert.Single(diagnostics);
        Assert.Contains(reason, diagnostic.GetMessage());
    }

    [Fact]
    public async Task Unmapped_public_adapter_member_does_not_escape_through_query_role()
    {
        var diagnostics = await Analyze(Contracts + """
            public sealed class Store : IAggregateRepository<Order, string>, IReadOrders
            {
                public Task<Order> FindByIdAsync(string id, CancellationToken ct) => Task.FromResult(new Order());
                public Task SaveAsync(Order aggregate, CancellationToken ct) => Task.CompletedTask;
                public Task<string[]> FindIdsAsync() => Task.FromResult(new string[0]);
                public Task PurgeEverythingAsync() => Task.CompletedTask;
            }
            """);
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetMessage().Contains("PurgeEverythingAsync"));
    }

    [Theory]
    [InlineData("Task UpdateAsync();", "persistence write")]
    [InlineData("Task<Order[]> ReadAsync();", "Aggregate Root or Domain Entity")]
    public async Task Query_inherits_restricted_members_from_unmarked_interface(string member, string reason)
    {
        var diagnostics = await Analyze(Contracts + "public interface IParent { " + member + " } public interface IReader : IParent, IQueryRepository { }");
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetMessage().Contains(reason));
    }

    [Theory]
    [InlineData("IReadOrders", true)]
    [InlineData("IMixed", false)]
    [InlineData("Store", false)]
    [InlineData("IWritable", false)]
    [InlineData("IEntities", false)]
    [InlineData("IProperty", false)]
    [InlineData("IPropertyPort", false)]
    [InlineData("IIndexer", false)]
    [InlineData("IPortReturn", false)]
    [InlineData("IReadProperty", true)]
    [InlineData("FieldReader", false)]
    [InlineData("IReference", false)]
    [InlineData("IReferenceProperty", false)]
    [InlineData("IUnrelated", false)]
    public void Query_only_uses_actual_dependency_view(string dependency, bool expected)
    {
        var compilation = AnalyzerTestHelper.CreateCompilation(Contracts + """
            public interface IMixed : IReadOrders, IAggregateRepository<Order, string> { }
            public abstract class Store : IReadOrders, IAggregateRepository<Order, string>
            {
                public abstract Task<Order> FindByIdAsync(string id, CancellationToken ct);
                public abstract Task SaveAsync(Order aggregate, CancellationToken ct);
                public abstract Task<string[]> FindIdsAsync();
            }
            public interface IWritable : IQueryRepository { Task DeleteAsync(); }
            public interface IEntities : IQueryRepository { Task<Order[]> ReadAsync(); }
            public interface IProperty : IQueryRepository { string Name { get; set; } }
            public interface IPropertyPort : IQueryRepository { IAggregateRepository<Order, string> Store { get; } }
            public interface IIndexer : IQueryRepository { Order this[int index] { get; } }
            public interface IPortReturn : IQueryRepository { Task<IAggregateRepository<Order, string>> GetStore(); }
            public interface IReadProperty : IQueryRepository { int Count { get; } }
            public class FieldReader : IQueryRepository { public string State; }
            public interface IReference : IQueryRepository { ref int Read(); }
            public interface IReferenceProperty : IQueryRepository { ref int Value { get; } }
            public interface IUnrelated { Task<string> ReadAsync(); }
            """);
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options(null));
        Assert.Equal(expected, RepositoryPortView.IsQueryOnly(compilation.GetTypeByMetadataName(dependency)!, classifier));
    }

    [Fact]
    public void Unresolved_query_signature_is_not_query_only()
    {
        var compilation = AnalyzerTestHelper.CreateCompilation("interface IQueryRepository {} interface IReader : IQueryRepository { Missing Read(); }");
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options(null));
        Assert.False(RepositoryPortView.IsQueryOnly(compilation.GetTypeByMetadataName("IReader")!, classifier));
    }

    [Fact]
    public async Task Custom_contract_mapping_works_with_conventions_disabled()
    {
        var source = """
            using System.Threading.Tasks;
            public interface IRoot<T> { }
            public class Order : IRoot<string> { }
            public interface IWrite<T, TId> where T : IRoot<TId> { Task<T> FindByIdAsync(TId id); Task SaveAsync(T value); }
            public interface IOrders : IWrite<Order, string> { }
            """;
        var config = """
            <ArchPrecept version="1" namingConventions="false">
              <Role name="aggregate" metadataName="IRoot`1" assembly="AnalyzerTests" />
              <Role name="aggregate-repository" metadataName="IWrite`2" assembly="AnalyzerTests" />
            </ArchPrecept>
            """;
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new RepositoryQueryMethodAnalyzer(), source, config));
    }

    [Fact]
    public async Task Same_named_unmapped_contract_is_not_an_aggregate_port_when_conventions_off()
    {
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new RepositoryQueryMethodAnalyzer(),
            "interface IAggregateRepository<T, TId> { void DeleteAll(); }",
            "<ArchPrecept version=\"1\" namingConventions=\"false\" />"));
    }

    [Theory]
    [InlineData("namespace System.Threading.Tasks { public class Task<T> {} }", "System.Threading.Tasks.Task<T> FindByIdAsync(TId id);")]
    [InlineData("namespace System.Threading { public struct CancellationToken {} }", "System.Threading.Tasks.Task SaveAsync(T value, System.Threading.CancellationToken ct);")]
    public async Task Source_BCL_lookalike_does_not_satisfy_repository_signature(string lookalike, string member)
    {
        var source = lookalike + " public interface IAggregateRoot<T> {} public interface IAggregateRepository<T, TId> where T : IAggregateRoot<TId> { " + member + " }";
        var diagnostic = Assert.Single(await Analyze(source));
        Assert.Contains("signature", diagnostic.GetMessage());
    }

    [Fact]
    public async Task Custom_aggregate_contract_does_not_hide_wrong_identity_type()
    {
        var source = """
            using System.Threading.Tasks;
            interface IAggregateRoot<T> { }
            class Order : IAggregateRoot<string> { }
            interface IAggregateRepository<T, TId> { Task<T> FindByIdAsync(TId id); Task SaveAsync(T value); }
            interface IOrders : IAggregateRepository<Order, int> { }
            """;
        var diagnostics = await Analyze(source);
        Assert.Contains(diagnostics, diagnostic => diagnostic.GetMessage().Contains("does not match its Aggregate identity"));
    }

    [Fact]
    public void Custom_query_service_uses_same_exposed_capability_checks()
    {
        var compilation = AnalyzerTestHelper.CreateCompilation("interface IRead { int Count { get; } } interface IBad : IRead { void Save(); }");
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options("""
            <ArchPrecept version="1" namingConventions="false"><Role name="query-service" metadataName="IRead" assembly="AnalyzerTests" /></ArchPrecept>
            """));
        Assert.True(RepositoryPortView.IsQueryOnly(compilation.GetTypeByMetadataName("IRead")!, classifier));
        Assert.False(RepositoryPortView.IsQueryOnly(compilation.GetTypeByMetadataName("IBad")!, classifier));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task External_generic_port_contract_is_checked_at_adapter(bool explicitImplementation)
    {
        var reference = AnalyzerTestHelper.ReferenceAssembly("""
            using System.Threading.Tasks;
            public interface IAggregateRoot<T> { }
            public sealed class Order : IAggregateRoot<string> { }
            public interface IAggregateRepository<T, TId> where T : IAggregateRoot<TId>
            { Task<T> FindByIdAsync(TId id); Task SaveAsync(T value); Task<T> FindByStatusAsync(string status); }
            public interface IOrders : IAggregateRepository<Order, string> { }
            """, "External.Contracts");
        var qualifier = explicitImplementation ? "IAggregateRepository<Order, string>." : "";
        var accessibility = explicitImplementation ? "" : "public ";
        var source = $$"""
            using System.Threading.Tasks;
            public sealed class Store : IOrders
            {
                {{accessibility}}Task<Order> {{qualifier}}FindByIdAsync(string id) => Task.FromResult(new Order());
                {{accessibility}}Task {{qualifier}}SaveAsync(Order value) => Task.CompletedTask;
                {{accessibility}}Task<Order> {{qualifier}}FindByStatusAsync(string status) => Task.FromResult(new Order());
            }
            """;
        var diagnostic = Assert.Single(await AnalyzerTestHelper.GetDiagnosticsAsync(new RepositoryQueryMethodAnalyzer(), source, references: new[] { reference }));
        Assert.Contains("FindByStatusAsync", diagnostic.GetMessage());
        Assert.Equal("Store", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    [Fact]
    public void Missing_mapping_is_not_query_only_or_an_applicability_success()
    {
        var compilation = AnalyzerTestHelper.CreateCompilation("interface IQueryRepository { int Read(); }");
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options("""
            <ArchPrecept version="1" namingConventions="false"><Role name="query-repository" metadataName="Missing" assembly="Missing.Assembly" /></ArchPrecept>
            """));
        Assert.NotEmpty(classifier.Configuration.Errors);
        Assert.False(RepositoryPortView.IsQueryOnly(compilation.GetTypeByMetadataName("IQueryRepository")!, classifier));
    }

    [Theory]
    [InlineData("bus")]
    [InlineData("transport")]
    [InlineData("use-case")]
    [InlineData("handler")]
    [InlineData("domain-event-dispatcher")]
    public void Query_view_cannot_return_forbidden_capability_via_wrapper_or_property(string role)
    {
        var compilation = AnalyzerTestHelper.CreateCompilation("""
            using System.Threading.Tasks;
            interface IQueryRepository { }
            interface ICapability { }
            interface IMethodReader : IQueryRepository { Task<ICapability[]> Read(); }
            interface IPropertyReader : IQueryRepository { ICapability Capability { get; } }
            """);
        var classifier = RoleClassifier.For(compilation, AnalyzerTestHelper.Options($$"""
            <ArchPrecept version="1"><Role name="{{role}}" metadataName="ICapability" assembly="AnalyzerTests" /></ArchPrecept>
            """));
        Assert.False(RepositoryPortView.IsQueryOnly(compilation.GetTypeByMetadataName("IMethodReader")!, classifier));
        Assert.False(RepositoryPortView.IsQueryOnly(compilation.GetTypeByMetadataName("IPropertyReader")!, classifier));
    }

    [Fact]
    public async Task Repository_diagnostic_honors_Roslyn_severity_override()
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new RepositoryQueryMethodAnalyzer(),
            "interface IQueryRepository { void Delete(); }",
            severities: System.Collections.Immutable.ImmutableDictionary<string, ReportDiagnostic>.Empty.Add("DBA1001", ReportDiagnostic.Warn));
        Assert.Equal(DiagnosticSeverity.Warning, Assert.Single(diagnostics).Severity);
    }

    private static Task<System.Collections.Immutable.ImmutableArray<Diagnostic>> Analyze(string source) =>
        AnalyzerTestHelper.GetDiagnosticsAsync(new RepositoryQueryMethodAnalyzer(), source);
}
