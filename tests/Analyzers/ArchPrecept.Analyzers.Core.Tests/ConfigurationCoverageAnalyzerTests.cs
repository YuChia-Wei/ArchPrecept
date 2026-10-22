using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class ConfigurationCoverageAnalyzerTests
{
    [Theory]
    [InlineData("<ArchPrecept version=\"2\" />")]
    [InlineData("<ArchPrecept version=\"1\" profiles=\"typo\" />")]
    [InlineData("<ArchPrecept version=\"1\" namingConventions=\"maybe\" />")]
    [InlineData("<ArchPrecept version=\"1\" ignored=\"no\" />")]
    [InlineData("<ArchPrecept version=\"1\"><Rule id=\"DBA9999\" /></ArchPrecept>")]
    [InlineData("<ArchPrecept version=\"1\"><Rule id=\"DBA1001\" severity=\"fatal\" /></ArchPrecept>")]
    [InlineData("<ArchPrecept version=\"1\"><Role name=\"entity\" metadataName=\"Missing\" assembly=\"AnalyzerTests\" /></ArchPrecept>")]
    [InlineData("<ArchPrecept version=\"1\"><Role name=\"typo\" metadataName=\"Subject\" assembly=\"AnalyzerTests\" /></ArchPrecept>")]
    [InlineData("<ArchPrecept version=\"1\"><Convention role=\"entity\" /></ArchPrecept>")]
    [InlineData("<ArchPrecept version=\"1\"><Rule id=\"DBA1001\"/><Rule id=\"DBA1001\"/></ArchPrecept>")]
    [InlineData("<!DOCTYPE x [<!ENTITY a SYSTEM 'file:///not-read'>]><ArchPrecept version=\"1\">&a;</ArchPrecept>")]
    public async Task Invalid_configuration_is_an_actionable_error(string config)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ConfigurationCoverageAnalyzer(), "class Subject {}", config);
        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, diagnostic => { Assert.Equal("DBA0001", diagnostic.Id); Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity); });
    }

    [Fact]
    public async Task Duplicate_configuration_files_fail_instead_of_using_incidental_order()
    {
        var compilation = AnalyzerTestHelper.CreateCompilation("class Subject {}");
        var options = new AnalyzerOptions(ImmutableArray.Create<AdditionalText>(
            new AnalyzerTestHelper.TextFile("a/archprecept.repository.xml", "<ArchPrecept version=\"1\" />"),
            new AnalyzerTestHelper.TextFile("b/archprecept.repository.xml", "<ArchPrecept version=\"1\" />")));
        var diagnostics = await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new ConfigurationCoverageAnalyzer()), options).GetAnalyzerDiagnosticsAsync();
        Assert.Equal("DBA0001", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task Project_overrides_repository_then_rule_then_compiler_severity()
    {
        const string source = "class SampleMapper {}";
        const string repository = "<ArchPrecept version=\"1\" profiles=\"framework\"><Rule id=\"DBA1007\" severity=\"error\" /></ArchPrecept>";
        const string project = "<ArchPrecept version=\"1\" profiles=\"core\"><Rule id=\"DBA1007\" enabled=\"true\" severity=\"warning\" /></ArchPrecept>";
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new MapperComplianceAnalyzer(), source, repository, project);
        Assert.Equal(DiagnosticSeverity.Warning, Assert.Single(diagnostics).Severity);
        diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new MapperComplianceAnalyzer(), source, repository, project,
            severities: ImmutableDictionary<string, ReportDiagnostic>.Empty.Add("DBA1007", ReportDiagnostic.Error));
        Assert.Equal(DiagnosticSeverity.Error, Assert.Single(diagnostics).Severity);
        diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new MapperComplianceAnalyzer(), source, repository,
            "<ArchPrecept version=\"1\"><Rule id=\"DBA1007\" enabled=\"false\" /></ArchPrecept>");
        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("core", 0)]
    [InlineData("framework", 1)]
    [InlineData("legacy", 1)]
    public async Task Convention_rule_requires_its_selected_profile(string profile, int expected)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new MapperComplianceAnalyzer(), "class SampleMapper {}",
            $"<ArchPrecept version=\"1\" profiles=\"{profile}\" />");
        Assert.Equal(expected, diagnostics.Length);
    }

    [Theory]
    [InlineData("class SampleMapper {}", "SampleMapper", "framework", "Test.cs", "applicable")]
    [InlineData("class SampleMapper {}", "SampleMapper", "core", "Test.cs", "disabled")]
    [InlineData("class SampleMapper {}", "SampleMapper", "framework", "Test.g.cs", "not-evaluated")]
    [InlineData("class SampleMapper {}", "Missing", "framework", "Test.cs", "not-evaluated")]
    [InlineData("class Helper {}", "Helper", "framework", "Test.cs", "not-evaluated")]
    public async Task Declared_scope_distinguishes_missing_disabled_generated_and_applicable(string source, string type, string profile, string path, string state)
    {
        var diagnostics = await AnalyzerTestHelper.GetDiagnosticsAsync(new ConfigurationCoverageAnalyzer(), source,
            $"<ArchPrecept version=\"1\" profiles=\"{profile}\"><Expected type=\"{type}\" role=\"mapper\" /></ArchPrecept>", path: path);
        var diagnostic = diagnostics.First(item => item.Properties["diagnosticId"] is "DBA1007" or "");
        Assert.Equal(state, diagnostic.Properties["status"]);
        Assert.Equal("DBA0002", diagnostic.Id);
    }

    [Fact]
    public async Task Helpers_without_declared_scope_are_not_coverage_gaps()
    {
        Assert.Empty(await AnalyzerTestHelper.GetDiagnosticsAsync(new ConfigurationCoverageAnalyzer(), "class Helper {}"));
    }
}
