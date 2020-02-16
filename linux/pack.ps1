param (
    [string] $BuildRoot = "$PSScriptRoot/../td/build"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Compress-Archive "$BuildRoot/libtdjson.so" tdlib.linux.zip
