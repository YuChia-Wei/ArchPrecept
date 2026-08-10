---
title: "Engineering Guardrails 分析器 Diagnostic 說明書"
document_id: DOC-DIAGNOSTICS
document_version: 0.1
status: draft
lang: zh-TW
canonical_path: docs/diagnostics/README.md
---

# 分析器 Diagnostic 說明書

本目錄是 `EngineeringGuardrails.Analyzers.Core` 目前所有 Roslyn Diagnostic 的可閱讀正式說明來源。每份手冊說明實際觸發條件、違規與合規範例、修正方式、已知限制、設定／suppression 選項，以及對應的實作與測試。

這些手冊記錄的是目前分析器已實作的行為；它們不是新的 Rule ID、規則目錄或架構決策。若分析器的實作、Diagnostic ID 或預設嚴重性變更，必須同步更新相應手冊與自動驗收測試。

## 使用方式

1. 從下表選取建置或 IDE 回報的 `DBA` ID。
2. 先確認「適用範圍與觸發條件」與「已知限制與例外」是否適用於目標程式碼。
3. 依「修正指引」調整程式碼；若例外是經過刻意核准的架構選擇，依「設定與 Suppression」留下可追溯的 suppression。

分析器專案也提供 [severity 範本](../../src/Analyzers/EngineeringGuardrails.Analyzers.Core/templates/analyzer-severity.editorconfig)；接線與建置方式請見其 [README](../../src/Analyzers/EngineeringGuardrails.Analyzers.Core/README.md)。

## Diagnostic 目錄

| Diagnostic ID | 摘要 | 預設嚴重性 |
|---|---|---|
| [DBA1001](DBA1001.md) | 維持 Aggregate、Query 與相容性 Repository 合約的語意邊界。 | `error` |
| [DBA1002](DBA1002.md) | 禁止 Use Case 或 Handler 注入 `IServiceProvider`。 | `error` |
| [DBA1003](DBA1003.md) | 禁止 Aggregate／Entity 相依基礎設施型別。 | `warning` |
| [DBA1004](DBA1004.md) | 要求 API Controller 宣告 `ApiControllerAttribute`。 | `warning` |
| [DBA1005](DBA1005.md) | 禁止 Controller 直接存取持久化。 | `error` |
| [DBA1006](DBA1006.md) | 禁止 Controller 直接建立 Handler 或 Use Case。 | `error` |
| [DBA1007](DBA1007.md) | 要求 Mapper 為 `static` 類別。 | `warning` |
| [DBA1008](DBA1008.md) | 禁止 Mapper 相依應用或持久化服務。 | `warning` |
| [DBA1009](DBA1009.md) | 要求事件溯源 Aggregate 經由事件轉換變更狀態。 | `warning` |
| [DBA1010](DBA1010.md) | 禁止 Use Case 使用 Service Locator 或屬性注入。 | `error` |
| [DBA1011](DBA1011.md) | 禁止 Handler 混合 Command 與 Query 職責。 | `error` |
| [DBA1012](DBA1012.md) | 禁止 Use Case 直接建立 Repository。 | `error` |
| [DBA1013](DBA1013.md) | 禁止 Projection 執行持久化寫入。 | `warning` |
| [DBA1014](DBA1014.md) | 要求 Controller 依賴 Use Case 邊界。 | `error` |
| [DBA1015](DBA1015.md) | 驗證 Use Case 的標準契約形狀。 | `error` |
| [DBA1016](DBA1016.md) | 禁止 Use Case 依賴被禁止的型別。 | `error` |
| [DBA1017](DBA1017.md) | 要求派送 Handler 轉接至剛好一個 Use Case。 | `error` |
