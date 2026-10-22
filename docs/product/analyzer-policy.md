# Analyzer 設定、角色與評估契約（0.0.3-preview）

本文件承接 #6、#7、#11 與 PRD CFG／RC／RM 需求，是本產品採用政策，無需安裝 Framework。一般 DDD 知識不自動使團隊慣例成為所有採用端的強制規範；Framework 的知識／skills 安裝仍依 `.dev/planning` 的等待決定另行評估。

## 相容基線與選用

未提供設定時沿用 `legacy`：保留 DBA1001–DBA1017 的啟用與 severity，以及原有名稱慣例。名稱證據標為低信心，不代表精確契約。修正真正領域事件 port 的誤報是本版刻意相容修正。

提供設定時可選 `core`、`web-api`、`messaging`、`event-sourcing`、`framework`、`ddd` 的聯集。`framework` 表示本產品明列的 ExecuteAsync／static Mapper／薄 Handler 等慣例，不表示已安裝或驗收 AI Collaboration Framework。`ddd` 才啟用兩條新增 VO 規則，初始 Warning；既有 consumer 未選用時不新增阻擋。實際規則清單、預設值、角色與來源統一由 `rules/catalog.xml` 提供。

## 設定合併

採用者透過 MSBuild `AdditionalFiles` 明確加入 `archprecept.repository.xml` 與／或 `archprecept.project.xml`。檔案必須唯一且符合 `rules/configuration.xsd`。缺檔、重複、未知鍵／角色／Profile、無效值、無法解析的明確 mapping 均回報 DBA0001；此狀態不算合規。

固定優先序：相容預設 → repository 設定 → project 設定 → 個別 Rule 的 enabled／severity → 標準 Roslyn `dotnet_diagnostic.DBAxxxx.severity`／命令列設定。Project 只覆寫明確提供的項目；Profile 集合整組替換，Role mapping／Convention 依 role、Expected 依 type、Rule 依 id 覆寫。選定 Profile 後，個別 Rule 的 enabled 可明確啟用／停用。標準 severity 不會自行啟用被 Profile 排除的規則。

```xml
<ArchPrecept version="1" profiles="core,web-api,ddd" namingConventions="false">
  <Role name="value-object" metadataName="My.Domain.IValue" assembly="My.Domain" />
  <Role name="entity" metadataName="My.Domain.IEntity`1" assembly="My.Domain" />
  <Expected type="My.Domain.Money" role="value-object" />
  <Rule id="DBA1018" enabled="true" severity="error" />
</ArchPrecept>
```

所有明確契約使用完整 metadata name（泛型 arity 用反引號），assembly 為實際組件名稱；以 Roslyn symbol equality 比對解析後原始定義，沿 interface／base／generic constraint 傳遞。內建來源另檢查組件身分（名稱、culture、public key token；版本採同一 major 的相容範圍），不依簡單型別名取得內建豁免。本產品組件目前未簽章，組件名稱不是防偽或安全邊界。

同一角色優先序：明確 mapping → 內建契約 → 繼承契約 → 明確選用的 Convention。`Convention` 可指定 suffix 與 namespace 前綴，兩者皆指定時必須同時符合；namespace 以完整區段比對。未啟用命名慣例時，同名無關型別不作為角色證據。多角色可同時存在；合法 domain-event-dispatcher 不豁免另一個禁止的 bus／transport／Use Case 角色。

## 宣告的評估範圍

只有 `Expected` 指定的型別／角色要求覆蓋回報，不掃描所有普通 helper 當作缺口。DBA0002 的 properties 區分 `applicable`、`not-evaluated`、`disabled`、`not-applicable`；`applicable` 只表示角色、設定與宣告形狀符合適用條件，並不驗證規則執行，通過仍須合併該次編譯所有診斷、設定錯誤與載入狀態判讀。不得將它稱為完整架構合格。

一般規則診斷 properties 包含 Rule／DBA ID、角色、證據、信心、型別與狀態；可由 `dotnet build -p:ErrorLog=diagnostics.sarif` 擷取。Generated code 不執行規則；明確預期的 generated 型別回報 not-evaluated。未涵蓋的語法／宣告位置、找不到型別或角色不足會給理由。Analyzer 載入錯誤與 AD0001 必須由 consumer／CI 另外當成失敗。

## 程序內領域事件 port

真正 `ArchPrecept.Contracts.Application.IDomainEventDispatcher`，或明確映射的自有 `domain-event-dispatcher`，可以注入 Use Case。其責任是程序內領域事件分派；不提供 Outbox、交易原子性、持久化或可靠傳遞。衍生 port 若混入禁止的 bus／mediator／transport／另一 Use Case，禁止角色優先，仍產生 DBA1016。只靠同名或改名不得取得合法 port 豁免。

## ValueObject 規則的本版語意

分配 DBA1018（VO 可變狀態）與 DBA1019（VO Entity 參考），不沿用其他產品 DDD001／DDD002 的識別碼。只以精確內建／明確映射辨識 VO／Entity，不以名稱含 ValueObject／Entity 推定。

- DBA1018 檢查 instance 一般 setter（含 private）、非 readonly field，以及已知可變集合／陣列。init 本身允許；readonly／getter-only 不免除成員型別檢查。靜態成員不屬 instance 狀態。
- DBA1019 檢查 instance property／field 宣告型別，追蹤 Entity、陣列、已知 collection／dictionary、tuple、nested container 與 generic constraint，回報具體路徑。未知 wrapper 的型別引數不等於真的持有 Entity，回報未評估而不製造違規。
- scalar、enum、可解析的 immutable snapshot 可通過這些有界檢查；object／dynamic、無約束 generic、未知 metadata reference type、循環或深度上限標為未評估。此規則不是 deep immutability 證明，也不推論反射或執行期 alias 行為。
- 繼承的 instance 成員納入檢查。已由被辨識基底處理的相同成員不在衍生型別重複報錯；來源外成員的診斷定位到採用者型別並說明原成員路徑。

上述嚴格 private setter 政策保護 VO 的狀態不變意圖；只允許外部不可寫不是本版另一模式。需要其他政策的採用者可不選 `ddd`、停用個別規則或對有理由的最小位置 suppression。
