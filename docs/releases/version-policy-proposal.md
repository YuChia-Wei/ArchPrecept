# 版本政策與首個正式版條件

首個無後綴版本定為 `0.1.0`。七包共用產品版號，支援 SDK 8／10；維護及相容承諾自首個穩定版發布起生效。當前開發版本仍適用 Preview 演進政策。

## Preview 演進政策

首個不帶任何 prerelease 後綴的公開版本之前，不考慮向後相容。可依已接受的產品政策直接調整 Diagnostic／Rule ID、設定 schema、預設值、公開 API 與套件邊界；不為既有 preview 強制維持 legacy 模式、雙軌 API、obsolete wrapper、replacement rule、type forwarding 或 migration scaffolding。這項政策不自動核准任何具體行為變更。

每個版本內的 Rule／Diagnostic ID 仍須唯一，文件、設定、實作與測試必須一致。變更說明應清楚列出新版本的使用方式與受影響範圍，讓採用者能理解當前契約；不必為舊 preview 提供相容路徑。歷史語意由 Git 與既有發布紀錄保存，不在現行規則本文堆疊版本沿革。

已發布的 Package ID／version 與 bytes 不得覆寫。核心試用期使用 `0.0.n-preview`，不同成品遞增 n。`0.0.5-preview` 是已發布的 starter／VO 基線；`0.0.6-preview` 是下一組變更的本機候選。實際發布流程見[CI/CD 規則](cicd-plan.md)。

## 合併與升版判準

