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
    * System
    * System.Core
    * System.Data
    * System.Drawing
    * System.IO.Compression.FileSystem
    * System.Numerics
    * System.Runtime.Serialization
    * System.Xml
    * System.Xml.Linq
* .NET Standard targets
    * All assemblies which are defined by the .NET Standard

### Example configuration

```xml
<PropertyGroup>
  <!-- Specifies the version of this rewriter to use. -->
  <TunnelVisionLabsReferenceAssemblyAnnotatorVersion>1.0.0-alpha.77</TunnelVisionLabsReferenceAssemblyAnnotatorVersion>

  <!-- Specifies the version of Microsoft.NETCore.App.Ref to obtain nullability information from. -->
  <AnnotatedReferenceAssemblyVersion>3.0.0</AnnotatedReferenceAssemblyVersion>

  <!-- Includes the nullable attributes from dotnet/coreclr as source code with 'internal' accessibility. Set this to
       false if the attributes are included from another source and/or are not needed. -->
  <GenerateNullableAttributes>true</GenerateNullableAttributes>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="TunnelVisionLabs.ReferenceAssemblyAnnotator" Version="$(TunnelVisionLabsReferenceAssemblyAnnotatorVersion)" PrivateAssets="all" />
  <PackageDownload Include="Microsoft.NETCore.App.Ref" Version="[$(AnnotatedReferenceAssemblyVersion)]" />
</ItemGroup>
```

#### Minimal:

```xml
<PropertyGroup>
  <AnnotatedReferenceAssemblyVersion>3.0.0</AnnotatedReferenceAssemblyVersion>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="TunnelVisionLabs.ReferenceAssemblyAnnotator" Version="1.0.0-alpha.77" PrivateAssets="all" />
  <PackageDownload Include="Microsoft.NETCore.App.Ref" Version="[$(AnnotatedReferenceAssemblyVersion)]" />
</ItemGroup>
```

#### When multi targeting

```xml
<PropertyGroup Condition="'$(TargetFramework)' != 'netcoreapp3.0'">
  <AnnotatedReferenceAssemblyVersion>3.0.0</AnnotatedReferenceAssemblyVersion>
  <GenerateNullableAttributes>true</GenerateNullableAttributes>
</PropertyGroup>
<ItemGroup>
  <PackageDownload Condition="'$(TargetFramework)' != 'netcoreapp3.0'" Include="Microsoft.NETCore.App.Ref" Version="[$(AnnotatedReferenceAssemblyVersion)]" />
  <PackageReference Include="TunnelVisionLabs.ReferenceAssemblyAnnotator" Version="1.0.0-alpha.77" PrivateAssets="all" />
</ItemGroup>
```

### Configuration reference

* MSBuild properties
    * `<AnnotatedReferenceAssemblyVersion>`: Specifies the version of Microsoft.NETCore.App.Ref to obtain nullability information from. There is no default value, so this must be specified by the user.
    * `<GenerateNullableAttributes>`: Set to `True` to include definitions of nullability attributes in the build; otherwise, `False` to exclude the definitions. The default value is `True`.
* MSBuild items
    * `<UnannotatedReferenceAssembly>`: Specifies reference assemblies to annotate. This is only required for assemblies that are not automatically annotated by this package.

## Releases

[![NuGet](https://img.shields.io/nuget/v/TunnelVisionLabs.ReferenceAssemblyAnnotator.svg)](https://www.nuget.org/packages/TunnelVisionLabs.ReferenceAssemblyAnnotator) [![NuGet Beta](https://img.shields.io/nuget/vpre/TunnelVisionLabs.ReferenceAssemblyAnnotator.svg)](https://www.nuget.org/packages/TunnelVisionLabs.ReferenceAssemblyAnnotator/absoluteLatest)

* [Binaries (NuGet)](https://www.nuget.org/packages/TunnelVisionLabs.ReferenceAssemblyAnnotator)
* [Release Notes](https://github.com/tunnelvisionlabs/ReferenceAssemblyAnnotator/releases)
* [License](https://github.com/tunnelvisionlabs/ReferenceAssemblyAnnotator/blob/master/LICENSE)
