$ErrorActionPreference = 'Stop'

brew install gperf nuget openssl
if (!$?) { throw 'Cannot install dependencies from brew' }

brew cask install dotnet-sdk
if (!$?) { throw 'Cannot install dotnet-sdk' }
