---
title: "ArchPrecept Framework 規範採納"
document_id: "DOC-DEV-FRAMEWORK-ADOPTION"
document_version: "0.2"
status: "adopted"
date: "2026-10-24"
owners:
  - "YuChia Wei"
proposed_path: ".dev/standards/framework-adoption.md"
target_source_commit: "45e7fd4949b4e85082876e4469de8b5f36af2ad7"
installation_status: "verify-from-lock"
normative_binding_status: "not-adopted"
---

# ArchPrecept Framework 規範採納

本文件保存 Framework 知識在 ArchPrecept 的適用範圍、既有產品決策、採納慣例與例外。依 2026-10-24 使用者授權，在無阻擋問題的分析結論下採納本文件並安裝所選成品；安裝結果仍以 lock／inspect 證據為準，不因此修改產品程式。

Framework v0.21.0 已於 2026-10-24 00:34:42 台灣時間正式發布。本文改以已校驗的 release commit `221695622dde248a2dc84ffb9fc675d7c45d0768` 為來源；目標已交付 ArchPrecept 0.0.3-preview。新增條文在本文件明列範圍生效；既有決策持續有效。

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

依 [文件治理規範](../../docs/governance/document-governance-and-traceability.md)，`docs/` 仍保存產品正式來源，`rules/` 承接機器規則資料，`.dev/` 保存開發治理及證據。`rules/catalog.xml` 已交付 19 條產品規則；DBA0001／0002 另承接設定與適用性診斷，仍不等於完整 Conformance CLI；本文件不補一份平行 catalog。

## 已確認與暫定的產品決策

下列是既有來源的索引及導入影響，不新增 Decision ID，也不重新批准歷史發布。

| 既有來源 | 原有效力 | 對 Framework 採納的約束 |
|---|---|---|
| DEC-003 | 已確認 | 單一 repository、多成品共同演進，不為符合某個應用模板拆 repository |
| DEC-004／DEC-005 | 已確認 | Analyzer 不依賴 reference contracts；核心規則應支援不採官方契約的 consumer。0.0.3 已交付共用 classifier／custom mapping；其有界範圍見 analyzer-policy，不宣稱任意型別完整覆蓋 |
| DEC-006 | 暫定 | Reference contracts 留在同 repo 且選配；後續已確認的 CT-004／CT-005 具體拆分另按 DEC-017／018 處理，不把暫定狀態隱去 |
| DEC-007 | 已確認 | Core 宿主中立；ASP.NET Core、Messaging 等規則按 Profile 適用，不能對所有 .NET 庫套用 HTTP／broker 政策 |
| DEC-008／DEC-009／DEC-013 | 已確認 | 規則資料與追溯有正式 owner；零 Diagnostic 不等於合規，缺少 Rule／來源／機制需揭露 |
| DEC-011／DEC-012 | 已確認 | Markdown 為產品文件正本；保留 docs，以 .dev 補開發治理，不搬移產品文件或人工維護第二份標準 |
| DEC-015／DEC-016 | 已確認 | ArchPrecept 品牌、Solution／Project／Assembly／Namespace；保留本機 dotnet-architecture-kit 目錄。遠端現況以 README／來源讀回為準 |
| DEC-017、[CT-005](../../docs/product/event-sourcing-contract.md) | 負責人同意且已有公開成品證據 | EventSourcing 為同 repo 的獨立選配套件；一般 Domain 不反向依賴它；ES Analyzer 仍留 Core |
| DEC-018、[CT-004](../../docs/product/messaging-contract.md) | 負責人同意且已有公開成品證據 | 訊息契約與 Application 發布 port 分離；組態、broker SDK、DI、重試及交付保證由 host／adapter 擁有 |
| DEC-019、[CI/CD 規則](../../docs/releases/cicd-plan.md) | 負責人同意，2026-10-21 生效 | 初期使用 ff-only 整合與 main push CI／發布；不因選 pr-author 或 Framework source 政策新增 mandatory PR／reviewer 門檻。每次操作仍受當次授權限制 |
| DEC-020、[0.0.3-preview](../../docs/releases/0.0.3-preview.md) | 負責人同意且已交付 | 核心 #1／#6／#7／#8／#9／#11 已交付；本輪不得退回舊版缺陷敘述或重開已完成規則決策 |
| [License 與來源評估](../../docs/releases/licensing-assessment.md)、產品規劃書第 14 節 | 已有明確決定 | 自有內容採 Apache-2.0，第三方條款分開；不因 Framework 導入重開已完成選型 |

