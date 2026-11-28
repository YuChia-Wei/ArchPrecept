# ArchPrecept 開發導覽

[根目錄協作入口](../AGENTS.md)定義開發邊界、授權、驗證與暫存資料的共同規則。

| 工作 | 來源 |
|---|---|
| 產品範圍、能力、相依與驗收 | [產品決策](../docs/product/README.md#已確認的產品決策)、[PRD](product/prd.md) |
| DDD／CA／Hex Arch 規則或相容性 | [Framework 採用](standards/framework-adoption.md)、[診斷目錄](../docs/diagnostics/README.md) |
| Event Sourcing 的 replay、pending events、版本與提交 | [Event Sourcing 契約](../docs/product/event-sourcing-contract.md) |
| Messaging 契約、Application port 與 host／adapter 責任 | [Messaging 契約](../docs/product/messaging-contract.md) |
| 文件正式來源、Rule 與 Diagnostic 追溯 | [文件治理與規則追溯](governance/document-governance-and-traceability.md) |
| 版本、相容性、交付 | [版本政策](../docs/releases/version-policy.md)、[CI/CD 規則](releases/cicd-plan.md) |
| 既有工作、驗收與實作範圍 | [ArchPrecept Issues](https://github.com/YuChia-Wei/ArchPrecept/issues) |

領域設計以共同語言、Bounded Context、Aggregate 不變條件及行為例子表達。CA／Hex Arch 工作交代責任、相依方向、port／adapter 和技術取捨。任務需要兩者時，核對技術邊界如何維持領域規則。

## 驗證入口

| 變更類型 | 驗證 |
|---|---|
| 文件／規範 | 核對內容、適用性、來源、連結與差異 |
| 受影響的 .NET 專案 | 核對 [global.json](../global.json) 與已安裝 SDK，選擇對應建置與測試 |
| 套件結構或相依 | [Build-Packages.ps1](../eng/Build-Packages.ps1) |
| SDK、安裝與公開 API 相容性 | [Test-PackageConsumer.ps1](../eng/Test-PackageConsumer.ps1) 的對應 consumer 情境 |
| 合併與發布 | [CI/CD 規則](releases/cicd-plan.md)的適用檢查及[首個正式版條件](releases/cicd-plan.md#首個正式版條件) |
