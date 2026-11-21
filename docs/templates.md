# 從空目錄建立 starter，再加入 Price

`ArchPrecept.Templates` 是 .NET SDK 範本套件。準備能建置目標 TFM 的 SDK（.NET 8 或 10），並設定可讀取當版 ArchPrecept 套件的 NuGet source。GitHub Packages 的驗證前提見[安裝指南](getting-started.md)，當版成品狀態見[發布紀錄](releases/0.0.5-preview.md)。

在一般開發目錄安裝至 SDK，列出兩個 short name 並檢視參數。使用已設定且可讀取該版本的來源；若使用本機成品，將 install 的套件規格換成 `ArchPrecept.Templates.0.0.5-preview.nupkg` 的完整路徑，並讓後續 restore 可讀到同版 Analyzer 與所選 Contracts。

```sh
dotnet new install ArchPrecept.Templates::0.0.5-preview
dotnet new list ap-
dotnet new ap-starter --help
dotnet new ap-vo --help
```

## 參考契約

以下命令建立一個使用 `ArchPrecept.Contracts.Domain.IValueObject` 的 console 專案，並將新的 `Price` 類別放入 Domain 資料夾。從專案根目錄執行 item 命令，先預覽，再實際寫入。

```sh
dotnet new ap-starter -n Shop --framework net8.0 --contracts reference -o Shop
cd Shop
dotnet new ap-vo -n Price --namespace Shop.Domain --contracts reference -o Domain --dry-run
dotnet new ap-vo -n Price --namespace Shop.Domain --contracts reference -o Domain
dotnet build
dotnet run --no-build -- Paper
```

## 自有契約

在另一個空目錄執行下列命令。此模式的 marker 由專案擁有；starter 會把實際型別與 assembly 映射到 value-object 角色，並透過 MSBuild 的 AdditionalFiles 載入設定。Analyzer 是開發相依，執行程式不需要 ArchPrecept.Contracts.*。

```sh
dotnet new ap-starter -n MyShop --framework net8.0 --contracts custom -o MyShop
cd MyShop
dotnet new ap-vo -n Price --namespace MyShop.Domain --contracts custom -o Domain
dotnet build
dotnet run --no-build -- Paper
```

兩例皆可將 TFM 換成 `net10.0`。有效商品名稱會顯示建立結果，空名稱會拒絕建立並以非零狀態結束。請依實際領域自行定義價格幣別及範圍。加入既有專案時，使用有效的 C# 型別名稱與 namespace；自有 marker 位於不同 namespace 時，傳入 `--marker` 和其完整 C# 名稱，確認專案已有該介面、相依與精確的 Analyzer mapping。

## 確認 DBA1018 確實執行

在任一模式下，將 `Domain/Price.cs` 的 `public decimal Value { get; }` 改成 `public decimal Value { get; set; }`，執行 `dotnet build`。starter 選用 ddd 並把 DBA1018 提升為 error，因此這次建置應指向 Price 的可變成員並失敗。移除 `set;` 後再次建置，DBA1018 應消失。未調整 severity 的既有專案可能只收到 warning。詳見 [DBA1018 的適用範圍](diagnostics/DBA1018.md)。

若同名檔案將被覆寫，先檢查衝突並調整名稱或位置。更新 SDK 已安裝的範本只影響後續生成。使用 `dotnet new uninstall ArchPrecept.Templates` 可移除範本並保留專案原始碼。
