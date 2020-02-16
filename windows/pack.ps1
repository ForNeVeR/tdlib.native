param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Compress-Archive "$BuildRoot/Release/*.dll" tdlib.windows.zip
