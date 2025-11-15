# SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [Parameter(Mandatory = $true)]
    [string] $Platform,
    [Parameter(Mandatory = $true)]
    [string] $DotNetArch,
    [Parameter(Mandatory = $true)]
    [string] $PackageName,
    [string] $RepoRoot = "$PSScriptRoot/..",
    [string] $Package = "$RepoRoot/build/$PackageName/runtimes/linux-$DotNetArch/native",
    [string] $GoldFile = "$RepoRoot/linux/libraries.$Platform.$DotNetArch.gold.txt",
    [string] $ResultFile = "$RepoRoot/linux/libraries.temp.txt",
    [switch] $GenerateGold
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output "Saving library check results to $ResultFile…"

if (Test-Path $ResultFile) {
    Remove-Item $ResultFile
}

Get-ChildItem "$Package/*.so" | Sort-Object -Property Name | ForEach-Object {
    $libraryPath = $_.FullName

    Write-Output "Checking file `"$libraryPath`"…"
    $output = ldd $libraryPath
    if (!$?) {
        throw "ldd returned an exit code $LASTEXITCODE."
    }

    Write-Output "Output from ldd $($libraryPath):"
    Write-Output $output

    $libraryNames = $output | ForEach-Object {
        $filePath = $_.Trim().Split(' ')[0]
        $filePath
    } | Sort-Object
    $_.Name >> $ResultFile
    $libraryNames | ForEach-Object { "  $_" >> $ResultFile }
}

if ($GenerateGold) {
    Move-Item -Force $ResultFile $GoldFile
} else {
    $goldContent = Get-Content -Raw $GoldFile
    $tempContent = Get-Content -Raw $ResultFile
    if ($goldContent -ne $tempContent) {
        Write-Output "Expected contents are following:`n"
        Write-Output $tempContent
        throw "File contents are not equal: $GoldFile and $ResultFile"
    }
}
