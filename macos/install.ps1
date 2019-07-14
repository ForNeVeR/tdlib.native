$ErrorActionPreference = 'Stop'

brew install gperf openssl
if (!$?) { throw 'Cannot install dependencies from brew' }
