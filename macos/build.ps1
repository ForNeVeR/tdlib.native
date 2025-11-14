# SPDX-FileCopyrightText: 2018-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [string] $td = "$PSScriptRoot/../td",
    [string] $CheckUpToDateScript = "$PSScriptRoot/../common/Test-UpToDate.ps1",
    [switch] $SkipUpToDateCheck
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($SkipUpToDateCheck -or !$(& $CheckUpToDateScript)) {
    if (-not (Test-Path $td/build)) {
        New-Item -Type Directory $td/build
    }

    $architecture = machine
    $isArm64 = $architecture -eq 'arm64e'
    $openSslDir = if ($isArm64) { '/opt/homebrew/opt/openssl' } else { '/usr/local/opt/openssl' }

    Push-Location $td/build
    try {
        $cmakeArguments = @(
            '-DCMAKE_BUILD_TYPE=Release'
            "-DOPENSSL_ROOT_DIR=$openSslDir"
            '..'
        )
        $cmakeBuildArguments = @(
            '--build'
            '.'
        )

        cmake @cmakeArguments
        if (!$?) {
            throw 'Cannot execute cmake'
        }

        cmake @cmakeBuildArguments
        if (!$?) {
            throw 'Cannot execute cmake --build'
        }
    } finally {
        Pop-Location
    }
} else {
    Write-Host 'The build result is up to date.'
}
