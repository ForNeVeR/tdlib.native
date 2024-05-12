param (
    [string] $td = "$PSScriptRoot/../td",
    [string] $Platform = 'x64-windows',
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

    Push-Location $td/build
    try {
        $vcpkgArguments = @(
            'install'
            "gperf:$platform"
            "openssl:$platform"
            "zlib:$platform"
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

        if ($Platform -eq 'x64-windows') {
            $cmakeArguments += @('-A', 'X64')
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
