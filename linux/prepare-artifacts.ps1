param (
    [string] $BuildRoot = "$PSScriptRoot/../build",
    [string] $InstallPrefix = "$BuildRoot/install",
    [string] $TargetLocation = "$PSScriptRoot/../artifacts",
    [string] $CheckUpToDateScript = "$PSScriptRoot/../common/Test-UpToDate.ps1",
    [switch] $SkipUpToDateCheck
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($SkipUpToDateCheck -or !$(& $CheckUpToDateScript)) {
    if (!(Test-Path -LiteralPath $TargetLocation -Type Container)) {
        New-Item -Type Directory $TargetLocation
    }
    Copy-Item "$InstallPrefix/lib/*" $TargetLocation
}
