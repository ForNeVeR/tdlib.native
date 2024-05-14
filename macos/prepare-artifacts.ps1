param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build",
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
    Copy-Item "$BuildRoot/libtdjson.dylib" $TargetLocation
}
