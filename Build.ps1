Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Building LzString..." -ForegroundColor Green

# ==================================== Setup

Install-Module VSSetup -Scope CurrentUser
$MSBuildExe = "$((Get-VSSetupInstance).InstallationPath)\MSBuild\Current\Bin\MSBuild.exe"
If(-not (Test-Path $MSBuildExe)) {
	Throw "Could not find MSBuild 15.0"
}
Write-Host "Using MSBuild 15.0 at: $MSBuildExe"

# ==================================== Variables

$NuGet = "$PSScriptRoot\.nuget\NuGet.exe"
$CSProjPath = "$PSScriptRoot\src\LZStringCSharp.csproj"
$BuildPath = "$PSScriptRoot\src\bin\Release\netstandard1.0"

# ==================================== Build

If(Test-Path -Path $BuildPath) {
	Remove-Item -Confirm:$false "$BuildPath\*.*" -Recurse
}

&($NuGet) restore

&($MSBuildExe) .\src\LZStringCSharp.csproj `
	/t:pack `
	/tv:15.0 `
	/p:Configuration=Release `
	/p:AllowedReferenceRelatedFileExtensions=- `
	/p:DebugSymbols=false `
	/p:DebugType=None `
	/clp:ErrorsOnly `
	/v:m

Write-Host "Output NuGet package can be found in .\src\bin\Release\LZStringCSharp.(version).nupkg"
