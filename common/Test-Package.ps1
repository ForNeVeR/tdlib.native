# SPDX-FileCopyrightText: 2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [string] $RepositoryRoot = "$PSScriptRoot/..",
    [string] $Packages = "$RepositoryRoot/build/*.nupkg",
    [string] $Data = "$RepositoryRoot/common/packages/"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $Data -ErrorAction Ignore

[array] $packageFiles = Get-Item $Packages
[array] $dataFiles = @(Get-Item "$Data/*.txt")

Write-Output "$($packageFiles.Length) input files found."
Write-Output "$($dataFiles.Length) data files found."

$testSuccess = $true

foreach ($packageFile in $packageFiles) {
    if (!($packageFile.Name -Match '(\.\d+)+\.nupkg$')) {
        $testSuccess = $false
        Write-Warning "Unrecognized package file name: $($packageFile.Name)."
    } else {
        $expectedDataFileName = $packageFile.Name -Replace $Matches[0], '.txt'
    }


    $dataFile = $dataFiles | Where-Object { $_.Name -eq $expectedDataFileName }
    if (!$dataFile) {
        $testSuccess = $false
        Write-Warning "Cannot find the data file for package $($packageFile.Name): $expectedDataFileName."
    } else {
        $dataFiles = @($dataFiles | Where-Object { $_ -ne $dataFile })
    }

    [array] $entries = @()
    $zipFile = [IO.Compression.ZipFile]::Open($packageFile.FullName, [IO.Compression.ZipArchiveMode]::Read)
    try {
        foreach ($entry in $zipFile.Entries) {
            $fullName = $entry.FullName
            if ($fullName.EndsWith('.psmdcp') -or $fullName.EndsWith('.rels') -or $fullName -eq '[Content_Types].xml') { continue }
            $entries += $fullName
        }
    } finally {
        $zipFile.Dispose()
    }

    $entries = $entries | Sort-Object
    $description = $entries -join "`n"

    $actualContent = if ($dataFile) { Get-Content -Raw $dataFile } else { '' }
    $actualContent = $actualContent.Trim()
    if ($actualContent -ne $description) {
        $testSuccess = $false
        Write-Warning "Content doesn't match. Expected content of $Data/$($expectedDataFileName):`n$description"

        $tempFilePath = "$Data/$expectedDataFileName.tmp"
        Write-Output "Content will be created in $tempFilePath."
        Set-Content -LiteralPath $tempFilePath $description
    }
}

if ($dataFiles.Length -gt 0) {
    $testSuccess = $false
    Write-Warning 'Unused data files:'
    foreach ($dataFile in $dataFiles) {
        Write-Warning $dataFile.FullName
    }
}

if (!$testSuccess) {
    throw 'Test failed.'
}
