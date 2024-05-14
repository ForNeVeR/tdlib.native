param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build",
    [string] $BinLocation = "$BuildRoot/Release",
    [string] $TargetLocation = "./artifacts/",
    [string] $CheckUpToDateScript = "$PSScriptRoot/../common/Test-UpToDate.ps1",
    [switch] $SkipUpToDateCheck
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($SkipUpToDateCheck -or !$(& $CheckUpToDateScript)) {
    if (!(Test-Path -LiteralPath $TargetLocation -Type Container)) {
        New-Item -Type Directory $TargetLocation
    }
    Copy-Item "$BinLocation/*.dll" $TargetLocation
    & $CheckUpToDateScript -GenerateResultKey
}
