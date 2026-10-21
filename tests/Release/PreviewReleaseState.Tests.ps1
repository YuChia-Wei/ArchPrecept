$ErrorActionPreference = 'Stop'
$scriptUnderTest = Join-Path $PSScriptRoot '../../eng/Get-PreviewReleaseState.ps1'
$oldToken = $env:GH_TOKEN
$env:GH_TOKEN = 'synthetic-test-token'
$version = ([xml](Get-Content (Join-Path $PSScriptRoot '../../Directory.Build.props') -Raw)).Project.PropertyGroup.Version
$mode = ''
function Invoke-RestMethod {
    param($Uri, $Headers)
    if ($mode -in @('absent', 'forbidden')) {
        $status = if ($mode -eq 'absent') { 404 } else { 403 }
        $response = [System.Net.Http.HttpResponseMessage]::new([System.Net.HttpStatusCode]$status)
        throw [Microsoft.PowerShell.Commands.HttpResponseException]::new('Synthetic API response', $response)
    }
    if ($Uri -match '/versions\?') {
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
        if ($state.shouldPublish -ne $expectedPublish -or $state.packages.Count -ne 6) { throw "Incorrect publication decision: $mode" }
    }
    foreach ($mode in @('partial', 'private', 'wrong-repo', 'forbidden')) {
        $failed = $false
        try { & $scriptUnderTest -Repository 'YuChia-Wei/ArchPrecept' | Out-Null } catch { $failed = $true }
        if (!$failed) { throw "Expected publication rejection: $mode" }
    }
    $mode = 'absent'
    $failed = $false
    try { & $scriptUnderTest -Repository 'YuChia-Wei/ArchPrecept' -RequirePublished | Out-Null } catch { $failed = $true }
    if (!$failed) { throw 'Post-publication verification accepted missing packages.' }
    $mode = 'published'
    & $scriptUnderTest -Repository 'YuChia-Wei/ArchPrecept' -RequirePublished | Out-Null
    Write-Host 'Release state checks passed: absent/new/existing versions, pagination, partial publication, private visibility, linkage, forbidden access and required publication.'
} finally { $env:GH_TOKEN = $oldToken }
