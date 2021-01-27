$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# TODO[F]: Replace install.ps1 with this file after removing Travis support.

Write-Output 'Installing gperf from brew'
brew install gperf
if (!$?) { throw 'Cannot install gperf from brew' }
