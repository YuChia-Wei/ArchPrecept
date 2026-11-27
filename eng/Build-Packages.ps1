param([string]$OutputDirectory = 'artifacts/packages', [string]$ScratchRoot, [switch]$SkipPack)

& (Join-Path $PSScriptRoot '../.github/workflows/scripts/Build-Packages.ps1') @PSBoundParameters
