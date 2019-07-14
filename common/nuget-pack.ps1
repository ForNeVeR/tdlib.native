param (
    $Version = '1.3.0',
    $BuildDirectory = "$PSScriptRoot/../build",

    $NuGet = 'NuGet.exe'
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
