# 依賴授權檢查結果

2026-10-19，檢查本次還原的 31 個 package/version。22 個 MIT、8 個 Apache-2.0、1 個 Microsoft .NET Library 條款。這是各套件主授權的核對，不抹除 upstream notices 內其他元件的授權。每個 nupkg、nuspec 與隨附 legal file SHA-256 見 [證據 manifest](dependency-license-evidence.json)。

| 套件 | 主授權 | 證據 |
|---|---|---|
| `Microsoft.CodeAnalysis.Analyzers/3.11.0` | MIT | nuspec SPDX expression |
| `Microsoft.CodeAnalysis.Analyzers/4.14.0` | MIT | nuspec SPDX expression |
| `Microsoft.CodeAnalysis.Common/4.8.0` | MIT | nuspec SPDX expression |
| `Microsoft.CodeAnalysis.Common/5.0.0` | MIT | nuspec SPDX expression |
| `Microsoft.CodeAnalysis.CSharp/4.8.0` | MIT | nuspec SPDX expression |
| `Microsoft.CodeAnalysis.CSharp/5.0.0` | MIT | nuspec SPDX expression |
| `Microsoft.CodeCoverage/18.0.1` | MIT | nuspec SPDX expression |
| `Microsoft.Extensions.Configuration.Abstractions/8.0.0` | MIT | nuspec SPDX expression |
| `Microsoft.Extensions.Primitives/8.0.0` | MIT | nuspec SPDX expression |
| `Microsoft.NET.Test.Sdk/18.0.1` | MIT | nuspec SPDX expression |
| `Microsoft.NETCore.Platforms/1.1.0` | Microsoft .NET Library terms | bundled dotnet_library_license.txt |
| `Microsoft.TestPlatform.ObjectModel/18.0.1` | MIT | nuspec SPDX expression |
| `Microsoft.TestPlatform.TestHost/18.0.1` | MIT | nuspec SPDX expression |
| `NETStandard.Library/2.0.3` | MIT | bundled LICENSE.TXT |
| `Newtonsoft.Json/13.0.3` | MIT | nuspec SPDX expression |
| `System.Buffers/4.5.1` | MIT | bundled LICENSE.TXT |
| `System.Collections.Immutable/7.0.0` | MIT | nuspec SPDX expression |
| `System.Memory/4.5.5` | MIT | bundled LICENSE.TXT |
| `System.Numerics.Vectors/4.4.0` | MIT | bundled LICENSE.TXT |
| `System.Reflection.Metadata/7.0.0` | MIT | nuspec SPDX expression |
| `System.Runtime.CompilerServices.Unsafe/6.0.0` | MIT | nuspec SPDX expression |
| `System.Text.Encoding.CodePages/7.0.0` | MIT | nuspec SPDX expression |
| `System.Threading.Tasks.Extensions/4.5.4` | MIT | bundled LICENSE.TXT |
| `xunit.abstractions/2.0.3` | Apache-2.0 | 2.0.3 tag 489a2b3949a9ae8587201a22a8ecfc1d791fa1f3; nuspec licenseUrl points to xunit/xunit/license.txt; historical license 8d0cdc1cf585f5cf285d221e0c8ff793e6aeea30 (Apache-2.0); tag has no bundled license file |
| `xunit.analyzers/1.18.0` | Apache-2.0 | nuspec SPDX expression |
| `xunit.assert/2.9.3` | Apache-2.0 | nuspec SPDX expression |
| `xunit.core/2.9.3` | Apache-2.0 | nuspec SPDX expression |
| `xunit.extensibility.core/2.9.3` | Apache-2.0 | nuspec SPDX expression |
| `xunit.extensibility.execution/2.9.3` | Apache-2.0 | nuspec SPDX expression |
| `xunit.runner.visualstudio/3.1.5` | Apache-2.0 | nuspec SPDX expression |
| `xunit/2.9.3` | Apache-2.0 | nuspec SPDX expression |

## 散布判斷

Messaging 的 Configuration.Abstractions 8.0.0 與 Primitives 8.0.0 是另由 NuGet 還原的 runtime 依賴，隨包 MIT 文字及 upstream THIRD-PARTY-NOTICES 已讀取並記錄 hash。其 notices 涵蓋 .NET Runtime 多項元件，不代表那些元件全部被 ArchPrecept 使用。若之後把 runtime 依賴包進應用交付物，應保留適用聲明；本 repo 不刪改上游套件。

Microsoft.NETCore.Platforms 1.1.0 的 dotnet_library_license.txt 明文允許用於設計、開發與測試，但再散布有額外條件。當前僅建置使用，四個 nupkg 未含該套件 DLL／runtime.json，也未將其宣告為產品相依，因此不將它改標 MIT，也不把它打包再授權為 Apache。

Roslyn 與 xUnit／TestPlatform 僅 build/test 使用，其 DLL 不在產品 nupkg。可取得的 notices 檔案已盤點雜湊；未替未包含的第三方程式建立虛假的「ArchPrecept 內嵌元件」聲明。

## 七個 legacy licenseUrl 的處理

NETStandard.Library 2.0.3、System.Buffers 4.5.1、System.Memory 4.5.5、System.Numerics.Vectors 4.4.0、System.Threading.Tasks.Extensions 4.5.4：直接讀取實際還原套件中的 MIT LICENSE.TXT，不再依賴會漂移的 master URL。Microsoft.NETCore.Platforms 1.1.0：讀取其隨附 Microsoft 條款。

xunit.abstractions 2.0.3：上游 [2.0.3 固定 tag](https://github.com/xunit/abstractions.xunit/tree/489a2b3949a9ae8587201a22a8ecfc1d791fa1f3) 與 nupkg 的 nuspec 都採 xunit/xunit 的 licenseUrl；對照 [發布時期固定 License](https://github.com/xunit/xunit/blob/8d0cdc1cf585f5cf285d221e0c8ff793e6aeea30/license.txt) 為 Apache-2.0。tag 本身不附 LICENSE，且其 nuspec 仍標 2.0.1（2.0.1／2.0.2／2.0.3 tags 指向同一 commit）；已還原 nupkg 的 nuspec 則為 2.0.3，也沒有 source-build SHA，故證據是實際套件授權 URL 與歷史授權文字，加上上游標籤的有限佐證，不聲稱已重現上游 build。此限制記錄為證據品質限制，當前不散布其 binary，不需要負責人猜測授權。

選定 Apache-2.0 不會改變依賴原條款。沒有依據要求替本次未散布的 build/test 套件複製所有上游 notices 到產品中；若未來改為 vendor／bundle／重發依賴，需重做散布範圍審查。
