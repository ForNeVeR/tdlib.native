$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output 'Installing gperf'
choco install gperf
if (!$?) { throw 'Cannot install dependencies from choco' }

# TODO: Remove, no longer required on GitHub Actions
Write-Output 'Updating the Git submobules'
git submodule update --init --recursive
if (!$?) { throw 'Cannot update the Git submodules' }
