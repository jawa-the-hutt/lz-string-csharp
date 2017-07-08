[CmdletBinding()]
Param(
	[String][Parameter(Mandatory=$True)]$Version
)

Set-StrictMode -version Latest
$ErrorActionPreference = "Stop"

Write-Host "Updating version number to $Version" -ForegroundColor Green

Function AssignVersionToFile {
	[CmdletBinding()]
	Param (
		[String][Parameter(Mandatory=$True)]$Path,
		[String][Parameter(Mandatory=$True)]$RegEx,
		[String][Parameter(Mandatory=$True)]$Replacement
	)
	
	(Get-Content $Path) -Replace $RegEx, $Replacement | Out-File $Path -Encoding UTF8
}

AssignVersionToFile -Path "$PSScriptRoot\src\LZStringCSharp.csproj" -RegEx "Version>\d+\.\d+\.\d+" -Replacement "Version>$($Version)</"
AssignVersionToFile -Path "$PSScriptRoot\package.json" -RegEx "`"version`": `"[^`"]+`""-Replacement "`"version`": `"$($Version)`""

Write-Host "Version updated!" -ForegroundColor Green
