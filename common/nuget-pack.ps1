param (
    [string] $Version = '1.6.0',
    [string] $BuildDirectory = "$PSScriptRoot/../build",

    [string] $NuGet = 'NuGet.exe'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Push-Location $BuildDirectory
try {
    & $NuGet pack ../tdlib.native.nuspec -BasePath . -Version $Version
    if (!$?) { throw 'NuGet execution error' }
} finally {
    Pop-Location
}
