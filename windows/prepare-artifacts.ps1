param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build",
    [string] $BinLocation = "$BuildRoot/Release",
    [string] $TargetLocation = "./artifacts/"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (!(Test-Path -LiteralPath $TargetLocation -Type Container)) {
    New-Item -Type Directory $TargetLocation
}
Copy-Item "$BinLocation/*.dll" $TargetLocation
