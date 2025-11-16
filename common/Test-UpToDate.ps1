# SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [string] $RepoRoot = "$PSScriptRoot/..",
    [string] $RepoDirectory = "$RepoRoot/td",
    [string] $ArtifactsDirectory = "$RepoRoot/artifacts",
    [switch] $GenerateCacheKey,
    [switch] $GenerateResultKey,
    [string] $CacheKeyFile = "$RepoRoot/.github/cache-key.json",
    [string] $ResultKeyFile = "$ArtifactsDirectory/cache-key.json",
    [string] $CacheVersion = 'v7'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$date = Get-Date -AsUTC
$inputs = [pscustomobject] @{
    InputCommitHash = (git --git-dir=$RepoDirectory/.git rev-parse HEAD)
    CacheVersion = $CacheVersion
    TimePeriod = "$($date.Year).$([Globalization.ISOWeek]::GetWeekOfYear($date))"
}

function compareCacheObject($a, $b) {
    $a.InputCommitHash -eq $b.InputCommitHash -and $a.CacheVersion -eq $b.CacheVersion -and $a.TimePeriod -eq $b.TimePeriod
}

if ($GenerateCacheKey) {
    if (!(Test-Path $ArtifactsDirectory)) {
        New-Item -Type Directory $ArtifactsDirectory | Out-Null
    }

    $inputs | ConvertTo-Json | Set-Content $CacheKeyFile
    Write-Host "Result cache file generated: `"$CacheKeyFile`"."
    return $true
} elseif ($GenerateResultKey) {
    if (!(Test-Path $ArtifactsDirectory)) {
        New-Item -Type Directory $ArtifactsDirectory | Out-Null
    }

    Copy-Item -LiteralPath $CacheKeyFile -Destination $ResultKeyFile
    Write-Host "Result cache file generated: `"$ResultKeyFile`"."
    return $true
} elseif (Test-Path $ResultKeyFile) {
    $result = Get-Content $ResultKeyFile | ConvertFrom-Json
    $upToDate = compareCacheObject $result $inputs
    Write-Host "Cache found: cache key is $result, current key is $inputs. Up to date: $upToDate."
    return $upToDate
} else {
    Write-Host "Last result cache file not found: `"$ResultKeyFile`"."
    return $false
}
