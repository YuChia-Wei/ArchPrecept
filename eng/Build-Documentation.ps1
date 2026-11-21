param(
    [Parameter(Mandatory)][string] $ScratchRoot,
    [Parameter(Mandatory)][string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
$repo = Split-Path $PSScriptRoot
$docs = Join-Path $repo 'docs'
$work = Join-Path ([IO.Path]::GetFullPath($ScratchRoot)) ([guid]::NewGuid().ToString('N'))
$output = [IO.Path]::GetFullPath($OutputDirectory)
$docset = Join-Path $work 'docset'
$assemblies = Join-Path $docset 'assemblies'
$build = Join-Path $work 'build'

Push-Location $repo
try {
    $commit = git rev-parse --verify HEAD
    dotnet tool restore --tool-manifest .config/dotnet-tools.json
    New-Item -ItemType Directory -Path $assemblies -Force | Out-Null
    Copy-Item -Path "$PSScriptRoot/docfx/*" -Destination $docset -Recurse

    # The nupkg and GitHub views keep their own relative links. On Pages,
    # targets outside docs/ point back to the exact source used for this build.
    foreach ($file in Get-ChildItem -LiteralPath $docs -File -Recurse) {
        $relative = [IO.Path]::GetRelativePath($docs, $file.FullName)
        if ($relative -ceq 'README.md') { $relative = 'index.md' }
        $target = Join-Path $docset $relative
        New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
        if ($file.Extension -ne '.md') {
            Copy-Item -LiteralPath $file.FullName -Destination $target
            continue
        }
        $markdown = [IO.File]::ReadAllText($file.FullName)
        $markdown = [regex]::Replace($markdown, '(?<=\]\()(?<path>(?:\.\./)+[^\s#)]+)', {
            param($match)
            $path = [IO.Path]::GetFullPath($match.Value, $file.DirectoryName)
            $docsPath = [IO.Path]::GetRelativePath($docs, $path).Replace('\', '/')
            if (!$docsPath.StartsWith('../')) {
                if ($docsPath -ceq 'README.md') { return $match.Value -creplace 'README\.md$', 'index.md' }
                return $match.Value
            }
            $repoPath = [IO.Path]::GetRelativePath($repo, $path).Replace('\', '/')
            return "https://github.com/YuChia-Wei/ArchPrecept/blob/$commit/$repoPath"
        })
        [IO.File]::WriteAllText($target, $markdown)
    }

    # Generate API pages from the five shipping contracts and their XML comments.
    $projects = Get-ChildItem -Path "$repo/src/Contracts/*/*.csproj" -File
    foreach ($project in $projects) {
        dotnet build $project.FullName --configuration Release --artifacts-path $build --nologo
        $binary = Join-Path $build "bin/$($project.BaseName)/release/$($project.BaseName)"
        Copy-Item -LiteralPath "$binary.dll", "$binary.xml" -Destination $assemblies
    }
    dotnet tool run docfx -- "$docset/docfx.json" --output $output
} finally {
    Pop-Location
}
