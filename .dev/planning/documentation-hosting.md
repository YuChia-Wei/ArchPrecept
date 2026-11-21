# 套件說明網站的託管選擇

建議用 **DocFX 產生靜態網站，部署到 GitHub Pages**。ArchPrecept 的程式、Markdown、發布 tag 和 CI 都在 GitHub，公開 repository 已符合免費 Pages 資格。2026-11-21 查詢時，預設分支為 `main`，GitHub 回報 `has_pages=false`。若採預設網域，預期入口為 `https://yuchia-wei.github.io/ArchPrecept/`。網站版本與內容範圍見[正式版路線](../../docs/releases/stable-release-roadmap.md)與 [Issue #21](https://github.com/YuChia-Wei/ArchPrecept/issues/21)。

## 免費方案比較

以下以公開靜態文件、平台提供的 HTTPS 網址及一般 Linux runner 為前提，無須購買網域。

| 方案 | 部署及維護 | 適用條件 |
|---|---|---|
| **GitHub Pages** | 在 Pages 選 GitHub Actions，文件 workflow 建置 DocFX 後上傳靜態成品。部署 job 使用 `github-pages` environment、`pages: write` 與 `id-token: write`，可沿用 repository 權限管理。 | 公開 repo 的標準 GitHub-hosted runner 運算免費。站點上限 1 GB，每月頻寬軟限 100 GB。專案站點位於 `/ArchPrecept/` 子路徑。[Pages 資格與網址](https://docs.github.com/en/pages/getting-started-with-github-pages/what-is-github-pages) · [用量限制](https://docs.github.com/en/pages/getting-started-with-github-pages/github-pages-limits) · [Actions 計費](https://docs.github.com/en/billing/concepts/product-billing/github-actions) · [部署 workflow](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages) |
| **Cloudflare Pages Free** | 可由相同的 GitHub 建置 job 將 DocFX 輸出交給 Wrangler，另需維護 Cloudflare 帳戶、Pages 專案及部署 token。若希望每個同 repo 的 PR 自動取得預覽網址，可改選 Git 整合並確認其建置環境可執行所選 DocFX。 | 預設網域為 `<project>.pages.dev`。免費 Git 建置每月 500 次、一次 1 個，單次上限 20 分鐘；每站上限 20,000 個檔案。團隊需要平行審閱多個文件版本時，分支預覽的價值較高。[Git 整合](https://developers.cloudflare.com/pages/get-started/git-integration/) · [預覽行為](https://developers.cloudflare.com/pages/configuration/preview-deployments/) · [免費限制](https://developers.cloudflare.com/pages/platform/limits/) · [上傳既有成品](https://developers.cloudflare.com/pages/how-to/use-direct-upload-with-continuous-integration/) |

## 首次部署的實作範圍

1. 先用既有開發 SDK 和固定版本的 DocFX 產生可本機瀏覽的 HTML。從 `docs/` 選入安裝、範本、套件、診斷與版本頁，以對應版本的 Contracts DLL 和原生 XML 註解產生 API 參考。機器規則沿用 `rules/catalog.xml`。[DocFX 支援的輸入](https://dotnet.github.io/docfx/)
2. 首頁標示目前公開預覽版。歷史版先連到對應的發布 tag；未發布的內容標示為開發中。首次正式版再提供 `versions/0.1.0/` 與 `stable/` 入口。版本路徑以站點為基準，保留 GitHub 專案前綴。
3. 抽查「安裝 → 產生 Price → DBA1018 → 修正」和一個 Contracts API 頁。現有手冊另有指向站點來源以外的連結，例如安裝頁的 `../samples/Products/README.md`，以及診斷目錄的 `../../rules/catalog.xml` 和 severity 範本。產生版應將這些目標指向對應發布 commit 的 GitHub 檔案，並在 `/ArchPrecept/` 下檢查導覽與樣式。
4. 候選網站可瀏覽後再啟用 Pages，以專用文件 workflow 發布該 HTML。從公開網址重走上述路徑並記錄實際 URL 與來源提交。公開 URL 確認後再評估後續套件的 `PackageProjectUrl`。