產品規劃書仍保留 DEC-001 的舊遠端名稱、DEC-010 的早期套件命名待決記錄，以及舊版本停止發布的歷史段落。已有 README、DEC-019、現行 CI/CD 與 [0.0.3-preview 發布紀錄](../../docs/releases/0.0.3-preview.md) 說明後續狀態。接手時應沿這些明確演進讀取；本次不修改歷史產品文件，也不因舊段落重新要求決定已核准的六個套件 ID 或 public 範圍。

## DDD CA 與 Hex Arch 慣例的採納範圍

下表是本次採納的適用性對照。「產品約束」引用前節既有決策；其他條文僅在明列的工作／技術範圍內適用，不建立額外的 Framework normative-rule binding。Framework Rule ID 只列實際已登錄者，不由文件名稱產生新 ID；同一文件有 Rule ID 不代表整份內容都已登錄。

| 主題與來源 | 採納語意及範圍 | 機械化與未決界線 |
|---|---|---|
| [DDD discovery／strategic／tactical][fw-ddd] | 用共同語言、Bounded Context、Aggregate 不變條件、Entity 身分與 VO 值語意說明模型；需求與規格可使用 DDD／BDD | 人工探索、設計及審查；不得用資料夾結構或 class suffix 代替語意 |
| [CA 與 Hex 架構][fw-playbook] | Domain 保持 transport／ORM／broker 中立；Application 擁有 UseCase 與 ports，adapters 處理外部機制 | 對應產品部分診斷與未來架構驗證；Backend 四層目錄不套到 Analyzer 庫 |
| [Controller／UseCase／Handler 關係][fw-relationship] | 同步 API 預設 Controller 直接呼叫 UseCase interface；不經一般派送 Handler 繞一層。不建立預設 ApplicationService 中介層 | DBA1004／1005／1006／1014 為目前相關診斷；完整角色文件的 skill 讀取仍有缺口 |
| [UseCase 標準][fw-usecase]及[角色契約][fw-relationship] | `I…UseCase`／具體 `…UseCase`、`ExecuteAsync`、非 optional CancellationToken；transport-neutral Input／Output，無輸出可回 `Task` | DBA1015 為現行部分機制；共用角色辨識支援契約／mapping／選定慣例，不等於整份規範機械化 |
| [Handler 責任][fw-relationship] | 真正有 dispatcher／message delivery 才設 Handler；薄轉接至一個 UseCase，不在 Controller／Handler 編排多個 UseCase | DBA1011／1017；DomainEventHandler／Reactor 不直接等同派送 Handler |
| [UseCase 相依][fw-usecase] | 以建構式注入 outbound ports；禁止 Service Locator、屬性注入、直接建立 Repository、transport framework 或另一個 UseCase 相依 | DBA1002／1010／1012／1016；0.0.3 已修正合法 `IDomainEventDispatcher` 誤報；混入 bus／transport 等禁止角色仍會回報 DBA1016 |
| [Repository][fw-repository] | Aggregate write port 與 read-only Query Port 分離；保留 FindById／Save、相容別名與單 Aggregate 語意；不公開預設可寫 generic CRUD | DBA1001 是部分機制，0.0.3 涵蓋具體 repository 類別；雙 Port adapter 的局部拆分是選用套件政策，不宣稱其普遍違反 DDD／CA |
| [交易與一致性][fw-usecase] | 一個 Command 預設改一個 Aggregate；跨 Aggregate 用事件及最終一致性。多 Aggregate 強一致性是有條件例外 | `ARCH-UOW-001` 已登錄；不以一個 repository／UoW 介面推定交易已實作 |
| [事件與交易訊息][fw-messaging] | Domain Event、Integration Event、Reactor 與 Outbox 責任分離；事件 port 不等於 broker，也不證明原子提交 | `MESSAGING-TX-001`；ArchPrecept 的實際 API 語意以 CT-004／CT-005 為準 |
| [Aggregate／Building Blocks][fw-aggregate] | ES 為選定策略；一般狀態式 Aggregate 不需要 ES 機制。共享 base 是可選產品能力，不是所有 consumer 的採用條件 | `AGGREGATE-ES-001`、DBA1009；產品已發布 ES 行為不可為貼齊範例而無聲改寫 |
| [ValueObject][fw-aggregate] | 採納建構完成後不可變、值相等、可使用 record／readonly record struct；另維持 #11 對 Entity 參考規則的獨立需求 | DBA1018／1019 已隨 ddd opt-in 交付；一般 setter（含 private）、非 readonly field、已知可變集合依明列政策處理。init 允許；未知、循環、IReadOnlyCollection 底層狀態等保持 not-evaluated，不等於深層不可變證明 |
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
| UseCase 沒有業務輸入，或只有單一 BCL scalar | Framework 已記錄的例外，隨此慣例採納 | 可無 Input 或直接傳單一 scalar；collection、tuple、自訂 record/class、多值仍應有專用 Input。不得擴成接受 HTTP Request／broker message |
| Controller 直接查詢的 fast path | Framework 提及的顯式例外，不是本專案一律核准 | 預設仍經 UseCase；具體 consumer 需提供已接受決策與限制，不能由 AI 因簡單或效能想像自行開例外 |
| 同 BC 多 Aggregate transaction | Framework 已明載的條件式例外 | 具名 all-or-nothing 不變條件、中間狀態不可接受且不可補償，先重查 Aggregate 邊界；跨 BC、減少 I/O、共享 DB／ORM 便利均不構成例外 |
| ArchPrecept 提供 Entity／Aggregate／VO base | 適用範圍差異，非已證實衝突 | 提供選配能力與強迫所有 consumer 繼承不同；保留 DEC-004／005 的獨立採用方向 |
| 自有契約或非 suffix 型別 | 已交付的有界能力 | 依 analyzer-policy 明確 metadata／assembly mapping；Expected 的 applicable 是適用性，不是已執行或通過。未知／無法評估仍須揭露 |
| 合法 `IDomainEventDispatcher` 觸發 DBA1016 | 0.0.2 歷史缺陷，0.0.3 已修正 | 新出現時先核對實際版本、mapping 與混合角色，不重新引入全域 suppression；產品政策見 analyzer-policy |
| ES 的 Domain mutator 與 commit | CT-005 已確認的產品行為限制 | ES 拒絕一般 Add／Remove／ClearDomainEvents，使用 Apply／append／commit；不能因一般 Aggregate API 可呼叫就當作可替換。產品行為不由 Framework 範例改寫 |
| ES projector 更新 read model | CT-005 已確認的角色區別 | 不把它直接視為 DBA1013 的 read-only `IProjection` query port；釐清角色後再評估 |
| `init` 或唯讀集合外觀 | 0.0.3 已明訂的語意與靜態限制 | init 本身允許，仍檢查成員；readonly 引用及 IReadOnlyCollection 外觀不證明底層不可變，需保留未評估狀態及實際行為測試 |
| 不使用 BDDfy、沒有 mock 的 Analyzer 測試 | 適用性差異 | 沒有採用該 library／場景不須替換測試框架；有 mock 時按 consumer 的明確技術選擇。此 .NET Profile 的 NSubstitute 預設不得被誤寫成所有測試的硬性依賴 |
| 反例及不合規 consumer fixture | 測試意圖，非 production waiver | 保留預期 Diagnostic 與位置；不為遵循協作文件把反例改成正例 |

