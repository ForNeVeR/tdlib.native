$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output 'Installing gperf from brew'
brew install gperf
if (!$?) { throw 'Cannot install gperf from brew' }

# https://github.com/actions/virtual-environments/issues/4195
Write-Output 'Setting up an OpenSSL symlink for CMake.'
sh -c 'ln -sf $(brew --cellar openssl@1.1)/1.1* /usr/local/opt/openssl'
if (!$?) { throw 'Cannot set up an OpenSSL symlink.' }
