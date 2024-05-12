param (
    [string] $RepoDirectory = "$PSScriptRoot/../td",
    [string] $ArtifactsDirectory = "$PSScriptRoot/../artifacts",
    [switch] $GenerateCheckResult
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$resultFile = "$ArtifactsDirectory/artifact.json"
$inputs = [pscustomobject] @{
    InputCommitHash = (git --git-dir=$RepoDirectory/.git rev-parse HEAD)
}

if ($GenerateCheckResult) {
    $inputs | ConvertTo-Json | Set-Content $resultFile
    Write-Host "Result cache file generated: `"$resultFile`"."
    return $true
} elseif (Test-Path $resultFile) {
    $result = Get-Content $resultFile | ConvertFrom-Json
    $upToDate = $result.InputCommitHash -eq $inputs.InputCommitHash
    Write-Host "Cache found: cached commit hash is $($result.InputCommitHash), current commit hash is $($inputs.InputCommitHash). Up to date: $upToDate."
    return $upToDate
} else {
    Write-Host "Last result cache file not found: `"$resultFile`"."
    return $false
}
