param (
    [string] $RepoDirectory = "$PSScriptRoot/../td",
    [string] $ArtifactsDirectory = "$PSScriptRoot/../artifacts",
    [switch] $GenerateCacheKey,
    [string] $CacheKeyFile = "$ArtifactsDirectory/cache-key.json"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$inputs = [pscustomobject] @{
    InputCommitHash = (git --git-dir=$RepoDirectory/.git rev-parse HEAD)
}

if ($GenerateCacheKey) {
    if (!(Test-Path $ArtifactsDirectory)) {
        New-Item -Type Directory $ArtifactsDirectory | Out-Null
    }

    $inputs | ConvertTo-Json | Set-Content $CacheKeyFile
    Write-Host "Result cache file generated: `"$CacheKeyFile`"."
    return $true
} elseif (Test-Path $CacheKeyFile) {
    $result = Get-Content $CacheKeyFile | ConvertFrom-Json
    $upToDate = $result.InputCommitHash -eq $inputs.InputCommitHash
    Write-Host "Cache found: cached commit hash is $($result.InputCommitHash), current commit hash is $($inputs.InputCommitHash). Up to date: $upToDate."
    return $upToDate
} else {
    Write-Host "Last result cache file not found: `"$CacheKeyFile`"."
    return $false
}
