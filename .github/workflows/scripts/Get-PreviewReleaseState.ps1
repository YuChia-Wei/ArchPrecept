param(
    [Parameter(Mandatory)][string]$Repository,
    [switch]$RequirePublished
)
$ErrorActionPreference = 'Stop'
if (!$env:GH_TOKEN) { throw 'GH_TOKEN is required to inspect package state.' }
$version = & (Join-Path $PSScriptRoot 'Get-PackageVersion.ps1')
$owner = $Repository.Split('/')[0]
$headers = @{ Authorization = "Bearer $env:GH_TOKEN"; Accept = 'application/vnd.github+json'; 'X-GitHub-Api-Version' = '2022-11-28' }
$ids = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ReleasePackageIds.txt')
function Get-PackageRecords {
    foreach ($id in $ids) {
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
    }
}

# Only the post-publish read can observe a version that is still being indexed.
$maxAttempts = if ($RequirePublished) { 6 } else { 1 }
for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
    try {
        $records = @(Get-PackageRecords)
        $publishedCount = @($records | Where-Object published).Count
        if (!$RequirePublished -or $publishedCount -eq $ids.Count) { break }
        $reason = "Expected $($ids.Count) public packages at $version; found $publishedCount."
        if ($attempt -eq $maxAttempts) { throw $reason }
    } catch [Microsoft.PowerShell.Commands.HttpResponseException] {
        $status = [int]$_.Exception.Response.StatusCode
        if ($attempt -eq $maxAttempts -or $status -notin @(404, 408, 429, 500, 502, 503, 504)) { throw }
        $reason = "GitHub Packages returned HTTP $status."
    }
    Write-Warning "$reason Retrying verification in 10 seconds (attempt $attempt of $maxAttempts)."
    Start-Sleep -Seconds 10
}
if ($publishedCount -ne 0 -and $publishedCount -ne $ids.Count) {
    throw "Partial release at $version ($publishedCount/$($ids.Count)). Recover missing packages from the original run artifacts; do not rebuild or overwrite existing versions."
}
[pscustomobject]@{ version = $version; shouldPublish = ($publishedCount -eq 0); packages = $records }
