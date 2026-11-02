# SPDX-FileCopyrightText: 2018-2026 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output 'Installing gperf from brew'
brew install gperf
if (!$?) { throw 'Cannot install gperf from brew' }

if (!(Test-Path "$(brew --prefix)/opt/openssl@3")) {
    Write-Output 'Installing openssl@3 from brew'
    brew install openssl@3
    if (!$?) { throw 'Cannot install openssl@3 from brew' }
}
