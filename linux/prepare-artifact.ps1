param (
    [string] $InstallPrefix = "$PSScriptRoot/../build/install",
    [string] $Target = "$PSScriptRoot/../artifacts"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $Target
Copy-Item "$InstallPrefix/lib/libtdjson.so" $Target
