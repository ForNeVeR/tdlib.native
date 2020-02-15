param (
    [string] $td = "$PSScriptRoot/../td"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $td/build)) {
    New-Item -Type Directory $td/build
}

Push-Location $td/build
try {
    $cmakeArguments = @(
        '-DCMAKE_BUILD_TYPE=Release'
        '..'
    )
    $cmakeBuildArguments = @(
        '--build'
        '.'
    )

    cmake $cmakeArguments
    if (!$?) {
        throw 'Cannot execute cmake'
    }

    cmake $cmakeBuildArguments
    if (!$?) {
        throw 'Cannot execute cmake --build'
    }
} finally {
    Pop-Location
}
