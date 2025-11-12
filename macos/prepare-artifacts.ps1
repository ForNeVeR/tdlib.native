# SPDX-FileCopyrightText: 2018-2025 tdlib.native contributors <https://github.com/ForNeVeR/tdlib.native>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build",
    [string] $TargetLocation = "./artifacts/",
    [string] $CheckUpToDateScript = "$PSScriptRoot/../common/Test-UpToDate.ps1",
    [switch] $SkipUpToDateCheck
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($SkipUpToDateCheck -or !$(& $CheckUpToDateScript)) {
    if (!(Test-Path -LiteralPath $TargetLocation -Type Container)) {
        New-Item -Type Directory $TargetLocation
    }
    Copy-Item "$BuildRoot/libtdjson.dylib" $TargetLocation
    & $CheckUpToDateScript -GenerateResultKey
} else {
    Write-Host 'The build result is up to date.'
}
