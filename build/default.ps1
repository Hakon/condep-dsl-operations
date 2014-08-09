properties {
	$pwd = Split-Path $psake.build_script_file	
	$build_directory  = "$pwd\output\condep-dsl-operations"
	$configuration = "Release"
	$preString = "beta"
	$releaseNotes = ""
}
 
include .\..\tools\psake_ext.ps1

function GetNugetAssemblyVersion($assemblyPath) {
	$versionInfo = Get-Item $assemblyPath | % versioninfo

	return "$($versionInfo.FileMajorPart).$($versionInfo.FileMinorPart).$($versionInfo.FileBuildPart)-$preString"
}

task default -depends Build-All, Pack-All
task ci -depends Build-All, Pack-All

task Build-All -depends Clean, Build, Create-BuildSpec-ConDep-Dsl-Operations
task Pack-All -depends Pack-ConDep-Dsl-Operations

task Build {
	Exec { msbuild "$pwd\..\src\condep-dsl-operations.sln" /t:Build /p:Configuration=$configuration /p:OutDir=$build_directory /p:GenerateProjectSpecificOutputFolder=true}
}

task Clean {
	Write-Host "Cleaning Build output"  -ForegroundColor Green
	Remove-Item $build_directory -Force -Recurse -ErrorAction SilentlyContinue
}

task Create-BuildSpec-ConDep-Dsl-Operations {
	Generate-Nuspec-File `
		-file "$build_directory\condep.dsl.operations.nuspec" `
		-version $(GetNugetAssemblyVersion $build_directory\ConDep.Dsl.Operations\ConDep.Dsl.Operations.dll) `
		-id "ConDep.Dsl.Operations" `
		-title "ConDep.Dsl.Operations" `
		-licenseUrl "http://www.con-dep.net/license/" `
		-projectUrl "http://www.con-dep.net/" `
		-description "ConDep is a highly extendable Domain Specific Language for Continuous Deployment, Continuous Delivery and Infrastructure as Code on Windows. This package contians all the default operations found in ConDep. For additional operations, look for ConDep.Dsl.Operations.Contrib." `
		-iconUrl "https://raw.github.com/torresdal/ConDep/master/images/ConDepNugetLogo.png" `
		-releaseNotes "$releaseNotes" `
		-tags "Continuous Deployment Delivery Infrastructure WebDeploy Deploy msdeploy IIS automation powershell remote" `
		-dependencies @(
			@{ Name="ConDep.Dsl"; Version="[3.0.16-beta,4)"},
			@{ Name="SlowCheetah.Tasks.Unofficial"; Version="1.0.0"}
		) `
		-files @(
			@{ Path="ConDep.Dsl.Operations\ConDep.Dsl.Operations.dll"; Target="lib/net40"}, 
			@{ Path="ConDep.Dsl.Operations\ConDep.Dsl.Operations.xml"; Target="lib/net40"}
		)
}

task Pack-ConDep-Dsl-Operations {
	Exec { nuget pack "$build_directory\condep.dsl.operations.nuspec" -OutputDirectory "$build_directory" }
}