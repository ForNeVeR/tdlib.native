param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build/Release",
    [string] $TargetLocation = "./artifacts/"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $TargetLocation
Copy-Item "$BuildRoot/*.dll" $TargetLocation
