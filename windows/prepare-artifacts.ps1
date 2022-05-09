param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build",
    [string] $BinLocation = "$BuildRoot/Release",
    [string] $TargetLocation = "./artifacts/"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $TargetLocation
Copy-Item "$BinLocation/*.dll" $TargetLocation
