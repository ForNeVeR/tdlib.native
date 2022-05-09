param (
    [string] $Source = "$PSScriptRoot/../artifacts/*",
    [string] $Target = "$PSScriptRoot/../build/runtimes/linux-x64/native/"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $Target
Copy-Item $Source $Target
