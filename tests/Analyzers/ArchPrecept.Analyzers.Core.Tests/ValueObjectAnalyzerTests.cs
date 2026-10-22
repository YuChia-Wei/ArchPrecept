using Microsoft.CodeAnalysis;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class ValueObjectAnalyzerTests
{
    private const string Configuration = """
        <ArchPrecept version="1" profiles="ddd" namingConventions="false">
          <Role name="value-object" metadataName="IValue" assembly="AnalyzerTests" />
          <Role name="entity" metadataName="IEntity" assembly="AnalyzerTests" />
        </ArchPrecept>
        """;
    private const string Prefix = """
        using System; using System.Collections.Generic; using System.Collections.Immutable;
        public interface IValue {} public interface IEntity {}
        public class Account : IEntity {}
        """;

    [Theory]
    [InlineData("public int Amount { get; set; }")]
    [InlineData("public int Amount { get; private set; }")]
    [InlineData("public int Amount { get; protected internal set; }")]
    [InlineData("private int amount;")]
    [InlineData("public readonly List<int> Values = new();")]
    [InlineData("public List<int> Values { get; } = new();")]
    [InlineData("public List<int> Values { get; init; } = new();")]
    [InlineData("public int[] Values { get; } = Array.Empty<int>();")]
    [InlineData("public IDictionary<string, int> Values { get; } = new Dictionary<string, int>();")]
    public async Task Reports_mutable_instance_state_regardless_of_accessibility(string member)
    {
        var diagnostics = await Analyze("public class Money : IValue { " + member + " }");
        var diagnostic = Assert.Single(diagnostics.Where(item => item.Id == "DBA1018"));
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("configured-contract", diagnostic.Properties["roleEvidence"]);
        Assert.DoesNotContain(diagnostics, item => item.Id == "DBA1019");
    }

    [Theory]
    [InlineData("public class Money : IValue { public int Amount { get; init; } }")]
    [InlineData("public record Money(int Amount) : IValue;")]
    [InlineData("public readonly record struct Money(int Amount) : IValue;")]
    [InlineData("public class Money : IValue { public readonly int Amount; }")]
    [InlineData("public class Money : IValue { public Guid Id { get; init; } }")]
    [InlineData("public record Snapshot(string Name, int Count); public record Money(Snapshot State) : IValue;")]
    [InlineData("public record EntityId(Guid Value) : IValue; public record Money(EntityId Id) : IValue;")]
    [InlineData("public class Money : IValue { public static int Cached { get; set; } public const int Scale = 2; }")]
    [InlineData("public record Money(ImmutableArray<int> Values) : IValue;")]
    public async Task Allows_init_readonly_scalar_records_and_immutable_snapshots(string declaration)
    {
        Assert.Empty(await Analyze(declaration));
    }

    [Theory]
    [InlineData("Account")]
    [InlineData("Account[]")]
    [InlineData("IEnumerable<Account>")]
    [InlineData("List<Account>")]
    [InlineData("Dictionary<string, Account>")]
    [InlineData("Dictionary<Account, string>")]
    [InlineData("List<List<Account>>")]
    [InlineData("IEntity")]
    [InlineData("(string Label, Account Entity)")]
    [InlineData("ImmutableArray<Account>")]
    public async Task Finds_entity_reference_paths_in_known_containers(string memberType)
    {
        var diagnostics = await Analyze($"public class Money : IValue {{ public {memberType} Value {{ get; init; }} }}");
        var diagnostic = Assert.Single(diagnostics.Where(item => item.Id == "DBA1019"));
        Assert.Contains("Money.Value", diagnostic.GetMessage());
        Assert.Contains(memberType == "IEntity" ? "IEntity" : "Account", diagnostic.GetMessage());
    }

    [Fact]
    public async Task Checks_readonly_entity_field_and_generic_constraint()
    {
        var diagnostics = await Analyze("public class Money<T> : IValue where T : IEntity { public readonly T Entity; }");
        Assert.Single(diagnostics.Where(item => item.Id == "DBA1019"));
    }

    [Theory]
    [InlineData("public class Base { public int Amount { get; set; } } public class Money : Base, IValue {}")]
    [InlineData("public class Base : IValue { public int Amount { get; set; } } public class Money : Base {}")]
    public async Task Checks_inherited_state_once(string source)
    {
        var diagnostics = await Analyze(source);
        Assert.Single(diagnostics.Where(item => item.Id == "DBA1018"));
    }

    [Fact]
    public async Task Metadata_inherited_members_report_at_the_consumer_declaration()
    {
        var reference = AnalyzerTestHelper.ReferenceAssembly("public class ForeignBase { public int State { get; set; } }", "Foreign");
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ValueObjectAnalyzer(),
            Prefix + "public class Money : ForeignBase, IValue {}", Configuration, references: new[] { reference });
        var diagnostic = Assert.Single(diagnostics.Where(item => item.Id == "DBA1018"));
        Assert.Equal("Test.cs", diagnostic.Location.SourceTree!.FilePath);
        Assert.Contains("ForeignBase.State", diagnostic.GetMessage());
    }

    [Theory]
    [InlineData("public record Money(object Value) : IValue;")]
    [InlineData("public record Money(dynamic Value) : IValue;")]
    [InlineData("public record Money<T>(T Value) : IValue;")]
    [InlineData("public class Node { public Node Next { get; init; } } public record Money(Node Value) : IValue;")]
    [InlineData("public record Money(System.Threading.Tasks.Task<Account> Value) : IValue;")]
    public async Task Unknown_or_recursive_state_is_not_presented_as_deep_immutability(string source)
    {
        var diagnostics = await Analyze(source);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "DBA0002" && diagnostic.Properties["status"] == "not-evaluated");
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "DBA1019");
    }

    [Fact]
    public async Task Does_not_assume_phantom_generic_argument_is_stored_but_follows_real_source_wrapper()
    {
        var diagnostics = await Analyze("""
            public class Phantom<T> {}
            public record Holder<T>(T Value);
            public record Money(Phantom<Account> Phantom, Holder<Account> Real) : IValue;
            """);
        var diagnostic = Assert.Single(diagnostics.Where(item => item.Id == "DBA1019"));
        Assert.Contains("Real.Value", diagnostic.GetMessage());
        Assert.DoesNotContain("Phantom", diagnostic.GetMessage());
    }

    [Fact]
    public async Task Same_short_names_and_legacy_default_do_not_enable_new_rules()
    {
        const string source = "namespace Other { public interface IValueObject {} public class Money : IValueObject { public int Amount {get;set;} } }";
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new ValueObjectAnalyzer(), source,
            "<ArchPrecept version=\"1\" profiles=\"ddd\" />"));
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new ValueObjectAnalyzer(), Prefix + "class Money : IValue { public int Amount {get;set;} }"));
    }

    [Fact]
    public async Task Generated_declaration_is_excluded()
    {
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new ValueObjectAnalyzer(), Prefix + "class Money : IValue { public int Amount {get;set;} }",
            Configuration, path: "Model.g.cs"));
    }

    private static Task<System.Collections.Immutable.ImmutableArray<Diagnostic>> Analyze(string source) =>
        AnalyzerTestHelper.GetDiagnosticsAsync(new ValueObjectAnalyzer(), Prefix + Environment.NewLine + source, Configuration);
}
