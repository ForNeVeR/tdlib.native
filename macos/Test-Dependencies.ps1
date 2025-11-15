# SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [Parameter(Mandatory = $true)]
    [string] $DotNetArch,
    [Parameter(Mandatory = $true)]
    [string] $PackageName,
    [string] $RepoRoot = "$PSScriptRoot/..",
    [string] $Package = "$RepoRoot/build/$PackageName/runtimes/osx-$DotNetArch/native",
    [string] $GoldFile = "$RepoRoot/macos/libraries.$DotNetArch.gold.txt",
    [string] $ResultFile = "$RepoRoot/macos/libraries.temp.txt",
    [switch] $GenerateGold
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output "Saving library check results to $ResultFile…"

if (Test-Path $ResultFile) {
    Remove-Item $ResultFile
}

Get-ChildItem "$Package/*.dylib" | Sort-Object -Property Name | ForEach-Object {
    $libraryPath = $_.FullName

    Write-Output "Checking file `"$libraryPath`"…"
    $output = otool -L $libraryPath
    if (!$?) {
        throw "otool returned an exit code $LASTEXITCODE."
    }

    Write-Output "Output from otool -L $($libraryPath):"
    Write-Output $output

    $libraryNames = $output | Where-Object { $_.StartsWith("`t") } | ForEach-Object {
        $line = $_.Trim()
        $entryPath = if ($line -match '(.*?) \(.*?\)') {
            $Matches[1]
        } else {
            $line
        }

        if ($entryPath -match "@rpath/$([IO.Path]::GetFileNameWithoutExtension($libraryPath))\.([\d\.]+)\.dylib") {
            $null
        } else {
            $entryPath
        }
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
