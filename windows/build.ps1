# SPDX-FileCopyrightText: 2018-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [Parameter(Mandatory = $true)] [string] $DotNetArch,
    [string] $td = "$PSScriptRoot/../td",
    [Parameter(Mandatory = $true)] [string] $VcpkgToolchain,
    [string] $CheckUpToDateScript = "$PSScriptRoot/../common/Test-UpToDate.ps1",
    [switch] $SkipUpToDateCheck
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($SkipUpToDateCheck -or !$(& $CheckUpToDateScript)) {
    if (-not (Test-Path $td/build)) {
        New-Item -Type Directory $td/build
    }

    $vcPkgPlatform = switch ($DotNetArch) {
        'x64' { 'x64-windows' }
        'arm64' { 'arm64-windows' }
        else { throw "Unknown architecture: $DotNetArch." }
    }

    Push-Location $td/build
    try {
        $vcpkgArguments = @(
            'install'
            "gperf:$vcPkgPlatform"
            "openssl:$vcPkgPlatform"
            "zlib:$vcPkgPlatform"
        )
        $cmakeArguments = @(
            "-DCMAKE_TOOLCHAIN_FILE=$VcpkgToolchain"
            '-DCMAKE_DISABLE_FIND_PACKAGE_Readline=TRUE' # workaround for #76
            '..'
        )
        $cmakeBuildArguments = @(
            '--build'
            '.'
            '--config'
            'Release'
        )

        if ($DotNetArch -eq 'x64') {
            $cmakeArguments += @('-A', 'X64')
        } elseif ($DotNetArch -eq 'arm64') {
            $cmakeArguments += @('-A', 'ARM64')
        }  else {
            else { throw "Unknown architecture: $DotNetArch." }
        }

        vcpkg @vcpkgArguments
        if (!$?) {
            throw 'Cannot execute vcpkg'
        }

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
