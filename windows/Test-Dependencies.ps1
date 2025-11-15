# SPDX-FileCopyrightText: 2022-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [Parameter(Mandatory = $true)]
    [string] $DotNetArch,

    [string] $Dependencies = "$PSScriptRoot/../build/tools/dependencies/Dependencies.exe",

    [string] $Package = "$PSScriptRoot/../build/tdlib.native.win-x64/runtimes/win-x64/native",
    [string] $GoldFile = "$PSScriptRoot/../windows/libraries.$DotNetArch.gold.txt",
    [string] $ResultFile = "$PSScriptRoot/../windows/libraries.$DotNetArch.temp.txt",
    [switch] $GenerateGold
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output "Saving library check results to $ResultFile…"

if (Test-Path $ResultFile) {
    Remove-Item $ResultFile
}

Get-ChildItem "$Package/*.dll" | Sort-Object -Property Name | ForEach-Object {
    $libraryPath = $_.FullName

    Write-Output "Checking file `"$libraryPath`"…"
    $output = & $Dependencies -json -imports $libraryPath | ConvertFrom-Json
    if (!$?) {
        throw "Dependencies.exe returned an exit code $LASTEXITCODE."
    }

    $libraryNames = $output.Imports | Select-Object -ExpandProperty Name | Sort-Object
    $_.Name >> $ResultFile
    $libraryNames | ForEach-Object { "  $_" >> $ResultFile }
}

if ($GenerateGold) {
    Move-Item -Force $ResultFile $GoldFile
} else {
    $goldContent = Get-Content -Raw $GoldFile
    $tempContent = Get-Content -Raw $ResultFile
    if ($goldContent -ne $tempContent) {
        Write-Output "Current contents are following:`n"
        Write-Output $tempContent
        throw "File contents are not equal: $GoldFile and $ResultFile"
    }
}
