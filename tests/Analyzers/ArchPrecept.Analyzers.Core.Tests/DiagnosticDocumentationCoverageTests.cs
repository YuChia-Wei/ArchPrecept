using System.Text.RegularExpressions;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class DiagnosticDocumentationCoverageTests
{
    private static readonly string[] RequiredSections =
    [
        "## 目的與預設嚴重性",
        "## 適用範圍與觸發條件",
        "## 違規範例",
        "## 合規範例",
        "## 修正指引",
        "## 已知限制與例外",
        "## 設定與 Suppression",
        "## 對應實作與測試"
    ];

    [Fact]
    public void Every_analyzer_diagnostic_has_a_complete_zh_tw_manual()
    {
        var repositoryRoot = FindRepositoryRoot();
        var descriptorPath = Path.Combine(
            repositoryRoot,
            "src",
            "Analyzers",
            "ArchPrecept.Analyzers.Core",
            "RuleDescriptors.cs");
        var diagnosticIds = Regex.Matches(File.ReadAllText(descriptorPath), "id:\\s*\\\"(DBA\\d{4})\\\"")
            .Select(match => match.Groups[1].Value)
            .Order()
            .ToArray();

        Assert.Equal(17, diagnosticIds.Length);
        Assert.Equal(diagnosticIds.Length, diagnosticIds.Distinct(StringComparer.Ordinal).Count());

        var documentationDirectory = Path.Combine(repositoryRoot, "docs", "diagnostics");
        var indexPath = Path.Combine(documentationDirectory, "README.md");
        Assert.True(File.Exists(indexPath), "找不到分析器 Diagnostic 說明書索引 docs/diagnostics/README.md。");

        var index = File.ReadAllText(indexPath);
        AssertMetadataField(index, "document_id", "DOC-DIAGNOSTICS");
        AssertMetadataField(index, "document_version", "0.2");
        AssertMetadataField(index, "status", "draft");
        AssertMetadataField(index, "lang", "zh-TW");
        AssertMetadataField(index, "canonical_path", "docs/diagnostics/README.md");

        var documentedIds = Directory.EnumerateFiles(documentationDirectory, "DBA*.md", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Order()
            .ToArray();
        Assert.Equal(diagnosticIds, documentedIds);

        foreach (var diagnosticId in diagnosticIds)
        {
            var documentPath = Path.Combine(documentationDirectory, $"{diagnosticId}.md");
            Assert.True(File.Exists(documentPath), $"找不到 {diagnosticId} 的說明書：{documentPath}");

            var document = File.ReadAllText(documentPath);
            AssertMetadataField(document, "diagnostic_id", diagnosticId);
            AssertMetadataField(document, "document_version", "0.2");
            AssertMetadataField(document, "status", "draft");
            AssertMetadataField(document, "lang", "zh-TW");
            AssertMetadataField(document, "canonical_path", $"docs/diagnostics/{diagnosticId}.md");

            foreach (var section in RequiredSections)
            {
                Assert.Contains(section, document, StringComparison.Ordinal);
            }

            Assert.Contains($"[{diagnosticId}]({diagnosticId}.md)", index, StringComparison.Ordinal);
        }
    }

    private static void AssertMetadataField(string document, string field, string value)
    {
        var pattern = $"(?m)^{Regex.Escape(field)}:\\s*\\\"?{Regex.Escape(value)}\\\"?\\s*$";
        Assert.Matches(pattern, document);
    }

    private static string FindRepositoryRoot()
    {
        foreach (var startPath in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            for (var directory = new DirectoryInfo(startPath); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ArchPrecept.slnx")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new DirectoryNotFoundException("找不到 ArchPrecept.slnx，因此無法判定說明書根目錄。");
    }
}
