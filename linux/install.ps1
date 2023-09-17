param (
    [string] $NuGetDownloadUrl = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe',
    [string] $NuGetPath = "$PSScriptRoot/../tools/nuget.exe",
    [switch] $ForBuild,
    [switch] $ForTests
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($ForBuild) {
    try {
        # The following command requires installing tzdata which is interactive, so let's forbid its interactivity.
        $oldDebianFrontend = $env:DEBIAN_FRONTEND
        $env:DEBIAN_FRONTEND = 'noninteractive'
        $dependencies = @(
            'cmake'
            'g++'
            'git'
            'gperf'
            'gperf'
            'libssl-dev'
            'make'
            'php-cli'
            'zlib1g-dev'
        )

        Write-Output 'Installing dependencies.'
        sudo apt-get install -y @dependencies
        if (!$?) {
            throw 'Cannot install dependencies from apt-get'
        }
    } finally {
        $env:DEBIAN_FRONTEND = $oldDebianFrontend
    }
}

if ($ForTests) {
    $dependencies = @(
        'ca-certificates'
        'gnupg'
    )

    Write-Output 'Installing dependencies.'
    sudo apt-get install -y @dependencies
    if (!$?) {
        throw 'Cannot install dependencies from apt-get'
    }

    Write-Output 'Installing Mono.'
    sudo gpg --homedir /tmp --no-default-keyring --keyring /usr/share/keyrings/mono-official-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
    if (!$?) {
        throw "Error exit code from gpg: $LASTEXITCODE."
    }

    echo "deb [signed-by=/usr/share/keyrings/mono-official-archive-keyring.gpg] https://download.mono-project.com/repo/ubuntu stable-focal main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
    if (!$?) {
        throw "Cannot add a package source, exit code: $LASTEXITCODE."
    }

    sudo apt update
    if (!$?) {
        throw "Cannot execute apt update, exit code: $LASTEXITCODE."
    }

    sudo apt install -y mono-devel
    if (!$?) {
        throw "Cannot execute apt install, exit code: $LASTEXITCODE."
    }

    Write-Output "Downloading NuGet client to $NuGetPath"
    New-Item -Type Directory ([IO.Path]::GetDirectoryName($NuGetPath))
    Invoke-WebRequest -OutFile $NuGetPath $NuGetDownloadUrl
}
