# SPDX-FileCopyrightText: 2018-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
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
        sudo apt-get update
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
}
