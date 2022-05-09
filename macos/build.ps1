param (
    [string] $td = "$PSScriptRoot/../td"
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
        '-DOPENSSL_ROOT_DIR=/usr/local/opt/openssl/'
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
