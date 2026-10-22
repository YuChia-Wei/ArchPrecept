param([string]$OutputDirectory = 'artifacts/packages', [string]$ScratchRoot)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Push-Location (Split-Path $PSScriptRoot)
try {
    $buildArguments = @()
    $resultsDirectory = 'artifacts/test-results'
    if ($ScratchRoot) {
        $buildArguments += @('--artifacts-path', (Join-Path $ScratchRoot 'build'))
        $resultsDirectory = Join-Path $ScratchRoot 'test-results'
    }
    dotnet restore ArchPrecept.slnx @buildArguments
    dotnet build ArchPrecept.slnx -c Release --no-restore @buildArguments
    dotnet test ArchPrecept.slnx -c Release --no-build --logger trx --results-directory $resultsDirectory @buildArguments
    dotnet pack ArchPrecept.slnx -c Release --no-build -o $OutputDirectory @buildArguments
    $version = ([xml](Get-Content Directory.Build.props -Raw)).Project.PropertyGroup.Version
    $ids = @('ArchPrecept.Analyzers.Core', 'ArchPrecept.Contracts.Domain', 'ArchPrecept.Contracts.Application', 'ArchPrecept.Contracts.Messaging', 'ArchPrecept.Contracts.Application.Messaging', 'ArchPrecept.Contracts.Domain.EventSourcing')
    $expectedDependencies = @{
        'ArchPrecept.Analyzers.Core' = @()
        'ArchPrecept.Contracts.Domain' = @()
        'ArchPrecept.Contracts.Application' = @('ArchPrecept.Contracts.Domain')
        'ArchPrecept.Contracts.Messaging' = @()
        'ArchPrecept.Contracts.Application.Messaging' = @('ArchPrecept.Contracts.Messaging')
        'ArchPrecept.Contracts.Domain.EventSourcing' = @('ArchPrecept.Contracts.Domain')
    }
    $packages = @(Get-ChildItem $OutputDirectory -Filter *.nupkg)
    if ($packages.Count -ne $ids.Count) { throw "Expected exactly $($ids.Count) packages; use a fresh output directory." }
    foreach ($id in $ids) {
        $file = Join-Path $OutputDirectory "$id.$version.nupkg"
        $zip = [IO.Compression.ZipFile]::OpenRead((Resolve-Path $file))
        try {
            $entries = @($zip.Entries.FullName)
            $expectedDll = if ($id -eq 'ArchPrecept.Analyzers.Core') { "analyzers/dotnet/cs/$id.dll" } else { "lib/net8.0/$id.dll" }
            if ($expectedDll -notin $entries -or 'README.md' -notin $entries) { throw "Invalid package layout: $id" }
            if ($id -eq 'ArchPrecept.Analyzers.Core' -and ($entries -match '^lib/|Microsoft.CodeAnalysis.*\.dll$')) { throw 'Analyzer must not ship runtime or Roslyn assemblies.' }
            $reader = [IO.StreamReader]::new($zip.GetEntry("$id.nuspec").Open())
            try { [xml]$spec = $reader.ReadToEnd() } finally { $reader.Dispose() }
            if ($spec.package.metadata.version -ne $version -or $spec.package.metadata.id -ne $id) { throw 'Package identity mismatch.' }
            $dependencies = @($spec.package.metadata.dependencies.group.dependency | Where-Object { $_ })
            $expected = $expectedDependencies[$id]
            if ($dependencies.Count -ne $expected.Count) { throw "Unexpected runtime dependency count: $id" }
            foreach ($dependency in $dependencies) {
                if ($dependency.id -notin $expected -or $dependency.version -ne $version) { throw "Unexpected runtime dependency for $id : $($dependency.id) $($dependency.version)" }
            }
            if ($spec.package.metadata.repository.url -ne 'https://github.com/YuChia-Wei/ArchPrecept') { throw 'Repository metadata mismatch.' }
            if ($spec.package.metadata.license.type -ne 'expression' -or $spec.package.metadata.license.InnerText -ne 'Apache-2.0') { throw 'Expected Apache-2.0 SPDX license.' }
            foreach ($notice in @('LICENSE', 'NOTICE', 'THIRD-PARTY-NOTICES.md')) {
                if ($notice -notin $entries) { throw "Missing legal file: $notice" }
            }
        } finally { $zip.Dispose() }
    }
    $packages | Sort-Object Name | ForEach-Object { "$( (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() )  $($_.Name)" } | Set-Content (Join-Path $OutputDirectory 'SHA256SUMS')
} finally { Pop-Location }
