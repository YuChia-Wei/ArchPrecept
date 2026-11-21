# PR 品質檢查與套件、文件發布流程

PR 負責合併前的品質檢查。套件與網站採各自的發布 tag，從已整合到 `main` 的提交交付。套件目前以 GitHub Packages 提供公開 Preview；首個無後綴版本與 nuget.org 的準備條件見[版本政策](version-policy-proposal.md)。

## 已決定的規則

| 事件 | 執行內容 | 交付結果 |
|---|---|---|
| 開啟或更新以 `main` 為目標的 PR | 還原、建置並測試 solution，執行發布狀態測試，建置 DocFX 文件供檢查 | PR 顯示品質結果。 |
| 一般分支 push，包括合併到 `main` 後的 push | 更新分支 ref | 套件與網站維持各自最近一次已發布版本。 |
| 推送套件 tag `v<PackageVersion>` | 核對 tag、版本、來源及 registry 狀態，建置七個套件並驗證 consumer | 通過後將尚未發布的該版成品送至 GitHub Packages 並驗證。 |
| 推送文件 tag `docs-YYYY.MM.DD.N` | 從該提交產生 DocFX 靜態網站，再部署至 GitHub Pages | 網站可以單獨修正；套件維持原版號與 bytes。 |

在工作分支完成 review 與必要驗證，沿用 `git merge --ff-only` 整合至 `main` 並推送。依當次交付目的建立新的 annotated tag，將 tag 綁定到已核對的提交，再單獨推送該 ref。文件 tag 使用年月日及當日序號，序號從 1 開始。保留所有已發布 tag 的原始指向，包括由 [0.0.5-preview 發布 run](0.0.5-preview.md) 的實際來源補建的套件 tag。

## 發布要求觸發的文件與規則檢查

收到發布要求後，執行 AI 依本節在推送發布 tag 前完成審查。套件 Preview 與正式版須核對當版規則、文件與實際成品；單獨發布網站時，範圍為本次變更的文件、其對應套件版本及實際閱讀路徑。GitHub Actions 的建置與測試之外，本專案另行執行以下 AI 語意審查。

### 模型與成本

文件／規則落差檢查使用低成本模型。使用者指定的允許候選為 GPT 6.1 SOL、GPT 6.1 Terra、Claude Sonnet 5.5、Claude Haiku 5.5，或經當下 provider 資訊確認屬同級低成本的較新模型；這份清單不是價格或可用性保證。執行時確認宿主支援的精確模型 ID，記錄實際模型及可觀察的執行識別，不以顯示名稱猜測 API ID，也不自行宣稱某候選最便宜。

目前執行模型符合條件時可直接檢查；否則使用宿主支援的指定模型委派。此用途允許有界的低成本審查子代理，不要求安裝 Framework sub-agent profiles 或跨 provider 整合。無法指定或確認模型時，先完成非 AI 的差異與連結檢查，將語意檢查標為未完成，請使用者切換至可用低成本模型或明確同意替代方案；不得默默改用昂貴模型，也不得省略檢查後發布。

預設一次有界審查：給定發布基線、候選 commit、diff、文件入口及必要來源，由變更影響擴展閱讀。先用機械檢查縮小範圍，不整庫重讀、不重建完整 SDLC、不重跑無關昂貴驗收。修正後只複查受影響項目；遇到實質政策衝突或證據不足才升級討論，不以多輪模型投票代替證據。

### 檢查範圍與交付

