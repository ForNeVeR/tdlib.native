param (
    $SourceRoot = "$PSScriptRoot/..",
    $PackageDir = "$SourceRoot/build",
    $PackageSource = "$SourceRoot/nuget",
    $NuGetConfigTarget = "$SourceRoot/nuget.config",

    [Parameter(Mandatory = $true)]
    [string] $NuGet,
    [switch] $UseMono,
    $Mono = 'mono'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

New-Item -Type Directory $PackageSource -ErrorAction Ignore
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="build" value="$(Resolve-Path $PackageSource)"/>
    </packageSources>
</configuration>
"@ > $NuGetConfigTarget

Write-Output "Contents of $($NuGetConfigTarget):"
Get-Content $NuGetConfigTarget

Get-Item $PackageDir/*.nupkg | ForEach-Object {
    Write-Output "Adding a package $package into NuGet source $PackageSource"
    if ($UseMono) {
        & $Mono $NuGet add $package -Source $PackageSource
    } else {
        & $NuGet add $package -Source $PackageSource
    }
}
