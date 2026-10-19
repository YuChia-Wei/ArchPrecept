# Apache-2.0 採用與發布前來源檢查

2026-10-19。負責人已選定 Apache-2.0，並確認主要來源為自己在 ai-collaboration-framework、dotnet-distributed-architecture-lab 及 dotnet-webapi-lab 的知識累積。當前版本為 0.0.1-preview，支援 .NET 8／10；正式版本政策及 nuget.org 留到正式版前處理。

## 已完成

- 採用官方 Apache-2.0 原文 LICENSE；NOTICE 標示 Copyright 2026 YuChia Wei；NuGet metadata 使用 Apache-2.0 SPDX expression，不同時設定 LicenseFile。
- LICENSE、NOTICE、THIRD-PARTY-NOTICES.md 隨四個套件封裝；build script 驗證 SPDX 及必要檔案存在。
- 34 個初始匯入 C# 檔案與固定上游快照逐檔內容一致（忽略行尾）：17 個 Contracts 來自 distributed architecture Lab，9 個 Analyzer 與 8 個測試來自 Framework；兩個文件覆蓋測試在本 repo 新增。
- Analyzer README 與 props template 差異核對為移植說明及路徑修改；severity template 相同。現行改名及 XML 文件改動保留於本 repo 歷史，不假定現行 bytes 與初始來源相同。
- 36 個 C# 檔案的現行路徑／匯入路徑／固定來源連結已建表；webapi Lab 明確列為較早參考，沒有冒認為直接逐檔來源。
- 31 個依賴版本已記錄 nupkg、nuspec 與 legal files 雜湊；七個 legacy URL 已以隨包條款或固定上游授權補查。22 個 MIT、8 個 Apache-2.0、1 個 Microsoft .NET Library 條款。
- 四個成品未內嵌第三方 DLL/source；保留 build/test、runtime PackageReference 與再散布範圍的差別。
- 文件／測試／templates 的外部 URL 與 copyright 標記檢視，未找到其他直接第三方授權標記；未進行全網相似性檢索，不以沒有標記推定絕對原創。

逐檔證據：[source-inventory.md](source-inventory.md)、[source-comparison-evidence.json](source-comparison-evidence.json)。依賴證據：[dependency-licenses.md](dependency-licenses.md)、[dependency-license-evidence.json](dependency-license-evidence.json)。

## 負責人來源補充已完成

已接受負責人對三個來源專案為自有知識累積的說明，不再重複要求一般 ownership 證明。

2026-10-19 負責人補充：Teddysoft 課程主要採用 Java，啟發的是與 AI 協作的模式。套件與分析器原本已在使用，也曾研究相關工具；看到課程以純文字比較處理這類需求後，想到將既有 .NET 生態系的分析器做法用於 AI 協作，以可執行的檢查支援規範，並進一步發展自己的套件與框架。

依此補充，課程關係記錄為 AI 協作方法的啟發；.NET 套件與分析器的技術來源為負責人既有使用、研究及實作累積。本次私人教材來源待確認項目已依負責人說明結案，沒有新增需要負責人確認的授權項目。此項為來源聲明，並非取得私人教材後的逐檔比對證據。

IIntegrationEvent 的 Medium 連結已按觀念參考保留：目前 interface 的成員與文章範例不同，沒有發現直接複製該範例的證據。它不額外形成一個泛泛的作者確認要求。

## 發布與可見性狀態

2026-10-19 負責人要求接續完成發布。PR #2 已合併，四個 `0.0.1-preview` 已由固定提交 `54b6a88fe373107f85a52caf79d129d396209cd4` 上傳 GitHub Packages，.NET 8／10 registry 安裝、執行與 Analyzer 診斷驗證通過。四套件版本與 repository linkage 皆已讀回。

實際可見性為 public，與原先 private 預期不同，因此發布及後續唯讀驗證工作流程的可見性斷言失敗。`PREVIEW_PUBLISH_ENABLED` 已設回 false，保留既有套件，等待負責人決定公開範圍；不將工作流程整體標為成功。來源與依賴授權檢查已完成，這項未決事項屬於發布存取範圍。完整證據見 [preview.md](preview.md)。

nuget.org 留到正式版前準備；新手入門另以本機 HTML 交付，位於 Git 忽略的 artifacts/handouts，不納入本 PR。

Source Link 可取回、symbols 決策、完整 public API baseline、依賴弱點查詢是其他發布品質工作，不混稱為本次授權已驗證事項；授權清查不等於弱點掃描。本次來源與依賴授權檢查範圍已完成，結論結合固定上游比對、依賴條款及負責人來源聲明，不宣稱完成未知外部資料的全面相似性檢索。
