---
title: "ArchPrecept 分析器 Diagnostic 說明書"
document_id: DOC-DIAGNOSTICS
document_version: 0.4
last_reviewed: 2026-10-25
status: draft
lang: zh-TW
canonical_path: docs/diagnostics/README.md
---

# 分析器 Diagnostic 說明書

本目錄是 `ArchPrecept.Analyzers.Core` 目前所有 Roslyn Diagnostic 的可閱讀正式說明來源。每份手冊說明實際觸發條件、違規與合規範例、修正方式、已知限制、設定／suppression 選項，以及對應的實作與測試。

這些手冊說明各規則的有界行為；唯一機器可讀 Rule／Diagnostic／Profile 對照在 [Catalog](../../rules/catalog.xml)。Descriptor 由同一份 Catalog 建立；共同設定與角色證據見 [Analyzer 採用政策](../product/analyzer-policy.md)。

[規則設計理由](../product/rule-design-rationale.md)說明共同架構責任及產品取捨，[方法論與平台來源](../product/methodology-references.md)限定作者原則與具體產品政策的關係。每份手冊的「設計理由與取捨」說明該項限制為何存在；「適用範圍與觸發條件」及「已知限制與例外」則描述目前已實作的子集。合規範例只表示符合該項展示的檢查條件，不表示完整架構或執行期行為已通過。

## 使用方式

1. 從下表選取建置或 IDE 回報的 `DBA` ID。
2. 先確認「適用範圍與觸發條件」與「已知限制與例外」是否適用於目標程式碼。
3. 依「修正指引」調整程式碼；若例外是經過刻意核准的架構選擇，依「設定與 Suppression」留下可追溯的 suppression。

分析器專案也提供 [severity 範本](../../src/Analyzers/ArchPrecept.Analyzers.Core/templates/analyzer-severity.editorconfig)；接線與建置方式請見其 [README](../../src/Analyzers/ArchPrecept.Analyzers.Core/README.md)。

## Diagnostic 目錄

| Diagnostic ID | 摘要 | 預設嚴重性 |
|---|---|---|
| [DBA0001](DBA0001.md) | 無效設定／無法解析的明確 mapping，不能當成零診斷通過。 | `error` |
| [DBA0002](DBA0002.md) | 明確宣告範圍與部分未評估狀態。 | `info` |
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
| [DBA1018](DBA1018.md) | VO 不得包含一般 setter、mutable field 或已知可變集合。 | `warning`，ddd opt-in |
| [DBA1019](DBA1019.md) | VO 不得直接或經已知容器參考 Entity。 | `warning`，ddd opt-in |

目前未提供設定時採 `legacy`，啟用 DBA1001–DBA1017；VO 規則不自動啟用。這是現版預設，不構成保留 preview 行為的義務；首個公開且不含 `-preview` 的版本之前不要求向後相容，詳見[版本邊界](../product/rule-design-rationale.md#版本邊界)。一般規則診斷提供 Rule ID、角色、證據、信心與 HelpLink；可透過 `dotnet build -p:ErrorLog=diagnostics.sarif` 取回。Info 狀態與零診斷都不是完整架構合格證明。
