param (
    [string] $BuildRoot = "$PSScriptRoot/../build",
    [string] $PackageSource = "$BuildRoot/../build/nuget/",
    [string] $TdSharpRoot = "$PSScriptRoot/../tdsharp",
    [string] $TdSharpTestProjectName = 'TdLib.Tests',
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
} else {
    & $NuGet add $package -Source $PackageSource
}
if (!$?) { throw 'Cannot add a NuGet package into source' }

Push-Location "$TdSharpRoot/$TdSharpTestProjectName"
try {
    Write-Output "Removing a package $PackageName from the project $TdSharpTestProjectName"
    & $dotnet remove "$TdSharpTestProjectName.csproj" package $PackageName
    if (!$?) { throw 'Cannot uninstall package from the test project' }

    Write-Output 'Performing dotnet restore'
    & $dotnet restore
    if (!$?) { throw 'Cannot perform dotnet restore' }

    Write-Output "Adding a package $PackageName from the project $TdSharpTestProjectName"
    & $dotnet add "$TdSharpTestProjectName.csproj" package $PackageName --source $PackageSource
    if (!$?) { throw 'Cannot add package into the test project' }

    Write-Output "Running tests"
    & $dotnet test --verbosity normal
    if (!$?) { throw 'Tests failed' }
} finally {
    Pop-Location
}
