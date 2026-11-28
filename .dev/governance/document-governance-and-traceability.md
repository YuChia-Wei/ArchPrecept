---
document_id: "DOC-GOV-TRACEABILITY"
status: "draft"
---

# 文件治理與規則追溯

本文件整理文件責任、規則追溯與來源衝突的處理方式。標示為草案的文件按其中已確認的適用決策執行，其他提案保留待決狀態。

## 正式來源

| 內容 | 維護位置 |
|---|---|
| 對外產品定位、能力與決策 | [產品文件](../../docs/product/README.md) |
| 需求、Requirement ID 與驗收條件 | [PRD](../product/prd.md) |
| 契約、規則選擇、適用範圍與方法論 | [產品契約與規則設計](../../docs/product/README.md) |
| 現行 Diagnostic 行為、修正方法與限制 | [診斷目錄](../../docs/diagnostics/README.md) |
| Rule ID、適用條件與 Diagnostic 映射 | [Rule Catalog](../../rules/catalog.xml)及其 [schema](../../rules/catalog.xsd) |
| Consumer 設定結構 | [設定 schema](../../rules/configuration.xsd) |
| 維護版本、相容性與交付流程 | [版本政策](../../docs/releases/version-policy.md)、[CI/CD 規則](../releases/cicd-plan.md) |
| Framework 知識與專案規範的適用範圍 | [Framework 採用](../standards/framework-adoption.md) |

每項內容維持一份穩定路徑的正式來源。文件可引用 Catalog，設定說明須對應現行 schema。Word／PDF 等閱讀格式由 Markdown 產生並標示來源版本，修訂回到正式來源。正式發布的產品說明可保留對應版本的行為與使用資訊。文件歷史由 Git 保存，正文直接更新理由、決策、契約與驗收。

專案文件引用外部工具、規範或成品時，依可辨識的公開版本引用。發布說明可記錄成品對應的 Git 提交 SHA。AI Collaboration Framework 的選配、完整版本與安裝一致性由 installer 的受管狀態維護。

產品定位、能力與共用政策變更時維護產品文件；需求、優先順序、驗收或 Requirement 與規則的關係變更時維護 PRD。Requirement ID 在章節重排時沿用。更改已發布識別碼及相容性的條件依版本政策。單一 Issue／PR 可引用相關需求和規則，記錄該次變更的範圍、驗收與結論。測試輸出、原始工作紀錄、備份與研究過程留在 Git 之外，位置依 [協作入口](../../AGENTS.md#暫存與工作資料)處理。

## 規則追溯與缺口

從產品需求及設計理由可找到 Rule、適用 Profile 與角色、Diagnostic 或其他執行機制，以及對應的實作與驗證入口。從公開 Diagnostic 或 Architecture Test 可反查規則、需求及設計理由。追溯沿用已有檔案與識別碼，包含規則及其證據契約需要的資料。設定與適用性 Diagnostic 說明自身契約。

調整公開規則時，同步核對已交付的執行機制、Diagnostic 說明、Help Link 和可觀察行為的測試。機械判定須有對應機制及驗證；輔助檢查須列出工具能判定的範圍及仍需審查的條件。若缺少角色映射、環境或執行結果，標示為未評估。規則來源、機制或必要驗收有缺口時，說明受影響範圍並依適用的發布要求處理。逐規則說明、套件使用文件及 API XML 的驗收見[首個正式版條件](../releases/cicd-plan.md#首個正式版條件)。

## 來源與衝突

產品規則說明應分別交代可查證的外部方法論主張、其支持範圍，以及 ArchPrecept 選擇的做法、理由與代價。私人討論及跨專案研究只供釐清背景，程式碼來源歸因依各自授權維護。參見 [規則設計依據](../../docs/product/rule-design-rationale.md)及[方法論來源](../../docs/product/methodology-references.md)。

同一適用範圍的有效要求互斥時，指出受影響的 Rule、Diagnostic、Profile、角色及場景，列出雙方來源、狀態、原始主張、互斥條件與 consumer 影響。只暫停依賴該衝突的判斷，提出可行處置及代價。決策成立後直接更新規則理由、產品需求與驗收。若方法論容許不同做法，依本產品已確認的政策和適用條件執行。
