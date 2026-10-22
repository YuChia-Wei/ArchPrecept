using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ArchPrecept.Analyzers.Core.Tests;

internal static class AnalyzerTestHelper
{
    internal static readonly ImmutableArray<MetadataReference> PlatformReferences =
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToImmutableArray();

    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(DiagnosticAnalyzer analyzer, string source,
        string? repositoryConfig = null, string? projectConfig = null, IEnumerable<MetadataReference>? references = null,
        string path = "Test.cs", ImmutableDictionary<string, ReportDiagnostic>? severities = null, bool verifyCompilation = true)
    {
        var compilation = CreateCompilation(source, references, path, severities);
        if (verifyCompilation)
        {
            var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
            Xunit.Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
        }
        var options = Options(repositoryConfig, projectConfig);
        var diagnostics = await compilation.WithAnalyzers(ImmutableArray.Create(analyzer), options).GetAnalyzerDiagnosticsAsync();
        Xunit.Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == "AD0001");
        return diagnostics;
    }

    internal static CSharpCompilation CreateCompilation(string source, IEnumerable<MetadataReference>? references = null,
        string path = "Test.cs", ImmutableDictionary<string, ReportDiagnostic>? severities = null, string assemblyName = "AnalyzerTests") =>
        CSharpCompilation.Create(assemblyName, new[] { CSharpSyntaxTree.ParseText(source, path: path) },
            PlatformReferences.AddRange(references ?? Array.Empty<MetadataReference>()),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithSpecificDiagnosticOptions(severities));

    internal static AnalyzerOptions Options(string? repository, string? project = null)
    {
        var files = ImmutableArray.CreateBuilder<AdditionalText>();
        if (repository is not null) files.Add(new TextFile("archprecept.repository.xml", repository));
        if (project is not null) files.Add(new TextFile("archprecept.project.xml", project));
        return new AnalyzerOptions(files.ToImmutable());
    }

    internal static MetadataReference ReferenceAssembly(string source, string assemblyName)
    {
        using var stream = new MemoryStream();
        var result = CreateCompilation(source, assemblyName: assemblyName).Emit(stream);
        Xunit.Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    internal sealed class TextFile(string path, string text) : AdditionalText
    {
        public override string Path => path;
        public override Microsoft.CodeAnalysis.Text.SourceText GetText(CancellationToken cancellationToken = default) =>
            Microsoft.CodeAnalysis.Text.SourceText.From(text);
    }
}
