$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Compress-Archive ./td/build/Release/*.dll tdlib.windows.zip
