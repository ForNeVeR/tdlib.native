# SPDX-FileCopyrightText: 2019-2025 tdlib.native contributors <https://github.com/ForNeVeR/tdlib.native>
#
# SPDX-License-Identifier: BSL-1.0

param (
    [string] $BuildRoot = "$PSScriptRoot/../build",
    [string] $PackageSource = "$BuildRoot/../build/nuget/",
    [string] $TdSharpRoot = "$PSScriptRoot/../tdsharp",
    [string] $TdSharpTestProjectName = 'TdLib.Tests',
    [string] $BasePackageName = 'tdlib.native',
    [Parameter(Mandatory = $true)]
    [string] $PackageName,

    [string] $dotnet = 'dotnet'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $PackageSource -ErrorAction Ignore
$package = Get-Item $BuildRoot/*.nupkg

Write-Output "Copying the package $package into NuGet source $PackageSource"
Copy-Item $package $PackageSource

Push-Location "$TdSharpRoot/$TdSharpTestProjectName"
try {
    Write-Output 'Removing the global.json file of the submodule to disable its override by the submodule…'
    Remove-Item "$TdSharpRoot/global.json"

    Write-Output "Removing a package $BasePackageName from the project $TdSharpTestProjectName"
    & $dotnet remove "$TdSharpTestProjectName.csproj" package $BasePackageName
    if (!$?) { throw 'Cannot uninstall package from the test project' }

    Write-Output 'Performing dotnet restore'
    & $dotnet restore
    if (!$?) { throw 'Cannot perform dotnet restore' }

    Write-Output 'Available files at the package source:'
    Get-ChildItem $PackageSource

    Write-Output "Adding a package $PackageName from the project $TdSharpTestProjectName"
    & $dotnet add "$TdSharpTestProjectName.csproj" package $PackageName --prerelease --source $PackageSource
    if (!$?) { throw 'Cannot add package into the test project' }

    Write-Output "Resulting content of `"$TdSharpTestProjectName.csproj`":"
    Get-Content "$TdSharpTestProjectName.csproj"

    Write-Output "Running tests"
    & $dotnet test --verbosity normal
    if (!$?) { throw 'Tests failed' }
} finally {
    Pop-Location
}
