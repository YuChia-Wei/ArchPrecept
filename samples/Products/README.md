# Products 套件範例

建立及讀取產品的一個小流程。HTTP Controller 將 request 轉為 Input，UseCase 建立 Product 並呼叫 Repository port；Domain 要求名稱不可空白、價格不可為負。`ProductPrice` 是不可變 VO。`InMemoryProductRepository` 只提供程序內儲存，重啟會遺失資料，沒有交易、持久化、跨程序一致性、Outbox 或 broker 保證。

API 實際引用 **0.0.3-preview NuGet**（Contracts.Application、其傳遞的 Domain、Analyzers.Core）；沒有產品 ProjectReference，也不需要 ES、Messaging、AI CLI 或 Framework。測試的 ProjectReference 僅引用這個非發布 sample API。

## 還原與執行

需要 .NET SDK 8 或 10。先依 [套件安裝入口](../../docs/getting-started.md) 設定 GitHub Packages feed 與本機憑證；或使用本機 `eng/Build-Packages.ps1` 實際產生的 nupkg feed。以 repository 根目錄為工作目錄：

```powershell
dotnet restore samples/Products/Tests/Products.Tests.csproj --source https://nuget.pkg.github.com/YuChia-Wei/index.json --source https://api.nuget.org/v3/index.json
dotnet test samples/Products/Tests/Products.Tests.csproj --no-restore
dotnet run --project samples/Products/Api/Products.Api.csproj --no-restore -- --urls http://127.0.0.1:5080
```

本機臨時 build／cache 請依開發環境約定導向暫存區；維護者在 Windows 使用 F: 專用目錄。既有 `eng/Test-PackageConsumer.ps1 -ScratchRoot F:\<task>` 自動隔離 feed/cache/output 並執行本範例。SDK 10 可加 `-p:SampleTargetFramework=net10.0`，預設 net8.0。

另一個終端送出成功與失敗請求：

```powershell
$created = Invoke-RestMethod -Method Post http://127.0.0.1:5080/products -ContentType application/json -Body '{"name":"Notebook","price":12.5}'
Invoke-RestMethod "http://127.0.0.1:5080/products/$($created.id)"
Invoke-WebRequest -Method Post http://127.0.0.1:5080/products -ContentType application/json -Body '{"name":"Notebook","price":-1}' -SkipHttpErrorCheck
```

建立回傳 201 與 Location；讀取存在項目回傳 200，未知 ID 回傳 404，違反 invariant 回傳 400。HTTP 測試在 loopback 隨機 port 啟動 Kestrel，另測試 UseCase 失敗時不呼叫儲存；不需 DB 或其他外部服務。

## 看見診斷並恢復

正常 sample 可建置。`violations/BadPrice.cs.txt` 不參與正常編譯；只在可丟棄的 sample 複本中複製為 `Api/Violation.cs`，建置會出現 DBA1018（setter）與 DBA1019（Product Entity 參考）。IDE HelpLink 連到各規則說明；移除該測試檔案後重新建置應通過。既有 consumer 腳本會完整執行這個失敗／修正流程。

`Api/archprecept.project.xml` 明確選 core、web-api、framework、ddd，VO 規則在本 sample 提升為 error。命名慣例有開啟，因此 UseCase 等角色的證據可能是低信心；真正 Product／VO／Repository 契約由完整身分辨識。無法從零診斷推論整個應用架構已合格。

## 來源與維護

依 #9 從自有 [Lab 的固定 commit 7a0bff401bea86428b5c24eab6362ef2170d20d7](https://github.com/YuChia-Wei/dotnet-distributed-architecture-lab/tree/7a0bff401bea86428b5c24eab6362ef2170d20d7) 的 Products 流程簡化重寫：

- `src/Product/Presentation/SaleProducts.WebApi/Controllers/ProductsController.cs`
- `src/Product/DomainCore/SaleProducts.Applications/UseCases/CreateProductUseCase.cs`
- `src/Product/DomainCore/SaleProducts.Domains/Product.cs`

省去更新／刪除、Description、domain events、ORM、驗證擴充與外部儲存；改用本產品精確套件契約與記憶體 adapter，不宣稱逐檔原樣匯入。來源 ownership 沿用 [已接受的自有來源聲明](../../docs/releases/licensing-assessment.md#負責人來源補充已完成)，新 sample 在本 repository 依 Apache-2.0 維護。沒有修改 Lab，也不把 sample／test 加入六套件發布 inventory。
