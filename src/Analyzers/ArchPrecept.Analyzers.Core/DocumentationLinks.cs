using System.Linq;
using System.Reflection;

namespace ArchPrecept.Analyzers.Core;

/// <summary>使用 SDK 嵌入的來源 revision 定位規則文件，已封裝版本不隨 main 漂移。</summary>
internal static class DocumentationLinks
{
    internal static readonly string SourceRevision = GetSourceRevision();

    /// <summary>從標準 informational version 取得 Git commit；無來源資訊的開發組件明列未版本化。</summary>
    private static string GetSourceRevision()
    {
        var version = typeof(DocumentationLinks).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var revision = version?.Split('+').LastOrDefault();
        return revision is { Length: >= 40 } && revision.Take(40).All(character =>
            character is >= '0' and <= '9' or >= 'a' and <= 'f') ? revision.Substring(0, 40) : "unversioned";
    }

    /// <summary>產生固定來源路徑；正式候選驗證會拒絕缺少 Git revision 的成品。</summary>
    internal static string For(string document) =>
        "https://github.com/YuChia-Wei/ArchPrecept/blob/" + SourceRevision + "/" + document;
}
