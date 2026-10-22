param(
    [ValidateSet(8, 10)][int]$SdkMajor = 8,
    [string]$PackageDirectory = 'artifacts/packages',
    [string]$RegistrySource,
    [string]$ExpectedPackageDirectory,
    [string]$ScratchRoot
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$root = Split-Path $PSScriptRoot
$packagePath = if ([IO.Path]::IsPathRooted($PackageDirectory)) { $PackageDirectory } else { Join-Path $root $PackageDirectory }
$feed = if ($RegistrySource) { $RegistrySource } else { (Resolve-Path $packagePath).Path }
$version = ([xml](Get-Content (Join-Path $root 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
$expectedPath = if ($ExpectedPackageDirectory -and [IO.Path]::IsPathRooted($ExpectedPackageDirectory)) { $ExpectedPackageDirectory } elseif ($ExpectedPackageDirectory) { Join-Path $root $ExpectedPackageDirectory } else { $null }
$expectedFeed = if ($expectedPath) { (Resolve-Path $expectedPath).Path } elseif (!$RegistrySource) { $feed } else { $null }
# A fresh cache proves this run consumes these nupkg bytes, not an older local build.
$workParent = if ($ScratchRoot) { $ScratchRoot } else { Join-Path $root 'artifacts' }
$work = Join-Path $workParent "consumer-$SdkMajor-$([guid]::NewGuid().ToString('N'))"
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
    foreach ($mode in @('analyzer-only', 'contracts', 'contracts-only', 'event-sourcing', 'messages', 'publisher', 'host-configuration')) {
        New-Item -ItemType Directory $mode | Out-Null
        $usesAnalyzer = $mode -in @('analyzer-only', 'contracts', 'event-sourcing')
        $directIds = @(switch ($mode) {
            'analyzer-only' { 'ArchPrecept.Analyzers.Core' }
            'contracts' { 'ArchPrecept.Analyzers.Core'; 'ArchPrecept.Contracts.Application' }
            'contracts-only' { 'ArchPrecept.Contracts.Application' }
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
        $source = if ($mode -in @('contracts', 'contracts-only')) {
@'
using ArchPrecept.Contracts.Application;
using ArchPrecept.Contracts.Domain;
if (!Result<int>.Success(42).IsSuccess) throw new Exception("Result contract failed");
Console.WriteLine(typeof(IAggregateRoot<>).FullName);
public interface IDerivedDispatcher : IDomainEventDispatcher { }
public sealed class SaveDocumentUseCase(IDomainEventDispatcher dispatcher, IDerivedDispatcher derived)
{
    public Task ExecuteAsync(CancellationToken ct) => Task.CompletedTask;
}
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
        if ($mode -in @('contracts', 'contracts-only', 'event-sourcing')) { $expectedIds += 'ArchPrecept.Contracts.Domain' }
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
        if ($mode -eq 'contracts') {
            @'
using ArchPrecept.Contracts.Application;
public interface IMessageBus { }
public interface IConflictingDispatcher : IDomainEventDispatcher, IMessageBus { }
public sealed class MixedUseCase(IConflictingDispatcher dispatcher)
{
    public Task ExecuteAsync(CancellationToken ct) => Task.CompletedTask;
}
'@ | Set-Content "$mode/Violation.cs"
            $PSNativeCommandUseErrorActionPreference = $false
            $diagnostics = dotnet build "$mode/Consumer.csproj" -c Release --no-restore 2>&1 | Out-String
            $code = $LASTEXITCODE
            $PSNativeCommandUseErrorActionPreference = $true
            if ($code -eq 0 -or $diagnostics -notmatch 'error DBA1016' -or $diagnostics -match 'CS8032|CS9057|AD0001|error CS\d+') { throw "Mixed port bypass regression: $diagnostics" }
            $global:LASTEXITCODE = 0
        }
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
    # All rule fixtures share one compiler process per severity/profile, not one project per case.
    New-Item -ItemType Directory 'policy' | Out-Null
    @"
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net$SdkMajor.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><WarningsAsErrors>CS8032;CS9057;AD0001</WarningsAsErrors></PropertyGroup><ItemGroup><PackageReference Include="ArchPrecept.Analyzers.Core" Version="[$version]" PrivateAssets="all" /><AdditionalFiles Include="archprecept.project.xml" /></ItemGroup></Project>
"@ | Set-Content 'policy/Consumer.csproj'
    $policyConfiguration = @'
<ArchPrecept version="1" profiles="legacy,ddd">
  <Role name="value-object" metadataName="IValue" assembly="Consumer" />
  <Role name="entity" metadataName="IEntity" assembly="Consumer" />
  <Role name="domain-event-dispatcher" metadataName="ILocalEvents" assembly="Consumer" />
  <Expected type="Money" role="value-object" />
  <Expected type="MissingExpectedType" role="use-case" />
</ArchPrecept>
'@
    $policyConfiguration | Set-Content 'policy/archprecept.project.xml'
    $policySource = Get-Content (Join-Path $root 'tests/Consumers/PolicyMatrix.cs.txt') -Raw
    $policySource | Set-Content 'policy/Cases.cs'
    dotnet restore 'policy/Consumer.csproj'
    $policyAssets = Get-Content 'policy/obj/project.assets.json' -Raw | ConvertFrom-Json
    if ($policyAssets.libraries.PSObject.Properties.Name -match '^ArchPrecept.Contracts\.') { throw 'Analyzer-only policy consumer gained Contracts runtime dependency.' }
    $ruleIds = @(1001..1019 | ForEach-Object { "DBA$_" })
    foreach ($severity in @('error', 'warning', 'none')) {
        (@('root = true', '[*.cs]', 'dotnet_diagnostic.DBA0002.severity = warning') + @($ruleIds | ForEach-Object { "dotnet_diagnostic.$_.severity = $severity" })) | Set-Content 'policy/.editorconfig'
        $PSNativeCommandUseErrorActionPreference = $false
        $diagnostics = dotnet build 'policy/Consumer.csproj' -c Release --no-restore -t:Rebuild '-p:ErrorLog=policy.sarif' 2>&1 | Out-String
        $code = $LASTEXITCODE
        $PSNativeCommandUseErrorActionPreference = $true
        $diagnostics | Set-Content "policy/$severity.log"
        if ($diagnostics -match 'CS8032|CS9057|AD0001|error CS\d+') { throw "Invalid policy fixture or analyzer load failure: $diagnostics" }
        if (($severity -eq 'error') -ne ($code -ne 0)) { throw "Unexpected $severity exit code $code : $diagnostics" }
        foreach ($id in $ruleIds) {
            if ($severity -eq 'none') { if ($diagnostics -match "(?:error|warning) $id") { throw "Disabled rule still reported: $id" } }
            elseif ($diagnostics -notmatch "$severity $id") { throw "Missing $severity diagnostic $id : $diagnostics" }
        }
        $sarif = Get-Content 'policy/policy.sarif' -Raw
        if ($sarif -notmatch 'not-evaluated' -or ($severity -eq 'none' -and $sarif -notmatch 'disabled')) { throw 'Coverage states missing from compiler SARIF.' }
        $global:LASTEXITCODE = 0
    }
    # Core profile excludes framework, web API, ES and opt-in DDD rules.
    $policyConfiguration.Replace('legacy,ddd', 'core') | Set-Content 'policy/archprecept.project.xml'
    (@('root = true', '[*.cs]', 'dotnet_diagnostic.DBA0002.severity = warning') + @($ruleIds | ForEach-Object { "dotnet_diagnostic.$_.severity = warning" })) | Set-Content 'policy/.editorconfig'
    $diagnostics = dotnet build 'policy/Consumer.csproj' -c Release --no-restore -t:Rebuild 2>&1 | Out-String
    if ($diagnostics -notmatch 'warning DBA1002' -or $diagnostics -match 'warning DBA100[4679]|warning DBA101[1456789]|CS8032|CS9057|AD0001') { throw "Profile selection regression: $diagnostics" }
    # Generated declarations remain excluded, while explicit coverage reports the exclusion.
    $policyConfiguration | Set-Content 'policy/archprecept.project.xml'
    Move-Item -LiteralPath 'policy/Cases.cs' -Destination 'policy/Cases.g.cs'
    $diagnostics = dotnet build 'policy/Consumer.csproj' -c Release --no-restore -t:Rebuild '-p:ErrorLog=generated.sarif' 2>&1 | Out-String
    if ($diagnostics -match '(?:error|warning) DBA10\d\d|CS8032|CS9057|AD0001') { throw "Generated code exclusion regression: $diagnostics" }
    if ((Get-Content 'policy/generated.sarif' -Raw) -notmatch 'generated declaration is excluded') { throw 'Generated coverage missing.' }
    Move-Item -LiteralPath 'policy/Cases.g.cs' -Destination 'policy/Cases.cs'
    '<ArchPrecept version="1" profiles="typo" />' | Set-Content 'policy/archprecept.project.xml'
    $PSNativeCommandUseErrorActionPreference = $false
    $diagnostics = dotnet build 'policy/Consumer.csproj' -c Release --no-restore -t:Rebuild 2>&1 | Out-String
    $code = $LASTEXITCODE
    $PSNativeCommandUseErrorActionPreference = $true
    if ($code -eq 0 -or $diagnostics -notmatch 'error DBA0001' -or $diagnostics -match 'CS8032|CS9057|AD0001|error CS\d+') { throw "Invalid configuration did not fail distinctly: $diagnostics" }
    $global:LASTEXITCODE = 0

    Copy-Item -LiteralPath (Join-Path $root 'samples/Products') -Destination 'Products' -Recurse
    dotnet test 'Products/Tests/Products.Tests.csproj' -c Release "-p:ArchPreceptVersion=$version" "-p:SampleTargetFramework=net$SdkMajor.0" --logger trx --results-directory 'Products/results'
    Copy-Item -LiteralPath 'Products/violations/BadPrice.cs.txt' -Destination 'Products/Api/Violation.cs'
    $PSNativeCommandUseErrorActionPreference = $false
    $diagnostics = dotnet build 'Products/Api/Products.Api.csproj' -c Release --no-restore "-p:ArchPreceptVersion=$version" "-p:SampleTargetFramework=net$SdkMajor.0" 2>&1 | Out-String
    $code = $LASTEXITCODE
    $PSNativeCommandUseErrorActionPreference = $true
    if ($code -eq 0 -or $diagnostics -notmatch 'error DBA1018' -or $diagnostics -notmatch 'error DBA1019' -or $diagnostics -match 'CS8032|CS9057|AD0001|error CS\d+') { throw "Sample negative fixture failed: $diagnostics" }
    $global:LASTEXITCODE = 0
    Remove-Item -LiteralPath (Join-Path $work 'Products/Api/Violation.cs')
    dotnet build 'Products/Api/Products.Api.csproj' -c Release --no-restore "-p:ArchPreceptVersion=$version" "-p:SampleTargetFramework=net$SdkMajor.0"
    if ($expectedFeed) {
        foreach ($id in $packageIds) {
            $cacheFolder = Join-Path $env:NUGET_PACKAGES "$($id.ToLowerInvariant())/$version"
            $cached = Join-Path $cacheFolder "$($id.ToLowerInvariant()).$version.nupkg"
            $expected = Join-Path $expectedFeed "$id.$version.nupkg"
            if ((Get-FileHash $cached -Algorithm SHA256).Hash -ne (Get-FileHash $expected -Algorithm SHA256).Hash) { throw "Restored bytes differ from tested artifact: $id" }
            $metadata = Get-Content (Join-Path $cacheFolder '.nupkg.metadata') -Raw | ConvertFrom-Json
            if ($metadata.source.TrimEnd('/') -ne $feed.TrimEnd('/')) { throw "Unexpected restore source: $id" }
        }
        Write-Host 'All six restored nupkg hashes and cache sources match the tested artifact and selected feed.'
    }
    Write-Host "Package consumers passed on SDK $selected (net$SdkMajor.0): seven adoption modes, all 19 rules/severities, explicit mapping, profiles, coverage, generated exclusions, invalid settings, Products HTTP/use-case tests and repair; no AI dependency."
} finally {
    Pop-Location
    $env:NUGET_PACKAGES = $previousPackages
}
