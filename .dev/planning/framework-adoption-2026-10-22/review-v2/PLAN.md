# ArchPrecept 採納文件與首次安裝檢核計畫

2026-10-22。建議先完成本次文件檢核，等待 Framework v0.21.0 的正式成品後再安裝。這份計畫已固定三份專案文件草案的內容；0.21.0 的 catalog、engine、payload 與正式安裝 plan 尚不能在發布前固定。

本次沒有寫入專案根 `AGENTS.md`、`.dev/`、`.ai/` 或 `.agents/`，沒有修改產品程式、build／CI、Issue、Git 分支或發布狀態。審閱資料夾中的草案不會啟用 Framework 或取代現行指令。

## 先檢核這三份內容

| 預計寫入位置 | 可檢核草案 | 內容 |
|---|---|---|
| `AGENTS.md` | [完整入口草案](project/AGENTS.md) | 保留使用者提供的 MCP graph 優先區塊；加入來源導覽、適用性、驗證及授權邊界 |
| `.dev/README.MD` | [完整開發導覽](project/.dev/README.MD) | 連回既有產品文件、工作 Issues 與驗證入口；不建立第二份 backlog／store |
| `.dev/standards/framework-adoption.md` | [完整採納文件](project/.dev/standards/framework-adoption.md) | 產品／下游規則／測試三種範圍；既有決策、擬採納慣例、例外、未決及版本等待 |

本次對這三個目標位置的觀察均為不存在，預計動作為新增。每個草案的 raw SHA-256、bytes、目標及來源位置見 [逐檔計畫](file-change-plan.json)。若寫入前目標已由其他工作建立，應重新比對，不覆寫或把相同內容視為 installer ownership。

草案內相對連結按照「未來放到目標 repository」的位置撰寫；核對時以草案與既有 repository 的合併視圖解析，沒有為了讓審閱資料夾自足而複製產品文件。

## 這次採納文件的關鍵選擇

- 保留既有產品決策的原始狀態：已確認、暫定、已取代及待完成能力分開，不把整份 draft PRD 當成全部已批准或全部無效。
- Analyzer 不依賴 reference contracts，custom-contract 為既有產品方向；實際 classifier／coverage 缺口仍交 #6／#7，不用 consumer 重構掩蓋。
- 共享 Entity／Aggregate／VO base 是可選能力，與 Framework「不要求作為預設基底」可同時成立。
- Domain Event、Integration Event、Application port、Outbox 及 broker 保證分開；已發布 CT-004／CT-005 的具體契約不由 Framework 範例覆寫。
- 保留 DEC-019 的初期 ff-only／main-push 流程。選入 pr-author 提供能力，不替本產品強制新增 PR、required reviewer 或 source-repository 政策。
- DDD／CA／Hex Arch 慣例列為待核准的範圍對照；已登錄 Rule ID、未登錄 guidance、skill 可讀資源及 Diagnostic 的實際覆蓋分開。
- 知識先以明確參考用途使用。此草案沒有有效 normative authority binding；核准與正式檔案狀態／bytes 固定後，才可按實際需求建立精確 binding。

## 等待 v0.21.0 的評估

