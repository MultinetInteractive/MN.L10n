<#
.SYNOPSIS
    Builds every NuGet package in MN.L10n.sln and collects them in one folder.

.DESCRIPTION
    Builds the solution in Release. The packable projects have GeneratePackageOnBuild
    enabled, so the build itself creates the packages; PackageOutputPath sends them all
    to one folder. The output folder is emptied first, so it only contains packages from
    this run and can be pushed as a whole without picking up old versions.

    This deliberately uses "dotnet build" and not "dotnet pack": with
    GeneratePackageOnBuild enabled, NuGet's Pack target does not depend on Build, so
    "dotnet pack" on a clean checkout fails with NU5026 (files to be packed not found).

    The build runs single-threaded (-m:1) because a parallel build races on outputs that
    other projects copy (MN.L10n.BuildTasks into dotnet-l10n-buildtask).

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

if (Test-Path -LiteralPath $OutputDirectory) {
    if (-not (Test-Path -LiteralPath $OutputDirectory -PathType Container)) {
        throw "OutputDirectory must be a directory: $OutputDirectory"
    }
    $existingItems = @(Get-ChildItem -LiteralPath $OutputDirectory -Force)
    if (@($existingItems | Where-Object { $_.PSIsContainer -or $_.Extension -notin '.nupkg', '.snupkg' }).Count -gt 0) {
        throw "Refusing to clear $OutputDirectory because it contains non-package files or subdirectories. Choose a dedicated package output directory."
    }
    foreach ($item in $existingItems) {
        Remove-Item -LiteralPath $item.FullName -Force
    }
} else {
    New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
}

$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path

Write-Host "Packing $solution into $OutputDirectory" -ForegroundColor Cyan

dotnet build $solution --configuration Release -m:1 "-p:PackageOutputPath=$OutputDirectory"
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

$packages = @(Get-ChildItem -LiteralPath $OutputDirectory -Filter '*.nupkg' | Sort-Object Name)
if ($packages.Count -eq 0) {
    throw "No packages were created in $OutputDirectory"
}

Write-Host ''
Write-Host "Created $($packages.Count) packages:" -ForegroundColor Green
$packages | ForEach-Object { Write-Host "  $($_.Name)" }
