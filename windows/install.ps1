$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output 'Installing gperf'
choco install gperf
if (!$?) { throw 'Cannot install dependencies from choco' }

Write-Output 'Installing .net core 2.2'
choco install dotnetcore-2.2-runtime
if (!$?) { throw 'Cannot install .net core 2.2' }

Write-Output 'Updating the Git submobules'
git submodule update --init --recursive
if (!$?) { throw 'Cannot update the Git submodules' }
