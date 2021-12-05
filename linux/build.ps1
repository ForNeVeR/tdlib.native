param (
    [string] $td = "$PSScriptRoot/../td",
    [string] $InstallPrefix = "$PSScriptRoot/../build/install"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not (Test-Path $td/build)) {
    New-Item -Type Directory $td/build
}

Push-Location $td/build
try {
    $cmakeArguments = @(
        '-DCMAKE_BUILD_TYPE=Release'
        "-DCMAKE_INSTALL_PREFIX:PATH=$InstallPrefix"
        '..'
    )
    $cmakePrepareCrossCompilingArguments = @(
        '--build'
        '.'
        '--target', 'prepare_cross_compiling'
    )
    $cmakeBuildArguments = @(
        '--build'
        '.'
        '--target', 'install'
    )

    cmake @cmakeArguments
    if (!$?) {
        throw 'Cannot execute cmake'
    }

    cmake @cmakePrepareCrossCompilingArguments
    if (!$?) {
        throw 'Cannot execute cmake --build --target prepare_cross_compiling'
    }

    Set-Location ..
    php SplitSource.php
    if (!$?) {
        throw 'Cannot execute php SplitSource.php'
    }

    Set-Location build
    cmake @cmakeBuildArguments
    if (!$?) {
        throw 'Cannot execute cmake --build --target install'
    }
} finally {
    Pop-Location
}
