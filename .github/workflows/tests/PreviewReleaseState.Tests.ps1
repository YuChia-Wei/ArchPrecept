$ErrorActionPreference = 'Stop'
# The HTTP cmdlet is mocked below, so explicitly load its exception type in fresh pwsh sessions.
Import-Module Microsoft.PowerShell.Utility
$scriptUnderTest = Join-Path $PSScriptRoot '../scripts/Get-PreviewReleaseState.ps1'
$oldToken = $env:GH_TOKEN
$env:GH_TOKEN = 'synthetic-test-token'
$version = & (Join-Path $PSScriptRoot '../scripts/Get-PackageVersion.ps1')
$mode = ''
$retry = @{ Waits = 0 }
function Start-Sleep {
    param($Seconds)
    $retry.Waits++
}
function Invoke-RestMethod {
    param($Uri, $Headers)
    if ($mode -in @('absent', 'forbidden') -or ($mode -eq 'temporary-outage' -and !$retry.Waits)) {
        $status = switch ($mode) { 'absent' { 404 } 'forbidden' { 403 } 'temporary-outage' { 503 } }
        $response = [System.Net.Http.HttpResponseMessage]::new([System.Net.HttpStatusCode]$status)
        throw [Microsoft.PowerShell.Commands.HttpResponseException]::new('Synthetic API response', $response)
    }
    if ($Uri -match '/versions\?') {
        if ($mode -eq 'indexing' -and !$retry.Waits -and $Uri -match '/ArchPrecept.Templates/') { return @() }
        if ($mode -eq 'new-version') { return @() }
        if ($mode -eq 'partial' -and $Uri -notmatch '/ArchPrecept.Analyzers.Core/') { return @() }
        if ($mode -eq 'pagination' -and $Uri -match 'page=1$') {
            return @(1..100 | ForEach-Object { @{ name = "older-$_" } })
        }
        return @(@{ name = $version })
    }
    return @{ visibility = $(if ($mode -eq 'private') { 'private' } else { 'public' }); repository = @{ full_name = $(if ($mode -eq 'wrong-repo') { 'someone/else' } else { 'YuChia-Wei/ArchPrecept' }) } }
}
try {
    foreach ($mode in @('absent', 'new-version', 'published', 'pagination')) {
        $state = & $scriptUnderTest -Repository 'YuChia-Wei/ArchPrecept'
        $expectedPublish = $mode -in @('absent', 'new-version')
        if ($state.shouldPublish -ne $expectedPublish -or 'ArchPrecept.Templates' -notin $state.packages.id) { throw "Incorrect publication decision: $mode" }
    }
    foreach ($mode in @('partial', 'private', 'wrong-repo', 'forbidden')) {
        $failed = $false
        try { & $scriptUnderTest -Repository 'YuChia-Wei/ArchPrecept' | Out-Null } catch { $failed = $true }
        if (!$failed) { throw "Expected publication rejection: $mode" }
    }
    foreach ($mode in @('published', 'indexing', 'temporary-outage')) {
        $retry.Waits = 0
        $state = & $scriptUnderTest -Repository 'YuChia-Wei/ArchPrecept' -RequirePublished
        $expectedWaits = if ($mode -eq 'published') { 0 } else { 1 }
        if ($state.shouldPublish -or $retry.Waits -ne $expectedWaits) { throw "Incorrect verification retry: $mode" }
    }
    foreach ($mode in @('absent', 'private', 'wrong-repo', 'forbidden')) {
        $retry.Waits = 0
        $failed = $false
        try { & $scriptUnderTest -Repository 'YuChia-Wei/ArchPrecept' -RequirePublished | Out-Null } catch { $failed = $true }
        $expectedWaits = if ($mode -eq 'absent') { 5 } else { 0 }
        if (!$failed -or $retry.Waits -ne $expectedWaits) { throw "Incorrect verification failure: $mode" }
    }
    Write-Host 'Release state and post-publication retry checks passed.'
} finally { $env:GH_TOKEN = $oldToken }
