# 套件與文件發布

GitHub Packages 與 NuGet.org 使用獨立的 workflow。兩者重用套件 CI，以同一來源提交各自建置及驗證七個套件，並上傳各自驗證通過的成品。

## 觸發條件

- 以 `main` 為目標的 PR 執行建置、測試、封裝及套件內容檢查。發布 workflow 另以 SDK 8 與 10 驗證套件安裝。
- 新推送的 `v*` tag 會啟動兩個發布 workflow。使用 `v` 加完整三段式版號，可加 SemVer 預發行後綴，例如 `v0.1.0-preview.1`、`v0.1.0-rc.1` 或 `v0.1.0`。NuGet 會從套件識別移除 `+build` metadata，所以發布 tag 省略這個欄位。
- 手動執行時選擇要發布的平台與既有 tag。預設分支與指定 tag 均須包含對應的 workflow。

在工作分支完成審查與必要驗證後，使用 `git merge --ff-only` 整合至 `main`。如推送會觸發網站部署，先完成發布前審查。每個已發布 tag 維持原始指向。

## 套件發布

1. 更新候選版本的使用說明。發布時從 tag 去掉 `v` 得到七包共用版號，starter 的精確套件參考隨封裝更新。一般 PR 與本機建置沿用 `Directory.Build.props` 的預設版號。將交付內容整合並推送至 `main`。完成發布前審查後，為 `main` 上通過審查的提交建立並單獨推送與版號相符的 annotated tag。
2. GitHub Packages 使用 repository 的 `GITHUB_TOKEN`。NuGet.org 的發布 job 使用 `nuget-org-packages` environment，透過 Trusted Publishing 為組織取得短效金鑰。兩邊只上傳各自同一次執行已驗證的 artifact。[GitHub Packages workflow](../../.github/workflows/publish-github.yml) · [NuGet.org workflow](../../.github/workflows/publish-nuget.yml)
3. 在 GitHub Packages 核對可見性與 repository linkage，並等候 NuGet.org 的七包可匿名下載。以 SDK 8 和 10 從各自 feed 還原，核對來源、版號、提交與成品內容，再執行 consumer 驗證。首次 NuGet.org 發布另確認七個 ID 的 Owner 為 ArchPrecept。
4. 上傳後的索引或 consumer 檢查失敗，待 feed 可用後重跑 failed jobs。若只有部分套件已上傳，保留原 run 的 artifact 與 SHA256SUMS，逐包核對已接受版本及原始內容，只從原 artifact 補送已確認缺少的項目。NuGet.org 會加 repository signature，內容比對依 consumer 腳本核對簽署前的內容雜湊。每個已發布的 ID 與版本均不可覆寫。
5. 各平台通過實際安裝驗收後，更新對應的公開安裝文件。

## 發布要求觸發的文件與規則檢查

套件發布須在推送發布 tag 或手動啟動發布 workflow 前完成本節的 AI 語意審查，核對本版規則、文件與實際成品。網站發布須在將更新推送至 `main` 並觸發部署前完成審查，核對本次變更的文件、對應套件版本及實際閱讀路徑。一併發布時，兩個範圍均須檢查。語意審查是 GitHub Actions 建置與測試以外的必要發布檢查。

### 模型與成本

文件與規則落差檢查使用低成本模型。允許使用 GPT 6.1 SOL、GPT 6.1 Terra、Claude Sonnet 5.5、Claude Haiku 5.5，或經當下 provider 資訊確認屬同級低成本的較新模型。執行時從宿主支援清單確認精確的模型 ID，並在審查結果中回報實際使用的模型及可觀察的執行識別。

目前執行模型符合條件時可直接檢查，否則可委派宿主支援的低成本子代理進行有界審查。無法指定或確認模型時，先完成差異與連結檢查，將語意檢查標為未完成，請使用者切換至可用低成本模型或明確同意替代方案。完成語意檢查後才可發布。

預設進行一次有界審查。提供發布基線、候選 commit、diff、文件入口及相關來源，先以機械檢查定位影響，再依該範圍追查。修正後複查受影響項目。遇到實質政策衝突或證據不足時釐清後再判定。

### 檢查範圍與交付

