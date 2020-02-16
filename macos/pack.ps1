$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Compress-Archive ./td/build/libtdjson.dylib tdlib.osx.zip
