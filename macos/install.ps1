$ErrorActionPreference = 'Stop'

brew install gperf openssl nuget
if (!$?) { throw 'Cannot install dependencies from brew' }
