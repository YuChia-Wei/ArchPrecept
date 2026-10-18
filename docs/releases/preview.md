# 0.0.1-preview 發布準備

2026-10-18：負責人允許準備 GitHub CI/CD、PR 與 GitHub Packages preview；若有顧慮可停在發布前設定。遠端已確認改名為 `YuChia-Wei/ArchPrecept`。本次採發布前準備：License 尚未選定，產品規劃 §14 的來源檢查亦未完成；不替負責人選 License，不上傳 registry、不建立 Release/tag。

## 套件 inventory

| Package ID | 內容 | 基線 |
|---|---|---|
| ArchPrecept.Analyzers.Core | `analyzers/dotnet/cs` 中的 Analyzer DLL，不含 Roslyn 或 Contracts runtime 相依 | netstandard2.0 / Roslyn 4.8 |
| ArchPrecept.Contracts.Domain | 領域參考契約 | net8.0 |
| ArchPrecept.Contracts.Application | 應用契約，依賴 Domain | net8.0 |
| ArchPrecept.Contracts.Messaging.Abstractions | 訊息抽象，依賴 Configuration.Abstractions 8.0.0 | net8.0 |

全部使用 `Directory.Build.props` 的 `0.0.1-preview`。只有這四個專案 opt in；未實作能力、tests、samples 不發布。這是此次 GitHub preview 的命名與封裝設定，不表示已取得 nuget.org ID。

## 驗證與 CI

負責人已確認目前支援 .NET 8／10；.NET 9 不列為承諾支援版本，.NET 11 待後續發布後再評估。Preview 維持現有版號安排，正式版版本維護政策暫緩決定。

```powershell
./eng/Build-Packages.ps1
./eng/Test-PackageConsumer.ps1 -SdkMajor 8
./eng/Test-PackageConsumer.ps1 -SdkMajor 10
```

安裝 global.json 指定的 .NET 10 SDK feature band（允許同 band patch）建置；消費測試各自選用指定 major 的最新已安裝 feature band。CI 的 Package CI 在 PR、main 和 codex 分支執行：Release build、既有 Analyzer regression suite、封裝內容/metadata 檢查、SHA-256，以及 SDK 8/10 的套件消費測試。每次 consumer 使用獨立 cache 與本機 feed，避免同版號快取遮蔽新產物；驗證 Analyzer-only、自訂 IRepository 違規及參考 Contracts 安裝與執行。

CI artifacts 是候選成品。公開 repo 的 Actions artifacts 不應視為私密儲存空間。套件 README 和 repository metadata 一併封裝；SDK 提供 repository/source-link 建置整合，但獨立 symbols 發布、完整 Source Link 對外取回測試、完整 public API baseline/provenance audit 仍屬公開發布前工作。

## GitHub Packages CD

`Publish GitHub preview` 只接受 main 的手動 workflow_dispatch，核對輸入版號與專案版號。預設 repository variable `PREVIEW_PUBLISH_ENABLED` 未設定，因此 gate 明確失敗，不能意外上傳。

啟用前需完成：

1. 檔案來源與發布權利檢查，決定 License 並加入根目錄 `LICENSE`（封裝會自動帶入）。
2. 檢查 GitHub Packages 的 package 存取權、repo permission inheritance、帳戶配額及費用；先保留 private，另行確認是否對團隊開放，不自動改成 public。個人帳戶的 private package 不等於企業的 internal visibility。
3. 建立／檢查 `github-packages-preview` Environment，可配置 required reviewer；此 workflow 不會替你建立 reviewer 規則。
4. 合併已通過 CI 的 PR，才將 repository variable `PREVIEW_PUBLISH_ENABLED` 設為 `true` 並手動執行。此次不設定此開關。

CD 會對該次 workflow 的固定 commit 重新走完整 CI；成功後下載**同一次 run 已驗證的 nupkg**，核對 SHA-256，以 job 級 `packages: write` 和 `GITHUB_TOKEN` 上傳，不重新打包、不需要長效發布 PAT。PR 不取得發布權限。重複版號會失敗，不以 `--skip-duplicate` 隱藏不一致；若四套件部分成功，先盤點 registry 與原 artifact，不刪除既有版本或重建同版號後盲目重試。

registry：`https://nuget.pkg.github.com/YuChia-Wei/index.json`。GitHub NuGet 安裝包含 public package 仍需驗證；本機一般用 PAT classic 的 `read:packages`，GitHub Actions 可在具存取權時用 GITHUB_TOKEN。Token 放使用者層級 NuGet 設定或 CI secret，不提交 repo、不寫入命令範例；消費專案應用 package source mapping，將 `ArchPrecept.*` 指定到 GitHub source。

實際發布後仍須讀回四套件的版號、可見性、repository linkage，再從 registry 以獨立 cache 安裝驗證，才可宣稱已發布完成。此次只完成 local-feed/CI 驗證，沒有 registry round trip。

## 日後 nuget.org 注意事項

- 決定 License，完成來源與第三方授權盤點；公開 repo 不等於授權條款已齊全。
- 確認四個 Package ID 可用、帳戶/組織 ownership 與交接方式；GitHub namespace 不會替你保留 nuget.org ID。
- Preview 仍是公開可取得的套件。ID/version 不可覆寫；unlist 不能視為完全撤回已被下載的成品。
- 建議使用 nuget.org Trusted Publishing（GitHub OIDC 短效憑證），將信任限制到指定 repo、workflow 與 environment；另建 nuget.org job，不沿用 GitHub Packages token。
- 完成 README、License、repository commit、Source Link/symbols、dependency/security 檢查與相容性報告，先以本機 feed 或 nuget.org 測試站驗證。
- 只有核准的原始 bytes 才能上傳；發布後檢查索引狀態並做真實 registry restore，不能只看 push exit code。

官方參考：[GitHub NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)、[nuget.org 發布](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package)、[Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)。

License 選型與發布缺口詳見[授權及來源盤點](licensing-assessment.md)。[版本規則提案](version-policy-proposal.md)保留至轉正式版前再評估。
