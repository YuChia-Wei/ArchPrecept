param([string]$OutputDirectory = 'artifacts/packages')
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Push-Location (Split-Path $PSScriptRoot)
try {
    dotnet restore ArchPrecept.slnx
    dotnet build ArchPrecept.slnx -c Release --no-restore
    dotnet test ArchPrecept.slnx -c Release --no-build --logger trx --results-directory artifacts/test-results
    dotnet pack ArchPrecept.slnx -c Release --no-build -o $OutputDirectory
    $version = ([xml](Get-Content Directory.Build.props -Raw)).Project.PropertyGroup.Version
    $ids = @('ArchPrecept.Analyzers.Core', 'ArchPrecept.Contracts.Domain', 'ArchPrecept.Contracts.Application', 'ArchPrecept.Contracts.Messaging.Abstractions')
    $packages = @(Get-ChildItem $OutputDirectory -Filter *.nupkg)
    if ($packages.Count -ne $ids.Count) { throw 'Expected exactly four packages; use a fresh output directory.' }
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
            if ($spec.package.metadata.repository.url -ne 'https://github.com/YuChia-Wei/ArchPrecept') { throw 'Repository metadata mismatch.' }
            if ($spec.package.metadata.license.type -ne 'expression' -or $spec.package.metadata.license.InnerText -ne 'Apache-2.0') { throw 'Expected Apache-2.0 SPDX license.' }
            foreach ($notice in @('LICENSE', 'NOTICE', 'THIRD-PARTY-NOTICES.md')) {
                if ($notice -notin $entries) { throw "Missing legal file: $notice" }
            }
        } finally { $zip.Dispose() }
    }
    $packages | Sort-Object Name | ForEach-Object { "$( (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() )  $($_.Name)" } | Set-Content (Join-Path $OutputDirectory 'SHA256SUMS')
} finally { Pop-Location }
