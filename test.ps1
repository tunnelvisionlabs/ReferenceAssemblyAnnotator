param (
	[switch]$Clean
)

$ErrorActionPreference = 'Stop'

if ($Clean) {
	git 'clean' '-dxf' 'tests'
}

dotnet msbuild -restore tests /t:Rebuild /p:TreatWarningsAsErrors=true /v:Minimal
