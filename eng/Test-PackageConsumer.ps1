param(
    [ValidateSet(8, 10)][int]$SdkMajor = 8,
    [string]$PackageDirectory = 'artifacts/packages',
    [string]$RegistrySource
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot
$feed = if ($RegistrySource) { $RegistrySource } else { (Resolve-Path (Join-Path $root $PackageDirectory)).Path }
$version = ([xml](Get-Content (Join-Path $root 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
# A fresh cache proves this run consumes these nupkg bytes, not an older local build.
$work = Join-Path $root "artifacts/consumer-$SdkMajor-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $work -Force | Out-Null
'<Project />' | Set-Content (Join-Path $work 'Directory.Build.props')
@{sdk = @{version = "$SdkMajor.0.100"; rollForward = 'latestFeature'; allowPrerelease = $false}} | ConvertTo-Json | Set-Content (Join-Path $work 'global.json')
$escapedFeed = [Security.SecurityElement]::Escape($feed)
# Exercise consumers whose feeds allow individual package IDs, including transitive dependencies.
$packageIds = @('ArchPrecept.Analyzers.Core', 'ArchPrecept.Contracts.Domain', 'ArchPrecept.Contracts.Application', 'ArchPrecept.Contracts.Messaging', 'ArchPrecept.Contracts.Application.Messaging', 'ArchPrecept.Contracts.Domain.EventSourcing')
$sourceMappings = ($packageIds | ForEach-Object { "<package pattern=`"$_`" />" }) -join ''
@"
<configuration><packageSources><clear /><add key="local" value="$escapedFeed" /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources><packageSourceMapping><packageSource key="local">$sourceMappings</packageSource><packageSource key="nuget.org"><package pattern="*" /></packageSource></packageSourceMapping></configuration>
"@ | Set-Content (Join-Path $work 'NuGet.Config')
$previousPackages = $env:NUGET_PACKAGES
$env:NUGET_PACKAGES = Join-Path $work 'cache'
Push-Location $work
try {
    $selected = dotnet --version
    if ($selected -notlike "$SdkMajor.*") { throw "Wrong SDK selected: $selected" }
    foreach ($mode in @('analyzer-only', 'contracts', 'event-sourcing', 'messages', 'publisher', 'host-configuration')) {
        New-Item -ItemType Directory $mode | Out-Null
        $usesAnalyzer = $mode -in @('analyzer-only', 'contracts', 'event-sourcing')
        $directIds = @(switch ($mode) {
            'analyzer-only' { 'ArchPrecept.Analyzers.Core' }
            'contracts' { 'ArchPrecept.Analyzers.Core'; 'ArchPrecept.Contracts.Application' }
            'event-sourcing' { 'ArchPrecept.Analyzers.Core'; 'ArchPrecept.Contracts.Domain.EventSourcing' }
            'messages' { 'ArchPrecept.Contracts.Messaging' }
            'publisher' { 'ArchPrecept.Contracts.Application.Messaging' }
        })
        $references = ''
        foreach ($id in $directIds) {
            $privateAssets = if ($id -eq 'ArchPrecept.Analyzers.Core') { ' PrivateAssets="all"' } else { '' }
            $references += "<PackageReference Include=`"$id`" Version=`"$version`"$privateAssets />"
        }
        if ($mode -eq 'host-configuration') {
            $references += '<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />'
            Copy-Item -LiteralPath (Join-Path $root 'samples/Messaging/Host/MessagingTransportOptions.cs') -Destination "$mode/MessagingTransportOptions.cs"
        }
        @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net$SdkMajor.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><WarningsAsErrors>CS8032;CS9057;AD0001</WarningsAsErrors></PropertyGroup><ItemGroup>$references</ItemGroup></Project>
"@ | Set-Content "$mode/Consumer.csproj"
        $source = if ($mode -eq 'contracts') {
@'
using ArchPrecept.Contracts.Application;
using ArchPrecept.Contracts.Domain;
if (!Result<int>.Success(42).IsSuccess) throw new Exception("Result contract failed");
Console.WriteLine(typeof(IAggregateRoot<>).FullName);
'@
        } elseif ($mode -eq 'event-sourcing') {
            Get-Content (Join-Path $root 'samples/EventSourcing/Program.cs') -Raw
        } elseif ($mode -eq 'messages') {
            Get-Content (Join-Path $root 'samples/Messaging/Messages.cs') -Raw
        } elseif ($mode -eq 'publisher') {
            Get-Content (Join-Path $root 'samples/Messaging/Publisher.cs') -Raw
        } elseif ($mode -eq 'host-configuration') {
            Get-Content (Join-Path $root 'samples/Messaging/Host/Program.cs') -Raw
        } else { 'Console.WriteLine("Analyzer-only consumer");' }
        $source | Set-Content "$mode/Program.cs"
        dotnet run --project "$mode/Consumer.csproj" -c Release
        $assets = Get-Content "$mode/obj/project.assets.json" -Raw | ConvertFrom-Json
        $installed = @($assets.libraries.PSObject.Properties.Name)
        $expectedIds = @($directIds)
        if ($mode -in @('contracts', 'event-sourcing')) { $expectedIds += 'ArchPrecept.Contracts.Domain' }
        if ($mode -eq 'publisher') { $expectedIds += 'ArchPrecept.Contracts.Messaging' }
        $actualArchPrecept = @($installed | Where-Object { $_ -like 'ArchPrecept.*' })
        if ($actualArchPrecept.Count -ne $expectedIds.Count) { throw "Unexpected ArchPrecept dependency set for $mode : $actualArchPrecept" }
        foreach ($id in $expectedIds) {
            if ("$id/$version" -notin $actualArchPrecept) { throw "Missing expected dependency for $mode : $id/$version" }
        }
        if ($mode -ne 'host-configuration' -and ($installed -match '^Microsoft\.Extensions\.Configuration')) { throw "Configuration leaked into contracts for $mode" }
        if (!$usesAnalyzer) { continue }
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
        if ($mode -eq 'event-sourcing') {
            @'
using ArchPrecept.Contracts.Domain;
using ArchPrecept.Contracts.Domain.EventSourcing;
public sealed class InvalidAggregate : EsAggregateRoot<string>
{
    public int State { get; private set; }
    public void ChangeDirectly() { State = 1; }
    protected override void When(IDomainEvent e) { }
}
'@ | Set-Content "$mode/Violation.cs"
            $PSNativeCommandUseErrorActionPreference = $false
            $diagnostics = dotnet build "$mode/Consumer.csproj" -c Release --no-restore -warnaserror:DBA1009 2>&1 | Out-String
            $code = $LASTEXITCODE
            $PSNativeCommandUseErrorActionPreference = $true
            $diagnostics | Set-Content "$mode/expected-es-diagnostic.log"
            if ($code -eq 0 -or $diagnostics -notmatch 'error DBA1009' -or $diagnostics -match 'CS8032|CS9057|AD0001') { throw "EventSourcing analyzer smoke failed: $diagnostics" }
            $global:LASTEXITCODE = 0
        }
    }
    Write-Host "Package consumers passed on SDK $selected (net$SdkMajor.0): six adoption modes including standalone messages, publisher and host-owned configuration; DBA1001/DBA1009 verified."
} finally {
    Pop-Location
    $env:NUGET_PACKAGES = $previousPackages
}
