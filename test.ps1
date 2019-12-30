$ErrorActionPreference = 'Stop'

if (Test-Path 'tests\.isolatednugetcache') { Remove-Item -Recurse -Force 'tests\.isolatednugetcache' }

dotnet msbuild -restore tests /t:Rebuild /p:TreatWarningsAsErrors=true /v:Minimal
