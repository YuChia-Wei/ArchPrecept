# 版本政策與首個正式版條件

狀態：首個不帶 `-preview` 後綴的公開版本之前，採本文件的 Preview 演進政策；正式版的版號模型、維護線與相容承諾仍待決定。本文件定義要求，不代表已通過正式版驗收或已核准發布。

## Preview 演進政策

首個不帶 `-preview` 後綴的公開版本之前，不考慮向後相容。可依已接受的產品政策直接調整 Diagnostic／Rule ID、設定 schema、預設值、公開 API 與套件邊界；不為既有 preview 強制維持 legacy 模式、雙軌 API、obsolete wrapper、replacement rule、type forwarding 或 migration scaffolding。這項政策不自動核准任何具體行為變更。

每個版本內的 Rule／Diagnostic ID 仍須唯一，文件、設定、實作與測試必須一致。變更說明應清楚列出新版本的使用方式與受影響範圍，讓採用者能理解當前契約；不必為舊 preview 提供相容路徑。歷史語意由 Git 與既有發布紀錄保存，不在現行規則本文堆疊版本沿革。

已發布的 Package ID／version 與 bytes 不得覆寫。不同 bytes 必須使用尚未發布的新版本；main push 不自動升版，也不因本文件而選定任何下一版號。六個現行套件暫採共同 release train，且各自可獨立安裝。實際發布流程見[CI/CD 規則](cicd-plan.md)。

## 平台、成品與支援範圍

| 面向 | 現行基線 | 意義 |
|---|---|---|
| PackageVersion | 由具體發布決策選定的 `-preview` 版本 | 識別不可變套件成品；歷史發布結果見各版本紀錄 |
| Contracts TFM | `net8.0` | .NET 8 消費端可參考；不代表所有較新平台的執行行為皆已驗證 |
| Analyzer | `netstandard2.0`、Roslyn API 4.8 | 在編譯器內執行，相容性取決於 SDK／IDE 所帶的 Roslyn |
| 承諾支援的 .NET | .NET 8／10 | .NET 9 不列為承諾支援；.NET 11 待發布後另行評估 |

SDK 8／10 consumer 的 restore、build 與預期 Diagnostic 是套件安裝及載入證據。它不等於全部 API 的跨框架行為驗證，也不等於所有 Visual Studio／Rider 版本已驗證。可編譯或一次 smoke test 通過，不自行形成新的持續支援承諾。

## 首個不帶 `-preview` 公開版本的文件閘門

以下各項必須有實際交付與檢查證據，不能只由文件存在、XML 可產生或 CI exit 0 推定完成：

1. 每條公開產品規則都有可查證的外部方法論／作者來源、來源支持界線、產品選擇、採用理由、代價，以及與 DDD、Clean Architecture、Hexagonal Architecture 等相關方法論對齊或加強的說明。工具設定／範圍 Diagnostic 說明自身證據契約；不虛構方法論出處。來源缺口明列，不能用私人討論或研究過程充當設計權威。入口為[規則設計依據](../product/rule-design-rationale.md)及[方法論來源](../product/methodology-references.md)。
2. 每個可獨立採用的套件都有完整使用說明：安裝與認證前提、支援平台、採用模式、最小可用範例、設定與啟用方式、角色與相依邊界、失敗／修正途徑及已知限制。Analyzer-only／custom-contract 不得被要求改採參考契約。
3. 公開 API 對人與 AI 都有可讀的說明，交代責任、不變條件、所有權／可變性、生命週期與限制；XML 含有意義的 `summary`，並依簽章及行為提供 `typeparam`、`param`、`returns`／`value`、`exception` 與必要的 `remarks`。不適用的欄位不填造內容。
4. XML 實際包含於對應 `.nupkg`，且從 feed 還原後的 consumer 能讀到對應型別／成員的文件。原始碼註解完整、XML 生成、隨包交付、consumer 可讀及語意審查分別留證。
5. 既有建置、測試、套件、來源／授權與實際 feed consumer 閘門仍須滿足。文件和 XML 完整不等於 broker 交付、儲存交易、深層不可變性或完整架構符合性已驗證。

本政策只建立上述交付條件；缺少的規則依據、完整使用說明與 API XML 是待完成工作，不宣稱本次已全部補齊。

## 正式版政策待決事項

轉正式版前須明確選定產品 SemVer 或平台 major、共同版號範圍、維護線、支援期限及公開 API／Diagnostic／設定／報告的相容承諾。若採產品 SemVer，可評估以 major 表示不相容變更，minor 表示相容擴充，patch 表示相容修復；此處是待決方案，尚未生效。

平台版本與產品版本分別治理，不因產品 major 數字而自行提高 Contracts 最低 TFM。平台 EOL 與產品是否延長支援亦需分別決策。正式版政策尚未選定，不妨礙依 Preview 政策整理當前產品契約。
