param (
    [Parameter(Mandatory = $true)]
    [string] $DotNetArch,
    [Parameter(Mandatory = $true)]
    [string] $PackageName,
    [string] $RepoRoot = "$PSScriptRoot/..",
    [string] $Package = "$RepoRoot/build/$PackageName/runtimes/osx-$DotNetArch/native",
    [string] $GoldFile = "$RepoRoot/macos/libraries.$DotNetArch.gold.txt",
    [string] $ResultFile = "$RepoRoot/macos/libraries.temp.txt",
    [string] $LddApple = "$RepoRoot/ldd-apple/ldd-apple.sh",
    [switch] $GenerateGold
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output "Saving library check results to $ResultFile…"

if (Test-Path $ResultFile) {
    Remove-Item $ResultFile
}

Get-ChildItem "$Package/*.dylib" | Sort-Object -Property Name | ForEach-Object {
    $libraryPath = $_.FullName

    Write-Output "Checking file `"$libraryPath`"…"
    $output = & $LddApple $libraryPath *>&1
    if (!$?) {
        throw "ldd-apple returned an exit code $LASTEXITCODE."
    }

    Write-Output "Output from ldd-apple $($libraryPath):"
    Write-Output $output

    $libraryNames = $output | Where-Object { ([string]$_).Contains('dyld: loaded') } | ForEach-Object {
        if (!($_ -match 'dyld: loaded: <.*?> (.*)')) {
            throw "Failed to parse ldd-apple output: $_"
        }

        $filePath = $Matches[1]
        if ($filePath -ne $libraryPath) {
            $filePath
        }
    } | Sort-Object
    $_.Name >> $ResultFile
    $libraryNames | ForEach-Object { "  $_" >> $ResultFile }
}

if ($GenerateGold) {
    Move-Item -Force $ResultFile $GoldFile
} else {
    $goldContent = Get-Content -Raw $GoldFile
    $tempContent = Get-Content -Raw $ResultFile
    if ($goldContent -ne $tempContent) {
        Write-Output "Current contents are following:`n"
        Write-Output $tempContent
        throw "File contents are not equal: $GoldFile and $ResultFile"
    }
}
