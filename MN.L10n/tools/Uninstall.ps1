﻿param($installPath, $toolsPath, $package, $project)
 
  # Need to load MSBuild assembly if it's not loaded yet.
  Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

  # Grab the loaded MSBuild project for the project
  $msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

  # Find all the imports and targets added by this package.
  $itemsToRemove = @()

  # Allow many in case a past package was incorrectly uninstalled
  $itemsToRemove += $msbuild.Xml.Imports | Where-Object { $_.Project.EndsWith($package.Id + '.targets') }
    
  $saveProject = $false

  # Remove the elements and save the project
  $saveProject = ($itemsToRemove -and $itemsToRemove.length)
  foreach ($itemToRemove in $itemsToRemove)
  {
	 $msbuild.Xml.RemoveChild($itemToRemove) | out-null
  }

  $targetImports = @()
  $targetImports += $msbuild.Xml.Targets | Where-Object { $_.Name -eq "EnsureNuGetPackageBuildImports" }
  foreach ($targetImport in $targetImports)
  {
	$targetImportsToRemove = @()
	$targetImportsToRemove += $targetImports.Children | Where-Object { $_.Condition.Contains($package.Id + '.targets') }

	$saveProject = $saveProject -or ($targetImportsToRemove -and $targetImportsToRemove.length)
	foreach ($targetImport in $targetImportsToRemove)
	{
		$targetImports.RemoveChild($targetImport)
	}
  }

  if ($saveProject)
  {
     $isFSharpProject = ($project.Type -eq "F#")
     if ($isFSharpProject)
     {
         $project.Save("")
     }
     else
     {
         $project.Save()
     }
  }