新增真正例外時，沿既有 Issue／ADR／採納來源記錄條件、理由、owner、適用範圍及何時重看；沒有時間限制就不發明到期日。此表是本次採納的差異摘要，不另建 waiver 資料庫。

## Framework 版本與知識讀取

正式 v0.21.0 對應 common 0.4.0、dotnet-backend 0.1.6、domain-model-designer／software-architect 各 0.1.1。精確 16 skills 與 16 個 reference bindings 見 [專案 selection](../../.ai/project/installation.json)，版本與受管成員另由 `.ai/framework.lock` 保存；未選 Claude、sub-agents、模型政策或 workflow store。選入治理及編排能力不等於已授權每次執行。

所有 bindings 使用 `knowledge`、`required_for_coverage: false`、空 normative authority／Rule ID 清單。此決定允許參考所選知識，**不放寬本產品既有規則**，也不把 Framework 全包變成 ArchPrecept 的規範。需要機械判定時仍依產品 Rule Catalog、設定、測試與 Diagnostic 文件；若未來要宣稱 Framework normative coverage，另選真實 rule IDs、已生效 authority bytes 與 selector。

Selector 只允許 direct 操作，capability／operation 精確對應正式 metadata。路徑限 `.dev/planning`、`.dev/standards`、`docs`、`rules`、`samples`、`src`、`tests`，副檔名限 cs／csproj／json／md／props／targets／txt／xml／yaml。dotnet binding 另要求明選 `dotnet-backend` technology profile，不得由 C# 檔名推定；common 不指定技術。路徑外、其他檔案型態或 delegate execution 目前沒有匹配 binding。

正式包仍有以下缺口，選配不能自行修補上游 allowlist：

