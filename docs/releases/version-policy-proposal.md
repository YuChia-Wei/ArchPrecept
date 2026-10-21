# 版本維護規則提案

狀態：正式版政策仍暫緩決定。2026-10-21 依 CT-004／CT-005 套件拆分，建議下一版採 `0.0.2-preview`，已設定為本機候選；已發布版本仍為 `0.0.1-preview`。選擇理由與破壞性變更見 [候選變更](0.0.2-preview.md)。本次不啟用自動升版、不建立 tag，也不代表核准發布；以下正式版選項留待轉正式版前評估。

已確認支援範圍為 .NET 8／10；.NET 9 不列為承諾支援版本，.NET 11 待後續發布後評估。可編譯或曾通過 smoke test 不等於持續支援承諾。

## 三個獨立面向

| 面向 | 此次基線 | 意義 |
|---|---|---|
| PackageVersion | `0.0.2-preview`（本機候選） | 套件成品識別，六個套件暫採共同版號；已發布基線仍是四個 `0.0.1-preview` |
| Contracts TFM | `net8.0` | .NET 8 消費端可參考，較新 .NET 也能選用此資產；不代表所有執行行為皆已驗證 |
| Analyzer | `netstandard2.0`、Roslyn API 4.8 | Analyzer 在編譯器內執行，其相容性取決於 SDK／IDE 所帶的 Roslyn，不只消費端 TFM |

CI 使用真正的 SDK 8、10 分別還原套件、執行合規程式，並要求違規程式出現 DBA1001。這是套件安裝與載入的 smoke evidence，不是所有 API 的跨框架行為測試，也不代表所有 Visual Studio／Rider 版本已驗證。.NET 11 尚未納入驗證，不提前宣告支援。

## 可選策略

**建議優先考慮產品 SemVer**：`1.y.z` 表示 ArchPrecept 產品生命週期，支援的 .NET 版本由相容矩陣獨立維護。優點是 breaking change 能清楚反映在 major；不會因每年 .NET 發版被迫建立另一條維護線。

**若採你提出的 `11.y.z`**：將 `11` 定義為「對齊 .NET 11 的產品系列」，而非最低執行環境。該系列仍應保留 .NET 8 基線，除非另經明確決策及遷移公告。不得因 major 是 11 就把 Contracts 只編成 net11.0。

此策略的取捨是：若 .NET 11 系列中途需要不相容改動，`y`、`z` 不應承載破壞性變更。必須延後至下個系列，或接受產品 major 與 .NET major 脫鉤。因此在正式採用前需決定「對齊平台」與「自由進行 breaking release」哪一項優先。

## 建議的變更分級

| 變更 | 建議 |
|---|---|
| 錯誤修復、降低誤報、不擴大拒絕範圍 | patch |
| 向下相容的新 API、預設關閉的新規則 | minor |
| 移除／變更公開 API、提高最低 TFM 或 Roslyn 版本 | major |
| 新增預設 error、提高 severity、擴大規則導致原本合規程式無法建置 | breaking，依 major 處理 |
| Diagnostic ID、設定鍵或報告格式不相容 | major；保留淘汰與遷移說明 |

不重用 Diagnostic ID。修正誤報也需保留 regression test；「向下相容」是維護承諾及驗證結果，不能由功能性質推定。

## Preview 與維護提案

- 同一目標版本的預覽修訂可遞增 suffix，例如 `0.0.2-preview.1`；本次套件／API 邊界調整另設 `0.0.2-preview` 基線。每個已發布的 ID/version 永不覆寫。0.x 不承諾穩定 API，必須另列相容性與遷移影響。
- 六套件先採共同 release train，Analyzer 可單獨安裝，不因此依賴 Contracts；有獨立演進需要才拆分版號。
- 每次發版記錄來源 commit、依賴、支援矩陣、規則/API 變更及套件 SHA-256。
- .NET 8 先作相容下限；平台 EOL 與產品是否延長相容支援分開決策。若保留已 EOL 平台，要清楚標示產品測試範圍，不宣稱平台仍受 Microsoft 支援。
- 不先建立多條 `release/8`、`release/10`、`release/11` 分支；維護線與回補規則由負責人審查後決定。

轉正式版前再決定：採產品 SemVer 或平台 major、共同版號範圍、維護線與 breaking rule 的推出方式。此次候選版號不決定正式版政策；若之後要發布不同 bytes，仍須使用未發布的新版本，不覆寫已發布的 ID/version。
