# 用一般測試限制組件的直接依賴

本機候選 `0.0.6-preview` 的 `ArchPrecept.Templates` 提供 `ap-boundary-test`。在已有 xUnit 的測試專案加入受檢與禁止依賴的兩個專案參考，各選一個**定義在該組件內**的公開型別。產品專案無需加入 ArchPrecept 套件。

```shell
dotnet new ap-boundary-test -n DomainCannotReferenceSql --sourceType Shop.Domain.Order --forbiddenType Shop.Sql.OrderStore -o tests/Shop.Tests
dotnet test tests/Shop.Tests/Shop.Tests.csproj
```

`DomainCannotReferenceSql` 是採用端為這條規則指定的識別碼，也是產生的測試型別。當 Domain 編譯結果直接參考 Sql，失敗訊息會列出規則名稱、來源組件檔及目標組件身分。將資料存取移回外層後重新執行測試。Sql → Domain 與未選的其他組件均可存在。要增加一條禁止邊，以另一個名稱選擇另兩個型別；要測不同 TFM 或建置組態，對各自輸出執行測試。

測試從一般未裁剪組件的 AssemblyRef metadata 讀取直接參考，按名稱、culture、public key token 比對，涵蓋該組件身分的各版本。產生碼與第三方套件只要實際留下這條參考，就納入判讀。必要型別或組件缺少時，編譯或載入會失敗；選到同一組件也會失敗。不同 namespace 的同名型別不影響組件辨識。相同名稱、culture 及簽章身分的二進位在此規則視為同一目標。[.NET 組件參考 API](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly.getreferencedassemblies)

這個範圍不涵蓋未用到的 ProjectReference、傳遞依賴、namespace 之間的使用、反射載入或部署時服務呼叫。採用端選擇邊界及方向，測試只判讀選定的直接參考。[Dependency Rule 的來源](product/methodology-references.md#s2)；對單一 Aggregate／Entity 使用 DbContext 的語意檢查另見 [DBA1003](diagnostics/DBA1003.md)。
