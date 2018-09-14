Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Building LzString..." -ForegroundColor Green

# ==================================== Variables

$NuGet = "$PSScriptRoot\.nuget\NuGet.exe"
$CSProjPath = "$PSScriptRoot\src\LZStringCSharp.csproj"
$BuildPath = "$PSScriptRoot\src\bin\Release\net452"
$CoreBuildPath = "$PSScriptRoot\src\bin\Release\netstandard1.0"

# ==================================== Build

If(Test-Path -Path $BuildPath) {
	Remove-Item -Confirm:$false "$BuildPath\*.*" -Recurse
}

If(Test-Path -Path $CoreBuildPath) {
	Remove-Item -Confirm:$false "$CoreBuildPath\*.*" -Recurse
}

dotnet pack .\src\LZStringCSharp.csproj `
	-c Release

Write-Host "Output NuGet package can be found in .\src\bin\Release\LZStringCSharp.(version).nupkg"
