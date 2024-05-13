param (
    [string] $BuildRoot = "$PSScriptRoot/../build",
    [string] $InstallPrefix = "$BuildRoot/install",
    [string] $TargetLocation = "$PSScriptRoot/../artifacts"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (!(Test-Path -LiteralPath $TargetLocation -Type Container)) {
    New-Item -Type Directory $TargetLocation
}
Copy-Item "$InstallPrefix/lib/*" $TargetLocation
