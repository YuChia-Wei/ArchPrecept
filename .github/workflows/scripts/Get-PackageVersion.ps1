param([switch]$RequireTag)

if ($env:GITHUB_REF_TYPE -ceq 'tag') {
    $tag = $env:GITHUB_REF_NAME
    if ($tag -clike 'v*') {
        $version = $tag.Substring(1) -as [semver]
        if ($version -and !$version.BuildLabel -and "v$version" -ceq $tag) {
            return $version.ToString()
        }
    }
    throw "Expected a tag such as v1.2.3 or v1.2.3-preview.1 (without build metadata). Got '$tag'."
}

if ($RequireTag) { throw 'Select a release tag when running this workflow.' }
([xml](Get-Content (Join-Path $PSScriptRoot '../../../Directory.Build.props') -Raw)).Project.PropertyGroup.Version
