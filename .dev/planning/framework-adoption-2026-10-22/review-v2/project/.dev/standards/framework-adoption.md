---
title: "ArchPrecept Framework 規範採納草案"
document_id: "DOC-DEV-FRAMEWORK-ADOPTION"
document_version: "0.1"
status: "draft"
date: "2026-10-22"
owners:
  - "YuChia Wei"
proposed_path: ".dev/standards/framework-adoption.md"
target_source_commit: "20c82b731385e8f1c939b939fb86ae6bd5dcfc05"
installation_status: "not-performed"
normative_binding_status: "not-adopted"
---

# ArchPrecept Framework 規範採納草案

本文件整理 Framework 知識在 ArchPrecept 的適用範圍、既有產品決策、擬採納慣例與例外。選配方向已獲接受；本文件內容仍待 owner 檢核，尚未啟用規範、安裝 Framework 或修改產品程式。

建議等待 Framework v0.21.0 正式發布後首次安裝。本文用已核對的 v0.20.2 作規範來源基線，並分開標示已合併、尚未發布的 skill 拆分。版本等待不改變 ArchPrecept 已確認決策的效力。

## 適用範圍

| 範圍 | 適用內容 | 不可推定的事 |
|---|---|---|
| 開發 ArchPrecept 產品 | 公開 API、套件與組件相依、Roslyn 分析、角色與規則模型、產品文件、測試及發布 | 本產品不是 Backend Application；不因選 dotnet-backend 就加入 Controller、MQ、資料庫、Outbox 或四層應用骨架 |
| 定義及驗證下游應用規則 | 明選 DDD／CA／Hex Arch 慣例的語意、Profile、適用角色、例外與可機械檢查範圍 | 文件存在不代表全部 consumer 啟用該 Profile；不得把未辨識角色或未執行檢查報成通過 |
| Consumer 範例及測試 fixture | 明列情境採用的契約、宿主、預期 Diagnostic 與相容性 | 故意不合規的反例不是產品規範例外；不得為使整個 tests 目錄零 Diagnostic 而改掉反例 |

以上是工作適用範圍，不是新增的產品 Profile ID 或 installer selector。實際可用的 Profile／角色辨識／設定能力須由現行產品證據確認。只有特定工作明選 backend 語意，才使用相應 knowledge selector。

## 來源與效力

1. 先看當次使用者授權，再辨識來源的 owner、狀態與適用範圍；已接受的決策不因新 session 而失效。
2. 產品決策由 [產品規劃書](../../docs/product/product-plan.md) 的 Decision Log 與其明確後續決定保存，需求及驗收在 [PRD](../../docs/product/prd.md)。本文件只引用，不另建產品決策正本。
3. Framework 擁有通用規範及其原始版本；ArchPrecept 擁有將規範映射為 Rule／Diagnostic 的產品語意和機械化界線；consumer 擁有其業務事實、Profile、Role Mapping 及例外。
4. 產品文件目前可為 draft，但其中有明列「已確認／負責人同意」的個別決策。不得把整份 draft 宣稱已核准，也不得因此忽略已確認決策。
5. 實作及 Diagnostic 手冊描述現在會發生什麼；需求、規範及已接受決策描述應有行為。兩者不一致是待處理缺口，不是任一方可被默默刪除。
6. 技術範例、Profile default、建議與已採納規範分開。新文件或較高版本不自動取代專案決策；取代須有可定位的 scope、理由及生效決定。
7. 真正衝突發生在同一適用範圍內的兩項有效要求無法同時成立。不同範圍、已允許例外、暫定方案或現行缺陷，不先混成規範衝突。

依 [文件治理規範](../../docs/governance/document-governance-and-traceability.md)，`docs/` 仍保存產品正式來源，`rules/` 承接機器規則資料，`.dev/` 保存開發治理及證據。目前對 `rules/` 的規劃不等於完整 catalog 已實作；本文件不補一份平行 catalog。

## 已確認與暫定的產品決策

下列是既有來源的索引及導入影響，不新增 Decision ID，也不重新批准歷史發布。

