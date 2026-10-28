---
title: "ArchPrecept 規則設計理由"
document_id: DOC-RULE-DESIGN-RATIONALE
document_version: 0.2
last_reviewed: 2026-10-29
status: active
owner: "YuChia Wei"
lang: zh-TW
canonical_path: docs/product/rule-design-rationale.md
---

# 規則設計理由

ArchPrecept 把明確選定的架構責任轉成可重複檢查的限制。規則的正當性來自它保護的責任、具體風險及接受的成本；方法論提供支持原則，產品再選擇適合自身範圍的程式契約。`FindByIdAsync`／`SaveAsync`、純 Mapper 採 `static`、標準 Use Case 及薄 Handler 都是這樣的產品選擇。

本文件說明跨規則的一致理由，[方法論與平台來源](methodology-references.md)限定原文支持範圍，各 [Diagnostic 手冊](../diagnostics/README.md)保存逐項理由與目前觸發條件。[Catalog](../../rules/catalog.xml)仍是已交付規則、Profile、角色、嚴重性及實作映射的唯一機器來源；本文不另列一份平行 Catalog。

本文的「產品政策」描述應保護的責任；「目前檢查」對應本文件同一來源修訂中的 Analyzer 與 Catalog。尚未發布的來源變更不代表可以從既有 `0.0.3-preview` 套件取得；該版原行為可由[固定發布來源](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.3-preview)及[發布紀錄](../releases/0.0.3-preview.md)定位。來源文件、測試、本機候選套件與 feed 成品分別驗證，不以文件更新宣稱執行期保證。Analyzer-only 與自有契約可藉明確角色映射採用同一政策，不要求安裝 Reference Contracts；自訂角色證據也不自動豁免所選契約。

## 版本邊界

在首個公開且不含 `-preview` 的版本之前，ArchPrecept 不承諾向後相容。修正規則、API、設定或預設時，不必為保留 preview 行為另建 legacy 模式、遷移層、替代 ID 或並存機制。變更仍須說清楚新政策、受影響行為及驗證結果；現版的事實不能因新政策而被改寫。

現行設定值 `legacy` 與現行 `compatibility-repository` 角色如仍存在，僅表示該來源修訂中的功能，不構成必須保留的相容性義務。首個不含 `-preview` 的公開版本之後採何種相容政策，須另行確定。具體判定改進的版本差異由當版文件與來源證據說明，不改寫舊版發布事實。

