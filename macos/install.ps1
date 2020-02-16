$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output 'Installing dependencies from brew'
brew install gperf nuget openssl
if (!$?) { throw 'Cannot install dependencies from brew' }

Write-Output 'Performing brew tap'
brew tap isen-ng/dotnet-sdk-versions
if (!$?) { throw 'Cannot execute brew tap' }

Write-Output 'Installing dotnet-sdk from cask'
brew cask install dotnet-sdk-2.2.100
if (!$?) { throw 'Cannot install dotnet-sdk' }
