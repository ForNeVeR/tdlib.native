param (
    [string] $Target = "$PSScriptRoot/../build/runtimes/osx-x64/native/",
    [string] $Source = "$PSScriptRoot/../artifacts/libtdjson.dylib"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $Target
Copy-Item $Source $Target
