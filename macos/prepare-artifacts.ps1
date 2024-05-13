param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build",
    [string] $TargetLocation = "./artifacts/"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (!(Test-Path -LiteralPath $TargetLocation -Type Container)) {
    New-Item -Type Directory $TargetLocation
}
Copy-Item "$BuildRoot/libtdjson.dylib" $TargetLocation
