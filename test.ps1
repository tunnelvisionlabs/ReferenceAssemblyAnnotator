$ErrorActionPreference = 'Stop'

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$visualStudioInstallation = & $vswhere -latest -version [16,] -requires Microsoft.Component.MSBuild -products * -property installationPath
if (!$visualStudioInstallation) { throw 'Cannot find installation of Visual Studio 2019 or newer.' }
$msbuild = Join-Path $visualStudioInstallation 'MSBuild\Current\Bin\MSBuild.exe'

if (Test-Path 'tests\.isolatednugetcache') { Remove-Item -Recurse -Force 'tests\.isolatednugetcache' }

&$msbuild tests /restore /t:Rebuild /p:TreatWarningsAsErrors=true /v:Minimal
