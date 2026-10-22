# ArchPrecept Analyzers Core

此 NuGet 套件提供編譯期架構規則，不包含 Contracts runtime、AI CLI 或 Framework。一般 `dotnet build` 可執行。

- [安裝與最小驗證入口](../../../docs/getting-started.md)
- [Diagnostic 手冊與 HelpLink](../../../docs/diagnostics/README.md)
- [Profile、角色映射、優先序與評估界線](../../../docs/product/analyzer-policy.md)
- [唯一 Rule Catalog](../../../rules/catalog.xml)
- [Products Web API 套件 sample](../../../samples/Products/README.md)

0.0.3-preview 保留 DBA1001–DBA1017 的 legacy 預設；新增 DBA1018／DBA1019 為 ddd opt-in Warning。DBA0001 表示無效設定，DBA0002 提供明確宣告範圍與部分未評估狀態。每項規則的實際 Profile／角色／severity 由 Catalog 提供，Descriptor 直接讀取同一份資料。

```xml
<PackageReference Include="ArchPrecept.Analyzers.Core" Version="[0.0.3-preview]" PrivateAssets="all" />
```

需要精確自有契約時，用 AdditionalFiles 的 repository／project XML 設定完整 metadata 與 assembly；不需要安裝產品 Contracts。命名慣例仍可選用，但診斷會標示低信心。規則沒有聲稱覆蓋未辨識的全部型別；用 Expected 取得特定範圍的狀態。

原始碼維護可執行 Analyzer 測試，發布驗收使用 `eng/Build-Packages.ps1` 與 `eng/Test-PackageConsumer.ps1`，對 SDK 8／10 的真實 nupkg 驗證。原始碼內含式 ProjectReference 範例仍保留在 templates，但不代替套件驗收。

```powershell
dotnet test tests/Analyzers/ArchPrecept.Analyzers.Core.Tests/ArchPrecept.Analyzers.Core.Tests.csproj
```

此專案為可封裝的六個產品專案之一；Roslyn 與第三方 DLL 不會隨 Analyzer nupkg 內嵌。發布狀態以 [版本紀錄](../../../docs/releases/0.0.3-preview.md) 為準。標準 IDE style preferences 與架構 Diagnostic 的 CI 接線是不同責任，單純安裝套件不會替整個 repository 建立完整 gate。
