# NuGet Package Dependencies

If a source generator has a NuGet package dependency, it cannot be simply added like it normally would.

## Referencing the Package

The source generator project should reference the package as normal, but the `PrivateAssets` and `GeneratePathProperty` attributes must be added like so.

```xml
<PackageReference Include="Example.Package" Version="1.0.0" PrivateAssets="all" GeneratePathProperty="true" />
```

## When Referenced by Project

When the source generator is used as a project reference, these elements must be added.

```xml
<PropertyGroup>
    <GetTargetPathDependsOn>$(GetTargetPathDependsOn);GetDependencyTargetPaths</GetTargetPathDependsOn>
</PropertyGroup>

<Target Name="GetDependencyTargetPaths">
    <ItemGroup>
        
    </ItemGroup>
</Target>
```

Then, for each dependant package, add this element to the `GetDependencyTargetPaths` `ItemGroup`. The path property variable used in the `Include` attribute is based on the name of the package; `PKG` followed by the package name with periods replaced with underscores. The correct path to the dll can be determined by opening the package in [Nuget Package Explorer]

```xml
<TargetPathWithTargetPlatformMoniker Include="$(PKGExample_Package)\path\to\Example.Package.dll" IncludeRuntimeDependency="false" />
```

## When Referenced as a Package

Source generators NuGet packages must include any dependencies. This can be done by adding the following for any dependencies. The path property variable used in the `Include` attribute is based on the name of the package; `PKG` followed by the package name with periods replaced with underscores. The correct path to the dll can be determined by opening the package in [Nuget Package Explorer]

```xml
<ItemGroup>
    <None Include="$(PKGExample_Package)\path\to\Example.Package.dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
</ItemGroup>
```

[Nuget Package Explorer]: https://apps.microsoft.com/store/detail/nuget-package-explorer/9WZDNCRDMDM3?hl=en-au&gl=au