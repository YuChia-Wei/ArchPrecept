---
title: "ArchPrecept 方法論與平台參考來源"
document_id: DOC-METHODOLOGY-REFERENCES
document_version: 0.2
last_reviewed: 2026-10-29
status: active
owner: "YuChia Wei"
lang: zh-TW
canonical_path: docs/product/methodology-references.md
---

# 方法論與平台參考來源

本文件登錄產品規則所使用的作者原文及官方平台文件，並限定各來源支持的結論。[規則設計理由](rule-design-rationale.md)說明 ArchPrecept 如何從這些原則選擇具體政策；[Diagnostic 手冊](../diagnostics/README.md)說明目前程式實際檢查的範圍。方法論原則、產品選擇及分析器覆蓋是三種不同資訊。

以下為必要摘要及產品解讀，不重製原文全文、圖表或範例，也不表示原作者為產品背書。來源公開可讀不代表整個網站或相關書籍均可自由再散布。查閱日期為 2026-10-29；平台文件的行為仍須依採用端實際版本驗證。

來源資格限作者、出版社、官方組織直接提供，或已核實由其授權的內容。未核實第三方鏡像、轉載 PDF 及來源不明作者稿不作規範依據。每項保留作者／作品、可查日期或版本、使用章節與官方 URL；沒有固定發行版本的平台頁以明列的文件版本及查閱日期定位，不將網站更新日期當成套件驗收結果。

S1 的改寫依其明示授權保留署名、作品、來源、授權連結及改寫說明。S2～S9 在本產品的使用方式僅為官方連結與原創必要摘要；本文不以未核實的開放重製權為依據，也不將第三方全文、圖表或教材打包。未來若要重製程式碼、翻譯或其他素材，須另核對該素材適用的授權與署名條件，不能沿用另一份文件的權利。

<a id="s1"></a>
## S1：Domain-Driven Design Reference

