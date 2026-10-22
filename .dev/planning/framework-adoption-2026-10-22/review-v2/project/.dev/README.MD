# ArchPrecept 開發導覽

> 待檢核草案。此目錄補上開發治理與接手入口；產品正式來源仍在既有 `docs/`。本次只形成文件與檔案計畫，尚未安裝或初始化 Framework。

## 從工作目的開始

| 要處理的工作 | 先讀來源 | 應得到的結果 |
|---|---|---|
| 產品範圍、能力與相依 | [產品決策](../docs/product/product-plan.md#3-已確認的關鍵決策)、[PRD](../docs/product/prd.md) | 明確的既有決策、Requirement 與尚未完成能力 |
| DDD／CA／Hex Arch 規則或相容性 | [採納文件](standards/framework-adoption.md)、[Diagnostic 目錄](../docs/diagnostics/README.md) | 適用範圍、規範來源、現行行為、例外及缺口 |
| Event Sourcing | [CT-005](../docs/product/event-sourcing-contract.md) | Replay／pending／version／commit 及基底行為限制 |
| Messaging | [CT-004](../docs/product/messaging-contract.md) | 訊息契約、Application port、host／adapter 的責任邊界 |
| 文件與規則追溯 | [治理規範](../docs/governance/document-governance-and-traceability.md) | 保留 `docs/`、`rules/` 與 `.dev/` 的不同 ownership |
| 套件／發布 | [現行發布紀錄](../docs/releases/0.0.2-preview.md)、[CI/CD 規則](../docs/releases/cicd-plan.md) | 分清 build、package、registry publication 與 consumer 證據 |
| 接手開發 | [GitHub Issues](https://github.com/YuChia-Wei/ArchPrecept/issues) | 以 live Issue 的 scope、驗收與當次授權開始，不將本頁當第二份 backlog |

## 已知承接項目

下表是來源索引，實際狀態以 GitHub 讀回為準，不在此同步維護完成百分比。

| 工作 | Owner 與界線 |
|---|---|
| [#6 規則及 Profile](https://github.com/YuChia-Wei/ArchPrecept/issues/6) | ArchPrecept 的權威、適用性與規則映射 |
| [#7 角色辨識及 DBA1016](https://github.com/YuChia-Wei/ArchPrecept/issues/7) | 合法領域事件 port 誤判、共用角色辨識及其驗收 |
| [#8 測試責任](https://github.com/YuChia-Wei/ArchPrecept/issues/8) | 對應上述行為與採用邊界的測試，避免只驗證 helper 實作 |
| [#11 ValueObject](https://github.com/YuChia-Wei/ArchPrecept/issues/11) | 不可變狀態與 Entity 參考兩項獨立規則，包含內建／自有契約 |
| [#1 Diagnostic 文件](https://github.com/YuChia-Wei/ArchPrecept/issues/1) | 讓使用者能判斷原因、修正、限制與例外 |
| [#5 採用回饋](https://github.com/YuChia-Wei/ArchPrecept/issues/5) | 保存真實 consumer 回饋及環境限制 |
| [Framework #179](https://github.com/YuChia-Wei/ai-collaboration-framework/issues/179) | Framework 與 ArchPrecept Contracts 選配相容映射；不接管本產品 Analyzer 實作 |

## 工作的最小交接

每次只為所選工作補必要資訊：目標與授權、既有決策／Requirement、可觀察的 Given／When／Then、預計變更、驗證與未評估範圍。引用既有 Issue、產品文件與證據，不複製成第二份台帳。

DDD 工作說明共同語言、Bounded Context、Aggregate 不變條件及例子；CA／Hex Arch 工作說明責任、依賴方向、port／adapter 與技術取捨。兩者相互約束，但不能用資料夾層次代替領域模型，也不能用領域物件名稱證明技術邊界已正確實作。

先依目前範圍確認問題或行為，再選最小必要步驟。已接受的設計沒有新衝突時直接重用；只有新的證據、版本或變更要求才需要重開決策。實作前檢查已存在的工作項目，避免建立重複 Issue。

## 驗證入口

| 變更類型 | 選擇方式 |
|---|---|
| 文件／採納草案 | 核對狀態、來源、相對連結、版本及差異；不以產品 build 代替內容檢核 |
| 受影響 .NET 專案 | 使用 [global.json](../global.json) 的 SDK，選定相應 build／tests |
| 套件結構或相依 | [Build-Packages.ps1](../eng/Build-Packages.ps1)；執行前確認 output 及現有產物 |
| SDK／安裝／公開 API 相容性 | [Test-PackageConsumer.ps1](../eng/Test-PackageConsumer.ps1) 的相關 consumer 情境 |
| 合併與發布 | [CI/CD 規則](../docs/releases/cicd-plan.md)；驗證結果與發布授權分開 |

## Framework 導入狀態

- 選配方向已接受；目前建議等待 v0.21.0 正式成品，尚未授權安裝。
- `AGENTS.md` 與本頁是導覽；[採納文件](standards/framework-adoption.md) 是範圍及差異草案。它們不證明 skill 已被 runtime 發現或執行。
- 不建立 `project-config.yaml`、新的 backlog／SQLite store 或空的 ADR／Workflow 目錄。既有來源已足夠時直接引用。
- v0.21.0 發布後核對已選 skill 的實際 ID、版本、知識依賴與讀取範圍，再產生新 candidate／file plan。舊 v0.20.2 的 386-file 預演僅供比較。
- 文件核准後仍須依當次指示決定是否寫入正式位置；產品程式、NuGet、CI、GitHub Issues 及安裝狀態不隨文件草案自動改變。
