param (
    [string] $NuGetDownloadUrl = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe',
    [string] $NuGetPath = "$PSScriptRoot/../tools/nuget.exe"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$dependencies = @(
    'cmake'
    'gperf'
    'make'
    'git'
    'zlib1g-dev'
    'libssl-dev'
    'gperf'
    'php-cli'
    'cmake'
    'g++'
)

Write-Output 'Installing dependencies.'
apt-get install -y @dependencies
if (!$?) { throw 'Cannot install dependencies from apt-get' }

Write-Output "Downloading NuGet client to $NuGetPath"
New-Item -Type Directory ([IO.Path]::GetDirectoryName($NuGetPath))
Invoke-WebRequest -OutFile $NuGetPath $NuGetDownloadUrl
