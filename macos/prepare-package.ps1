param (
    [string] $Source = "$PSScriptRoot/../artifacts/*",
    [Parameter(Mandatory = $true)]
    [string] $DotNetArch,
    [string] $Target = "$PSScriptRoot/../build/runtimes/osx-$DotNetArch/native/"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $Target
Copy-Item $Source $Target