1. 固定遠端／本機身分、工作樹狀態、最近實際發布來源與候選 commit、目標版號及套件 inventory。以發布基線到候選的完整差異為範圍。文件發布以最近的網站版本為基線，首次網站部署則對照其說明的已發布套件。無法確認基線時揭露缺口。
2. 每次核對 README、使用指南、版本政策及發布說明中的版本、安裝方式、平台、套件 ID 與交付能力。依變更追查 Diagnostic 文件、規則設計理由、方法論來源、產品決策／PRD、`rules/catalog.xml`、schema、Analyzer predicate、公開契約／API XML、測試與 samples；交叉比對預設值、Profile、severity、角色辨識、限制及驗收聲明，不只檢查連結存在。
3. 檢查實際 pack 輸入、隨包 README／XML 與發布紀錄是否一致，依[合併與升版判準](version-policy-proposal.md#合併與升版判準)判定升版。原始碼文件、封裝內容與 feed consumer 可讀性分別留證；發布前不能預填發布成功。正式版另覆蓋全部正式版文件閘門，Preview 不必等待所有未來能力完成，但當版說明必須正確。
4. 回報候選 SHA、基線、實際模型、已檢查範圍、未評估範圍及逐項落差。每項提供檔案位置、對照證據、影響、修正方向及是否阻擋本次發布；結果在現有對話／工作紀錄呈現，正式版本紀錄僅保存產品交付與技術驗證證據，不建立平行規則台帳。
5. 已確認的當版說明錯誤、規則與實作不一致、誤稱驗收通過，以及缺少必要發布證據，須修正或釐清後才發布。發布要求涵蓋必要且不改變已接受政策的文件修正；不得為消除落差而自行改 Analyzer 行為或放寬政策。已揭露且不影響當版交付正確性的後續工作可保留，不能把所有 open Issues 都當阻擋。
6. 修正後讀回 diff、連結與必要行為證據，固定最終候選 SHA。候選內容再變更時複查受影響範圍；通過後沿既有授權繼續發布，不重複索取已授權步驟的許可。無法完成必要檢查則明確回報未完成，不以 CI 綠燈代替語意審查。

## 套件發布

1. 選定下一個尚未發布的版本，在同一候選提交更新 `Directory.Build.props`、套件使用說明與當版發布說明。先記錄預期交付的行為與限制，實際發布結果待 run 完成後補入。
2. 檢查候選差異並整合到 `main`，使用 PR 時先確認其品質檢查通過。在核對過的來源建立 `v<PackageVersion>` annotated tag，例如 `v0.0.6-preview`。Tag 去掉 `v` 後須與專案的 `X.Y.Z-preview` 或 `X.Y.Z-preview.N` 版本完全相符。
3. 推送該 tag 後，[Publish GitHub preview](../../.github/workflows/publish-preview.yml) 檢查 tag 指向已整合至 `main` 的提交、版號與 LICENSE，並讀回七個 ID 的 registry 狀態。Tag push 決定本次發布意圖。PR 的權限限於品質檢查，只有套件發布 job 以 `GITHUB_TOKEN` 取得 `packages: write`。
4. 從 tag 的精確來源建置、測試及封裝，使用 SDK 8／10 驗證本機 feed 消費行為。上傳時使用同一次 run 已驗證的七包，逐一核對 SHA-256。發布後確認七包的 public 可見性、版本及來源 repository，再以獨立快取進行 registry consumer 驗收。
5. 將實際 tag、來源 SHA、run、成品雜湊與 consumer 結果補入當版發布紀錄，再依下節發布網站。已發布套件中的文件隨該版本固定。

## 文件發布

1. 將對應已發布版本的安裝、範例、診斷及 API 說明整合至 `main`。只有文件變更時使用此流程，套件版號沿用當前已發布版本。若功能與文件一併交付，先確認本版套件已可從 feed 還原，並讓網站標示該版號。
2. 從已核對的文件提交推送新的 `docs-YYYY.MM.DD.N` annotated tag。[文件工作流程](../../.github/workflows/documentation.yml) 使用該提交的來源產生 DocFX 靜態成品，部署到 GitHub Pages 的 `/ArchPrecept/` 專案路徑。可在後續提交修正文案，再推送另一個文件 tag。
3. 從公開首頁實際開啟安裝指南、Price 範例與 DBA1018 修正說明。確認頁面標示的套件版本、站內連結與靜態資源，記錄文件 tag、部署來源、run 與實際網址。若需要回復文件，在 `main` 還原到已知正確內容並以新文件 tag 部署，保留可追溯的歷史版本。

## 相同版本與部分失敗

`eng/Get-PreviewReleaseState.ps1` 使用具權限的 GitHub API，檢查七個 ID、repository linkage、
public 可見性及所有版本分頁。403 或其他 API 錯誤會失敗，只有 404 視為不存在。

| Registry 狀態 | 行為 |
|---|---|
| 七個 ID 都沒有目前版本 | CI 通過後上傳七個 nupkg；使用同次 run 的 artifact，先驗 SHA-256 |
| 七個 ID 都已有目前版本 | 仍執行 CI，跳過上傳，讀回並驗證已發布版本；不以新 build 覆寫舊版本 |
| 只有部分 ID 有目前版本 | 停止；保留最初 run 的 artifact，盤點已存在 bytes，再用原成品恢復缺少項目 |
| 既有 package 非 public、連結錯誤或權限不足 | 停止並回報，不能把 API 錯誤視為尚未發布 |

新版產品內容若需發布不同 bytes，必須先選定未發布的新版本。文件整合、隨包內容更新與產品行為變更的升版判斷，統一依[合併與升版判準](version-policy-proposal.md#合併與升版判準)。
首個不帶任何 prerelease 後綴的公開版本之前不考慮向後相容，可直接調整 ID、schema、defaults 與 API；不強制 legacy／replacement／migration scaffolding。具體政策與正式版文件閘門見[版本政策](version-policy-proposal.md)。
沒有使用 `--skip-duplicate`。七次 push 不是原子交易，不可刪除、覆寫或重建同版號後盲目重試。

上傳與 metadata verification 分為不同 jobs：上傳成功後若只因 metadata／可見性檢查失敗，
完成修正後可以 rerun failed jobs，不會重新執行已成功的上傳 job。
[Verify published GitHub preview](../../.github/workflows/verify-preview.yml) 也可手動執行唯讀驗證。
獨立 verify 依所選來源的版號與 inventory 工作，不會自動切回歷史四套件。

## Public 設定與驗證

七個現行套件採 public。既有公開 package 的新版本沿用 package 可見性；
新的 Package ID 首次上傳後仍須確認。GitHub 文件指出新 package 預設 private，
可在 Package settings 改為 public；public 不能改回 private。
已查閱的官方 REST Packages API 未提供變更 visibility 的操作，因此 workflow 不使用未公開端點，
也不把上傳成功等同已公開。若新 ID 為 private，須在該次發布的具體授權範圍內完成官方 public 設定並重跑驗證。
參考 [GitHub 可見性規則](https://docs.github.com/en/packages/learn-github-packages/configuring-a-packages-access-control-and-visibility)
與 [REST Packages API](https://docs.github.com/en/rest/packages/packages)。

新上傳成功時，registry consumer 在獨立 cache 還原七套件，逐一比對還原 nupkg 與同次 run artifact 的
SHA-256 及 `.nupkg.metadata` source，再完成 SDK 8／10 的七種消費模式。
套件 tag 觸發的本機 feed 與 registry consumer 逐包核對 nuspec 的 `repository.commit` 與 tag 指向的提交。若七個 ID 均已存在，仍驗證上述來源、該版 registry metadata 與消費行為。首次發布 bytes 的比對證據保留於原發布 run。

GitHub NuGet 的 public 套件安裝仍需要驗證；Actions 使用具存取權的 `GITHUB_TOKEN`，
本機使用適當 scope 的 PAT classic。見 [GitHub NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)。

## 證據邊界與後期評估

PR 品質檢查、套件 tag 的上傳、public metadata、registry consumer 及文件 tag 的公開網站驗收各有對應 run 與來源。實際發行成品見 [0.0.5-preview 發布紀錄](0.0.5-preview.md) 與各版本發布紀錄。
nuget.org 以 0.1.0 起的 0.y.z 發布為目標（y ≥ 1），進度見[正式版路線](stable-release-roadmap.md)。GitHub Release、branch protection、required reviewers 及正式版維護線留待後期評估。

首個不帶任何 prerelease 後綴的公開版本另需逐規則設計理由、完整套件使用說明，以及有語意且隨 `.nupkg` 交付、由 consumer 可讀的 API XML 證據。這是待落實的產品 gate，並非現有 preview workflow 已自動檢查或本次已通過；正式版發布機制需另行實作與驗證。
