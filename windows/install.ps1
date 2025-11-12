# SPDX-FileCopyrightText: 2018-2025 tdlib.native contributors <https://github.com/ForNeVeR/tdlib.native>
#
# SPDX-License-Identifier: BSL-1.0

# NOTE: Only used in tests right now.
param (
    $DependenciesLink = 'https://github.com/lucasg/Dependencies/releases/download/v1.11.1/Dependencies_x64_Release_.without.peview.exe.zip',
    $DependenciesHash = '7D22DC00F1C09FD4415D48AD74D1CF801893E83B9A39944B0FCE6DEA7CEAEA99',
    $DownloadStorage = "$PSScriptRoot/../build/downloads",
    $DependenciesInstallPath = "$PSScriptRoot/../build/tools/dependencies"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (!(Test-Path $DownloadStorage)) {
    New-Item -Type Directory $DownloadStorage | Out-Null
}

$dependenciesFileName = [Uri]::new($DependenciesLink).Segments | Select-Object -Last 1
$dependenciesDownloadPath = "$DownloadStorage/$dependenciesFileName"
if (Test-Path $dependenciesDownloadPath) {
    Write-Output "File `"$dependenciesDownloadPath`" already exists; checking its hash…"
    $actualHash = Get-FileHash -Algorithm SHA256 $dependenciesDownloadPath
    if ($actualHash.Hash -eq $DependenciesHash) {
        Write-Output 'Hash check succeeded; reusing the downloaded item.'
    } else {
        Write-Output "Actual hash: $($actualHash.Hash)."
        Write-Output "Expected hash: $DependenciesHash."
        Write-Output "Deleting file for re-download…"
        Remove-Item $dependenciesDownloadPath
    }
}

if (!(Test-Path $dependenciesDownloadPath)) {
    Write-Output "Downloading `"$DependenciesLink`"…"
    Invoke-WebRequest $DependenciesLink -OutFile $dependenciesDownloadPath
}

$actualHash = Get-FileHash -Algorithm SHA256 $dependenciesDownloadPath
if ($actualHash.Hash -ne $DependenciesHash) {
    Write-Output "Actual hash: $($actualHash.Hash)."
    Write-Output "Expected hash: $DependenciesHash."
    Write-Output "Hashes don't match."
    throw 'Cannot download Dependencies tool.'
}

if (Test-Path $DependenciesInstallPath) {
    Remove-Item -Recurse $DependenciesInstallPath
}

Expand-Archive $dependenciesDownloadPath $DependenciesInstallPath
Write-Output "Dependencies tool installed to directory `"$DependenciesInstallPath`"."
