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
} else {
    Write-Host 'The build result is up to date.'
}
