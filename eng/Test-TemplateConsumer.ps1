param(
    [Parameter(Mandatory)][string]$PackageFile,
    [Parameter(Mandatory)][ValidateSet(8, 10)][int]$SdkMajor,
    [Parameter(Mandatory)][string]$ScratchRoot,
    # Omit when the caller has already created an isolated consumer NuGet.Config and cache.
    [string]$PackageDirectory,
    [string]$BoundaryTestDirectory
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$package = (Resolve-Path -LiteralPath $PackageFile).Path
$feed = if ($PackageDirectory) { (Resolve-Path -LiteralPath $PackageDirectory).Path }
$version = ([xml](Get-Content (Join-Path (Split-Path $PSScriptRoot) 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
$work = New-Item -ItemType Directory -Path (Join-Path $ScratchRoot "template-$SdkMajor-$([guid]::NewGuid().ToString('N'))")
$hive = @('--debug:custom-hive', (Join-Path $work.FullName 'template-hive'))
$previousPackages = $env:NUGET_PACKAGES

# Compile the installed items in the generated consumer. Keep custom contracts
# local to that consumer so an accidental dependency on a reference package fails.
function Test-ItemTemplates([string]$mode, [string]$project) {
    $domain = @('--namespace', 'Shop.Domain', '-o', "$mode/Domain")
    $app = @('--namespace', 'Shop.Application', '-o', "$mode/Application")
    $aggregateBase = $eventContract = $writeBase = $queryBase = @()
    $customRoles = ''
    if ($mode -eq 'custom') {
        @'
namespace Shop.Domain;
public abstract class IdentityRoot<T>(T id) where T : notnull { public T Id { get; } = id; }
public interface IFact { Guid EventId { get; } DateTime OccurredOn { get; } }
public interface IWrite<T, in TId> where T : IdentityRoot<TId> where TId : notnull
{
    Task<T?> FindByIdAsync(TId id, CancellationToken cancellationToken);
    Task SaveAsync(T aggregate, CancellationToken cancellationToken);
}
public interface IRead { }
'@ | Set-Content "$mode/Domain/ExistingContracts.cs"
        $aggregateBase = @('--base', 'Shop.Domain.IdentityRoot')
        $eventContract = @('--contract', 'Shop.Domain.IFact')
        $writeBase = @('--base', 'Shop.Domain.IWrite')
        $queryBase = @('--base', 'Shop.Domain.IRead')
        $customRoles = @'
  <Role name="aggregate" metadataName="Shop.Domain.IdentityRoot`1" assembly="Shop" />
  <Role name="aggregate-repository" metadataName="Shop.Domain.IWrite`2" assembly="Shop" />
  <Role name="query-repository" metadataName="Shop.Domain.IRead" assembly="Shop" />
'@
    } else {
        dotnet add $project package ArchPrecept.Contracts.Application --version $version
    }
    $configPath = "$mode/archprecept.project.xml"
    $roles = @"
$customRoles
  <Role name="mapper" metadataName="Shop.Application.OrderMapper" assembly="Shop" />
  <Role name="dispatch-handler" metadataName="Shop.Dispatch.PlaceOrderHandler" assembly="Shop" />
  <Expected type="Shop.Application.PlaceOrderUseCase" role="use-case" />
  <Expected type="Shop.Application.IOrders" role="aggregate-repository" />
  <Expected type="Shop.Application.IFindOrders" role="query-repository" />
</ArchPrecept>
"@
    (Get-Content $configPath -Raw).Replace('</ArchPrecept>', $roles) | Set-Content $configPath
    dotnet new ap-aggregate -n Order @domain @aggregateBase @hive
    dotnet new ap-domain-event -n OrderPlaced @domain @eventContract @hive
    dotnet new ap-repository -n IOrders @app --aggregate Shop.Domain.Order @writeBase @hive
    dotnet new ap-query-port -n IFindOrders @app --model Shop.Application.CreateProductOutput @queryBase @hive
    dotnet new ap-usecase -n PlaceOrder @app --base IUseCase --port IOrders @hive
    dotnet new ap-mapper -n OrderMapper @app --source-type PlaceOrderInput --target-type PlaceOrderOutput @hive
    dotnet new ap-handler -n PlaceOrder --namespace Shop.Dispatch --application Shop.Application -o "$mode/Dispatch" @hive
    dotnet new ap-execute --input-type Shop.Application.PlaceOrderInput --output-type Shop.Application.PlaceOrderOutput -o "$mode/snippets" @hive
    $declaration = Get-Content "$mode/snippets/ExecuteAsync.txt" -Raw
    "namespace Shop.Application; public interface IReviewOrder { $declaration }" | Set-Content "$mode/Application/IReviewOrder.cs"
    dotnet build $project -c Release -warnaserror
}

Push-Location $work.FullName
try {
    @{ sdk = @{ version = "$SdkMajor.0.100"; rollForward = 'latestFeature'; allowPrerelease = $false } } |
        ConvertTo-Json | Set-Content global.json
    if ($feed) {
        $env:NUGET_PACKAGES = Join-Path $work.FullName 'cache'
        $escapedFeed = [Security.SecurityElement]::Escape($feed)
        "<configuration><packageSources><clear/><add key=""candidate"" value=""$escapedFeed""/></packageSources></configuration>" |
            Set-Content NuGet.Config
    }
    $sdk = dotnet --version
    if ($sdk -notlike "$SdkMajor.*") { throw "Unexpected SDK $sdk." }
    dotnet new install $package @hive
    dotnet new ap-starter --help @hive | Out-Host
    dotnet new ap-vo --help @hive | Out-Host

    foreach ($mode in 'reference', 'custom') {
        $starter = @('ap-starter', '-n', 'Shop', '-o', $mode, '--contracts', $mode, '--framework', "net$SdkMajor.0")
        dotnet new @starter --dry-run @hive
        if (Test-Path "$mode/Shop.csproj") { throw 'Starter preview wrote a project.' }
        dotnet new @starter @hive
        $project = "$mode/Shop.csproj"
        dotnet build $project -c Release
        $success = dotnet run --no-build --project $project -c Release -- Paper
        if ($success -notcontains 'Created: Paper') { throw "Unexpected starter result: $success" }
        $PSNativeCommandUseErrorActionPreference = $false
        $rejected = dotnet run --no-build --project $project -c Release -- ' ' 2>&1 | Out-String
        $exit = $LASTEXITCODE
        $PSNativeCommandUseErrorActionPreference = $true
        if ($exit -ne 1 -or $rejected -notmatch 'Product name is required') { throw "Invalid input result: $exit $rejected" }

        $assets = Get-Content "$mode/obj/project.assets.json" -Raw | ConvertFrom-Json
        $libraries = @($assets.libraries.PSObject.Properties.Name)
        if ("ArchPrecept.Analyzers.Core/$version" -notin $libraries) { throw "Incorrect analyzer dependency for $mode." }
        if ($mode -eq 'reference' -and "ArchPrecept.Contracts.Domain/$version" -notin $libraries) { throw 'Missing versioned Domain contracts.' }
        if ($mode -eq 'custom' -and $libraries -match '^ArchPrecept\.Contracts\.') { throw 'Custom consumer acquired a reference contract.' }

        $item = @('ap-vo', '-n', 'Price', '-o', "$mode/Domain", '--namespace', 'Shop.Domain', '--contracts', $mode)
        $price = Join-Path $work.FullName "$mode/Domain/Price.cs"
        dotnet new @item --dry-run @hive
        if (Test-Path $price) { throw 'Item preview wrote Price.' }
        dotnet new @item @hive
        dotnet build $project -c Release --no-restore -warnaserror

        # Mutate the generated Price itself, so this cannot pass on a different fixture.
        $original = [IO.File]::ReadAllText($price)
        $violation = $original -replace '(\bdecimal\s+Value\s*\{\s*get\s*;)(\s*\})', '$1 set;$2'
        if ($violation -ceq $original) { throw 'Could not add the public setter to the generated Price.Value.' }
        [IO.File]::WriteAllText($price, $violation)
        $PSNativeCommandUseErrorActionPreference = $false
        $conflict = dotnet new @item @hive 2>&1 | Out-String
        $exit = $LASTEXITCODE
        $PSNativeCommandUseErrorActionPreference = $true
        if ($exit -eq 0 -or [IO.File]::ReadAllText($price) -cne $violation) { throw "Conflict did not preserve the edited Price: $conflict" }

        $PSNativeCommandUseErrorActionPreference = $false
        $failure = dotnet build $project -c Release --no-restore '-p:ErrorLog=price-setter.sarif' 2>&1 | Out-String
        $exit = $LASTEXITCODE
        $PSNativeCommandUseErrorActionPreference = $true
        $failure | Set-Content "$mode/price-setter.log"
        $results = (Get-Content "$mode/price-setter.sarif" -Raw | ConvertFrom-Json).runs[0].results
        $errors = @($results | Where-Object level -eq 'error')
        if ($exit -eq 0 -or 'DBA1018' -notin $errors.ruleId -or ($errors | Where-Object ruleId -ne 'DBA1018') -or
            -not ($errors.locations.resultFile.uri -match 'Price\.cs$') -or ($results.ruleId -match '^(CS8032|CS9057|AD0001)$')) {
            throw "Expected a DBA1018 error on Price: $failure"
        }
        [IO.File]::WriteAllText($price, $original)
        dotnet build $project -c Release --no-restore -warnaserror
        Write-Host "A1 passed on $sdk ($mode): sample accepted/rejected input; generated Price built, failed DBA1018 with setter, then built after repair."
        Test-ItemTemplates $mode $project
    }
    if ($BoundaryTestDirectory) {
        dotnet new ap-boundary-test -n DomainCannotReferenceProductsApi --sourceType ArchPrecept.Contracts.Domain.IDomainEvent --forbiddenType Products.Sample.Product -o $BoundaryTestDirectory @hive
        $violations = Join-Path (Split-Path $BoundaryTestDirectory) 'violations'
        dotnet new ap-boundary-test -n ProductsCannotReferenceDomain --sourceType Products.Sample.Product --forbiddenType ArchPrecept.Contracts.Domain.IDomainEvent -o $violations @hive
    }
    dotnet new uninstall ArchPrecept.Templates @hive
} finally {
    Pop-Location
    $env:NUGET_PACKAGES = $previousPackages
}
