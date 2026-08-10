# .NET Engineering Guardrails 文件索引

此目錄保存產品層級、對外可閱讀且需長期維護的正式文件。

## 正式文件

| Document ID | 文件 | 狀態 | 正式路徑 |
|---|---|---|---|
| `DOC-PRODUCT-PLAN` | 產品規劃書 | `draft` | `docs/product/product-plan.md` |
| `DOC-PRD` | 產品需求文件（PRD） | `draft` | `docs/product/prd.md` |
| `DOC-GOV-TRACEABILITY` | 文件治理與規則追溯規範 | `draft` | `docs/governance/document-governance-and-traceability.md` |
| `DOC-DIAGNOSTICS` | 分析器 Diagnostic 說明書 | `draft` | `docs/diagnostics/README.md` |

## 正式來源原則

- Markdown 是 Repository 內唯一可編輯的正式來源。
- Word、PDF 等格式為衍生輸出，不得與 Markdown 平行人工修改。
- 正式文件使用穩定檔名；版本與狀態由文件 Metadata、Git commit、Tag 與 Release 保存。
- 未來導入 AI Collaboration Framework 後，`docs/` 保留產品文件責任；`rules/` 保存機器可讀規則；`.dev/` 承接需求切片、規格、工作流程與證據。

詳細規則請閱讀 [`governance/document-governance-and-traceability.md`](governance/document-governance-and-traceability.md)。
