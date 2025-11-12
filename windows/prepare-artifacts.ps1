# SPDX-FileCopyrightText: 2021-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build",
    [string] $BinLocation = "$BuildRoot/Release",
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
    Copy-Item "$BinLocation/*.dll" $TargetLocation
    & $CheckUpToDateScript -GenerateResultKey
} else {
    Write-Host 'The build result is up to date.'
}
