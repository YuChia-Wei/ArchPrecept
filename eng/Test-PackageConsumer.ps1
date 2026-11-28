param(
    [ValidateSet(8, 10)][int]$SdkMajor = 8,
    [string]$PackageDirectory = 'artifacts/packages',
    [string]$RegistrySource,
    [string]$ExpectedPackageDirectory,
    [string]$ScratchRoot,
    [ValidatePattern('^[0-9a-f]{40}$')][string]$ExpectedSourceCommit
)

& (Join-Path $PSScriptRoot '../.github/workflows/scripts/Test-PackageConsumer.ps1') @PSBoundParameters