| 既有來源 | 原有效力 | 對 Framework 採納的約束 |
|---|---|---|
| DEC-003 | 已確認 | 單一 repository、多成品共同演進，不為符合某個應用模板拆 repository |
| DEC-004／DEC-005 | 已確認 | Analyzer 不依賴 reference contracts；核心規則應支援不採官方契約的 consumer。未完成的 classifier／custom mapping 仍是產品缺口 |
| DEC-006 | 暫定 | Reference contracts 留在同 repo 且選配；後續已確認的 CT-004／CT-005 具體拆分另按 DEC-017／018 處理，不把暫定狀態隱去 |
| DEC-007 | 已確認 | Core 宿主中立；ASP.NET Core、Messaging 等規則按 Profile 適用，不能對所有 .NET 庫套用 HTTP／broker 政策 |
| DEC-008／DEC-009／DEC-013 | 已確認 | 規則資料與追溯有正式 owner；零 Diagnostic 不等於合規，缺少 Rule／來源／機制需揭露 |
| DEC-011／DEC-012 | 已確認 | Markdown 為產品文件正本；保留 docs，以 .dev 補開發治理，不搬移產品文件或人工維護第二份標準 |
| DEC-015／DEC-016 | 已確認 | ArchPrecept 品牌、Solution／Project／Assembly／Namespace；保留本機 dotnet-architecture-kit 目錄。遠端現況以 README／來源讀回為準 |
| DEC-017、[CT-005](../../docs/product/event-sourcing-contract.md) | 負責人同意且已有公開成品證據 | EventSourcing 為同 repo 的獨立選配套件；一般 Domain 不反向依賴它；ES Analyzer 仍留 Core |
| DEC-018、[CT-004](../../docs/product/messaging-contract.md) | 負責人同意且已有公開成品證據 | 訊息契約與 Application 發布 port 分離；組態、broker SDK、DI、重試及交付保證由 host／adapter 擁有 |
| DEC-019、[CI/CD 規則](../../docs/releases/cicd-plan.md) | 負責人同意，2026-10-21 生效 | 初期使用 ff-only 整合與 main push CI／發布；不因選 pr-author 或 Framework source 政策新增 mandatory PR／reviewer 門檻。每次操作仍受當次授權限制 |
| [License 與來源評估](../../docs/releases/licensing-assessment.md)、產品規劃書第 14 節 | 已有明確決定 | 自有內容採 Apache-2.0，第三方條款分開；不因 Framework 導入重開已完成選型 |

產品規劃書仍保留 DEC-001 的舊遠端名稱、DEC-010 的早期套件命名待決記錄，以及舊版本停止發布的歷史段落。已有 README、DEC-019、現行 CI/CD 與 [0.0.2-preview 發布紀錄](../../docs/releases/0.0.2-preview.md) 說明後續狀態。接手時應沿這些明確演進讀取；本次不修改歷史產品文件，也不因舊段落重新要求決定已核准的六個套件 ID 或 public 範圍。

## 擬採納的 DDD CA 與 Hex Arch 慣例

下表是待核准的適用性對照。「產品約束」引用前節既有決策；「擬採納」表示本次尚未使它生效。Framework Rule ID 只列實際已登錄者，不由文件名稱產生新 ID；同一文件有 Rule ID 不代表整份內容都已登錄。

