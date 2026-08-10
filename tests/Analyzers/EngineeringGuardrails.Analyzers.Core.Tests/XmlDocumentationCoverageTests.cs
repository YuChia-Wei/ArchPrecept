using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace EngineeringGuardrails.Analyzers.Core.Tests;

public sealed class XmlDocumentationCoverageTests
{
    [Fact]
    public void Interfaces_and_methods_in_product_source_have_chinese_summaries()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceRoot = Path.Combine(repositoryRoot, "src");
        var missingDocumentation = new List<string>();

        foreach (var filePath in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(IsProductSourceFile))
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
            var root = syntaxTree.GetRoot();

            var documentedNodes = root.DescendantNodes().Where(node =>
                node is InterfaceDeclarationSyntax
                    or MethodDeclarationSyntax
                    or ConstructorDeclarationSyntax
                    or OperatorDeclarationSyntax
                    or ConversionOperatorDeclarationSyntax);

            foreach (var node in documentedNodes)
            {
                var summary = GetSummary(node);
                if (!ContainsChineseText(summary))
                {
                    var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    missingDocumentation.Add($"{Path.GetRelativePath(repositoryRoot, filePath)}:{line} {Describe(node)}");
                }
            }
        }

        Assert.True(
            missingDocumentation.Count == 0,
            "下列產品介面或方法缺少含中文內容的 XML <summary>：" + Environment.NewLine
            + string.Join(Environment.NewLine, missingDocumentation));
    }

    private static string FindRepositoryRoot()
    {
        foreach (var startPath in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            for (var directory = new DirectoryInfo(startPath); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "EngineeringGuardrails.slnx")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new DirectoryNotFoundException("找不到 EngineeringGuardrails.slnx，因此無法判定產品原始碼根目錄。");
    }

    private static bool IsProductSourceFile(string filePath)
    {
        var segments = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return !segments.Contains("bin", StringComparer.OrdinalIgnoreCase)
            && !segments.Contains("obj", StringComparer.OrdinalIgnoreCase);
    }

    private static string? GetSummary(SyntaxNode node)
    {
        var documentation = node.GetLeadingTrivia()
            .Select(trivia => trivia.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .LastOrDefault();
        var summary = documentation?.Content
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(element => element.StartTag.Name.LocalName.ValueText == "summary");

        return summary?.Content.ToFullString();
    }

    private static bool ContainsChineseText(string? summary) =>
        !string.IsNullOrWhiteSpace(summary)
        && summary.Any(character => character is >= '\u4e00' and <= '\u9fff');

    private static string Describe(SyntaxNode node) => node switch
    {
        InterfaceDeclarationSyntax declaration => $"介面 {declaration.Identifier.ValueText}",
        MethodDeclarationSyntax declaration => $"方法 {declaration.Identifier.ValueText}",
        ConstructorDeclarationSyntax declaration => $"建構子 {declaration.Identifier.ValueText}",
        OperatorDeclarationSyntax declaration => $"運算子 {declaration.OperatorToken.ValueText}",
        ConversionOperatorDeclarationSyntax declaration => $"轉型運算子 {declaration.ImplicitOrExplicitKeyword.ValueText}",
        _ => node.Kind().ToString()
    };
}
