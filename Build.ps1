Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Building LzString..." -ForegroundColor Green

# ==================================== Functions

Function GetMSBuildExe {
	[CmdletBinding()]
	$DotNetVersion = "14.0"
	$RegKey = "HKLM:\software\Microsoft\MSBuild\ToolsVersions\$DotNetVersion"
	$RegProperty = "MSBuildToolsPath"
	$MSBuildExe = Join-Path -Path (Get-ItemProperty $RegKey).$RegProperty -ChildPath "msbuild.exe"
	Return $MSBuildExe
}

# ==================================== Variables

$NuGet = "$PSScriptRoot\.nuget\NuGet.exe"
$BuildPath = "$PSScriptRoot\src\bin\Release"
$CSProjPath = "$PSScriptRoot\src\LZStringCSharp.csproj"

# ==================================== Build

If(Test-Path -Path $BuildPath) {
	Remove-Item -Confirm:$false "$BuildPath\*.*" -Recurse
}

&($NuGet) restore

&(GetMSBuildExe) lz-string-csharp.sln `
	/t:Clean`;Rebuild `
	/p:Configuration=Release `
	/p:AllowedReferenceRelatedFileExtensions=- `
	/p:DebugSymbols=false `
	/p:DebugType=None `
	/clp:ErrorsOnly `
	/v:m

&($NuGet) pack $CSProjPath -Prop Configuration=Release
