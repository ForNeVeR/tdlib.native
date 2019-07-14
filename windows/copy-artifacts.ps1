param (
    [string] $Target = "$PSScriptRoot/../build/runtimes/win-x64/native/",
    [string] $Source = "$PSScriptRoot/../td/build/Release/*.dll"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $Target
Copy-Item $Source $Target
