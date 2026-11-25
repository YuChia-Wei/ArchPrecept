# ArchPrecept 套件使用手冊

ArchPrecept 在 C# 編譯期間檢查 Repository、Use Case、Controller 與 Value Object 等角色的架構規則。你可以在既有模型上安裝 Analyzer 並映射自有契約，也可以按需採用參考契約或用 `dotnet new` 建立範例專案。

**此手冊對應 `0.0.6-preview`。** 七個套件的試用來源為 GitHub Packages，consumer 支援 .NET 8 與 10。安裝前請核對[這一版的發布與驗收紀錄](releases/0.0.6-preview.md)。其他版本請選下方版本表的固定手冊。

## 開始使用

| 我想做的事 | 從這裡開始 |
|---|---|
| 第一次安裝，選擇專案需要的套件 | [安裝指南](getting-started.md) |
| 從空目錄建立可執行的 starter，再新增 `Price` | [Price 範例與範本命令](templates.md) |
| 測試 Domain 是否直接參考指定的 Infrastructure 組件 | [加入可自訂的 xUnit 邊界測試](architecture-testing.md) |
| 修正 Value Object 的可變狀態 | [DBA1018 修正說明](diagnostics/DBA1018.md#修正指引) |
| 保留自己的領域介面，只加入 Analyzer | [安裝 Analyzer 與設定 AdditionalFiles](getting-started.md)，再[映射契約型別並選擇 Profile](product/analyzer-policy.md) |
| 在 Domain 或 Application 採用參考契約 | [各套件的最小使用範例、相依與 API 說明](package-usage.md) |
| 重播 Aggregate 事件，或向外部發布訊息 | [Event Sourcing 的重播與提交責任](product/event-sourcing-contract.md) · [Messaging 的訊息契約與 publisher port](product/messaging-contract.md) |

還原 Contracts 套件後，IDE 可從 `lib/net8.0/` 中與 DLL 同名的 XML 顯示個別 API 說明。

## 查詢建置診斷

依編譯器或 IDE 顯示的 `DBA` 編號，在[診斷目錄](diagnostics/README.md)查觸發條件、範例、修正方式與已知限制。例如 `Price.Value` 因公開 setter 觸發 [DBA1018](diagnostics/DBA1018.md) 時，可先核對 Value Object 的角色辨識，再決定如何限制可變狀態。Profile、severity 與設定合併順序見 [Analyzer 設定](product/analyzer-policy.md)。

要了解規則保護的架構責任及採用成本，可閱讀[規則設計依據](product/rule-design-rationale.md)及[方法論來源](product/methodology-references.md)。回報疑似誤判時，請附上套件與 SDK 版本、Diagnostic ID、相關設定及可重現的最小程式片段。[ArchPrecept Issues](https://github.com/YuChia-Wei/ArchPrecept/issues)

## 依版本閱讀

| 套件版本 | 當版文件 | 發布紀錄 |
|---|---|---|
| 0.0.6-preview | [固定版本文件](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.6-preview/docs) | [Domain／ES 升級、套件試裝及 registry 驗收](releases/0.0.6-preview.md) |
| 0.0.5-preview | [文件](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.5-preview/docs) | [首次提供 starter 與 Value Object 範本](releases/0.0.5-preview.md) |
| 0.0.4-preview | [文件](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.4-preview/docs) | [Repository、角色辨識與 Id 初始化行為](releases/0.0.4-preview.md) |
| 0.0.3-preview | [文件](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.3-preview/docs) | [0.0.3-preview 發布紀錄](releases/0.0.3-preview.md) |
| 0.0.2-preview | [文件](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.2-preview/docs) | [0.0.2-preview 發布紀錄](releases/0.0.2-preview.md) |
| 0.0.1-preview | [文件](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.1-preview/docs) | [首次 preview 發布紀錄](releases/preview.md) |

## 產品與交付

| 文件 | 用途 |
|---|---|
| [產品規劃書](product/product-plan.md) | 能力邊界與目前產品決策；未決項維持明示 |
| [PRD](product/prd.md) | Requirement、優先序與可觀察驗收 |
| [文件治理](governance/document-governance-and-traceability.md) | docs／rules／開發資料分工、來源及內容品質 |
| [版本與發布準備政策](releases/version-policy-proposal.md) | 首個 non-preview 之前無舊版相容義務、正式版文件與套件準備條件 |
| [CI/CD 規則](releases/cicd-plan.md) | 現行 preview 建置、驗證與發布流程 |
| [授權與來源檢查](releases/licensing-assessment.md) | 程式與依賴的授權證據；不作為方法論依據 |

## 來源責任

- Markdown 是產品說明的唯一可編輯正式來源；Word／PDF 是衍生輸出。
- 套件與 repository 中的 `rules/catalog.xml` 保存機器可讀的規則，`rules/configuration.xsd` 定義設定格式。[0.0.5-preview 規則與 schema](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.5-preview/rules)
- 正式文案寫出設計理由及權威參考資料；私人討論、跨 repo 研究摘錄與中間草案不併入產品文件。
- 文件狀態與個別已確認政策分開；完整 PRD 的 draft 狀態不撤銷其中已明確確認的規則，也不代表所有規劃已實作。
- API XML、Diagnostic HelpLink、套件 README 與 docs 應指向同一版本的實際能力。來源、設定、程式、隨包內容及 consumer 可讀性分別驗證。
