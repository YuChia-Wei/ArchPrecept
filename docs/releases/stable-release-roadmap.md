# 正式版與線上文件路線

規劃日期：2026-11-02。0.0.4-preview 已選定為本次交付；後續版本與網站為規劃，不表示已發布或已部署。nuget.org 帳號及 ownership 準備可與 consumer 驗證並行，不以穩定版版號為前提。

## 社群慣例與本產品選擇

.NET 套件有共同的 SemVer／NuGet 預覽版機制，但尾綴命名、候選階段及發布週期並不完全一致。以下是具體例子，不是全社群統計：

| 來源 | 觀察 | ArchPrecept 採用方式 |
|---|---|---|
| [Microsoft .NET library guidance](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning) | 建議以 SemVer 表達套件變更，非穩定套件使用 prerelease 後綴 | 採產品 SemVer；套件版號不與最低 TFM 綁定 |
| [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection) | 使用 preview／rc，部分版本帶額外 build 識別 | 採 preview.N → rc.N → 無後綴；目前不需要長 build 序號 |
| [Azure SDK release policy](https://azure.github.io/azure-sdk/policies_releases.html) | 以 beta.N 迭代公開預覽 | 說明 Microsoft 內部也無單一尾綴；本產品不混用 beta 與 preview |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) | 套件歷史有 beta1／beta2 形式 | 不照搬文字數字黏合格式，使用 SemVer 數字段 N |
| [.NET SDK／Runtime versioning](https://learn.microsoft.com/en-us/dotnet/core/versions/) | SDK 有 feature bands，runtime 有平台發布節奏 | 不套用 SDK feature bands、年度 major 或平台 LTS 承諾 |

本產品的階段定義：

- `X.Y.Z-preview.N`：公開試用，容許目標契約調整；N 從 1 遞增。當目標 X.Y.Z 改變時重新編號。本次既有序列保留 `0.0.4-preview`，不另發相同內容只為補 `.1`。
- `X.Y.Z-rc.N`：目標範圍、公開 API／Diagnostic／設定及文件已收斂；剩餘工作為候選驗收與必要修錯。重大範圍變化應重新評估候選資格。RC 不等於 NuGet 保證品質或適合 production。
- `X.Y.Z`：無任何 prerelease 後綴，NuGet 視為穩定版，包括 `0.1.0`。`-stable`、`-release` 或 `-ga` 仍會被 NuGet 當成 prerelease，因此不使用。
- 後綴降低穩定版安裝／更新流程選到候選的機會，不是存取控制；明確指定或容許 prerelease 的相依圖仍可使用。詳見 [NuGet 預覽版](https://learn.microsoft.com/en-us/nuget/create-packages/prerelease-packages)及[相依解析](https://learn.microsoft.com/en-us/nuget/concepts/dependency-resolution)。

### 相容承諾提案

後續首個穩定目標建議為 `0.1.0`，尚不以本規劃宣告門檻通過或授權發布。`0.1.x` 保持相容，破壞性變更進入下一個 `0.y.0`；進入 `1.0.0` 後依 SemVer，以 major 表示不相容、minor 表示相容能力擴充、patch 表示相容修復。六套件維持共同 release train。

公開契約涵蓋 API、套件相依／最低平台、Diagnostic ID 與語意、預設 Profile／severity、設定 schema 及公開報告格式。新增預設啟用診斷或提高 severity 可能破壞 consumer 建置，不能只因 C# API 沒變便當作相容修復；新增 opt-in 規則可按相容擴充評估。修正誤報、漏判仍須說明建置影響，不能以 bugfix 名義隱藏破壞性結果。

正式採用上述相容承諾、支援期限與首個穩定版目標時，更新[版本政策](version-policy-proposal.md)；現行「首個穩定版前不保相容」政策在此之前繼續有效。本規劃不回溯改寫歷史版本，也不新增相容轉接層。

## 排程與完成條件

下列為依相依順序安排的工作窗口，不是自動排程或無條件發布日期。若 consumer 發現阻擋，先處理阻擋並調整窗口；不為趕日期移除 prerelease 後綴。

| 階段／目標窗口 | 工作 | 完成條件 |
|---|---|---|
| 2026-11-02 起：0.0.4-preview | GitHub Packages 發布；MQ／Web API lab 隔離 worktree 驗證；使用者並行準備 nuget.org | 發布及 feed consumer 證據、各 lab 通過／失敗與限制分開回報 |
| 第一輪 consumer 結果後，預估 1–2 工作日 | 整理誤報／漏判或採用問題，固定首版六套件範圍、相容承諾及支援政策 | 接受的修正範圍與剩餘限制可查；沒有為符合規則強迫 consumer 改架構 |
| 與修正並行，預估 2–3 工作日 | 線上官方文件最小網站、版本導覽及 API 參考 | 可瀏覽的候選網站，主要採用路徑與版本連結人工抽查通過 |
| nuget.org 準備及必要修正完成後 | `0.1.0-preview.1`；打通實際 nuget.org 安裝與匿名 restore | 六包 ownership／metadata、真正 feed 安裝及最小 consumer 通過 |
| 契約凍結及文件就緒後 | `0.1.0-rc.1`，預留至少一輪兩個 lab 的候選回饋 | 無未處理的首版阻擋；正式版文件與既有發布門檻有證據 |
| RC 結果接受後 | `0.1.0`；後續按實際採用經驗評估 1.0 | 穩定相容承諾、維護線與支援期限明確；不以等待固定天數代替驗收 |

## 線上官方文件最小範圍

建議採 [DocFX](https://dotnet.github.io/docfx/) 搭配 [GitHub Pages](https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages)。DocFX 可將既有 Markdown 與原生 XML 文件註解呈現為網站；它是文件產生工具，不是新的 API 完整性驗證框架。使用現成版型及本機 tool manifest 固定工具版本即可，初期不建立自訂前端、後端、會員或搜尋服務。

`docs/` 與來源 XML 保持唯一 authored source；網站只產生展示內容，不複製另一份 Rule Catalog 或維護兩套規則文字。最低導覽包括：

1. 產品定位、支援平台、六套件選擇與安裝。
2. Analyzer-only、自有契約 mapping、參考契約的最小使用路徑。
3. Diagnostic 目錄、設定／Profile／severity、修正與已知限制。
4. Contracts API 參考、Messaging／ES 責任與保證界線。
5. 版本歷史、升級差異、來源定位、支援及回報問題入口。

每個公開版本保留固定文件路徑，例如 `/versions/0.1.0/`；`/preview/` 顯示清楚的預覽版提示，首個穩定版之後才設 `/stable/` 導向。未發布的 main 文件不得冒充穩定版本文件。初期可將歷史版本連回 immutable Git tag，不要求一次重建所有歷史網站。

網站在第一個 nuget.org preview 前應有可用的安裝／規則入口；RC 前完成首版 API 參考與版本導覽。只驗證產生、關鍵連結與實際閱讀路徑，不因 Markdown 或生成工具版本改變重跑全套產品測試。網站部署、正式網域及 NuGet PackageProjectUrl 更新在實作時另行確認，不在本次規劃中預填完成。
