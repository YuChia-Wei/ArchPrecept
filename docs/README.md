# ArchPrecept 文件索引

此目錄保存產品目前的設計、規則、使用及發布說明。文件直接表達有效理由與契約；修改歷程由 Git 保存。

## 安裝與判讀

- [安裝與使用](getting-started.md)：套件選擇、feed、設定、建置、SARIF 與常見問題。
- [規則設計依據](product/rule-design-rationale.md)：每條規則的設計理由、取捨、方法論關係及未決範圍。
- [方法論來源](product/methodology-references.md)：官方作者／組織來源、支持界線與使用條件。
- [Analyzer 設定與角色](product/analyzer-policy.md)：本版 Profile、mapping、severity、評估狀態與限制。
- [Diagnostic 說明](diagnostics/README.md)：逐項 predicate、正反例、修正方向與 suppression。
- [Event Sourcing 契約](product/event-sourcing-contract.md)、[Messaging 契約](product/messaging-contract.md)：選配能力及責任邊界。

## 產品與交付

| 文件 | 用途 |
|---|---|
| [產品規劃書](product/product-plan.md) | 能力邊界與目前產品決策；未決項維持明示 |
| [PRD](product/prd.md) | Requirement、優先序與可觀察驗收 |
| [文件治理](governance/document-governance-and-traceability.md) | docs／rules／開發資料分工、來源及內容品質 |
| [版本與發布準備政策](releases/version-policy-proposal.md) | 首個 non-preview 之前無舊版相容義務、正式版文件與套件準備條件 |
| [CI/CD 規則](releases/cicd-plan.md) | 現行 preview 建置、驗證與發布流程 |
| [0.0.3-preview 發布紀錄](releases/0.0.3-preview.md) | 已發布版本及其驗證證據 |
| [授權與來源檢查](releases/licensing-assessment.md) | 程式與依賴的授權證據；不作為方法論依據 |

## 來源責任

- Markdown 是產品說明的唯一可編輯正式來源；Word／PDF 是衍生輸出。
- [rules/catalog.xml](../rules/catalog.xml) 保存產品機器規則 metadata；[configuration.xsd](../rules/configuration.xsd) 保存已支援設定結構，不另建立平行 Catalog。
- 正式文案寫出設計理由及權威參考資料；私人討論、跨 repo 研究摘錄與中間草案不併入產品文件。
- 文件狀態與個別已確認政策分開；完整 PRD 的 draft 狀態不撤銷其中已明確確認的規則，也不代表所有規劃已實作。
- API XML、Diagnostic HelpLink、套件 README 與 docs 應指向同一版本的實際能力。來源、設定、程式、隨包內容及 consumer 可讀性分別驗證。
