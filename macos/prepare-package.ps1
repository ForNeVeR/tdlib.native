param (
    [string] $Source = "$PSScriptRoot/../artifacts/*",
    [string] $Target = "$PSScriptRoot/../build/runtimes/osx-x64/native/"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $Target
Copy-Item $Source $Target
