# 0.0.1-preview 發布紀錄

2026-10-19，四個套件於 GitHub Packages 公開發布，並以 .NET SDK 8 與 10 完成安裝驗收。[套件發布](https://github.com/YuChia-Wei/ArchPrecept/actions/runs/39207624875) · [套件還原驗收](https://github.com/YuChia-Wei/ArchPrecept/actions/runs/39208069155)

## 發布成品

發布來源為 `54b6a88fe373107f85a52caf79d129d396209cd4`。[v0.0.1-preview](https://github.com/YuChia-Wei/ArchPrecept/tree/v0.0.1-preview)

## 套件內容

| Package ID | 內容 | 基線 |
|---|---|---|
| ArchPrecept.Analyzers.Core | `analyzers/dotnet/cs` 中的 Analyzer DLL，不含 Roslyn 或 Contracts runtime 相依 | netstandard2.0 / Roslyn 4.8 |
| ArchPrecept.Contracts.Domain | 領域參考契約 | net8.0 |
| ArchPrecept.Contracts.Application | 應用契約，依賴 Domain | net8.0 |
| ArchPrecept.Contracts.Messaging.Abstractions | 訊息抽象，依賴 Configuration.Abstractions 8.0.0 | net8.0 |
