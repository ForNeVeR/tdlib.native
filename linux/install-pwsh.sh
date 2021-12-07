set -e

apt-get update && apt-get install wget

cd /tmp
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb

apt-get update && apt-get install powershell
