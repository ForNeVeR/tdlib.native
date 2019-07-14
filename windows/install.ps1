$ErrorActionPreference = 'Stop'

choco install gperf
if (!$?) { throw 'Cannot install dependencies from choco' }

git submodule update --init --recursive
