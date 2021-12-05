param (
    [string] $NuGetDownloadUrl = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe',
    [string] $NuGetPath = "$PSScriptRoot/../tools/nuget.exe"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output 'Installing gperf'
apt-get install gperf
if (!$?) { throw 'Cannot install dependencies from apt-get' }

Write-Output "Downloading NuGet client to $NuGetPath"
New-Item -Type Directory ([IO.Path]::GetDirectoryName($NuGetPath))
Invoke-WebRequest -OutFile $NuGetPath $NuGetDownloadUrl

Write-Output 'Updating the Git submobules'
git submodule update --init --recursive
if (!$?) { throw 'Cannot update the Git submodules' }
