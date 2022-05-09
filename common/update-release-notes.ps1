param (
    [string] $ReleaseNotesFile,
    [string] $TargetFile = "$PSScriptRoot/../tdlib.native.nuspec"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

[xml] $contents = Get-Content $TargetFile
$contents.package.metadata.releaseNotes = Get-Content $ReleaseNotesFile
$contents.Save($(Resolve-Path $TargetFile))
