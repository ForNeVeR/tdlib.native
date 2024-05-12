$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output 'Installing gperf from brew'
brew install gperf
if (!$?) { throw 'Cannot install gperf from brew' }
