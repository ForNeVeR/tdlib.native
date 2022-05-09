param (
    [string] $Artifacts = "$PSScriptRoot/../artifacts",
    [string] $GoldFile = "$PSScriptRoot/../windows/libraries.gold.txt",
    [string] $ResultFile = "$PSScriptRoot/../windows/libraries.temp.txt",
    [switch] $GenerateGold
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Write-Output "Saving library check results to $ResultFile…"

if (Test-Path $ResultFile) {
    Remove-Item $ResultFile
}

Get-ChildItem "$Artifacts/*.dll" | Sort-Object -Property Name | ForEach-Object {
    $libraryPath = $_.FullName

    Write-Output "Checking file $libraryPath…"
    $output = dumpbin /DEPENDENTS $libraryPath
    if (!$?) {
        throw "dumpbin /DEPENDENTS $libraryPath returned an exit code $LASTEXITCODE; output: $output"
    }

    $libraryNames = $output | Where-Object { $_ -match '^    [^ ]' } | ForEach-Object { $_.TrimStart() } | Sort-Object
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
