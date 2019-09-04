# Reference Assembly Annotator

IL weaver for adding nullability annotations to .NET Framework and .NET Standard reference assemblies.

[![Build status](https://ci.appveyor.com/api/projects/status/pikrerggo7mi7dy5/branch/master?svg=true)](https://ci.appveyor.com/project/sharwell/referenceassemblyannotator/branch/master)

[![codecov](https://codecov.io/gh/tunnelvisionlabs/ReferenceAssemblyAnnotator/branch/master/graph/badge.svg)](https://codecov.io/gh/tunnelvisionlabs/ReferenceAssemblyAnnotator)

[![Join the chat at https://gitter.im/tunnelvisionlabs/ReferenceAssemblyAnnotator](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/tunnelvisionlabs/ReferenceAssemblyAnnotator?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Requirements

* Build: C# compiler version 3.2 Beta 2 or newer (ships with Visual Studio 2019 version 16.3 Preview 2 and newer). Older
  versions of the compiler interpret nullable metadata differently, which can lead to incorrect build warnings.
* Editing experience: Visual Studio 2019 version 16.3 Preview 2 or newer.

## Usage

### Assemblies included by default

* .NET Framework targets
    * mscorlib
* .NET Standard targets
    * All assemblies which are defined by the .NET Standard

### Example configuration

```xml
<PropertyGroup>
  <!-- Specifies the version of this rewriter to use -->
  <TunnelVisionLabsReferenceAssemblyAnnotatorVersion>1.0.0-alpha.31</TunnelVisionLabsReferenceAssemblyAnnotatorVersion>
  <!-- Specifies the version of Microsoft.NETCore.App.Ref to obtain nullability information from -->
  <AnnotatedReferenceAssemblyVersion>3.0.0-preview8-28405-07</AnnotatedReferenceAssemblyVersion>
  <!-- Includes the nullable attributes from dotnet/coreclr as source code with 'internal' accessibility. Set this to
       false if the attributes are included from another source and/or are not needed. -->
  <GenerateNullableAttributes>true</GenerateNullableAttributes>
</PropertyGroup>

<ItemGroup>
  <!-- Include the necessary packages -->
  <PackageReference Include="TunnelVisionLabs.ReferenceAssemblyAnnotator" Version="$(TunnelVisionLabsReferenceAssemblyAnnotatorVersion)" PrivateAssets="all" />
  <PackageDownload Include="Microsoft.NETCore.App.Ref" Version="[$(AnnotatedReferenceAssemblyVersion)]" />
</ItemGroup>

<ItemGroup>
  <!-- Specifies the reference assemblies to rewrite -->
  <UnannotatedReferenceAssembly Include="System" />
  <UnannotatedReferenceAssembly Include="System.Core" />
</ItemGroup>
```

## Releases

[![NuGet](https://img.shields.io/nuget/v/TunnelVisionLabs.ReferenceAssemblyAnnotator.svg)](https://www.nuget.org/packages/TunnelVisionLabs.ReferenceAssemblyAnnotator) [![NuGet Beta](https://img.shields.io/nuget/vpre/TunnelVisionLabs.ReferenceAssemblyAnnotator.svg)](https://www.nuget.org/packages/TunnelVisionLabs.ReferenceAssemblyAnnotator/absoluteLatest)

* [Binaries (NuGet)](https://www.nuget.org/packages/TunnelVisionLabs.ReferenceAssemblyAnnotator)
* [Release Notes](https://github.com/tunnelvisionlabs/ReferenceAssemblyAnnotator/releases)
* [License](https://github.com/tunnelvisionlabs/ReferenceAssemblyAnnotator/blob/master/LICENSE)
