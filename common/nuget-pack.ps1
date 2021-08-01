param (
    [string] $Version = '1.6.0',
    [string] $BuildDirectory = "$PSScriptRoot/../build",

    [string] $NuGet = 'NuGet.exe',
    [switch] $UseMono,
    [string] $Mono = 'mono'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output "Preparing a NuGet package version $Version."

Push-Location $BuildDirectory
try {
    if ($UseMono) {
        & $Mono $NuGet pack ../tdlib.native.nuspec -BasePath . -Version $Version
    } else {
        & $NuGet pack ../tdlib.native.nuspec -BasePath . -Version $Version
    }
    if (!$?) { throw 'NuGet execution error' }
} finally {
    Pop-Location
}