本次線上讀回的 [最新公開 Release](https://github.com/YuChia-Wei/ai-collaboration-framework/releases/latest) 為 v0.20.2，2026-10-20 23:02:51 UTC 發布。[PR #485](https://github.com/YuChia-Wei/ai-collaboration-framework/pull/485) 已於 2026-10-22 合併，來源 `1d9a42c80a0f0db2d69dc36d335a090bfb071281`；其回讀記錄將 #476–#480 排為 v0.21.0、Not yet published。未找到可據以承諾的發布日期。

| 選項 | 好處 | 成本與限制 | 本次建議 |
|---|---|---|---|
| 現在安裝 v0.20.2 | 有公開成品及既有 386-file 受管 payload 預演，立即取得所選能力 | 之後須退役合併的 ddd-ca-hex-architect，換兩個新 skills；重核 common／dotnet、Spec／BDD／DDD、bindings 與導覽。現有讀取缺口仍在 | 保留為需要提前使用時的備案；目前不執行 |
| 等 v0.21.0 正式發布後首次安裝 | 直接使用分拆職責，避免一次過渡安裝及後續遷移；更符合本次規範與需求／設計分工 | 發布時間及最終 payload 尚未固定；新版本仍須核對必要資源缺口、catalog 相容性和實際行為 | **建議採用，符合目前使用者偏好** |
| 從未發布 main／本機 commit 先組裝安裝 | 可提早試最新來源 | 額外承擔候選來源、版本演進與再驗證，並不等同正式 0.21.0 | 本次不採用 |

已合併的拆分直接改善本案的職責與知識消費：DDD exploration／modeling 與 software architecture 各有 skill；spec-author 新增 BDD／DDD 直接 consumption；orchestrator 與 review／implementation 的責任、知識 selectors 與版本 pins 一併調整。來源證據不是本專案安裝或 AI 操作驗收。

### 選配差異

以已合併來源作比較；右欄是來源版本，**不是對未發布 v0.21.0 成品的保證**。

| 能力 | v0.20.2 已核對版本 | 已合併的後續來源 |
|---|---|---|
| 領域與架構 | ddd-ca-hex-architect 0.3.1 | domain-model-designer 0.1.0 ＋ software-architect 0.1.0；舊 ID 退役，無 alias |
| 需求 | requirement-author 0.2.1 | 0.2.2 |
| 規格 | spec-author 0.2.0 | 0.3.0，含 BDD／DDD consumption |
| 測試設計 | test-designer 0.4.1 | 0.4.2 |
| 審查 | code-reviewer 0.2.4 | 0.2.5 |
| 局部／切片實作 | local-change-implementer、slice-implementer 0.2.5 | 各 0.2.6 |
| 編排 | software-development-orchestrator 0.3.2 | 0.4.0 |
| 共通知識 | engineering-common 0.3.0 | 0.4.0 |
| .NET 知識 | dotnet-backend 0.1.4 | 0.1.5，對應 common 0.4.0 |
| 其餘已選能力 | init 0.2.1；governance 0.1.1；auditor／ADR／diagnostic／compliance／PR 各 0.1.0 | 在上述拆分 commit 中版本不變；正式包仍需讀回 |

因此選配用途延續原方向，skills 數由 15 改為 16；仍為兩個 knowledge packages、Codex-only、original naming，不加入 workflow-records、Claude adapter 或 sub-agent profiles。詳見 [非執行用的 0.21 選配意向](selection-intent-v0.21.json)。

本輪另觀察到 Framework checkout 有 `5d80f8cf…` 的 #487 sub-agent task/model policy 本機 commit，而當時遠端 main 仍回傳 `1d9a42c8…`。本計畫不把它當成已發布或已確定納入 0.21.0 的內容，也不複製其中的模型或專案客製設定。

### 等待的條件

等待的是「適合本次選配的正式 v0.21.0 成品」，不是要求 Framework 所有 open Issues 都完成。

1. 正式 Release、ZIP、checksum、catalog 與 engine pin 可核對；不能只有 source merge／Project Version。
2. 重新讀取已選 16 skills、兩個知識包及相依版本；確認舊 ID 的處置，不能只改原 selection 的版本字串。
3. 核對所需 UseCase 角色關係資源。已合併 software-architect 仍缺少其直接 allowlist；不能預設 0.21.0 已修好。若仍缺，明列受影響操作，由 owner 接受有限覆蓋或等待修正，不宣稱完整符合。
4. 重建 candidate 及逐檔 plan，包含已核准的 project edits／authority pins；重新觀察目標是否有其他變更。
5. 有安裝授權後才 apply，並保留適當的持久 recovery 紀錄；之後再以少量代表操作驗證資源取得及決策保留。

#481／#483 的工作紀錄、#482／#484 的通用客製升級、#486 的記憶提案、#487 的 sub-agent 模型管理不是本次全部完成的先決條件。若正式包內變更會影響所選 init／orchestrator 的契約，再做該影響範圍的檢核。

## 最終檔案處置範圍

| 類別 | 預計變更 | 現在能固定的程度 |
|---|---|---|
| 專案文件 | 新增上述 3 份 Markdown；現有產品文件不搬移、不複製 | 已有完整草案與 raw hashes，可逐檔檢核 |
| 選配設定 | 未來新增 `.ai/custom/installation.json` | 0.21 正式成品確認後產生有效 selection；目前只有明確標示不可餵 installer 的選配意向 |
| Managed skills／knowledge | `.ai/core/skills/…`、`.ai/core/knowledge/…` | 以 0.21 正式 catalog 計算；本次不虛構檔案數或 SHA-256 |
| Codex runtime entries | `.agents/skills/<已選 skill>/SKILL.md` | 預計對應 16 skills；格式與 bytes 依正式 adapter |
| 引擎管理狀態 | `.ai/framework.lock`、`.ai/local/installation.guard`；操作期間可能有 recovery markers | 僅正式 apply 由引擎產生，不手寫 lock 或預建 marker |
| 既有資產 | src、tests、samples、docs、rules（若已有）、README、build／CI、IDE 與自訂 agent 設定 | 保留。未知碰撞先調和，不以目錄名稱推定 Framework ownership |
| 其他能力 | Claude、sub-agent profiles、workflow／memory stores、模型設定 | 不新增 |

這是供本輪檢核的文件與處置計畫，不是 0.21.0 的可執行 installer plan。正式包尚未存在，不能給出誠實的完整 payload identity／最終 apply hash。先前 v0.20.2 的 386 個新增 payload 檔案及引擎 plan 保留為比較基線，不稱為 0.21.0 計畫。

## 驗證與後續

本輪只核對草案 JSON／來源連結／相對路徑、使用者提供的 graph 區塊、文件狀態、選配差異與目標前置狀態。未跑產品 build／tests，因為產品沒有變更；也未執行 skill、安裝或發布驗收。

文件檢核及安裝授權分開。若採等待路徑，本次草案可先完成審閱；不因此建立排程、不自動通知或在版本出現時自行安裝。後續需要啟動監看或安裝時再依明確指示執行。
