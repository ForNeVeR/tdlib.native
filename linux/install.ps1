param (
    [string] $NuGetDownloadUrl = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe',
    [string] $NuGetPath = "$PSScriptRoot/../tools/nuget.exe",
    [switch] $ForBuild,
    [switch] $ForTests,
    [switch] $ForRelease
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
        apt-get install -y @dependencies
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
    apt-get install -y @dependencies
    if (!$?) {
        throw 'Cannot install dependencies from apt-get'
    }

    Write-Output 'Installing Mono.'
    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
    if (!$?) {
        throw 'Cannot execute apt-key adv.'
    }

    echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | tee /etc/apt/sources.list.d/mono-official-stable.list
    if (!$?) {
        throw 'Cannot add a package source.'
    }

    apt-get update && apt-get install -y mono-devel
    if (!$?) {
        throw 'Cannot execute apt-get update.'
    }
}

if ($ForTests -or $ForRelease) {
    Write-Output "Downloading NuGet client to $NuGetPath"
    New-Item -Type Directory ([IO.Path]::GetDirectoryName($NuGetPath))
    Invoke-WebRequest -OutFile $NuGetPath $NuGetDownloadUrl
}
