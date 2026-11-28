# Products 套件範例

建立及讀取產品的一個小流程。HTTP Controller 將 request 轉為 Input，UseCase 建立 Product 並呼叫 Repository port；Domain 要求名稱不可空白、價格不可為負。`ProductPrice` 是不可變 VO。`InMemoryProductRepository` 只提供程序內儲存，重啟會遺失資料，沒有交易、持久化、跨程序一致性、Outbox 或 broker 保證。

API 透過 NuGet 引用 Contracts.Application、其傳遞相依 Domain，以及 Analyzers.Core。下列命令用 `ArchPreceptVersion` 指定要驗證的版本；測試專案以 ProjectReference 引用範例 API。

## 還原與執行

準備 .NET SDK 8 或 10，從 repository 根目錄執行以下命令，即可從 NuGet.org 匿名還原 `0.1.0-preview.1`。要試裝本機套件時，請在 restore 命令加上 `--source <本機套件目錄>` 並保留 nuget.org 供外部相依使用。[0.1.0-preview.1 套件來源](../../docs/releases/0.1.0-preview.1.md#取得套件)

```powershell
$version = '0.1.0-preview.1'
dotnet restore samples/Products/Tests/Products.Tests.csproj -p:ArchPreceptVersion=$version --source https://api.nuget.org/v3/index.json
dotnet test samples/Products/Tests/Products.Tests.csproj -p:ArchPreceptVersion=$version --no-restore
dotnet run --project samples/Products/Api/Products.Api.csproj -p:ArchPreceptVersion=$version --no-restore -- --urls http://127.0.0.1:5080
```

`eng/Test-PackageConsumer.ps1` 可從指定的套件來源執行本範例。以 `-ScratchRoot` 指定暫存目錄，腳本會在其中建立獨立的還原快取與建置輸出。手動執行時，預設目標為 net8.0，使用 SDK 10 可加上 `-p:SampleTargetFramework=net10.0`。

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

此範例與測試僅隨原始碼提供，採用 [Apache-2.0 授權](../../LICENSE)。