- `software-architect` 的 design／review 未直接允許 `USECASE-COMMAND-HANDLER-RELATIONSHIP.MD`、`design/architecture-playbook.md` 或 Controller 完整標準。Markdown 連結不提供轉授權。
- 關係／Controller／Repository 文件仍有未登錄的規範段落；identity-allocation-required／unpacketized-fail-closed 不能改造成虛構 Rule ID。
- 正式包 aggregate 文件的 DBA1009 說明仍以 type name 描述 target analyzer，與本產品 0.0.3 的契約／mapping／慣例分類不完全相同。產品實際規則以 [analyzer-policy](../../docs/product/analyzer-policy.md)、[DBA1009](../../docs/diagnostics/DBA1009.md) 為準；不把參考文字當成新 API 契約。

本次以參考知識用途接受上述有限讀取範圍進行首次安裝，相關判斷引用本產品既有決策與明示的目標輸入。無 verified lock／reader、未在 skill allowlist、或未匹配 binding 的資源均不可宣稱 installed specialist coverage；不得回讀來源 checkout 假裝補齊。derive 的 unavailable=[] 只表示已宣告相依齊備，不證明未宣告的語意資源完整。

## 正式版新增能力與本產品差異

- `.ai/project/installation.json` 為本次選配保存位置；沒有舊 `.ai/custom` 資料需遷移。選配 JSON 不建立 custom skill registry、sub-agent registry 或模型政策。
- 初始化與 refresh 處理缺失入口／事實導覽；既有政策維護由 governance，唯讀差異判讀由 auditor。現有三份完整文件依本次確認的 bytes 寫入後，不再跑無界 initialize 覆寫。
- 公開 Issue／PR 保存產品需求、設計、驗收及技術證據。授權／操作交接不轉存 Issue；未選 workflow-records 時不假稱正式 workflow 記錄已建立，也不因此阻擋已有授權的單項工作。
- 不從 Framework 來源庫移植 mandatory PR／reviewer、模型選擇或自動委派。保留 DEC-019／020 的 ff-only 與 main-push 流程，當次授權仍決定是否提交／發布。

## 少量代表情境

| Given | When | Then |
|---|---|---|
| 開發 Analyzer 或 Contracts 庫 | 遇到 MQ-first、四層目錄、NSubstitute／BDDfy 建議 | 先確認適用範圍，不新增 backend runtime 或改寫 harness |
| 下游規則工作 | 使用 architecture design／review | 讀取允許的資源與既有產品決策，明列完整角色關係／playbook 不在 allowlist |
| 合法領域事件 dispatcher | 分析 DBA1016 | 核對 0.0.3 契約／mapping 及混合禁止角色，不沿用舊版 workaround |
| VO 有 init、巢狀 Entity 或唯讀集合 | 判讀 DBA1018／1019 | 分開違規、適用性與未評估；不宣稱零診斷證明深層不可變 |
| ES pending events | 一般 dispatcher 要 ClearDomainEvents | 保留 CT-005 的 append／commit 責任與一般 mutator 限制 |
| 選入 pr-author／orchestrator | 開始工作 | 能力可用不等於必經 PR、跨 chat 派工、模型升級或發布授權 |
| installer inspect 一致 | 評估接手能力 | 另驗證事實導覽、政策與 runtime，不將 project_readiness: not-assessed 當通過 |

## 核准與接續

本文件已採納上述適用範圍、參考知識用途及有限讀取範圍；仍不構成 Framework normative-rule binding。後續升級先核對目標與精確檔案計畫，沿用當次有效授權；狀態／bytes 變更須重新計算 paired-edit plan，不重用過期 hash。

採納不修改產品 API、Analyzer、測試、sample、CI 或發布紀錄；新的產品需求由既有 Issue／決策流程承接。任何真正同範圍衝突，列出兩方有效來源與選項，只暫停受影響判斷。

[fw-ddd]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/engineering-common/ddd/README.md
[fw-bdd]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/engineering-common/bdd/README.md
[fw-playbook]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/design/architecture-playbook.md
[fw-relationship]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/USECASE-COMMAND-HANDLER-RELATIONSHIP.MD
[fw-usecase]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/coding-standards/usecase-standards.md
[fw-repository]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/coding-standards/repository-standards.md
[fw-messaging]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/coding-standards/transactional-messaging-standards.md
[fw-aggregate]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/coding-standards/aggregate-standards.md
[fw-dbc]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/DESIGN-BY-CONTRACT.md
[fw-mapper]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/coding-standards/mapper-standards.md
[fw-projection]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/coding-standards/projection-standards.md
[fw-tests]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/coding-standards/test-standards.md
[fw-structure]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/dotnet-backend/standards/project-structure.md
[fw-identity]: https://github.com/YuChia-Wei/ai-collaboration-framework/blob/221695622dde248a2dc84ffb9fc675d7c45d0768/src/knowledge/engineering-common/references/ENGINEERING-IDENTITY-CONTRACT.md
[fw-split]: https://github.com/YuChia-Wei/ai-collaboration-framework/pull/485
