# Engineering Guardrails Analyzers Core

本專案的 Roslyn 架構分析器，係自 .NET backend AI Context framework 複製而來，作為此 Repository 的初始原始碼基線。

## 目前的 Diagnostic

- [`DBA1001`](../../../docs/diagnostics/DBA1001.md)（`error`）：驗證 Aggregate／Query Repository 的語意邊界、相容性繼承、Aggregate Root 限制、可攜式雙方法 Aggregate 合約，並禁止通用泛型可寫入 CRUD Port。
- [`DBA1002`](../../../docs/diagnostics/DBA1002.md)（`error`）：Use Case 或 Handler 不得注入 `IServiceProvider`。
- [`DBA1003`](../../../docs/diagnostics/DBA1003.md)（`warning`）：Aggregate／Entity 不應參考如 `DbContext` 等基礎設施型別。
- [`DBA1004`](../../../docs/diagnostics/DBA1004.md)（`warning`）：具體 Controller 類別應宣告 `ApiControllerAttribute`。
- [`DBA1005`](../../../docs/diagnostics/DBA1005.md)（`error`）：Controller 不得參考 `DbContext` 或呼叫 `SaveChanges`。
- [`DBA1006`](../../../docs/diagnostics/DBA1006.md)（`error`）：Controller 不得直接建立 Handler 或 Use Case 型別。
- [`DBA1007`](../../../docs/diagnostics/DBA1007.md)（`warning`）：物件 Mapper 應為靜態類別。
- [`DBA1008`](../../../docs/diagnostics/DBA1008.md)（`warning`）：物件 Mapper 不應相依 Repository、Use Case 或 Handler。
- [`DBA1009`](../../../docs/diagnostics/DBA1009.md)（`warning`）：事件溯源 Aggregate 的狀態只應在 `When` transition 中變更。
- [`DBA1010`](../../../docs/diagnostics/DBA1010.md)（`error`）：Use Case 不得使用 Service Locator 或 attribute-based 屬性注入。
- [`DBA1011`](../../../docs/diagnostics/DBA1011.md)（`error`）：Handler 不得混合處理 Command 與 Query marker 職責。
- [`DBA1012`](../../../docs/diagnostics/DBA1012.md)（`error`）：Use Case 不得直接建立 Repository。
- [`DBA1013`](../../../docs/diagnostics/DBA1013.md)（`warning`）：Projection 服務不應呼叫 EF 持久化寫入操作。
- [`DBA1014`](../../../docs/diagnostics/DBA1014.md)（`error`）：Controller 不得注入具體 Handler、訊息匯流排、調度器、寫入 Repository 或 Domain Service；Query Repository／Service 僅保留給已明確選定的純查詢端點。
- [`DBA1015`](../../../docs/diagnostics/DBA1015.md)（`error`）：Use Case 合約與實作必須使用 `ExecuteAsync`、非選用的 `CancellationToken` 與傳輸無關輸入；Use Case 也不得同時公開 `Handle`／`HandleAsync`。
- [`DBA1016`](../../../docs/diagnostics/DBA1016.md)（`error`）：Use Case 不得注入 `IMessageBus`、中介器／調度器或另一個 Use Case。
- [`DBA1017`](../../../docs/diagnostics/DBA1017.md)（`error`）：`*CommandHandler` 與 `*QueryHandler` 型別必須調適至恰好一個 Use Case，且不得相依 Repository 或 Domain Service。

完整的逐項手冊、觸發條件、範例、修正與 suppression 指引，請見 [`docs/diagnostics/README.md`](../../../docs/diagnostics/README.md)。

`DBA1001` 取代原有以 grep 進行的 Repository 合規腳本。它透過介面繼承分類標準、相容與衍生 Port，而非廣泛比對 `Repository` 名稱；目標專用的批次 Port 則刻意不納入此可攜式分析器。

`DBA1004` 至 `DBA1006` 取代原有以 grep 進行的 Controller 合規腳本；`DBA1007` 與 `DBA1008` 取代原有 Mapper 合規腳本。`DBA1014` 至 `DBA1017` 實作已核准 Use Case inbound-port 與 Handler adapter 模型中可確定判定的部分。純查詢端點是否已被明確選定，仍屬目標 Repository 的證據，不會從協調邏輯品質自動推論。

分析器不取代供審查與架構技能使用的 AI 軟體工程推理脈絡。

## 執行測試

```bash
dotnet test tests/Analyzers/EngineeringGuardrails.Analyzers.Core.Tests/EngineeringGuardrails.Analyzers.Core.Tests.csproj
```

## 初始擷取狀態

本專案仍不可封裝（non-packable）。已核准的專案、預設組件與根命名空間識別均為 `EngineeringGuardrails.Analyzers.Core`；NuGet 套件識別、支援的編譯器矩陣與發佈合約仍未核准。

## 接線至目標 Repository

進行原始碼內含式評估時，請透過 `Directory.Build.props` 將此分析器專案接線至目標專案。

使用：

- `templates/Directory.Build.props.snippet`
- `templates/analyzer-severity.editorconfig`

該片段會以以下設定將分析器專案加入為 `ProjectReference`：

- `OutputItemType="Analyzer"`
- `ReferenceOutputAssembly="false"`
- `PrivateAssets="all"`

如此一來，目標專案可在 `dotnet build` 期間接收分析器 Diagnostic，而不會把分析器專案當作執行階段組件參考。

嚴重性範本讓每個架構 Diagnostic 可獨立設定。目標 Repository 可依架構 Profile 與團隊共識選用 `none`、`silent`、`suggestion`、`warning` 或 `error`。

自訂 DBA Diagnostic 會在分析器專案接線後的建置期間執行。標準 IDE 程式碼風格偏好可能還需要 `EnforceCodeStyleInBuild`、`dotnet format --verify-no-changes` 或等效的 CI 命令。只有目標建置將 warning 視為 error 時，warning 才會阻擋建置。
