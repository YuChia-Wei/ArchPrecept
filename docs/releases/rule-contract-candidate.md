# 規則契約與 API 文件候選

狀態：**未發布的來源候選**。本頁描述 #12～#15、#17 與 Id 初始化修正的產品差異，不是 NuGet 發布紀錄。已發布版仍為 0.0.3-preview；本機重建暫沿用來源版號，其 bytes 不得上傳覆寫既有版本。需要交付更新到 feed 時，須另選未發布版本並完成發布前檢查。

## 相對已發布 0.0.3-preview 的差異

Rule／Diagnostic ID、Profile 與 severity 保持目前 Catalog。未提供設定仍採名為 `legacy` 的預設集合，沒有新建平行相容模式。具體差異如下：

| 範圍 | 已發布基線 | 來源候選 |
|---|---|---|
| DBA1001 | 直接方法、第一組 Aggregate／ID 契約 | 有效繼承、逐 port 泛型替換、顯式／基底 adapter 實作；分離 query port 不因聯集誤報，額外公開能力仍檢查 |
| DBA1002／1010 | IServiceProvider 文字與 Locator／Inject 短名近似 | BCL symbol、精確 service-locator mapping 與平台 injection attribute；普通同名型別不再觸發 |
| DBA1003／1008 | 外層語法可能包含巢狀型別 | 依實際型別歸責；DBA1003 訊息限已辨識 DbContext 子集 |
| DBA1004／1005／1006／1012 | ApiController 短名、SaveChanges 短名、只明示 new | 真正有效 attribute（direct／base／assembly）、持久化契約 API 與 override／實作鏈、target-typed new；ControllerBase／MVC 適用範圍不縮限 |
| DBA1007／1013 | static Mapper、query reader 禁寫 | 保留政策；精確寫入 API，static 不宣稱 purity；read-model projector store ownership 仍未分析 |
| DBA1009 | 任意 When 名稱豁免 | 精確 ES 根 canonical virtual／abstract transition 及 override；普通 When／lambda／local function 不取得豁免，排除 static 與其他實例狀態誤報 |
| DBA1014 | query role 寬豁免 | schema `PureQuery(type, assembly, enabled)` 明選整個 Controller，且實際依賴 view 為 query-only；未選、混合介面及寫入 concrete adapter 不放行 |
| DBA1015 | 直接 ExecuteAsync、Command／Request 名稱當 transport | 有效繼承／顯式實作／override；真正 BCL Task／CT／scalar；Input 形式政策與 transport 身分分開，含有界 generic／member 檢查 |
| DBA1011／1016／1017 | CQRS marker shape、constructor 禁止依賴、參數型別數 | Command 可回傳結果；1016 補直接 field／property／method parameter／invocation 與 Application-owned ports；1017 計入 field／property 的不同依賴型別，明確不驗證呼叫次數 |
| DBA1018／1019 | ddd opt-in／Warning，精確 VO role | 保留原有可變狀態與 Entity-reference-free 嚴格政策及 not-evaluated 界線 |
| DomainEntity.Id | 無參數建構後未初始化卻暴露 non-null Id | 保留重建路徑；未指派讀取擲 InvalidOperationException，null 指派擲 ArgumentNullException；不禁止合法 default value ID，也不新增 identity 不可再指派政策 |
| 文件 | 部分 API 說明與隨包文件不足，HelpLink 指向 main | 補充原生 XML 文件註解及使用指南，同源 docs／Catalog 隨包，HelpLink／SARIF 定位 source SHA |

各 predicate、正反例、設定與深度限制以同一修訂的 [Diagnostic 手冊](../diagnostics/README.md) 與 [Analyzer 契約](../product/analyzer-policy.md) 為準。`PureQuery` 不是任意 endpoint method selector；`service-locator`、`query-service`、`integration-event-publisher`、`outbox-port` 是新增的明確角色，不由名稱猜測能力。

## 不可變版本定位

| 成品 | 實際發布來源 | 固定規則文件 |
|---|---|---|
| 0.0.1-preview | `54b6a88fe373107f85a52caf79d129d396209cd4` | [v0.0.1-preview](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.1-preview/docs/diagnostics) |
| 0.0.2-preview | `a51bc09a7d68d0477788019b12c54b7e0b461982` | [v0.0.2-preview](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.2-preview/docs/diagnostics) |
| 0.0.3-preview | `fdc1f9ff3934c769047f797803abc8614290e889` | [v0.0.3-preview](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.3-preview/docs/diagnostics) |
| 目前來源候選 | `.nuspec` repository commit 與組件 InformationalVersion 的 `+<SHA>` | 隨包 `docs/`；Diagnostic HelpLink 為 `/blob/<SHA>/docs/diagnostics/<ID>.md` |

舊套件內既有 main HelpLink 無法修改；使用上表 immutable provenance tag 找到其真正原始 predicate，不能把目前 main 當成舊版說明。`45e7fd4` 是後補 0.0.3 發布紀錄／consumer 測試，不是產生成品的來源。新候選缺少 Git revision 時標為 `unversioned`，不通過版本文件驗收。

套件內 docs 與 repository 是同一來源檔案，不維護第二份 Catalog。規則 metadata 由 `rules/catalog.xml` 供應，schema 位於 `rules/`，Analyzer 另提供 `schemas/`；原生 XML 文件隨 DLL 提供，[封裝入口](../../eng/Build-Packages.ps1)檢查必要文件存在。

## 驗證界線

驗證依實際變更選取 Analyzer 正反例、同名干擾、未辨識／設定失敗與 severity，Domain／ES identity 生命週期、必要 nupkg entries，以及受影響的 SDK 8／10 package consumer、custom-contract 與版本文件定位。實際結果依對應來源及執行範圍判讀，不能由本頁存在推定通過，也不因提交 SHA 改變而重跑所有檢查。

低成本模型發布前審查需在候選成品與差異完成後執行；模型、候選 SHA、發現及複查保存在現有工作證據，不預填發布成功。本輪不建立新的 NuGet 版本，後續真正 feed 發布仍須其自己的成品與 consumer 證據。

全面禁止 static Aggregate factory、跨 BC 限 MQ 的理由及條件仍未決，不新增 Analyzer 禁令，也不藉這次 predicate 修正放寬既有已選政策。任意 helper effect、projector store ownership、深層不可變性、交易／broker 交付，以及完整架構合規均不在本候選的靜態證明範圍。
