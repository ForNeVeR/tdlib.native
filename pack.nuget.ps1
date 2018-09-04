param (
    $Tag = 'v1.2.0',
    $BaseAddress = "https://github.com/ForNeVeR/tdlib.native/releases/download/$Tag",
    $BuildDirectory = "$PSScriptRoot/build",
    
    $NuGet = 'NuGet.exe'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 6

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
        Invoke-WebRequest "$BaseAddress/$artifact" -OutFile $artifact
        Expand-Archive $artifact -DestinationPath "runtimes/$($artifacts[$artifact])"
    }

    Remove-Item *.zip

    & $NuGet pack ../tdlib.native.nuspec
    if (!$?) { throw 'NuGet execution error' }
} finally {
    Pop-Location
}