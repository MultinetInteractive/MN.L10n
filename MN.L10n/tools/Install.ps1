param($installPath, $toolsPath, $package, $project)
    # This is the MSBuild targets file to add
	$buildPath = '..\build'
    $targetsFile = [System.IO.Path]::Combine($toolsPath, $buildPath, $package.Id + '.targets')
	$targetsFile = [System.IO.Path]::GetFullPath($targetsFile)
 
    # Need to load MSBuild assembly if it's not loaded yet.
    Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

    # Grab the loaded MSBuild project for the project
    $msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1

	#Short circuit if OctoPack has already been installed (this could be by convention in newer versions of NuGet)
	$existingImports = @()
	$existingImports = $existingImports += $msbuild.Xml.Imports | Where-Object { $_.Project.EndsWith($package.Id + '.targets') }
	if ($existingImports -and $existingImports.length)
	{
		Write-Host "It looks like L10n has been installed, skipping Install.ps1"
		exit
	}

    # Make the path to the targets file relative.
    $projectUri = new-object Uri($project.FullName, [System.UriKind]::Absolute)
    $targetUri = new-object Uri($targetsFile, [System.UriKind]::Absolute)
    $relativePath = [System.Uri]::UnescapeDataString($projectUri.MakeRelativeUri($targetUri).ToString()).Replace([System.IO.Path]::AltDirectorySeparatorChar, [System.IO.Path]::DirectorySeparatorChar)
 
    # Add the import with a condition, to allow the project to load without the targets present.
    $import = $msbuild.Xml.AddImport($relativePath)
    $import.Condition = "Exists('$relativePath')"

    $isFSharpProject = ($project.Type -eq "F#")
    if ($isFSharpProject)
    {
        $project.Save("")
    }
    else
    {
        $project.Save()
    }
