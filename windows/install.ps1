$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

choco install gperf
if (!$?) { throw 'Cannot install dependencies from choco' }

git submodule update --init --recursive
