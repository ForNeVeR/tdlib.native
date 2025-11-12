# SPDX-FileCopyrightText: 2018-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output 'Installing gperf from brew'
brew install gperf
if (!$?) { throw 'Cannot install gperf from brew' }
