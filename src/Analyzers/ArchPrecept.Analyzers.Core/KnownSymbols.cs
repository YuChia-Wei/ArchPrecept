using System.Linq;
using Microsoft.CodeAnalysis;

namespace ArchPrecept.Analyzers.Core;

/// <summary>以編譯參考中的 metadata 與組件身分辨識平台 API，排除來源中的同名替身。</summary>
internal static class KnownSymbols
{
    /// <summary>比對 BCL 的泛型原始定義；不接受 consumer 自行宣告的 System 同名型別。</summary>
    internal static bool IsBcl(ITypeSymbol? type, Compilation compilation, string metadataName)
    {
        if (type is null || type.TypeKind == TypeKind.Error) return false;
        return compilation.SourceModule.ReferencedAssemblySymbols
            .Where(assembly => assembly.Identity.Name is "System.Private.CoreLib" or "mscorlib"
                or "System.Runtime" or "System.ComponentModel" or "netstandard")
            .Where(assembly => !assembly.Identity.PublicKeyToken.IsEmpty)
            .Any(assembly => SymbolEqualityComparer.Default.Equals(type.OriginalDefinition,
                assembly.GetTypeByMetadataName(metadataName)));
    }

    /// <summary>比對指定平台組件的 metadata 定義；組件身分是分類依據而非安全防偽機制。</summary>
    internal static bool IsPlatform(ITypeSymbol? type, Compilation compilation, string metadataName, string assemblyName)
    {
        if (type is null || type.TypeKind == TypeKind.Error) return false;
        var candidates = compilation.SourceModule.ReferencedAssemblySymbols
            .Where(assembly => assembly.Identity.Name == assemblyName)
            .Select(assembly => assembly.GetTypeByMetadataName(metadataName))
            .Where(symbol => symbol is not null).ToArray();
        return candidates.Length == 1 && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, candidates[0]);
    }
}
