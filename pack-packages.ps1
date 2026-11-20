<#
.SYNOPSIS
    Builds every NuGet package in MN.L10n.sln and collects them in one folder.

.DESCRIPTION
    Builds in Release and packs the solution. The output folder is emptied first, so it
    only contains packages from this run and can be pushed as a whole without picking up
    old versions.

    Pack runs single-threaded (-m:1): the projects have GeneratePackageOnBuild enabled,
    and a parallel pack of the solution races on build outputs that other projects copy
    (BuildTasks and dotnet-l10n-buildtask).

.PARAMETER OutputDirectory
    Where the .nupkg files end up. Default: artifacts/packages in the repository root.

.EXAMPLE
    ./pack-packages.ps1

.EXAMPLE
    ./pack-packages.ps1 -OutputDirectory C:\temp\l10n-packages
#>
[CmdletBinding()]
param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'artifacts/packages')
)

$ErrorActionPreference = 'Stop'

$solution = Join-Path $PSScriptRoot 'MN.L10n.sln'

if (Test-Path $OutputDirectory) {
    Remove-Item -Path $OutputDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null

Write-Host "Packing $solution into $OutputDirectory" -ForegroundColor Cyan

dotnet pack $solution --configuration Release --output $OutputDirectory -m:1
if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed with exit code $LASTEXITCODE"
}

$packages = Get-ChildItem -Path $OutputDirectory -Filter '*.nupkg' | Sort-Object Name
if ($packages.Count -eq 0) {
    throw "No packages were created in $OutputDirectory"
}

Write-Host ''
Write-Host "Created $($packages.Count) packages:" -ForegroundColor Green
$packages | ForEach-Object { Write-Host "  $($_.Name)" }
