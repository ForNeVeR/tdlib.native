param (
    [string] $BuildRoot = "$PSScriptRoot/../build",
    [string] $InstallPrefix = "$BuildRoot/install",
    [string] $TargetLocation = "$PSScriptRoot/../artifacts"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $TargetLocation
Copy-Item "$InstallPrefix/lib/*" $TargetLocation
