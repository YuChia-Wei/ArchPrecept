param(
    [Parameter(Mandatory)][string]$Repository,
    [switch]$RequirePublished
)
$ErrorActionPreference = 'Stop'
if (!$env:GH_TOKEN) { throw 'GH_TOKEN is required to inspect package state.' }
$version = ([xml](Get-Content (Join-Path (Split-Path $PSScriptRoot) 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
$owner = $Repository.Split('/')[0]
$headers = @{ Authorization = "Bearer $env:GH_TOKEN"; Accept = 'application/vnd.github+json'; 'X-GitHub-Api-Version' = '2022-11-28' }
$ids = @('ArchPrecept.Analyzers.Core', 'ArchPrecept.Contracts.Domain', 'ArchPrecept.Contracts.Application', 'ArchPrecept.Contracts.Messaging', 'ArchPrecept.Contracts.Application.Messaging', 'ArchPrecept.Contracts.Domain.EventSourcing')
$records = @(foreach ($id in $ids) {
    $uri = "https://api.github.com/users/$owner/packages/nuget/$id"
    try { $package = Invoke-RestMethod -Uri $uri -Headers $headers }
    catch {
        if ([int]$_.Exception.Response.StatusCode -ne 404) { throw }
        [pscustomobject]@{ id = $id; version = $version; published = $false; visibility = $null }
        continue
    }
    if ($package.repository.full_name -ne $Repository) { throw "Repository linkage mismatch: $id" }
    if ($package.visibility -ne 'public') {
        throw "Package $id must be public. Complete its GitHub Package settings, then rerun verification; do not republish existing bytes."
    }
    $found = $false
    $page = 1
    do {
        $response = Invoke-RestMethod -Uri "$uri/versions?per_page=100&page=$page" -Headers $headers
        $versions = @($response)
        if ($version -in $versions.name) { $found = $true }
        $page++
    } while (!$found -and $versions.Count -eq 100)
    [pscustomobject]@{ id = $id; version = $version; published = $found; visibility = $package.visibility }
})
$publishedCount = @($records | Where-Object published).Count
if ($RequirePublished -and $publishedCount -ne $ids.Count) { throw "Expected all six public packages at $version; found $publishedCount." }
if ($publishedCount -ne 0 -and $publishedCount -ne $ids.Count) {
    throw "Partial release at $version ($publishedCount/$($ids.Count)). Recover missing packages from the original run artifacts; do not rebuild or overwrite existing versions."
}
[pscustomobject]@{ version = $version; shouldPublish = ($publishedCount -eq 0); packages = $records }
