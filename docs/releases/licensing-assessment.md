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

## 僅需負責人補充確認

已接受負責人對三個來源專案為自有知識累積的說明，不再重複要求一般 ownership 證明。

仍有一個無法靠公開來源自行結案的具體問題：Teddysoft 私人教材／課程練習是否有直接帶入本產品的文字、程式或完整範例，還是僅受觀念啟發後自行實作？若只有後者，記錄此確認即可關閉這個來源缺口；若有直接帶入，請指出檔案與可適用的授權條款，再逐項處理，不能以致謝取代許可。

IIntegrationEvent 的 Medium 連結已按觀念參考保留：目前 interface 的成員與文章範例不同，沒有發現直接複製該範例的證據。它不額外形成一個泛泛的作者確認要求。

## 發布仍保持關閉

負責人要求來源檢查完成後再安排 GitHub Packages 發布。本次不設定 PREVIEW_PUBLISH_ENABLED、不建立 tag/Release、不上傳 registry、不合併 PR。來源最後確認完成後，再安排 Environment／存取權／配額檢查、固定 commit 的 CI 與發布後 registry restore。

nuget.org 留到正式版前準備；新手入門另以本機 HTML 交付，位於 Git 忽略的 artifacts/handouts，不納入本 PR。

Source Link 可取回、symbols 決策、完整 public API baseline、依賴弱點查詢是其他發布品質工作，不混稱為本次授權已驗證事項；授權清查不等於弱點掃描。私人教材未取得，所以目前不宣告所有來源已最終結案。
