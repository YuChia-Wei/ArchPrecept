using System.Xml.Linq;
using System.Xml.Schema;
using Xunit;

namespace ArchPrecept.Analyzers.Core.Tests;

public sealed class RuleCatalogTests
{
    [Fact]
    public void Catalog_schema_ids_descriptors_and_source_links_are_consistent()
    {
        var root = DiagnosticDocumentationCoverageTests.FindRepositoryRoot();
        var document = XDocument.Load(Path.Combine(root, "rules/catalog.xml"));
        var schemas = new XmlSchemaSet();
        schemas.Add(null, Path.Combine(root, "rules/catalog.xsd"));
        var errors = new List<string>();
        document.Validate(schemas, (_, args) => errors.Add(args.Message));
        Assert.Empty(errors);
        foreach (var rule in document.Root!.Elements("Rule"))
        {
            var id = (string)rule.Attribute("diagnostic")!;
            var descriptor = RuleCatalog.Descriptor(id);
            Assert.Equal((string)rule.Attribute("title")!, descriptor.Title.ToString());
            Assert.Equal((string)rule.Attribute("severity")!, descriptor.DefaultSeverity.ToString());
            Assert.All(RuleCatalog.GetProfiles(id), profile => Assert.Contains(profile, AnalyzerConfiguration.ProfileNames));
            Assert.All(RuleCatalog.GetRoles(id), role => Assert.Contains(role, AnalyzerConfiguration.Roles));
            foreach (var key in new[] { "document", "source", "implementation", "tests" })
                Assert.True(File.Exists(Path.Combine(root, (string)rule.Attribute(key)!)), $"{id} has broken {key} link");
        }
        Assert.Equal(RuleCatalog.Ids.Order(), document.Root.Elements("Rule").Select(rule => (string)rule.Attribute("diagnostic")!).Order());
    }

    [Fact]
    public void Schema_rejects_reused_identifiers_and_missing_metadata()
    {
        var root = DiagnosticDocumentationCoverageTests.FindRepositoryRoot();
        var schemas = new XmlSchemaSet();
        schemas.Add(null, Path.Combine(root, "rules/catalog.xsd"));
        var document = XDocument.Load(Path.Combine(root, "rules/catalog.xml"));
        document.Root!.Add(new XElement(document.Root.Elements("Rule").First()));
        document.Root.Elements("Rule").First().Attribute("title")!.Remove();
        var errors = new List<string>();
        document.Validate(schemas, (_, args) => errors.Add(args.Message));
        Assert.True(errors.Count >= 2);
    }
}
