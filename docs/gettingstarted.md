# Getting Started

## Generator Project

Create a new library project for the source generator using `netstandard2.0`, **not any other framework**. Not required, but recommended: set `LangVersion` to the latest version, and `ImplicitUsings` and `Nullable` to `enable`.

```xml
<PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <LangVersion>10.0</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

Add the following package references, ensuring the `PrivateAssets` attribute is set for both. The provided version are the recommended ones; at the time of writing `Microsoft.CodeAnalysis.CSharp 4.3.1` exists but cannot be used due to a bug.

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.2.0" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.3" PrivateAssets="all" />
</ItemGroup>
```

Create a class for your generator which implements the `Microsoft.CodeAnalysis.ISourceGenerator` interface and is decorated with the `Microsoft.CodeAnalysis.GeneratorAttribute`. Create a string containing the generated code in the `Execute` method and submit it like so.

```c#
context.AddSource($"FileName.cs", SourceText.From(generatedCodeString, Encoding.UTF8));
```

Also recommended, add the configuration described in [Debugging](./debugging.md).

## Syntax Receiver (optional)

A syntax receiver is the mechanism used to inspect and collect the syntax nodes assembled by the compiler for use in the source generator. To create a syntax receiver, create a new class and implement the `Microsoft.CodeAnalysis.ISyntaxReceiver` interface, and add a public variable to contain the syntax nodes selected by the receiver. In the `OnVisitSyntaxNode` method, inspect the `syntaxNode` provided to determine if it may be of interest to the generator, and if so, assign or add it to the public variable. For example, a syntax receiver that collects syntax nodes for any declaration of an enum:

```c#
public class AnyEnumReceiver : ISyntaxReceiver
{
    public IList<EnumDeclarationSyntax> EnumDeclarations = new List<EnumDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is EnumDeclarationSyntax enumDeclaration)
        {
            EnumDeclarations.Add(enumDeclaration);
        }
    }
}
```

To use a syntax receiver in a source generator, register it on the `context` with `RegisterForSyntaxNotifications`, and access it via `context.SyntaxReceiver` in the `Execute` method.

```c#
public void Initialize(GeneratorInitializationContext context)
{
    // ...
    context.RegisterForSyntaxNotifications(() => new AnyEnumReceiver());
}

public void Execute(GeneratorExecutionContext context)
{
    var syntaxReceiver = context.SyntaxReceiver as AnyEnumReceiver;
    // ...
}
```

## Entrypoint Project

Create a simple entry point project for testing and debugging the source generator during development. A command line application is the recommended option if the generator is not specific to another application type.

Add the following to to reference the generator project.

```xml
<ItemGroup>
    <ProjectReference Include="..\ExampleGenerator\ExampleGenerator.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```

## NuGet Package Generation

If the source generator library should be packaged into a NuGet package, add the following to a `PropertyGroup` element

```xml
<GeneratePackageOnBuild>True</GeneratePackageOnBuild>
<NoPackageAnalysis>true</NoPackageAnalysis>
<IncludeBuildOutput>false</IncludeBuildOutput>
```

and this item group, which ensures the package is properly formatted as an analyzer package.

```xml
<ItemGroup>
    <None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
</ItemGroup>
```

## Generator Usage

To use the generator as project reference, use the `ProjectReference` example used above in the entrypoint project. To use the generator NuGet package, add the package reference like normal.
