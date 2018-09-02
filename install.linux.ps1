sudo apt-get install gperf
if (!$?) { throw 'Cannot install dependencies from apt-get' }

git submodule update --init --recursive
