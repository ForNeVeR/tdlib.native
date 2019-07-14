param (
    [string] $Target = "$PSScriptRoot/../build/runtimes/linux-x64/native/",
    [string] $Source = "$PSScriptRoot/../td/build/libtdjson.so"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $Target
Copy-Item $Source $Target
