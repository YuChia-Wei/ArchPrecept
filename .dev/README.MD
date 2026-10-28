# ArchPrecept 開發導覽

> 本頁補上開發治理與接手入口；產品正式來源仍在 `docs/`。planning 中的副本尚未生效，安裝狀態另以 lock／inspect 證據判定。

## 從工作目的開始

| 要處理的工作 | 先讀來源 | 應得到的結果 |
|---|---|---|
| 產品範圍、能力與相依 | [產品決策](../docs/product/product-plan.md#3-已確認的關鍵決策)、[PRD](../docs/product/prd.md) | 明確的既有決策、Requirement 與尚未完成能力 |
| DDD／CA／Hex Arch 規則或相容性 | [採納文件](standards/framework-adoption.md)、[Diagnostic 目錄](../docs/diagnostics/README.md) | 適用範圍、規範來源、現行行為、例外及缺口 |
| Event Sourcing | [CT-005](../docs/product/event-sourcing-contract.md) | Replay／pending／version／commit 及基底行為限制 |
| Messaging | [CT-004](../docs/product/messaging-contract.md) | 訊息契約、Application port、host／adapter 的責任邊界 |
| 文件與規則追溯 | [治理規範](../docs/governance/document-governance-and-traceability.md) | 保留 `docs/`、`rules/` 與 `.dev/` 的不同 ownership |
| 套件／發布 | [現行發布紀錄](../docs/releases/0.0.3-preview.md)、[CI/CD 規則](../docs/releases/cicd-plan.md) | 分清 build、package、registry publication 與 consumer 證據 |
| 接手開發 | [GitHub Issues](https://github.com/YuChia-Wei/ArchPrecept/issues) | 以 live Issue 的 scope、驗收與當次授權開始，不將本頁當第二份 backlog |

## 既有交付與後續工作索引

下表保留來源索引，並非待辦清單。0.0.3-preview 已交付 #1／#6／#7／#8／#9／#11；#5 第二採用驗收亦已完成，#10 P2 不在本次交付。實際狀態以 GitHub 讀回為準。

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

每次只補所選工作的必要資訊。公開 Issue／PR 保存產品目標、既有決策／Requirement、可觀察的 Given／When／Then、設計、變更、技術驗證及未評估範圍；授權與交接留在對話及既有工作流程。未選 workflow-records，不代表改把操作紀錄存入 Issue。引用既有來源，不建立第二份台帳。

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

## 開發經驗

開發時的設計與驗證成本依 [AGENTS.md 的變更與驗證規則](../AGENTS.md#變更與驗證)處理。經驗回顧見 [LESSON：避免過度設計與過度測試](lessons/2026-10-29-proportionate-design-and-testing.md)；回顧本身不增加測試或執行步驟。

## Framework 導入狀態

- 正式 v0.21.0 選配與逐檔預覽見 本機 `.dev/planning/framework-adoption-2026-10-24/PLAN.md`（依使用者指示不提交）；共享選配來源為 [installation.json](../.ai/project/installation.json)。此計畫的內容確認與安裝授權依使用者當次指示。
- `AGENTS.md` 與本頁是導覽；[採納文件](standards/framework-adoption.md) 保存已採納範圍及差異。它們不證明 skill 已被 runtime 發現或執行。
- 不建立 `project-config.yaml`、新的 backlog／SQLite store 或空的 ADR／Workflow 目錄。既有來源已足夠時直接引用。
- 16 skills／2 knowledge／16 reference bindings 已按正式成品重建。選入不等於執行；三個 context skills 分別處理初始化／事實 refresh、政策維護及唯讀稽核。舊版預演不是本版安裝證據。
- 本次安裝與指引已獲授權；產品程式、NuGet、CI 與 GitHub Issues 不屬於此次變更。後續操作仍依當次指示。
- adr-author／pr-author 的工具操作需 Python、PyYAML、jsonschema，以及操作所需的 filesystem／Git／gh。此次只安裝能力，不建立預設 notes/adrs、notes/pull-requests 或其他 store；首次記錄工作先明選符合既有文件治理的位置。
