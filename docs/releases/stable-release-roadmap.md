# 正式版與線上文件路線

首個無後綴版本為 `0.1.0`。七包共同版號、SDK 支援及維護線以[版本政策](version-policy-proposal.md#首版範圍與相容承諾)為準。GitHub Packages 已發布 [0.0.5-preview](0.0.5-preview.md)；新契約與範本先在本機候選試用，待功能、文件與成品驗收完成後才進入 nuget.org 發布。

## 社群慣例與本產品選擇

.NET 套件有共同的 SemVer／NuGet 預覽版機制，但尾綴命名、候選階段及發布週期並不完全一致。以下是具體例子，不是全社群統計：

| 來源 | 觀察 | ArchPrecept 採用方式 |
|---|---|---|
| [Microsoft .NET library guidance](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning) | 建議以 SemVer 表達套件變更，非穩定套件使用 prerelease 後綴 | 採產品 SemVer；套件版號不與最低 TFM 綁定 |
| [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection) | 使用 preview／rc，部分版本帶額外 build 識別 | 核心試用期用 0.0.n-preview；範圍凍結且需要外部候選驗收時可選用 RC |
| [Azure SDK release policy](https://azure.github.io/azure-sdk/policies_releases.html) | 以 beta.N 迭代公開預覽 | 說明 Microsoft 內部也無單一尾綴；本產品不混用 beta 與 preview |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) | 套件歷史有 beta1／beta2 形式 | 不照搬文字數字黏合格式，使用 SemVer 數字段 N |
| [.NET SDK／Runtime versioning](https://learn.microsoft.com/en-us/dotnet/core/versions/) | SDK 有 feature bands，runtime 有平台發布節奏 | 不套用 SDK feature bands、年度 major 或平台 LTS 承諾 |

本產品的階段定義：

- `0.0.n-preview`：核心試用階段。每次需要交付不同 bytes 時增加 n，例如 0.0.5-preview → 0.0.6-preview。
- `X.Y.Z-rc.N`：目標範圍、公開 API／Diagnostic／設定及文件已收斂；剩餘工作為候選驗收與必要修錯。重大範圍變化應重新評估候選資格。RC 不等於 NuGet 保證品質或適合 production。
- `X.Y.Z`：無任何 prerelease 後綴，NuGet 視為穩定版，包括 `0.1.0`。`-stable`、`-release` 或 `-ga` 仍會被 NuGet 當成 prerelease，因此不使用。
- 後綴降低穩定版安裝／更新流程選到候選的機會，不是存取控制；明確指定或容許 prerelease 的相依圖仍可使用。詳見 [NuGet 預覽版](https://learn.microsoft.com/en-us/nuget/create-packages/prerelease-packages)及[相依解析](https://learn.microsoft.com/en-us/nuget/concepts/dependency-resolution)。


## 排程與完成條件

下列為依相依順序安排的工作窗口，不是自動排程或無條件發布日期。若 consumer 發現阻擋，先處理阻擋並調整窗口；不為趕日期移除 prerelease 後綴。

| 階段／目標窗口 | 工作 | 完成條件 |
|---|---|---|
| 已完成第一輪：0.0.4-preview | GitHub Packages 已發布；MQ／Web API lab 隔離 worktree 試用已執行但未通過採用驗收 | [發布證據](0.0.4-preview.md)與[consumer 結果](0.0.4-consumer-validation.md)分開保存 |
| 0.0.5-preview | A1 starter 與 VO 範本，公開至 GitHub Packages 後供新專案試用 | 安裝、生成、兩種契約的 DBA1018 違規與修正證據；參見[本版紀錄](0.0.5-preview.md) |
| 0.0.6-preview 本機候選 | [Web API 與 Domain／ES](0.0.6-consumer-validation.md)及 [MQ 多 port](mq-repository-adoption.md) | Web API 34 個原有測試通過。MQ 11 個 DBA1001 已重現並依公開能力逐項處理，受影響測試 107 項通過、41 項略過。兩個 lab 各使用隔離副本。 |
| 文件網站 | [#21](https://github.com/YuChia-Wei/ArchPrecept/issues/21) 已建立可辨識版本的安裝、API 與規則入口 | 已公開的網站與本機候選依各自版本閱讀 |
| 0.1.0 起的 0.y.z（y ≥ 1） | 依已確認政策審查精確候選，完成 #22 的 nuget.org 發布及匿名安裝 | 實際發布與 feed consumer 驗證分別留證 |

## 後續工作來源

工作狀態與驗收條件由 GitHub Issues 保存，本文件只維持規劃及相依關係：

| 工作 | 追蹤 |
|---|---|
| Web API identity 生命週期採用 | [#18](https://github.com/YuChia-Wei/ArchPrecept/issues/18)；與 MQ 判讀可分別處理 |
| MQ Repository 公開能力／多 port adapter 判讀 | [#19](https://github.com/YuChia-Wei/ArchPrecept/issues/19)；不預設誤報或強制改架構 |
| 首個穩定版範圍、版本及相容承諾 | [#20](https://github.com/YuChia-Wei/ArchPrecept/issues/20)；引用採用結果決定 RC／穩定版資格 |
| 版本可辨識的官方文件網站 | [#21](https://github.com/YuChia-Wei/ArchPrecept/issues/21)。採 DocFX 與 GitHub Pages，依文件 tag 部署，歷史套件文件保留版本連結。 |
| nuget.org 發布及匿名 restore | [#22](https://github.com/YuChia-Wei/ArchPrecept/issues/22)；目標為 0.1.0 起的發布，依賴核心採用驗收及簡易官網 |

首版納入 [#10 的直接組件依賴測試](../architecture-testing.md)、[#16 的 Domain／ES 能力契約](../product/event-sourcing-contract.md)與 [#23～#28 的標準生成入口](../templates.md)。採用者可單獨選用；個別功能的支援範圍及明確延後項目由各自使用指南定義。

## 線上官方文件最小範圍

網站採用 [DocFX](https://dotnet.github.io/docfx/) 與 [GitHub Pages](https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages)。從現有 Markdown 與 Contracts API 資料產生靜態頁面，以獨立文件 tag 部署。套件和網站均須發布時，先完成套件的 registry 驗收，再發布對應版本的文件。操作順序見[文件與套件發布流程](cicd-plan.md)。

`docs/` 與來源 XML 保持唯一 authored source；網站只產生展示內容，不複製另一份 Rule Catalog 或維護兩套規則文字。最低導覽包括：

1. 產品定位、支援平台、套件選擇與安裝。
2. Analyzer-only、自有契約 mapping、參考契約的最小使用路徑。
3. Diagnostic 目錄、設定／Profile／severity、修正與已知限制。
4. Contracts API 參考、Messaging／ES 責任與保證界線。
5. 版本歷史、升級差異、來源定位、支援及回報問題入口。

每個公開版本保留固定文件路徑，例如 `/versions/0.1.0/`；`/preview/` 顯示清楚的預覽版提示，首個穩定版之後才設 `/stable/` 導向。未發布的 main 文件不得冒充穩定版本文件。初期可將歷史版本連回 immutable Git tag，不要求一次重建所有歷史網站。

每次文件部署從公開首頁核對安裝指南、Price 範例及 DBA1018 的修正路徑。首次穩定版還須核對版本導覽與 nuget.org 安裝說明。
