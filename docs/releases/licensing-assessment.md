# ArchPrecept 授權選型與發布缺口盤點

日期：2026-10-18。狀態：評估，不是授權決定或來源清查完成證明。程式基線為 `baeb302`；此次另將支援政策縮為 .NET 8／10，preview 維持 `0.0.1-preview`，正式版版本政策延後。未加入 LICENSE、未啟用發布、未合併 PR。

## 建議：Apache-2.0 優先，MIT 為簡單可行的替代

本建議以「允許商業與閉源產品採用、方便企業導入、未要求使用者公開其產品原始碼」為前提。ArchPrecept 同時有編譯期 Analyzer 與會成為消費產品相依的 Contracts，建議四個自有套件採一致的寬鬆授權。

| 選項 | 對 ArchPrecept 的優點 | 需要接受的條件 | 建議 |
|---|---|---|---|
| Apache-2.0 | 可供商業及閉源產品使用；有明確的貢獻者專利授權與專利訴訟終止條款；不授予品牌商標權 | 再散布須遵守 License、保留相關聲明、標記修改；若帶有 NOTICE，須保留適用聲明 | 優先：適合長期工程工具及外部貢獻 |
| MIT | 短、容易理解，允許商用、修改、再散布與閉源採用 | 保留 copyright 及 permission notice；文本沒有 Apache 那種明文專利授權機制 | 若最重視簡單與低管理成本，完全合理 |
| MPL-2.0 | 希望修改過的受涵蓋檔案，在對外散布時維持可取得原始碼；可與其他授權檔案組合 | 有檔案層級 copyleft 與對外散布原始碼義務，需多做合規管理 | 只有明確希望修改回饋時才考慮 |

Apache 的專利條款僅涵蓋貢獻者可授權且符合條件的專利，不能保證沒有任何第三方權利主張。MIT 或 Apache 都不要求使用 ArchPrecept 的一般閉源產品因使用套件就公開產品原始碼；再散布套件或其衍生內容仍須履行各條款。若希望禁止商業使用或禁止競爭者採用，這兩個 License 都不符合該目標，需另行評估。

目前不建議拆成 Analyzer 與 Contracts 不同 License、或額外建立雙授權方案，因為尚無可支持這種複雜度的需求。選定後可讓自有 code、tests、examples、隨附文件採一致授權；第三方內容仍保留其原授權。名稱與商標不因選定 Apache 而自動取得權利。

