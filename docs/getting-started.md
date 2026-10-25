# 安裝與使用 ArchPrecept

ArchPrecept 的六個套件可依需要選用，執行檢核只需 .NET SDK 與 NuGet，無需 AI、API key 或 Framework。0.0.3-preview 支援 SDK 8／10；是否已發布及實際驗收見 [本版紀錄](releases/0.0.3-preview.md)。

| 套件 | 用途與必要相依 |
|---|---|
| ArchPrecept.Analyzers.Core | 編譯期檢查；無 Contracts runtime 相依 |
| ArchPrecept.Contracts.Domain | 選配的 Entity／Aggregate／VO／Domain Event 契約 |
| ArchPrecept.Contracts.Application | Repository、Result、程序內領域事件 port；依賴 Domain |
| ArchPrecept.Contracts.Domain.EventSourcing | 選配 ES 聚合生命週期；依賴 Domain，不包含 Event Store |
| ArchPrecept.Contracts.Messaging | 獨立訊息契約 |
| ArchPrecept.Contracts.Application.Messaging | Publisher port；依賴 Messaging，不包含 transport adapter |

## 1. 選 feed 與套件

發布來源是 GitHub Packages：`https://nuget.pkg.github.com/YuChia-Wei/index.json`。沿用已設定的 NuGet source／credential provider；本機需要有權限的 GitHub NuGet 憑證，Actions 可使用具存取權的 GITHUB_TOKEN。不要把 token 放進 repository。nuget.org 不承載這些 preview 套件。

在既有專案加入所需 PackageReference，例如僅使用 Analyzer：

```xml
<ItemGroup>
  <PackageReference Include="ArchPrecept.Analyzers.Core" Version="[0.0.3-preview]" PrivateAssets="all" />
</ItemGroup>
```

需要參考契約時另加 `ArchPrecept.Contracts.Application` 或所需能力；Analyzer 不要求你換掉自有介面，也不強制繼承產品基底。Contracts-only 可完全不安裝 Analyzer。

本機驗證可使用 `eng/Build-Packages.ps1` 的 nupkg 輸出作為 source；不要用產品 ProjectReference 代替套件安裝證據。

## 2. 明確選規則與角色

0.0.3-preview 不加設定時會採用 `legacy`，啟用 DBA1001–DBA1017 與命名慣例。需要控制規則及精確角色時，可建立 `archprecept.project.xml`：

```xml
<ArchPrecept version="1" profiles="core,ddd" namingConventions="false">
  <Role name="value-object" metadataName="My.Domain.IValue" assembly="My.Domain" />
  <Role name="entity" metadataName="My.Domain.IEntity`1" assembly="My.Domain" />
  <Expected type="My.Domain.Money" role="value-object" />
</ArchPrecept>
```

將檔案明確加入編譯：

```xml
<ItemGroup>
  <AdditionalFiles Include="archprecept.project.xml" />
</ItemGroup>
```

範例的 metadata／assembly 名稱必須改成你的實際契約；找不到會產生 DBA0001，不能照貼不存在的類別。跨專案可在 Directory.Build.props 明確加入一份 `archprecept.repository.xml`，各 Project 再加入自己的 override。Profile、role 名稱、Schema、合併順序、內建身分與例外都在 [設定契約](product/analyzer-policy.md)；可選 `core`、`web-api`、`messaging`、`event-sourcing`、`framework`、`ddd` 聯集。

`framework` 是規則慣例 Profile，不會安裝 AI Framework。VO 的 private setter 採嚴格不可變政策；兩條新 VO 規則只在選 ddd 或個別 enabled=true 時執行，預設 Warning。

## 3. 執行、讀懂並修正

```powershell
dotnet restore
dotnet build --no-restore -p:ErrorLog=diagnostics.sarif
dotnet test --no-restore
```

Error 會使 build 非零退出；Warning 預設不阻擋，除非採用端設定 warnings-as-errors。在 `.editorconfig` 可提高個別規則：

```ini
[*.cs]
dotnet_diagnostic.DBA1018.severity = error
```

標準 severity 最後套用，但不會啟用被 Profile 排除的規則。IDE HelpLink 或 [Diagnostic 索引](diagnostics/README.md) 說明修正方向；SARIF 可讀到 Rule ID、roleEvidence、confidence、status。命令列建置可能省略 DBA0002 的 Info（包含 SARIF）；需要範圍報告時設定 `dotnet_diagnostic.DBA0002.severity = warning`。applicable 僅代表適用，不是執行證明；not-evaluated、disabled 與不適用也不可視為通過。

採用端 CI 應將 CS8032／CS9057／AD0001 當作 Analyzer 載入／執行失敗，例如 csproj 設定 `<WarningsAsErrors>$(WarningsAsErrors);CS8032;CS9057;AD0001</WarningsAsErrors>`。NuGet 安裝本身不會替整個 repository 建立 CI gate，仍需 CI 實際執行受檢查的 project。

## 4. 從小範例開始

[Products Web API](../samples/Products/README.md) 使用實際套件、無外部服務的 adapter 與 HTTP／UseCase 測試，包含隔離的故意違規／修正步驟。[ES](product/event-sourcing-contract.md) 與 [Messaging](product/messaging-contract.md) 說明選配能力；發布 port／metadata overload 不等於交易、Outbox、可靠傳遞或完整執行環境。

## 5. 理解規則與版本範圍

[規則設計依據](product/rule-design-rationale.md) 說明每條規則保護的責任、採用限制及成本；[方法論來源](product/methodology-references.md) 區分作者原則與產品具體政策。先確認規則適用的角色與 Profile，再依 Diagnostic 的 predicate／限制判讀結果。自有契約 mapping 讓 Analyzer 辨識角色，不會自動免除該角色已選的契約限制。

例如 Aggregate Repository 的 criteria 查詢應由 query port 找候選 ID，再由命令端載入 Aggregate 並驗證目前狀態；純 Mapper 不負責查詢資料；UseCase 的 Input 不直接承載 HTTP 或 broker 型別。這些都是具體採用政策，其方法論支持與適用條件在各規則說明中列出。

首個不含 `-preview` 後綴的公開版本發布前不提供向後相容保證。安裝或更新時讀取該版本的使用與規則文件；目前文件中的 `legacy` 是已實作設定名稱，不是保留舊版本的承諾。詳見[版本與發布準備政策](releases/version-policy-proposal.md)。

### 安裝後的常見判讀

| 現象 | 檢查方式 |
|---|---|
| XML 放在專案中卻未生效 | 確認已加入 AdditionalFiles；沒有 DBA0001 不代表未傳入的檔案已被讀取 |
| 提高 severity 仍沒有規則執行 | 確認 Profile 包含該規則或 Rule enabled=true；severity 不啟用被排除的規則 |
| 自有型別沒有診斷 | 核對完整 metadata name、泛型 arity、assembly 與 Expected；不要將未辨識視為合規 |
| 結果與設計意圖不同 | 先讀 roleEvidence、predicate 與已知限制，確認誤判或不支援範圍；最小 suppression 只暫停檢查，不證明另一政策已被驗證 |
| API tooltip 或套件文件不足 | 以本版 docs 確認責任與限制；完整可讀的 XML summary 及實際隨包文件是正式版發布前要求，不以成功安裝替代文件驗收 |
