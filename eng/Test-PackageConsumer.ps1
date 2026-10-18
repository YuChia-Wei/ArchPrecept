param(
    [ValidateSet(8, 9, 10)][int]$SdkMajor = 8,
    [string]$PackageDirectory = 'artifacts/packages'
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot
$feed = (Resolve-Path (Join-Path $root $PackageDirectory)).Path
$version = ([xml](Get-Content (Join-Path $root 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
# A fresh cache proves this run consumes these nupkg bytes, not an older 0.0.1-preview.
$work = Join-Path $root "artifacts/consumer-$SdkMajor-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $work -Force | Out-Null
'<Project />' | Set-Content (Join-Path $work 'Directory.Build.props')
@{sdk = @{version = "$SdkMajor.0.100"; rollForward = 'latestFeature'; allowPrerelease = $false}} | ConvertTo-Json | Set-Content (Join-Path $work 'global.json')
$escapedFeed = [Security.SecurityElement]::Escape($feed)
@"
<configuration><packageSources><clear /><add key="local" value="$escapedFeed" /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources><packageSourceMapping><packageSource key="local"><package pattern="ArchPrecept.*" /></packageSource><packageSource key="nuget.org"><package pattern="*" /></packageSource></packageSourceMapping></configuration>
"@ | Set-Content (Join-Path $work 'NuGet.Config')
$previousPackages = $env:NUGET_PACKAGES
$env:NUGET_PACKAGES = Join-Path $work 'cache'
Push-Location $work
try {
    $selected = dotnet --version
    if ($selected -notlike "$SdkMajor.*") { throw "Wrong SDK selected: $selected" }
    foreach ($mode in @('analyzer-only', 'contracts')) {
        New-Item -ItemType Directory $mode | Out-Null
        $references = "<PackageReference Include=`"ArchPrecept.Analyzers.Core`" Version=`"$version`" PrivateAssets=`"all`" />"
        if ($mode -eq 'contracts') {
            foreach ($id in @('ArchPrecept.Contracts.Domain','ArchPrecept.Contracts.Application','ArchPrecept.Contracts.Messaging.Abstractions')) {
                $references += "<PackageReference Include=`"$id`" Version=`"$version`" />"
            }
        }
        @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net$SdkMajor.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><WarningsAsErrors>CS8032;CS9057;AD0001</WarningsAsErrors></PropertyGroup><ItemGroup>$references</ItemGroup></Project>
"@ | Set-Content "$mode/Consumer.csproj"
        $source = if ($mode -eq 'contracts') {
@'
using ArchPrecept.Contracts.Application;
using ArchPrecept.Contracts.Domain;
using ArchPrecept.Contracts.Messaging.Abstractions;
if (!Result<int>.Success(42).IsSuccess) throw new Exception("Result contract failed");
Console.WriteLine(typeof(IAggregateRoot<>).FullName);
Console.WriteLine(typeof(IIntegrationEvent).FullName);
'@
        } else { 'Console.WriteLine("Analyzer-only consumer");' }
        $source | Set-Content "$mode/Program.cs"
        dotnet run --project "$mode/Consumer.csproj" -c Release
        # Custom contract must produce the expected rule from the installed analyzer.
        'public interface IRepository<TEntity, TId> { }' | Set-Content "$mode/Violation.cs"
        $PSNativeCommandUseErrorActionPreference = $false
        $diagnostics = dotnet build "$mode/Consumer.csproj" -c Release --no-restore 2>&1 | Out-String
        $code = $LASTEXITCODE
        $PSNativeCommandUseErrorActionPreference = $true
        $diagnostics | Set-Content "$mode/expected-diagnostic.log"
        if ($code -eq 0 -or $diagnostics -notmatch 'error DBA1001' -or $diagnostics -match 'CS8032|CS9057|AD0001') { throw "Analyzer diagnostic smoke failed: $diagnostics" }
        # Actions' pwsh wrapper propagates LASTEXITCODE, including expected negative builds.
        # Clear it only after the diagnostic assertion succeeds; unexpected failures still throw.
        $global:LASTEXITCODE = 0
    }
    Write-Host "Package consumers passed on SDK $selected (net$SdkMajor.0): analyzer-only and reference contracts; DBA1001 verified."
} finally {
    Pop-Location
    $env:NUGET_PACKAGES = $previousPackages
}
