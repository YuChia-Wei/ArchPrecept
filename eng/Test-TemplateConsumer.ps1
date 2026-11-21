param(
    [Parameter(Mandatory)][string]$PackageFile,
    [Parameter(Mandatory)][ValidateSet(8, 10)][int]$SdkMajor,
    [Parameter(Mandatory)][string]$ScratchRoot,
    # Omit when the caller has already created an isolated consumer NuGet.Config and cache.
    [string]$PackageDirectory
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$package = (Resolve-Path -LiteralPath $PackageFile).Path
$feed = if ($PackageDirectory) { (Resolve-Path -LiteralPath $PackageDirectory).Path }
$version = ([xml](Get-Content (Join-Path (Split-Path $PSScriptRoot) 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
$work = New-Item -ItemType Directory -Path (Join-Path $ScratchRoot "template-$SdkMajor-$([guid]::NewGuid().ToString('N'))")
$hive = @('--debug:custom-hive', (Join-Path $work.FullName 'template-hive'))
$previousPackages = $env:NUGET_PACKAGES

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
    }
    dotnet new uninstall ArchPrecept.Templates @hive
} finally {
    Pop-Location
    $env:NUGET_PACKAGES = $previousPackages
}