參考原文：[Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0)、[MIT](https://opensource.org/license/mit)、[Mozilla MPL FAQ](https://www.mozilla.org/en-US/MPL/2.0/FAQ/)。本表是工程發布選型摘要；有爭議的教材／僱傭／第三方權利應依實際條款確認。

## 已取得的來源證據與限制

- Git 本地歷史起於 `494109b`；`133ecc03f12414217454e89e6ee2d295226d7712` 是集中匯入 building blocks 與 analyzers 的 commit。它記錄本 repo 的匯入，不記錄每檔上游 commit 或授權依據。
- `git shortlog -s HEAD` 僅列一個 author 名稱。這不能證明所有內容都是該作者原創，或排除僱主、教材與其他貢獻者權利。
- 追蹤中的 C# 檔案共 36 個：逐檔 `git log --follow` 有 26 個可追至初次匯入、8 個追到 `d7e452b` 的改名、2 個起於 `d0d23e9` 的文件覆蓋測試。改名處中斷的歷史仍要人工回接，不能把它當作原創起點。詳見[檔案盤點](source-inventory.md)。
- 根目錄尚無 LICENSE／NOTICE／THIRD-PARTY-NOTICES；GitHub 亦未辨識 ArchPrecept 的 License。
- README 的 `dotnet-mq-arch-lab` 舊網址回傳 404；唯讀查詢本機 origin 確認現行遠端為 `YuChia-Wei/dotnet-distributed-architecture-lab`。`ai-collaboration-prompts-dotnet-backend` 已改名為 `YuChia-Wei/ai-collaboration-framework`。
- GitHub 未辨識 `dotnet-webapi-lab`、`dotnet-distributed-architecture-lab`、`ai-collaboration-framework` 的 License；後兩者本機 HEAD 根目錄也沒有 LICENSE。這是 metadata／根目錄檢查，不是對所有上游歷史或子目錄條款的完整檢查。
- 產品規劃 §14 已揭露 Teddysoft 課程啟發；公開課程頁只能支持課程背景，不能作為教材或程式碼再授權的依據。本次沒有取得私人教材或課程合約，因此不判定有直接引用，也不宣告已排除。
- `IIntegrationEvent.cs` 的 XML remarks 含 [Medium 文章](https://medium.com/@serhatalftkn/domain-events-vs-integration-events-understanding-the-differences-and-when-to-use-each-3977278034d3)連結。現行 interface 與文章示範的成員不同；僅這點不足以完成來源判定。需確認是觀念參考、文字翻譯或程式碼改作。這份 XML 文件會隨 Messaging 套件散布，不能只檢查 DLL。

## 發布前還缺什麼

| 項目 | 目前狀態 | 完成條件／責任 |
|---|---|---|
| 自有程式權利 | 待確認 | 負責人確認自行撰寫／可授權的範圍，以及是否有僱傭、客戶或共同作者權利限制；Git author 不替代此確認 |
| 26 個 src C# 檔案的來源鏈 | 已盤點、未逐檔結案 | 工程核對上游 repo、固定 commit、原始 path、差異與當時 License；改名中斷處回接歷史。記錄 own work／third-party／pending 及依據 |
| 課程關係 | 已有啟發揭露、無完整對照 | 負責人確認是否直接取用教材程式、文字或範例；如有，按實際條款取得許可、保留聲明，或另行處理相關內容；致謝不替代授權 |
| Contracts 的 Lab 上游 | repo 身分已釐清，檔案映射未完成 | 確認 Domain／Application／Messaging 的每檔來源及共同作者；自有程式可由權利人直接授權，無須假設一定先替整個 Lab 補 License |
| 文件、tests、templates | 已列入清單，未逐一核對外部來源 | 核對 10 個測試檔、17 份 Diagnostic 手冊及範例、README、template 和工程規範文字；測試不進 nupkg，但會在 repo 公開 |
| 第三方套件 | 已查 31 個 package/version metadata | 24 個有 SPDX expression；7 個只有 legacy licenseUrl，需取得固定版本條款及檢查 NOTICE／第三方內容。不能把 licenseUrl 當作已清查 |
| License 決策及套件 metadata | 待負責人選定 | 加入正確權利人資訊及 LICENSE；可採 SPDX `PackageLicenseExpression`（如 Apache-2.0）或 `PackageLicenseFile`，兩者勿同時設定；以最終 nupkg 再驗證 |
| Attribution／NOTICE | 待來源檢查結果 | 根據實際義務建立 THIRD-PARTY-NOTICES；如有需保留的 NOTICE 則帶入。Apache-2.0 不代表必須憑空新增第三方致謝 |
| GitHub 存取與費用 | 尚未執行 registry round trip | 確認 private package 可見性／繼承權限、consumer repo access、帳戶配額、Environment 與發布開關，發布後用獨立 cache 還原 |
| 正式公開發布品質 | 部分完成 | 尚缺完整 public API baseline、Source Link 可取回驗證、symbols 決策及固定依賴／弱點檢查；現有 build、51 tests、封裝檢查和 smoke tests 不替代這些工作 |

「選 License」與「有權把所有內容按該 License 發布」是兩個不同問題。此次沒有發現足以直接認定侵權的證據，也沒有完成可排除所有來源問題的比對。

產品文件中的 Canonical Source Owner 是工程維護責任；把程式搬到本 repo 不會自動移轉其他人的著作權。

## 依賴與實際封裝範圍

本次讀取五個專案的 `obj/project.assets.json` 及已還原套件的 `.nuspec`，得到 31 個 package/version（包括 build/test；不代表 31 個 runtime dependencies）。完整表見[依賴盤點](dependency-licenses.md)。

- Messaging runtime 直接依賴 `Microsoft.Extensions.Configuration.Abstractions/8.0.0`，間接依賴 `Microsoft.Extensions.Primitives/8.0.0`，兩者 metadata 均為 MIT。
- Analyzer 以 Roslyn 4.8 建置；測試使用 Roslyn 5.0。已解析的 Microsoft.CodeAnalysis 套件 metadata 為 MIT；xUnit 大多為 Apache-2.0。
- 本機候選 nupkg 只包含各自的 ArchPrecept DLL、README／metadata，Contracts 另有 XML 文件；沒有將 Roslyn、xUnit 或其他依賴 DLL 直接複製進套件。
- 依賴透過 NuGet 分別還原與把依賴 DLL／原始碼複製進成品，散布範圍不同。不能因只是 PackageReference 就忽略條款，也不能誤稱所有 build/test 套件都會隨產品 DLL 散布。

## GitHub 先行、nuget.org 作公開採用通路

初期只在 GitHub Packages 發布是合理方案，尤其目前主要用於自己的 repo 與受控試用。技術上不必為了正式版而搬離 GitHub；是否使用 nuget.org 取決於受眾，而非版本字串。

GitHub NuGet 的限制是包含 public package 也要驗證身分；本機通常需 PAT classic，跨 repo 的 Actions 要配置 package access。Private packages 的 storage／transfer 受帳戶方案及計費影響，本次未查個人帳戶額度，不宣稱全部免費。自動發布使用 GITHUB_TOKEN，不需放長效 write PAT。

若 ArchPrecept 要成為可讓外部團隊直接採用的開源工具，建議正式公開推出前準備 nuget.org；也可以先在 nuget.org 發 preview，不必等 1.0。優點是正常 NuGet feed 的探索與安裝體驗，減少使用者的 GitHub 驗證配置。仍可保留 GitHub Packages 作受控 preview。

轉換時保留同一 Package ID，若同一 ID/version 同時出現在兩個 feed，應使用相同原始 nupkg，不重建不同內容；消費端設定來源映射避免來源混淆。先確認 nuget.org ID/ownership、License/來源、發布者權限及 CI Trusted Publishing，發布後等待索引並驗證真實 restore。選擇 private GitHub feed 不會自動解除第三方內容的授權義務。

官方來源：[GitHub NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)、[GitHub Packages 計費](https://docs.github.com/en/billing/concepts/product-billing/github-packages)、[NuGet.org 概覽](https://learn.microsoft.com/en-us/nuget/nuget-org/overview-nuget-org)、[Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)。