| 主題與來源 | 擬採納語意及範圍 | 機械化與未決界線 |
|---|---|---|
| [DDD discovery／strategic／tactical][fw-ddd] | 用共同語言、Bounded Context、Aggregate 不變條件、Entity 身分與 VO 值語意說明模型；需求與規格可使用 DDD／BDD | 人工探索、設計及審查；不得用資料夾結構或 class suffix 代替語意 |
| [CA 與 Hex 架構][fw-playbook] | Domain 保持 transport／ORM／broker 中立；Application 擁有 UseCase 與 ports，adapters 處理外部機制 | 對應產品部分診斷與未來架構驗證；Backend 四層目錄不套到 Analyzer 庫 |
| [Controller／UseCase／Handler 關係][fw-relationship] | 同步 API 預設 Controller 直接呼叫 UseCase interface；不經一般派送 Handler 繞一層。不建立預設 ApplicationService 中介層 | DBA1004／1005／1006／1014 為目前相關診斷；完整角色文件的 skill 讀取仍有缺口 |
| [UseCase 標準][fw-usecase]及[角色契約][fw-relationship] | `I…UseCase`／具體 `…UseCase`、`ExecuteAsync`、非 optional CancellationToken；transport-neutral Input／Output，無輸出可回 `Task` | DBA1015 為現行機制；規範與現行名稱式檢查的差異必須明列 |
| [Handler 責任][fw-relationship] | 真正有 dispatcher／message delivery 才設 Handler；薄轉接至一個 UseCase，不在 Controller／Handler 編排多個 UseCase | DBA1011／1017；DomainEventHandler／Reactor 不直接等同派送 Handler |
| [UseCase 相依][fw-usecase] | 以建構式注入 outbound ports；禁止 Service Locator、屬性注入、直接建立 Repository、transport framework 或另一個 UseCase 相依 | DBA1002／1010／1012／1016；合法 `IDomainEventDispatcher` 的名稱誤判交 #7，不把錯誤診斷當架構要求 |
| [Repository][fw-repository] | Aggregate write port 與 read-only Query Port 分離；保留 FindById／Save、相容別名與單 Aggregate 語意；不公開預設可寫 generic CRUD | DBA1001 是部分機制；custom contract 與角色辨識仍須依 #6／#7 的範圍完成 |
| [交易與一致性][fw-usecase] | 一個 Command 預設改一個 Aggregate；跨 Aggregate 用事件及最終一致性。多 Aggregate 強一致性是有條件例外 | `ARCH-UOW-001` 已登錄；不以一個 repository／UoW 介面推定交易已實作 |
| [事件與交易訊息][fw-messaging] | Domain Event、Integration Event、Reactor 與 Outbox 責任分離；事件 port 不等於 broker，也不證明原子提交 | `MESSAGING-TX-001`；ArchPrecept 的實際 API 語意以 CT-004／CT-005 為準 |
| [Aggregate／Building Blocks][fw-aggregate] | ES 為選定策略；一般狀態式 Aggregate 不需要 ES 機制。共享 base 是可選產品能力，不是所有 consumer 的採用條件 | `AGGREGATE-ES-001`、DBA1009；產品已發布 ES 行為不可為貼齊範例而無聲改寫 |
| [ValueObject][fw-aggregate] | 擬採納建構完成後不可變、值相等、可使用 record／readonly record struct；另維持 #11 對 Entity 參考規則的獨立需求 | #11 尚待實作；public init、巢狀可變物件、集合、欄位、繼承與自有契約須明訂，未確認前標記未評估，不直接沿用舊 DDD001／DDD002 |
| [DbC][fw-dbc] | 在適用的 domain 行為明列前置、後置與不變條件；以具體例子說明契約 | `CONTRACT-SEMANTICS-001`；guard/helper 存在不代表完整業務契約已驗證 |
| [Mapper][fw-mapper]與[Projection][fw-projection] | Query 與 write 行為分離；Mapper 純轉換；產品定義的 query Projection port 不執行持久化寫入 | `MAP-EVENTS-001`、DBA1007／1008／1013；ES read-model projector 的不同責任見例外表 |
| [跨 BC 與 DI 政策][fw-playbook] | 對明選此 backend Profile 的應用，保留 MQ-first、RabbitMQ／Kafka-only、避免跨 BC 直接 web API，以及明確 IServiceCollection 註冊 | 此政策不讓 ArchPrecept 核心依賴 broker／DI，不是所有 .NET 專案的通用真理；不同 consumer 政策需明列 |
| [刪除生命週期][fw-aggregate] | 只對選定生命週期的 Aggregate 採納 Soft Delete／Purge；區分軟刪除與實體清除 | `DELETE-SOFT-001`／`DELETE-PURGE-001`；不替所有 reference contracts 加 `IsDeleted` |
| [BDD][fw-bdd]與[測試規範][fw-tests] | Discovery／Formulation／Automation 分工；GWT 描述可觀察行為；xUnit、mocking、BDDfy 按真正選定技術及情境適用 | `TEST-GWT-001`／`TEST-MOCK-001`／`TEST-BDDFY-001`；不把測試寫法等同完整 BDD，也不無差別改寫 Roslyn harness |
| [結構規範][fw-structure] | 下游 backend 結構按 Profile；ArchPrecept 自身遵守 PRD RL-009／RL-010 的能力分組、測試及命名 | `PROJECT-GRAMMAR-001` 不自動覆寫本產品的 src／Contracts／Analyzers 結構 |
| [身分與追溯][fw-identity] | 保留來源版本、Requirement／Rule／Diagnostic 的不同身分；分清規範、安裝、角色辨識、執行及驗收 | 沿 RM-005、DG-005／006／007／011 與 #6／#7 承接；不新增平行 catalog 或以安裝成功宣稱合規 |

## 明確例外與差異

