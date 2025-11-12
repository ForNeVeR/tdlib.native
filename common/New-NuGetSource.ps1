# SPDX-FileCopyrightText: 2024-2025 Friedrich von Never <friedrich@fornever.me>
#
# SPDX-License-Identifier: BSL-1.0

param (
    $SourceRoot = "$PSScriptRoot/..",
    $PackageDir = "$SourceRoot/build",
    $PackageSource = "$SourceRoot/nuget",
    $NuGetConfigTarget = "$SourceRoot/nuget.config"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $PackageSource -ErrorAction Ignore
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="local" value="$(Resolve-Path $PackageSource)"/>
    </packageSources>
</configuration>
"@ > $NuGetConfigTarget

Write-Output "Contents of $($NuGetConfigTarget):"
Get-Content $NuGetConfigTarget

Write-Output "Contents of the packages directory:"
Get-ChildItem $PackageDir

Get-Item $PackageDir/*.nupkg | ForEach-Object {
    $package = $_.FullName

    Write-Output "Adding a package $package into NuGet source $PackageSource"
    Copy-Item $package $PackageSource
}

Write-Output "Content of the source ($(Resolve-Path $PackageSource)):"
Get-ChildItem -Recurse $PackageSource
