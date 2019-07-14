param (
    [string] $BuildRoot = "$PSScriptRoot/../build",
    [string] $PackageSource = "$BuildRoot/../build/nuget/",
    [string] $TdSharpRoot = "$PSScriptRoot/../tdsharp",
    [string] $TdSharpTestProjectName = 'TDLib.Tests',

    [string] $NuGet = 'NuGet.exe'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $PackageSource -ErrorAction Ignore
$package = Get-Item $BuildRoot/*.nupkg
& $NuGet add $package -Source $PackageSource
if (!$?) { throw 'Cannot add a NuGet package into source' }

Push-Location $TdSharpRoot
try {
    dotnet remove $TdSharpTestProjectName package tdlib.native
    if (!$?) { throw 'Cannot uninstall package from the test project' }
    dotnet add $TdSharpTestProjectName package tdlib.native --source $PackageSource
    if (!$?) { throw 'Cannot add package into the test project' }
    dotnet test
    if (!$?) { throw 'Tests failed' }
} finally {
    Pop-Location
}