| 情況 | 目前分類 | 處置與有效範圍 |
|---|---|---|
| UseCase 沒有業務輸入，或只有單一 BCL scalar | Framework 已記錄的例外，擬隨此慣例採納 | 可無 Input 或直接傳單一 scalar；collection、tuple、自訂 record/class、多值仍應有專用 Input。不得擴成接受 HTTP Request／broker message |
| Controller 直接查詢的 fast path | Framework 提及的顯式例外，不是本專案一律核准 | 預設仍經 UseCase；具體 consumer 需提供已接受決策與限制，不能由 AI 因簡單或效能想像自行開例外 |
| 同 BC 多 Aggregate transaction | Framework 已明載的條件式例外 | 具名 all-or-nothing 不變條件、中間狀態不可接受且不可補償，先重查 Aggregate 邊界；跨 BC、減少 I/O、共享 DB／ORM 便利均不構成例外 |
| ArchPrecept 提供 Entity／Aggregate／VO base | 適用範圍差異，非已證實衝突 | 提供選配能力與強迫所有 consumer 繼承不同；保留 DEC-004／005 的獨立採用方向 |
| 自有契約或非 suffix 型別 | 已接受產品方向，機制有未完成部分 | 必須揭露 #7 的辨識及 coverage 缺口；不以「無 Diagnostic」宣稱 custom contract 通過，也不要求 consumer 為工具改名 |
| 合法 `IDomainEventDispatcher` 觸發 DBA1016 | 已重現的 Analyzer 缺陷，不是規範例外 | #7 主責；不為消除診斷重構合法 port。若任務需要暫時 suppression，須以該任務已授權且有界的處置記錄理由／範圍；本草案不預先全域停用 |
| ES 的 Domain mutator 與 commit | CT-005 已確認的產品行為限制 | ES 拒絕一般 Add／Remove／ClearDomainEvents，使用 Apply／append／commit；不能因一般 Aggregate API 可呼叫就當作可替換。產品行為不由 Framework 範例改寫 |
| ES projector 更新 read model | CT-005 已確認的角色區別 | 不把它直接視為 DBA1013 的 read-only `IProjection` query port；釐清角色後再評估 |
| `init` 或唯讀集合外觀 | 尚待語意與 coverage 決定，非已核准例外 | 建構期賦值與建構後可變性分開；readonly 引用不代表深層不可變。由 #11 明訂驗收，不在導入時擅自定案 |
| 不使用 BDDfy、沒有 mock 的 Analyzer 測試 | 適用性差異 | 沒有採用該 library／場景不須替換測試框架；有 mock 時按 consumer 的明確技術選擇。此 .NET Profile 的 NSubstitute 預設不得被誤寫成所有測試的硬性依賴 |
| 反例及不合規 consumer fixture | 測試意圖，非 production waiver | 保留預期 Diagnostic 與位置；不為遵循協作文件把反例改成正例 |

新增真正例外時，沿既有 Issue／ADR／採納來源記錄條件、理由、owner、適用範圍及何時重看；沒有時間限制就不發明到期日。此表是本次採納的差異摘要，不另建 waiver 資料庫。

## Framework 版本與知識讀取