每次收到發布要求，先依 [CI/CD 發布前文件與規則檢查](cicd-plan.md#發布要求觸發的文件與規則檢查)使用低成本模型確認當版落差；此要求適用 Preview 與正式版，與下列升版判斷及正式版文件閘門共同適用。

先確認交付目的及實際封裝輸入，再決定是否升版；合併 repository 變更與發布 NuGet 成品分別判斷，不以提交數量或檔案副檔名決定版號。

| 交付情境 | 升版要求 |
|---|---|
| 只將產品文件、規則理由、開發治理或 Framework 採納整合到 main，沒有要交付新的 NuGet 成品 | 不必升版；完成受影響文件與整合檢查即可 |
| 在公開網站修正安裝說明、診斷範例或導覽 | 沿用文件描述的套件版本，為網站來源建立新的 `docs-YYYY.MM.DD.N` tag |
| 更新會隨包交付的 README、API XML、其他內容或套件 metadata，並要讓下游取得更新 | 必須使用尚未發布的新 preview 版本，即使執行行為沒有改變 |
| 發布 Analyzer predicate、Diagnostic、Catalog／schema、公開契約、套件相依或其他產品行為的變更 | 必須使用尚未發布的新 preview 版本，完成受影響行為及套件驗證，清楚描述當版能力與限制 |
| 只重新建置或驗證既有版本，沒有發布新成品 | 不因重建自動升版；重建產物不得覆寫既有版本，也不得冒稱為原發布 bytes |
| 發布首個不帶任何 prerelease 後綴的公開版本 | 先完成本文件的正式版閘門、版本政策決策及正式版發布機制驗證；文件合併或 CI 成功不代表已具備條件 |

根目錄 `README.md` 目前由 `Directory.Build.props` 納入套件，因此文件變更也可能改變封裝內容。判斷時須檢查實際 pack 設定；不能一律將「只改 Markdown」視為套件 bytes 不變。只更新 repository 文件時，既有套件仍保留原內附內容；若要把更新交付至 feed，才依上表選定新版本。

PR 在合併前進行品質檢查。套件交付由已整合至 main 的 `v<PackageVersion>` tag 觸發，執行版號、成品與 registry 驗證。若同時更新公開手冊，完成 feed 驗收後另以文件 tag 發布 DocFX。普通 main push 只整合來源。詳見 [CI/CD 規則](cicd-plan.md)。

尚未完成的產品 Issue 或來源缺口，若已如實標示且不影響本次內容正確性，不自動阻擋文件合併；仍須依其適用範圍保留後續實作或正式版驗收要求。具體新版本由發布決策選定，不由本表自動遞增，七套件沿用共同 release train。

## 平台、成品與支援範圍

| 面向 | 現行基線 | 意義 |
|---|---|---|
| PackageVersion | 由具體發布決策選定的 `-preview` 版本 | 識別不可變套件成品；歷史發布結果見各版本紀錄 |
| Contracts TFM | `net8.0` | .NET 8 消費端可參考；不代表所有較新平台的執行行為皆已驗證 |
| Analyzer | `netstandard2.0`、Roslyn API 4.8 | 在編譯器內執行，相容性取決於 SDK／IDE 所帶的 Roslyn |
| Consumer 支援版本 | .NET 8／10 | .NET 9 不在承諾支援範圍。SDK 11.0.100 用於建置與封裝產品，.NET 11 consumer 支援仍待評估。 |

SDK 8／10 consumer 的 restore、build 與預期 Diagnostic 是套件安裝及載入證據。它不等於全部 API 的跨框架行為驗證，也不等於所有 Visual Studio／Rider 版本已驗證。可編譯或一次 smoke test 通過，不自行形成新的持續支援承諾。

## 首個不帶任何 prerelease 後綴的公開版本的文件閘門

以下各項必須有實際交付與檢查證據，不能只由文件存在、XML 可產生或 CI exit 0 推定完成：

1. 每條公開產品規則都有可查證的外部方法論／作者來源、來源支持界線、產品選擇、採用理由、代價，以及與 DDD、Clean Architecture、Hexagonal Architecture 等相關方法論對齊或加強的說明。工具設定／範圍 Diagnostic 說明自身證據契約；不虛構方法論出處。來源缺口明列，不能用私人討論或研究過程充當設計權威。入口為[規則設計依據](../product/rule-design-rationale.md)及[方法論來源](../product/methodology-references.md)。
2. 每個可獨立採用的套件都有完整使用說明：安裝與認證前提、支援平台、採用模式、最小可用範例、設定與啟用方式、角色與相依邊界、失敗／修正途徑及已知限制。Analyzer-only／custom-contract 不得被要求改採參考契約。
3. 公開 API 對人與 AI 都有可讀的說明，交代責任、不變條件、所有權／可變性、生命週期與限制；XML 含有意義的 `summary`，並依簽章及行為提供 `typeparam`、`param`、`returns`／`value`、`exception` 與必要的 `remarks`。不適用的欄位不填造內容。
4. XML 實際包含於對應 `.nupkg`，且從 feed 還原後的 consumer 能讀到對應型別／成員的文件。原始碼註解完整、XML 生成、隨包交付、consumer 可讀及語意審查分別留證。
5. 既有建置、測試、套件、來源／授權與實際 feed consumer 閘門仍須滿足。文件和 XML 完整不等於 broker 交付、儲存交易、深層不可變性或完整架構符合性已驗證。

本政策只建立上述交付條件；缺少的規則依據、完整使用說明與 API XML 是待完成工作，不宣稱本次已全部補齊。

## 首版範圍與相容承諾

首版共同交付 Analyzer、Domain、Application、Domain.EventSourcing、Messaging、Application.Messaging 與 Templates。各包可獨立選用；SDK、最低 TFM 與 Roslyn 基線沿用上表。首版功能包含已文件化的 Analyzer 規則、自有角色映射、Domain／ES 分離的能力契約、starter、Domain／Application items、薄入口及[採用者自訂的組件邊界測試](../architecture-testing.md)。片段與編輯器的具體入口依[範本指南](../templates.md)。獨立 Architecture Testing 類別庫、傳遞依賴圖、Conformance CLI 與 IDE 外掛留在首版範圍外。

只維護最新穩定線。下一條穩定線公開時，上一條結束一般修補；無固定支援年限。平台 major 與套件版號分別決定。進入 `0.1.x` 後，patch 保持相容，破壞性變更進下一條 `0.y.0`；從 `1.0.0` 起，major 表示不相容變更、minor 表示相容擴充、patch 表示相容修正。

相容審查涵蓋公開 API、套件相依與最低平台、Diagnostic ID／判定語意、預設 Profile／severity、設定 schema、公開報告欄位，以及已文件化的範本命令與參數。新增預設診斷、提高 severity 或修正漏判若會使既有受支援建置失敗，依實際影響選用新的不相容版本線，再提供更新說明。新增 opt-in 規則、修正誤報及其他變更也須評估既有設定與 warnings-as-errors 的結果。

第一個公開 RC 為選配的 `0.1.0-rc.1`。需要外部驗證已凍結的候選時才發布 RC，必要修正以 `rc.2` 等新成品重驗；若契約或範圍再開放，退回 Preview。也可從已驗收的預覽來源直接準備 `0.1.0`。兩條路徑都須以**實際要發布的版號與成品**完成文件、規則、套件及對應 consumer 閘門。RC 一律視為 prerelease。實際採用依據與尚待 owner 完成的發布條件見[首版路線](stable-release-roadmap.md)。

## 後續發布與官方文件安排

現行自動上傳流程只接受符合規格的 Preview tag，目標為 GitHub Packages。nuget.org 的 owner 操作、成品追溯與匿名驗收見[發布設計](cicd-plan.md#nugetorg-正式版交付)。