每條產品規則的 Rule／Diagnostic ID、角色、Profile、預設 severity 與啟用範圍以 Catalog 為準，詳細正反例、不適用及未評估情況沿其 `document` 連到 Diagnostic 手冊。設定優先序、明確 mapping／慣例證據、無效或衝突選擇依[設定契約](analyzer-policy.md)與 [schema](../../rules/configuration.xsd)判讀。本文維護理由及取捨，不複製這些機器欄位；DBA0001／0002 則在本文件的[證據責任](#evidence)說明自身契約，不加入 19 條產品規則的清單。

<a id="repository"></a>
## Aggregate 儲存、查詢與一致性責任

Aggregate 是領域不變條件的一致性邊界；Repository 為模型物件提供存取抽象。[Evans 的 Aggregate／Repository](methodology-references.md#s1)支持這兩項原則。[CQRS](methodology-references.md#s6)提供讀取與更新採不同模型的選項，[ISP／DIP](methodology-references.md#s4)支持依消費者需要收斂 Port。這些原則沒有規定 Repository 只能有兩個方法。

ArchPrecept 選擇 Aggregate Root 專用、依識別碼載入及保存的最小寫入 Port：`FindByIdAsync`／`SaveAsync`。它讓 Application 明確表達「取得這個 Aggregate、執行領域行為、保存結果」，避免狀態搜尋、排序、分頁及畫面 DTO 的需求不斷擴張到寫入抽象，並減少各儲存 Adapter 被迫實作無關能力的壓力。`Save` 表達保存意圖，具體 insert、update 或事件 append 的責任由採用的 Adapter 契約決定。

條件搜尋由 Query Repository／Service 回傳 ID、純量或 Read Model。查詢所得候選 ID 不是執行命令的授權或有效性證明；命令流程仍須載入 Aggregate，依目前狀態檢查不變條件。泛用可寫 Entity CRUD 可能繞過 Aggregate，不因改名稱或包一層介面就取得相同語意。

代價是單筆載入不一定適合大量資料。已確認有基數或 I/O 壓力時，可以另選具體的有界批次 Port，並說明缺漏／重複 ID、排序、分塊、併發、部分失敗、重試及事件狀態；不讓每個單筆 Use Case 或 Adapter 承擔批次能力。批次 I/O 本身不等於跨 Aggregate 的 all-or-nothing 交易。軟刪除以領域行為及 Save 表達；實體清除是另有權限與責任的能力，不能藉泛用 CRUD 繞過模型。

[DBA1001](../diagnostics/DBA1001.md)按繼承後的有效成員及逐 Port 泛型替換檢查上述契約，再分開檢查 Adapter 實作對應及額外暴露能力；不能把第一組 Aggregate／ID 套到所有 Port，或僅因 Adapter 同時實作分離窄 Port 就判定 Aggregate Port 混責。反之，criteria finder 若直接加入 Aggregate Port，仍違反選定的最小政策。非 Aggregate／錯誤 ID、可寫 query 宣告與回傳 Entity 的情況按實際契約判定；靜態 shape 不證明 finder 無副作用或儲存成功。

[DBA1013](../diagnostics/DBA1013.md)保護讀取 Port，不得呼叫所辨識持久化契約宣告的寫入 API，含真正 override 與介面實作；無關同名方法不能單憑繼承型別就成為該 API。本文的 `projection` 是讀取端 Port／Service；消費事件並更新自己 Read Store 的 Projector 是不同責任。[Event Sourcing 平台模式](methodology-references.md#s8)支持後者可以更新物化檢視。使用同一個英文詞不能把兩種角色合併，也不能把合法 Projector 的寫入套成全面禁令。目前沒有 store ownership 模型，不因 Projector 沒有 DBA1013 就宣稱它只寫自己的 Read Store，或沒有 command-store／對外副作用。

<a id="application"></a>
## 應用流程、入口與組裝

[Clean Architecture](methodology-references.md#s2)與 [Hexagonal Architecture](methodology-references.md#s3)支持應用政策與交付／儲存機制分離，並可由不同入口驅動。ArchPrecept 據此選擇：Use Case 擁有應用編排，Controller 與派送 Handler 擁有輸入／輸出轉換，Repository Adapter 擁有資料存取，Composition Root 擁有具體協作者組裝。

明確注入的窄 Port 讓依賴、生命週期及替換邊界可見。[DBA1002](../diagnostics/DBA1002.md)、[DBA1010](../diagnostics/DBA1010.md)限制 Service Provider／Locator 及屬性注入；[DBA1012](../diagnostics/DBA1012.md)限制 Application 直接建立 Repository；[DBA1006](../diagnostics/DBA1006.md)限制 Controller 直接建立 Use Case／Handler。代價是需要顯式契約與組裝，但測試及其他入口可以使用同一個應用邊界。這些規則有角色範圍，不禁止 Composition Root 使用容器，也不禁止建立一般 Input、DTO 或領域值。

Use Case 選定唯一 `ExecuteAsync`、真正 BCL `Task`／`Task<TOutput>`、末尾非選用 `CancellationToken`，並採零個輸入、單一標準純量或專屬 `*Input`。[DBA1015](../diagnostics/DBA1015.md)依有效繼承契約與實作對應檢查這個形狀，包含 override／顯式介面實作；同名自有 Task、CancellationToken 或日期型別不自動取得 BCL 身分，ValueTask 也不是已選的回傳契約。必要取消參數讓呼叫者明確傳遞取消意圖；它不證明協作者真的遵守取消。專屬 Input 把 HTTP Request、派送 Command／Query 與應用資料的演進分開，代價是額外型別與轉換程式。`CreateOrderCommand` 即使由 Application 擁有，仍不符合已選的非純量 `*Input` 命名；這是命名契約問題，不足以宣稱該物件必為傳輸型別。

Transport 身分由真正 framework 組件／契約或明確 mapping 辨識。Input／Output 的陣列及泛型引數追蹤上限為 8 層，另檢查一層 public instance field／property；外層叫 Input 不會掩蓋已涵蓋的 transport 成員，但這個有界深度亦不證明任意物件圖傳輸中立。名稱政策、身分證據與未分析路徑須分別解讀。

Use Case 不依賴另一個 Use Case、Vendor Bus 或 Transport；共享行為應依其責任放入領域服務、窄 Port 或一個明確擁有協調責任的應用流程。[DBA1016](../diagnostics/DBA1016.md)檢查直接欄位／屬性、建構式及一般方法參數、直接呼叫所暴露的禁止型別，包含 abstract Use Case 的實際宣告；不追蹤任意 Wrapper 內部或完整跨方法相依圖。Application-owned 的 Domain Event Dispatcher／Publisher／Outbox 能力應按實際契約評估；介面名稱含 Publisher 或 Dispatcher 不等於 Vendor Bus，重新命名也不能隱藏對 Vendor API 的耦合。合法 Port 若混入 forbidden capability，禁止證據仍須生效。允許注入 Port 不提供交易原子性或可靠傳遞保證。

[DBA1017](../diagnostics/DBA1017.md)要求派送 Handler 對接一個 Use Case 目標，不自行持有 Repository／Domain Service 編排業務。這讓訊息入口、HTTP 及其他入口共用流程，並避免把業務流程綁在派送機制上。薄 Handler 增加一次轉換與一個入口類型；產品接受此成本以換取固定、可教導的責任界線。目前計算建構式參數、欄位與屬性中的不同 Use Case 相依型別，不能證明每條路徑恰好呼叫一次。[DBA1011](../diagnostics/DBA1011.md)另將所選 CQRS Handler 的 Command／Query 責任分開；Command 有回傳值仍可為 Command。

<a id="http"></a>
## HTTP API 的責任與查詢例外

Controller 處理 HTTP 輸入、呼叫應用邊界及轉換結果。[DBA1005](../diagnostics/DBA1005.md)限制直接持久化，[DBA1014](../diagnostics/DBA1014.md)限制繞過 Use Case 的相依。這讓 HTTP 狀態、序列化及框架生命週期不接管業務流程。相應的成本是簡單端點也可能需要小型 Use Case 與資料轉換。

只有明確選定的純查詢入口，可以直接使用窄 Query Repository／Service；此例外允許但不鼓勵。當版 `PureQuery` 以精確 controller type＋assembly＋enabled 明選整個 Controller，不是 action／endpoint method selector，也不自動繼承至 derived Controller。相依仍須通過共用 query-only 實際 Port 視圖；同時暴露 Aggregate／寫入能力的混合介面或 concrete Adapter 不因具有 query 角色放行。明選只記錄採用決定，靜態可見方法集合也不證明無副作用；是否為純查詢仍須驗證實際行為。

[DBA1004](../diagnostics/DBA1004.md)則選擇 ASP.NET Core Controller 使用真正 `ApiControllerAttribute` 的 API 行為，依據是 [Microsoft API 文件](methodology-references.md#s9)。當版核對精確 framework symbol 的直接、繼承與 assembly 層級屬性，不再接受無關同名 Attribute。此平台採用慣例與應用分層理由不同；現有 controller 角色仍可能涵蓋一般 MVC，不宣稱已縮限為 API-only，也不涵蓋 Minimal API 的完整入口判定。Attribute 存在並不驗證路由、模型驗證組態及實際 HTTP 執行結果。

<a id="mapper"></a>
## 純物件 Mapper 與資料完整性

ArchPrecept 的 `mapper` 指資料表示之間的純物件轉換：不持有執行個體狀態、不需要 DI、不存取 I/O、不讀取隱含時鐘或目前使用者，也不決定業務流程。依 [SRP／DIP](methodology-references.md#s4)分開轉換與流程責任後，產品再選定 `static` 類別作為明確程式契約。[DBA1007](../diagnostics/DBA1007.md)檢查 `static`，[DBA1008](../diagnostics/DBA1008.md)限制 Repository、Use Case 與 Handler 相依。

這項選擇降低轉換過程的依賴圖與隱藏狀態，讓映射方向、遺漏欄位及巢狀資料較容易逐項審查。代價是不以執行個體多型或注入服務完成此角色；需要補資料、查時鐘或決定策略的元件須按其實際 Assembler／Enricher／Adapter／Service 責任設計。不能只改成 `static` 就稱為純轉換，也不能藉「另一種架構允許 instance mapper」放寬已選角色。

[Fowler 的 Data Mapper](methodology-references.md#s5)是分離物件模型與資料庫的持久化模式，與本角色的範圍不同。重建必要狀態、集合與巢狀物件仍須依實際契約檢查資料完整性及不變條件；`static` 與無禁止相依均不證明往返完整、決定性或無副作用。

<a id="domain"></a>
## 領域狀態、事件重建與 Value Object

[DBA1003](../diagnostics/DBA1003.md)保護 Aggregate／Entity 的領域行為不依賴資料存取機制。其理由來自 [Dependency Rule](methodology-references.md#s2)及 [Aggregate 一致性邊界](methodology-references.md#s1)：不變條件應能在不啟動特定資料庫 Provider 的條件下推演與測試。這可能增加儲存模型轉換成本；目前 DbContext 子集檢查不是所有 I/O 或跨專案依賴的完整證明。

只有明確採用 Event Sourcing 的 Aggregate 受 [DBA1009](../diagnostics/DBA1009.md)約束。[事件重建及重播副作用分離](methodology-references.md#s7)支持事件轉換路徑，產品再選擇合法 Transition、pending events 與確認提交的契約。新行為與歷史重播須得到一致的領域狀態；重播不能成為新的 pending 變更，亦不能因重播重做外部副作用。代價是維護事件、版本演進及重建測試。一般 Aggregate 不因此被要求採 Event Sourcing。

當版只承認內建或精確映射 ES 根實際宣告的 canonical virtual／abstract `void When(TEvent)` 及其真正 override 為 transition；普通同名方法不取得豁免，宣告在 transition 內的 lambda／local function 也不繼承豁免。檢查範圍是方法內對 `this`／`base` instance state 的直接指派及遞增遞減，不包括 constructor／initializer、collection 呼叫或任意 helper 間接副作用；helper 自身的直接指派仍按其所在方法檢查。此契約與有界檢查不證明可重播性或事件已成功儲存；CT-005 的 apply／replay／append／commit 生命週期須另有行為證據。

[Value Object 的值語意與不變性](methodology-references.md#s1)支持避免值在共用後被悄悄修改。[DBA1018](../diagnostics/DBA1018.md)在明選 `ddd` 或單一 Rule enabled=true 時限制所有一般 setter（含 private／protected）、可寫欄位及已知可變集合。這比外部唯讀更嚴，成本是更新值須建立新值，持久化及序列化也須配合。允許 `init` 是一項程式形狀選擇，仍須檢查成員型別；`record`、getter-only 或唯讀集合介面不自動提供深層不可變性。

[DBA1019](../diagnostics/DBA1019.md)另選 VO 的宣告狀態不持有 Entity／Aggregate，以降低 identity、可變狀態及別名影響值語意的風險；以 ID 或不可變 Snapshot 表達必要資訊。這是保守產品政策，不聲稱作者 Reference 已提出「任何 VO 絕對不可參照 Entity」的普遍定理。成本是額外值模型與 Snapshot 語意。未知 Wrapper 的型別引數不能單獨證明實際持有 Entity，也不能當成安全證明；不確定部分須保持未評估。

<a id="evidence"></a>
## 設定、覆蓋與結論的證據責任

[DBA0001](../diagnostics/DBA0001.md)要求無效或歧義設定可觀察，避免設定讀取失敗後被當成合規。[DBA0002](../diagnostics/DBA0002.md)區分適用、停用、不適用及未評估，使角色辨識或靜態分析的界線可追查。這兩項是產品的證據契約，沒有必要為它們虛構方法論作者來源。

代價是設定錯誤會阻擋有效評估，而需要完整證據的團隊必須安排適當的輸出留存。磁碟上有設定檔不代表編譯器收到 AdditionalFiles；零 Diagnostic、`applicable` 或建置成功也不表示全部規則已執行並通過。判讀應合併實際編譯輸入、Analyzer 載入、違規、suppression、未評估路徑及所需執行期證據。局部 suppression 表示縮小檢查，不表示原規則已修復。

## 尚未定義的政策

「全面禁止 Aggregate static factory」與「跨 Bounded Context 傳遞一律限 MQ」的理由、適用條件及取捨仍未完整。前者仍須區分建構、領域建立不變條件與持久化重建的責任；[DDD Factory](methodology-references.md#s1)本身不足以支持全面禁令。後者仍須說明交付需求及跨邊界責任；[Hexagonal Architecture](methodology-references.md#s3)允許不同技術互動，不能直接推導 MQ-only。保留這兩項未決範圍，不據此新增 Analyzer 禁令，亦不自行放寬為另一政策；不能宣稱現有 Diagnostic 已強制或已驗證。
