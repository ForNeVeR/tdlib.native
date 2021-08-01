param (
    [string] $BuildRoot = "$PSScriptRoot/../build",
    [string] $PackageSource = "$BuildRoot/../build/nuget/",
    [string] $TdSharpRoot = "$PSScriptRoot/../tdsharp",
    [string] $TdSharpTestProjectName = 'TDLib.Tests',
    [string] $TdSharpProjectToRemove = 'Samples/TdLib.Samples.GetChats',
    [string] $PackageName = 'tdlib.native',

    [string] $NuGet = 'NuGet.exe',
    [switch] $UseMono,
    [string] $Mono = 'mono',
    [string] $dotnet = 'dotnet'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $PackageSource -ErrorAction Ignore
$package = Get-Item $BuildRoot/*.nupkg

Write-Output "Adding a package $package into NuGet source $PackageSource"
if ($UseMono) {
    & $Mono $NuGet add $package -Source $PackageSource
	& $dotnet nuget add source --name 'tdlib.native' ( Resolve-Path $PackageSource )
} else {
    & $NuGet add $package -Source $PackageSource
    & $dotnet nuget add source --name 'tdlib.native' ( Resolve-Path $PackageSource )
}
if (!$?) { throw 'Cannot add a NuGet package into source' }

Push-Location $TdSharpRoot
try {
    Write-Output "Removing a project $TdSharpProjectToRemove from the solution file"
    & $dotnet sln remove $TdSharpProjectToRemove
    if (!$?) { throw 'Cannot remove an unnecessary project from the solution' }

    Write-Output "Removing a package $PackageName from the project $TdSharpTestProjectName"
    & $dotnet remove $TdSharpTestProjectName package $PackageName
    if (!$?) { throw 'Cannot uninstall package from the test project' }

    Write-Output 'Performing dotnet restore'
    & $dotnet restore
    if (!$?) { throw 'Cannot perform dotnet restore' }

    Write-Output "Adding a package $PackageName from the project $TdSharpTestProjectName"
    & $dotnet add $TdSharpTestProjectName package $PackageName --version 1.7.0
    if (!$?) { throw 'Cannot add package into the test project' }

    Write-Output "Running tests"
    & $dotnet test --verbosity normal
    if (!$?) { throw 'Tests failed' }
} finally {
    Pop-Location
}
