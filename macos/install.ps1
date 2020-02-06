$ErrorActionPreference = 'Stop'

brew install gperf nuget openssl
if (!$?) { throw 'Cannot install dependencies from brew' }

brew tap isen-ng/dotnet-sdk-versions
if (!$?) { throw 'Cannot execute brew tap' }

brew cask install dotnet-sdk-2.2.100
if (!$?) { throw 'Cannot install dotnet-sdk' }
