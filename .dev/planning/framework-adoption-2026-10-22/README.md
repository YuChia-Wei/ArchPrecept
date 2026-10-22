# Framework 採納規劃保存

2026-10-22：負責人接受草案方向，決定等待 Framework **0.21.0 正式發布後**，再重新評估知識包、skills 與 AI 協作資源的安裝細項。目前不安裝。

`review-v2/` 原樣保存已檢核的八個檔案；複製後逐檔 SHA-256 與原審閱成品一致。原成品中的絕對路徑及發布觀察是當時快照。未套用其中的 patch；`project/` 是目標樹草案，當中相對連結應以 repository 目標位置解讀。

- [完整採納計畫與等待選項](review-v2/PLAN.md)
- [AGENTS.md 草案](review-v2/project/AGENTS.md)
- [.dev 導覽草案](review-v2/project/.dev/README.MD)
- [規範適用範圍、既有決策與例外](review-v2/project/.dev/standards/framework-adoption.md)
- [逐檔變更計畫](review-v2/file-change-plan.json)
- [文件 patch（未套用）](review-v2/proposed-project-documents.patch)
- [0.21 選配意向（不可直接執行）](review-v2/selection-intent-v0.21.json)
- [當時發布觀察](review-v2/release-observation.json)

正式版發布後重新核對 catalog identity、版本、依賴閉包、必要知識讀取範圍及 UseCase 權威文件缺口，再產生安裝逐檔計畫。不得將目前的候選版本、檔案數或草案當成正式安裝成品，也不得因產品開發已繼續而推定 AI 協作資源安裝已獲核准。

本次保存規劃與 ArchPrecept 0.0.3-preview 開發／發布是兩個範圍。產品 Analyzer、Contracts、測試與 consumer 不依賴 Framework 安裝。