Eric Evans，*Domain-Driven Design Reference*，2015-03。使用 [Domain Language 官方入口](https://www.domainlanguage.com/ddd/reference/)及[官方 PDF](https://www.domainlanguage.com/wp-content/uploads/2016/05/DDD_Reference_2015-03.pdf)。相關位置為印刷頁 12（Value Objects）、16（Aggregates）、17（Repositories）、18（Factories）及 22（Assertions）。

此來源支持值語意與不變性、Aggregate 的一致性邊界、Repository 對模型物件的存取抽象，以及 Factory 封裝建立責任。ArchPrecept 的 identity-only Repository、私有 setter 限制及 VO 不持有 Entity 是各自的具體產品選擇；不能把這些檢查形式歸為 Evans 規定的唯一程式形狀，也不能據此主張全面禁止 Factory。

本文件及規則理由對上述內容作摘要、重組與產品化解讀，屬改寫而非原文翻譯。原作者為 Eric Evans，作品及來源如上；該版 Reference 封面明示 © 2015 Eric Evans 及 [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)授權。這項授權不延伸為整本 DDD 原書、另一份 Pattern Summaries 或其他版本資料的授權聲明。

<a id="s2"></a>
## S2：The Clean Architecture

Robert C. Martin，[*The Clean Architecture*](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)，2012-08-13。使用 Dependency Rule、Use Cases、Interface Adapters 及 Crossing Boundaries 各節。

此來源支持內層政策不依賴外層機制，並區分應用流程與輸入／輸出轉接責任。ArchPrecept 以明確 Port、傳輸中立資料及薄入口落實這項方向；`ExecuteAsync`、`*Input`、`Task`、必要取消參數及單一 Use Case 目標均是產品的進一步收斂。

<a id="s3"></a>
## S3：Hexagonal Architecture

Alistair Cockburn，[*Hexagonal Architecture*](https://alistair.cockburn.us/hexagonal-architecture)，HaT Technical Report 2005.02，2005-09-04 v0.9。使用 Intent、Motivation 及 Ports and Adapters 的說明。

此來源支持應用由使用者、程式、測試或批次等不同入口驅動，且可與外部裝置及資料庫隔離。Port 應表達應用所需的語意互動。來源沒有將合法入口限定為 HTTP 或訊息佇列，也沒有要求特定介面數量或資料夾結構。

<a id="s4"></a>
## S4：SOLID

Robert C. Martin，[*Solid Relevance*](https://blog.cleancoder.com/uncle-bob/2020/10/18/Solid-Relevance.html)，2020-10-18。使用 SRP、LSP、ISP 及 DIP 的說明。

此來源支持依變更原因分責、語意可替代的介面、消費者不依賴不需要的能力，以及高層政策對抽象的依賴。ArchPrecept 用窄 Port 及明確協作者降低責任混合；類別或參數數量、`static` 修飾詞本身都不足以證明完整 SOLID。

<a id="s5"></a>
## S5：Data Mapper

Martin Fowler，[*Data Mapper*](https://martinfowler.com/eaaCatalog/dataMapper.html)，2003-03-05。

此模式描述物件模型與資料庫之間的轉換及隔離。它用於澄清術語：ArchPrecept 的純物件 `mapper` 角色與這個持久化模式責任不同。不能以 Data Mapper 需要資料庫互動為由，推導純物件 Mapper 可執行資料存取；來源也不要求所有 Mapper 使用 `static`。

<a id="s6"></a>
## S6：CQRS

Martin Fowler，[*CQRS*](https://martinfowler.com/bliki/CQRS.html)，2011-07-14。

此來源說明更新與讀取可採不同模型，並提醒其額外複雜度。ArchPrecept 在已選讀寫分離的範圍內收斂 Repository、查詢服務與 Handler 責任；不由此要求所有系統採 CQRS、不同資料庫或特定訊息中介軟體。Command 是否回傳結果，不能單獨決定它是否為 Query。

<a id="s7"></a>
## S7：Event Sourcing

Martin Fowler，[*Event Sourcing*](https://martinfowler.com/eaaDev/EventSourcing.html)，2005-12-12，作者標示為 draft。使用狀態重建及 External Updates 的討論，保留其草稿性質。

此來源支持從事件重建狀態，並區分重播與外部副作用。它不指定 ArchPrecept 的 `When`、pending events 或 commit API，也不提供儲存成功、交易原子性或 exactly-once 的證明。

<a id="s8"></a>
## S8：Event Sourcing 平台模式

Microsoft Azure Architecture Center，[*Event Sourcing pattern*](https://learn.microsoft.com/en-us/azure/architecture/patterns/event-sourcing)，頁面更新日期 2026-03-28。使用 Solution／Workflow、Pattern advantages、Issues and considerations 的討論。

此來源支持事件重建、物化檢視更新及重複交付下的冪等考量。讀模型更新者可以寫入其讀取儲存區，與只負責讀取的查詢 Port 不同。文中的平台配置及儲存拓樸不自動成為 ArchPrecept 的必要條件。

<a id="s9"></a>
## S9：ApiControllerAttribute

Microsoft，[*ApiControllerAttribute API*](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.apicontrollerattribute?view=aspnetcore-10.0)，ASP.NET Core 10.0 參考頁的 Definition；另用 [Create web APIs with ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0#apicontroller-attribute) 的 ApiController attribute、Attribute on multiple controllers 及 Attribute on an assembly 各節，頁面更新日期 2026-07-22。

API Definition 提供平台型別、Assembly／Class 適用位置及 Inherited 身分；Web API 指南說明屬性所啟用的 API 行為、基底類別繼承及 Assembly 層級用法。ArchPrecept 選擇 Controller 使用此能力，是平台採用慣例；這不是 DDD 或 Clean Architecture 的必要語法。官方區分支援 views 的 Controller 與 ControllerBase，而本產品目前 controller 角色仍可能包含一般 MVC。文件版本與靜態 symbol 回歸不等於全部受支援 ASP.NET Core 版本的 HTTP 行為均已驗證。

## 來源與規則的使用原則

每條手冊的「設計理由與取捨」列出支撐原則及產品收斂。設定有效性與覆蓋資訊屬於 ArchPrecept 自身的證據契約，不為 DBA0001／DBA0002 補造 DDD 或架構作者出處。外部原則未規定某項禁令，不能單獨用來推翻產品已選政策；新增政策也不能只由分析器現有判定倒推正當性。
