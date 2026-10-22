using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class SharedRoleIntegrationTests
{
    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { new AggregateInfrastructureDependencyAnalyzer(),
            "interface IModel {} class Store {} record Model : IModel { public Store Storage {get;init;} }",
            "core", "aggregate:IModel;db-context:Store", "DBA1003" };
        yield return new object[] { new ControllerComplianceAnalyzer(),
            "interface IEndpoint {} class Endpoint : IEndpoint {}", "web-api", "controller:IEndpoint", "DBA1004" };
        yield return new object[] { new MapperComplianceAnalyzer(),
            "interface ITransform {} class Convert : ITransform {}", "framework", "mapper:ITransform", "DBA1007" };
        yield return new object[] { new ProjectionReadOnlyAnalyzer(),
            "interface IReadSide {} class Store { public void SaveChanges(){} } class Reader : IReadSide { void Run(Store store) => store.SaveChanges(); }",
            "core", "projection:IReadSide;db-context:Store", "DBA1013" };
        yield return new object[] { new RepositoryQueryMethodAnalyzer(),
            "interface IReadPort {} interface IData : IReadPort { void Save(); }", "core", "query-repository:IReadPort", "DBA1001" };
        yield return new object[] { new UseCaseServiceProviderInjectionAnalyzer(),
            "interface IOperation {} class Checkout : IOperation { public Checkout(System.IServiceProvider provider){} }",
            "core", "use-case:IOperation", "DBA1002" };
        yield return new object[] { new EventSourcedAggregateMutationAnalyzer(),
            "class Root {} class Order : Root { int count; void Change(){count++;} }",
            "event-sourcing", "event-sourced-aggregate:Root", "DBA1009" };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Every_existing_analyzer_uses_the_same_explicit_mapping_entry(DiagnosticAnalyzer analyzer, string source,
        string profiles, string mappings, string id)
    {
        var mapping = string.Concat(mappings.Split(';').Select(item => item.Split(':')).Select(parts =>
            $"<Role name=\"{parts[0]}\" metadataName=\"{parts[1]}\" assembly=\"AnalyzerTests\" />"));
        var config = $"<ArchPrecept version=\"1\" profiles=\"{profiles}\" namingConventions=\"false\">{mapping}</ArchPrecept>";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(analyzer, source, config);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == id && diagnostic.Properties["roleEvidence"] == "configured-contract");
        var withoutMapping = $"<ArchPrecept version=\"1\" profiles=\"{profiles}\" namingConventions=\"false\" />";
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(analyzer, source, withoutMapping));
    }
}
