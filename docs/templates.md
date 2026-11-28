# 建立 starter 與新增 Domain、Application 程式碼

`ArchPrecept.Templates` 是 .NET SDK 範本套件。準備能建置目標 TFM 的 SDK（.NET 8 或 10），並設定可讀取所選版本的 NuGet source。GitHub Packages 的來源與憑證設定見[Preview 安裝步驟](releases/0.0.6-preview.md#專案試裝)，候選版上架狀態見[0.1.0-preview.1](releases/0.1.0-preview.1.md)。

`ap-starter` 會產生專案，其他 item 範本可加入既有專案。starter 會精確引用與已安裝範本同版的 Analyzer，以及所選的 Contracts。以下命令適用於 .NET SDK 8／10 與對應的 `net8.0`／`net10.0`。

```sh
dotnet new install ArchPrecept.Templates::0.1.0-preview.1
dotnet new list ap-
dotnet new ap-starter --help
dotnet new ap-vo --help
```

使用本機 feed 時，以成品的完整路徑執行 `dotnet new install <path-to-ArchPrecept.Templates.0.1.0-preview.1.nupkg>`，並將同一次 build 的套件目錄加入 consumer 的 NuGet 來源。使用 `dotnet new list ap-` 查詢 short name，使用 `dotnet new <short-name> --help` 查參數。安裝不同版本只影響之後的生成。

starter 原始檔中的套件版號是佔位符。CI 封裝時會將發布 tag 的版號寫入套件內的 `SampleShop.csproj`。要驗證該版本，請安裝該次建置的 `.nupkg`。直接安裝原始目錄會略過封裝，產生的專案會保留佔位符。

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

## 在既有專案加入一個型別

以下命令從 `Shop` 的專案根目錄執行，每個 item 都可以先加 `--dry-run`。提供 C# 識別字、有效 namespace，以及已存在型別的 C# 名稱。各命令只在指定輸出目錄寫入其產物。透過手動合併保留現有 csproj、Analyzer 設定及業務程式碼。

| 生成命令 | 產物及需要完成的工作 |
|---|---|
| `dotnet new ap-aggregate -n Order --namespace Shop.Domain -o Domain` | `Order(Guid id)` 把呼叫方提供的 identity 傳給 AggregateRoot。可用 `--id-type` 指定另一型別。請先決定空值、零值或空 Guid 在業務上的意義，並實作對應建構檢查及行為。 |
| `dotnet new ap-domain-event -n OrderPlaced --namespace Shop.Domain -o Domain` | 產生帶 `Guid EventId` 與 `DateTime OccurredOn` 的領域事實。呼叫端提供事件 ID 與時間，再加入所需的業務資料。通知發布、broker、儲存與交易各由對應 adapter 負責。 |
| `dotnet new ap-repository -n IOrders --namespace Shop.Application --aggregate Shop.Domain.Order -o Application` | 透過 `IAggregateRepository<Order, Guid>` 繼承 FindByIdAsync／SaveAsync。在 consumer 加入同版 `ArchPrecept.Contracts.Application` 套件；若使用其他 ID，傳入與 Order 相同的 `--id-type`。 |
| `dotnet new ap-query-port -n IFindOrders --namespace Shop.Application --model Shop.Application.CreateProductOutput -o Application` | 回傳指定的既有唯讀 model 或 null。示例重用 starter 中的輸出 record；實際專案應傳入該查詢自己的 read model。此 port 亦需 Application 契約。 |
| `dotnet new ap-usecase -n PlaceOrder --namespace Shop.Application --base IUseCase --port IOrders -o Application` | 產生 PlaceOrderInput／Output、IPlaceOrderUseCase 與實作，共用 starter 已映射的泛型 IUseCase 契約，並以建構子注入選定的既有 port。省略 `--port` 即產生無相依的骨架。實作在填入業務行為前會擲回 NotImplementedException。 |
| `dotnet new ap-mapper -n OrderMapper --namespace Shop.Application --source-type PlaceOrderInput --target-type PlaceOrderOutput -o Application` | static Map 方法引用既有型別。請完成欄位轉換並保留無 I/O 的特性；static 只描述呼叫形式。 |
| `dotnet new ap-handler -n PlaceOrder --namespace Shop.Dispatch --application Shop.Application -o Dispatch` | 接收傳輸側 Command，轉換成 Application Input 並呼叫單一 UseCase。此獨立項目無 messaging host 依賴，註冊、傳遞及錯誤處理由選定的 host 負責。 |
| `dotnet new ap-controller -n PlaceOrder --namespace Shop.Http --application Shop.Application -o Http` | 僅在已有 ASP.NET Core framework reference 的專案選用。產生帶 ApiController 的 POST 入口，並將 request／response 與 Application 輸入／輸出區隔。在 host 註冊 Controller、UseCase 與 port。 |

已有自有雙泛型 UseCase 契約時，傳入 `--base` 與開放泛型名稱。尚無共用基底時可省略該選項，將生成的單一介面映射為 use-case。每個設定檔的同一角色只可指定一個 mapping；多個 UseCase 應共用已映射的基底介面。UseCase 的最小示例以一個 string Value 作為 input 和 output 欄位。已有型別時，為 `ap-usecase` 傳入 `--input-type Shop.Application.ExistingInput --output-type Shop.Application.ExistingOutput`，只生成引用這些型別的契約與實作。薄入口預設沿用 `I<Name>UseCase`、`<Name>Input(string Value)` 與 `<Name>Output.Value` 形狀；用自訂輸入／輸出時依選定契約修改轉換。只生成 HTTP、dispatch 或兩者之一均可。

自有契約使用者在上述命令指定既有基底。Aggregate 的 `--base` 接受開放泛型類別名稱，其建構子需接受該 ID。事件的 `--contract` 接受具備相同 EventId／OccurredOn getters 的介面。兩種 port 的 `--base` 分別接受有 FindByIdAsync／SaveAsync 的雙泛型契約及 query marker。使用完整型別名稱，例如 `--base Shop.Domain.IdentityRoot`，不在參數內加泛型引數。這些 item 不會新增套件或修改 mapping。

在列入 AdditionalFiles 的 `archprecept.project.xml` 內補入實際角色。例如 starter 已選用 core、framework、ddd 並關閉命名推測，可將下列節點加入現有 ArchPrecept 根元素。若把 HTTP 放在獨立專案，請按該專案實際的 assembly 名稱設定。自有 Aggregate 基底及寫入／查詢契約也要以實際 metadata name（例如 ``Shop.Domain.IdentityRoot`1``）與 assembly 映射為 `aggregate`、`aggregate-repository`、`query-repository`。於開發情境新增 Expected 可查看哪些型別確實已被辨識。見[角色設定及診斷](product/analyzer-policy.md)。

```xml
<!-- starter 已將 Shop.Application.IUseCase 的雙泛型契約映射為 use-case。 -->
<Role name="mapper" metadataName="Shop.Application.OrderMapper" assembly="Shop" />
<Role name="dispatch-handler" metadataName="Shop.Dispatch.PlaceOrderHandler" assembly="Shop" />
<Expected type="Shop.Application.PlaceOrderUseCase" role="use-case" />
<Expected type="Shop.Application.IOrders" role="aggregate-repository" />
```

跨組件的實際引用方向可另用[組件相依測試](architecture-testing.md)檢查。

## 在既有介面加入 ExecuteAsync

在已安裝當版範本的 SDK 上，人或 AI 可以將宣告輸出到獨立目錄，再插入既有 UseCase interface：

```sh
dotnet new ap-execute --input-type Shop.Application.PlaceOrderInput --output-type Shop.Application.PlaceOrderOutput -o snippets
```

`snippets/ExecuteAsync.txt` 包含有必要 CancellationToken 的單一宣告。使用完整 C# 型別名稱時無須額外 using。先確認介面缺少此操作，再將內容貼到該介面的大括號內、補齊實作、啟用 framework Profile、映射介面為 use-case 並建置。缺少必要參數會由 SDK 拒絕，既有 C# 檔案由使用者掌控。用 optional CancellationToken 會觸發 [DBA1015](diagnostics/DBA1015.md)；還原必要參數後重新建置。

同樣是需要一個 ExecuteAsync，依放置位置選擇入口：

| 入口 | 操作、成本及首版選擇 |
|---|---|
| 完整 `ap-usecase` item | 建立新型別時，一次產生 interface、implementation 與選定的 Input／Output。已有介面時，由開發者手動整合，以保留周圍方法。 |
| `ap-execute` 文字片段 | 已有介面時，提供既有型別名稱，將產生的普通 C# 貼入正確位置。與完整 item 使用同一 SDK、套件及版本；人和 AI 均可操作。 |
| VS Code 原生 snippet | 可以在游標處填入型別；需另維護編輯器格式及驗證展開。待團隊在 VS Code 反覆新增這個方法且需要逐欄跳轉，再依實際版本驗收。 |
| Rider project／file／live templates | 建立整個專案或檔案可先採用 SDK CLI。對於既有介面的游標操作，待 Rider 使用者明確需要快捷填值或團隊共用設定，再驗證指定版本的匯入、重載、展開與移除。 |

首版交付前兩種入口。Rider／VS Code 的互動展開與設定層仍屬未交付範圍。