| 證據層 | 本次已知內容 | 不代表 |
|---|---|---|
| 公開 v0.20.2 | common 0.3.0、dotnet-backend 0.1.4、ddd-ca-hex-architect 0.3.1；已核對 ZIP 與 SHA-256 | 已安裝、全部來源可經 skill 讀取、已採納 normative rules |
| 已合併來源 1d9a42c8 | [PR #485][fw-split] 拆為 domain-model-designer 0.1.0／software-architect 0.1.0，common 0.4.0／dotnet 0.1.5，並調整 Spec／BDD／DDD 與流程責任 | 已發布 v0.21.0；最終成品版本或檔案數已固定 |
| 目前目標專案 | 只有本次計畫與草案；未安裝 Framework | installed lock、runtime discovery、AI 實際操作驗收 |

建議 v0.21.0 採用 16 個 skills：保留原已接受選配用途，以 `domain-model-designer`、`software-architect` 取代一個 `ddd-ca-hex-architect`。其餘 skills 及兩個知識包依正式 catalog 固定版本；Codex adapter 維持單選，不自動加入新出的能力。

知識 bindings 先沿明確操作與 consumer allowlist 使用參考知識。需要宣稱 registered normative coverage 時，才選實際 resource／Rule IDs、已核准的目標 authority、精確 raw SHA-256 與 selector，再重新 derive／plan。本草案仍為 draft，因此不產生有效 normative authority 宣告。

UseCase 角色關係、Controller、Repository 文件有未登錄部分；不能由文檔路徑生成 Rule ID，也不能因同檔有 `ARCH-UOW-001` 就把其他角色語意一併宣稱為已登錄。

截至本次查閱，已合併的 `software-architect` 仍未明列 `USECASE-COMMAND-HANDLER-RELATIONSHIP.MD`，完整 playbook 也未列入其 consumption。v0.21.0 發布後要重新核對，不能假定版本提升已解決。若某次操作需要的 Framework 資源不可讀，應明列 coverage 缺口；已接受的目標專案決策仍可依其本身權威引用，但不得宣稱取得了該 Framework 資源或繞回來源 checkout 來補 installed-reader 證據。

## 少量代表情境

以下是文件及未來採用驗收的預期行為，本次沒有執行為產品或 AI runtime 測試。

| Given | When | Then |
|---|---|---|
| 已確認 Analyzer-only／custom-contract 方向 | Framework 範例使用內建基底 | 保留獨立採用，不要求改寫全部 consumer；指出目前角色辨識缺口 |
| 任務是修改 Analyzer 本身 | 讀到 MQ-first、Backend 四層或 BDDfy 預設 | 先判斷適用性，不替產品加入 broker、Controller、資料庫或測試依賴 |
| 已接受 Controller → UseCase 與薄 Handler 慣例 | 新對話接手下游規則工作 | 引用已接受來源，不重新提議 mediator 分層；必要資源缺失仍明列 |
| 合法領域事件 port 被 DBA1016 擋下 | 分析 consumer 錯誤 | 區分規範與 Analyzer 誤判，連到 #7，不把改名當作語意修復 |
| ES instance 有 pending events | 通用 dispatcher 要 ClearDomainEvents | 依 CT-005 拒絕一般清除路徑，保留 append／commit 責任 |
| VO 有 init、集合或巢狀 Entity | 宣稱規則已涵蓋 | 對照 #11 的實際驗收；未定義或未實作部分標記未評估 |
| 新版 Framework 有不同預設 | 產生新選配或升級提案 | 列出來源差異、目標決策影響與需要決定的項目，不自動覆寫 |
| 正式 v0.21.0 可下載 | 準備首次安裝 | 核對 ZIP／catalog／engine／版本及 allowlist，重新產生逐檔計畫；發布本身不提供安裝授權 |

## 核准與接續

本次檢核項目是：三種工作範圍是否正確、既有決策引用是否完整、擬採納語意與例外是否符合 owner 慣例，以及是否採等待 v0.21.0 的路徑。

核准後才能將本文件改為相應有效狀態；該狀態變更會改變 raw bytes，若有 authority pin 必須同步重新產生。等待期間可維護這份草案及既有需求，不安裝 v0.20.2 作過渡、不安裝未發布 main，也不啟動背景監看。需要實作、安裝或通知時依後續指示執行。

正式安裝條件聚焦已選能力：可驗證的 v0.21.0 成品、相容選配與精確檔案計畫、必要知識讀取缺口已解決或其影響已明確接受、以及安裝授權。#481／#483 的工作儲存、#482／#484 的通用客製升級、#486 的記憶提案、#487 的 sub-agent 模型管理不因存在就成為本次全部完成的前置條件；本次沒有選這些額外能力。

[fw-ddd]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/engineering-common/ddd/README.md
[fw-bdd]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/engineering-common/bdd/README.md
[fw-playbook]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/design/architecture-playbook.md
[fw-relationship]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/USECASE-COMMAND-HANDLER-RELATIONSHIP.MD
[fw-usecase]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/coding-standards/usecase-standards.md
[fw-repository]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/coding-standards/repository-standards.md
[fw-messaging]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/coding-standards/transactional-messaging-standards.md
[fw-aggregate]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/coding-standards/aggregate-standards.md
[fw-dbc]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/DESIGN-BY-CONTRACT.md
[fw-mapper]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/coding-standards/mapper-standards.md
[fw-projection]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/coding-standards/projection-standards.md
[fw-tests]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/coding-standards/test-standards.md
[fw-structure]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/dotnet-backend/standards/project-structure.md
[fw-identity]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/3c7434fd3ea65bcbf519fef3e3ca232a0bc59043/src/knowledge/engineering-common/references/ENGINEERING-IDENTITY-CONTRACT.md
[fw-split]: https://github.com/YuChia-Wei/ai-collaboration-framework/pull/485