1. 固定遠端／本機身分、工作樹狀態、最近實際發布來源與候選 commit、目標版號及套件 inventory。以發布基線到候選的完整差異為範圍。文件發布以最近的網站版本為基線，首次網站部署則對照其說明的已發布套件。無法確認基線時揭露缺口。
2. 每次核對 README、使用指南、版本政策及發布說明中的版本、安裝方式、平台、套件 ID 與交付能力。依變更追查 Diagnostic 文件、規則設計理由、方法論來源、產品決策／PRD、`rules/catalog.xml`、schema、Analyzer predicate、公開契約／API XML、測試與 samples；交叉比對預設值、Profile、severity、角色辨識、限制及驗收聲明，不只檢查連結存在。
3. 確認實際 pack 輸入、隨包 README 與 XML 一致，分別核對原始碼文件、封裝內容與 feed consumer 可讀性，並依版本政策判定升版。Preview 文件須如實描述當版交付。首個正式版須完成下節全部條件。[合併與升版判準](../../docs/releases/version-policy.md#合併與升版判準)
4. 在發布作業中回報候選 SHA、基線、實際模型、已檢查及未評估範圍。對每項落差提供檔案位置、對照來源、影響、修正方向及是否阻擋發布。
5. 已確認的當版說明錯誤、規則與實作不一致、誤稱驗收通過，以及缺少必要發布證據，須修正或釐清後才發布。發布要求涵蓋必要且不改變已接受政策的文件修正；不得為消除落差而自行改 Analyzer 行為或放寬政策。已揭露且不影響當版交付正確性的後續工作可保留，不能把所有 open Issues 都當阻擋。
6. 修正後讀回 diff、連結與必要行為證據，固定最終候選 SHA。候選內容再變更時複查受影響範圍；通過後沿既有授權繼續發布，不重複索取已授權步驟的許可。無法完成必要檢查則明確回報未完成，不以 CI 綠燈代替語意審查。

## 首個正式版條件

`0.1.0` 共同交付 Analyzer、Domain、Application、Domain.EventSourcing、Messaging、Application.Messaging 與 Templates。交付範圍包括已文件化的 Analyzer 規則、自有角色映射、Domain 與 Event Sourcing 能力契約、starter、Domain／Application items、薄入口及採用者自訂的直接組件邊界測試。獨立 Architecture Testing 類別庫、傳遞依賴圖、Conformance CLI 與 IDE 外掛留待後續版本。[範本指南](../../docs/templates.md) · [組件邊界測試](../../docs/architecture-testing.md)

1. 每條公開產品規則須交代外部方法論或原作者來源、來源支持範圍、產品選擇、理由與代價，並說明與相關方法論的對齊或加強。設定與範圍 Diagnostic 須說明其證據契約。缺少權威來源時須明列缺口。[規則設計依據](../../docs/product/rule-design-rationale.md) · [方法論來源](../../docs/product/methodology-references.md)
2. 每個套件須具備完整使用說明，涵蓋安裝與認證前提、支援平台、採用模式、最小範例、設定、角色與相依邊界、失敗修正與已知限制。Analyzer-only 與 custom-contract 採用模式須能獨立使用。[安裝指南](../../docs/getting-started.md) · [套件使用指南](../../docs/package-usage.md)
3. 公開 API 的 XML 文件須供人與 AI 讀取，說明責任、不變條件、所有權或可變性、生命週期與限制。XML 須提供有意義的 `summary`，並按簽章及行為補充 `typeparam`、`param`、`returns` 或 `value`、`exception` 與必要的 `remarks`。須核對原始註解、生成的 XML、隨 `.nupkg` 交付的內容及從 feed 還原後的型別與成員文件。
4. 完成建置、測試、套件、發布前文件與規則檢查、來源及授權檢查。原始碼、文件及封裝須保留適用授權與歸屬，第三方內容須符合其再散布條款。[LICENSE](../../LICENSE) · [NOTICE](../../NOTICE) · [第三方授權聲明](../../THIRD-PARTY-NOTICES.md)
5. 在 NuGet.org 確認七個套件 ID 的組織所有權並完成 SDK 8 與 10 的匿名還原及套件驗收。官網的穩定版導覽須可開啟該版的固定文件與 NuGet.org 安裝說明。

## 文件發布

`docs/` 的 Markdown 及 `.github/workflows/docfx/` 的設定與樣式變更會觸發網站工作流程。每次建置從該次提交的 Contracts 組件與 XML 註解產生 API 文件。需要發布 Contracts 程式碼的文件更新時，連同相關網站內容一併更新。

1. 將安裝、範例、診斷及 API 說明整合至 `main`。文件與功能一併交付時，先在網站標示候選版與最近已發布的版本。等相應平台通過安裝驗收，再更新該平台的可用狀態。
2. 完成發布前審查後，將網站來源變更推送至 `main`。文件 workflow 從該次 push 的提交產生 DocFX 靜態成品，部署到 GitHub Pages 的 `/ArchPrecept/` 專案路徑。[文件工作流程](../../.github/workflows/documentation.yml)
3. 從公開首頁實際開啟安裝指南、Price 範例與 DBA1018 修正說明。確認部署來源，以及頁面標示的套件版本、站內連結與靜態資源。首個正式版發布時，核對 `/stable/` 導覽、`/versions/0.1.0/` 固定版本入口與 NuGet.org 安裝說明。歷史版本可連到對應 Git tag。需要回復網站時，在 `main` 恢復已知正確的 `docs/` 內容並觸發重新部署。
