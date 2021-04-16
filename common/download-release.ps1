param (
    [string] $Version = '1.7.0',
    [string] $Tag = "v$Version",
    [string] $BaseAddress = "https://github.com/ForNeVeR/tdlib.native/releases/download/$Tag",
    [string] $BuildDirectory = "$PSScriptRoot/../build"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$artifacts = @{
    'tdlib.linux.zip' = 'linux-x64'
    'tdlib.osx.zip' = 'osx-x64'
    'tdlib.windows.zip' = 'win-x64'
}

if (!(Test-Path -PathType Container $BuildDirectory)) {
    New-Item $BuildDirectory -ItemType Directory
}

Push-Location $BuildDirectory
try {
    foreach ($artifact in $artifacts.Keys) {
        $targetDirectory = "runtimes/$($artifacts[$artifact])/native"
        Invoke-WebRequest "$BaseAddress/$artifact" -OutFile $artifact
        Expand-Archive $artifact -DestinationPath $targetDirectory
    }

    Remove-Item *.zip
} finally {
    Pop-Location
}
