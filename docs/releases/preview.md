# 0.0.1-preview 發布紀錄

2026-10-19：來源與依賴授權檢查已完成，採 Apache-2.0，詳見[來源檢查結果](licensing-assessment.md)。負責人要求接續發布後，四個 `0.0.1-preview` 已上傳 GitHub Packages，並通過 .NET 8／10 registry 消費驗證。實際可見性為 **public**，與原先 private 預期不同；可見性斷言失敗，整體 workflow 不是成功狀態。後續發布開關已設為 false，等待負責人決定公開範圍。nuget.org 留到正式版前，未建立 GitHub Release/tag。

## 此次發布證據

- 原始碼：`54b6a88fe373107f85a52caf79d129d396209cd4`，由 [PR #2](https://github.com/YuChia-Wei/ArchPrecept/pull/2) 合併。
- [固定來源 main CI](https://github.com/YuChia-Wei/ArchPrecept/actions/runs/39207609688)：全部成功。
- [發布 run](https://github.com/YuChia-Wei/ArchPrecept/actions/runs/39207624875)：同次 build／51 tests／封裝檢查／.NET 8、10 consumer 成功，四次 push 成功；後續 private 可見性斷言失敗。
- [唯讀驗證 run](https://github.com/YuChia-Wei/ArchPrecept/actions/runs/39208069155)：四套件皆為 `0.0.1-preview`、public、連結 `YuChia-Wei/ArchPrecept`；兩個 registry consumer jobs 成功，包括獨立 cache restore、執行與預期 DBA1001；metadata job 因 private 預期不符而失敗。驗證流程由 [PR #3](https://github.com/YuChia-Wei/ArchPrecept/pull/3) 新增。
- 已發布成品沒有刪除、覆寫或重新上傳。本機原始成品與 SHA256SUMS 保留於 Git 忽略的 `artifacts/published-0.0.1-preview/`。

| 已發布 nupkg | SHA-256 |
|---|---|
| ArchPrecept.Analyzers.Core.0.0.1-preview.nupkg | `3d7045d6bd5e4ff26ba7ad284d27e85bdf9060791f858b600f7922470400d168` |
| ArchPrecept.Contracts.Application.0.0.1-preview.nupkg | `0073524f5a6871b0f81b536ae75262dcb6c4e06cf7e0e7131531bc3890d6d54b` |
| ArchPrecept.Contracts.Domain.0.0.1-preview.nupkg | `4e9f223c7ebfc0e2ed9fe31fdcfa3d047717c47d653dbf46b1317bf86a72405b` |
| ArchPrecept.Contracts.Messaging.Abstractions.0.0.1-preview.nupkg | `588f0fe3a148a394bb3f4c4fe65fa20196f496fbc86bf1ee5e33300c41240044` |

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

`Publish GitHub preview` 只接受 main 的手動 workflow_dispatch，核對輸入版號與專案版號。repository variable `PREVIEW_PUBLISH_ENABLED` 只有 true 才允許發布；此次發布後因可見性不符已設為 false。

啟用前需完成：

1. Apache-2.0、LICENSE、NOTICE、套件 SPDX 與本次來源確認已完成；依下列操作條件安排發布。
2. 確認 GitHub Packages 的實際存取權與可見性，不能只依預設值推定 private。此次四套件讀回為 public，須由負責人決定公開範圍。帳戶剩餘配額及費用尚未取得帳務證據；成功上傳不代表已驗證帳務。個人帳戶的 private package 不等於企業的 internal visibility。
3. 已建立 `github-packages-preview` Environment，目前沒有 required reviewer 規則。
4. 合併已通過 CI 的 PR 後才啟用發布。此次版號已存在，不能重跑上傳；可獨立執行 `Verify published GitHub preview` 進行唯讀驗證。

CD 會對該次 workflow 的固定 commit 重新走完整 CI；成功後下載**同一次 run 已驗證的 nupkg**，核對 SHA-256，以 job 級 `packages: write` 和 `GITHUB_TOKEN` 上傳，不重新打包、不需要長效發布 PAT。PR 不取得發布權限。重複版號會失敗，不以 `--skip-duplicate` 隱藏不一致；若四套件部分成功，先盤點 registry 與原 artifact，不刪除既有版本或重建同版號後盲目重試。

registry：`https://nuget.pkg.github.com/YuChia-Wei/index.json`。GitHub NuGet 安裝包含 public package 仍需驗證；本機一般用 PAT classic 的 `read:packages`，GitHub Actions 可在具存取權時用 GITHUB_TOKEN。Token 放使用者層級 NuGet 設定或 CI secret，不提交 repo、不寫入命令範例；消費專案應用 package source mapping，將 `ArchPrecept.*` 指定到 GitHub source。

此次已完成四套件版號、可見性、repository linkage 讀回及 registry round trip；套件發布與使用驗證成功，可見性符合預期的驗收尚未完成，不能把兩者混為整體 workflow 成功。

## 日後 nuget.org 注意事項

- 沿用已核准的 Apache-2.0 及已完成的來源確認；來源或依賴變動後重新檢查其授權與散布範圍。
- 確認四個 Package ID 可用、帳戶/組織 ownership 與交接方式；GitHub namespace 不會替你保留 nuget.org ID。
- Preview 仍是公開可取得的套件。ID/version 不可覆寫；unlist 不能視為完全撤回已被下載的成品。
- 建議使用 nuget.org Trusted Publishing（GitHub OIDC 短效憑證），將信任限制到指定 repo、workflow 與 environment；另建 nuget.org job，不沿用 GitHub Packages token。
- 完成 README、License、repository commit、Source Link/symbols、dependency/security 檢查與相容性報告，先以本機 feed 或 nuget.org 測試站驗證。
- 只有核准的原始 bytes 才能上傳；發布後檢查索引狀態並做真實 registry restore，不能只看 push exit code。

官方參考：[GitHub NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)、[nuget.org 發布](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package)、[Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)。

License 選型與發布缺口詳見[授權及來源盤點](licensing-assessment.md)。[版本規則提案](version-policy-proposal.md)保留至轉正式版前再評估。
