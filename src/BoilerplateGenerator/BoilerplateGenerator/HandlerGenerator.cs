using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

#if DEBUGGEN
using System.Diagnostics;
#endif

namespace BoilerplateGenerator;

/// <summary>
/// Generator which extends a partial class with boilerplate to handle any number of parameters.
/// </summary>
[Generator]
public class HandlerGenerator : ISourceGenerator
{
    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUGGEN
        if (!Debugger.IsAttached) { Debugger.Launch(); }
#endif

        context.RegisterForSyntaxNotifications(() => new HandlerClassReceiver());
    }

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReceiver = context.SyntaxReceiver as HandlerClassReceiver;

        if (syntaxReceiver is null)
            throw new Exception("Null syntax receiver");

        SemanticModel? semanticModel = null;

        foreach (ClassDeclarationSyntax handlerClassDeclaration in syntaxReceiver.HandlerClasses)
        {
            semanticModel ??= context.Compilation.GetSemanticModel(handlerClassDeclaration.SyntaxTree);

            HandlerType[] handlerTypes = handlerClassDeclaration.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Where(prop => prop.Modifiers.Any(mod => mod.Text == "protected"))
                .Select(prop => ToHandlerType(prop, semanticModel))
                .OfType<HandlerType>()
                .ToArray();

            if (!handlerTypes.Any())
                break;

            GenerateHandler(handlerClassDeclaration, handlerTypes, semanticModel, context);
        }
    }

    private HandlerType? ToHandlerType(PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel semanticModel)
    {
        TypeInfo typeInfo = semanticModel.GetTypeInfo(propertyDeclarationSyntax.Type);
        IPropertySymbol? propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) as IPropertySymbol;

        if (typeInfo.Type is null || propertySymbol is null)
            return null;

        if (propertySymbol.DeclaredAccessibility != Accessibility.Protected)
            return null;

        // checks that property is auto-property
        if (!propertySymbol.ContainingType.GetMembers().OfType<IFieldSymbol>().Any(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, propertySymbol)))
            return null;

        return new HandlerType(
            propertyDeclarationSyntax.Identifier.Text,
            typeInfo.Type.ToString());
    }

    private void GenerateHandler(ClassDeclarationSyntax classDeclaration, HandlerType[] handlerTypes, SemanticModel semanticModel, GeneratorExecutionContext context)
    {
        string className = classDeclaration.Identifier.Text;
        string @namespace = semanticModel.GetDeclaredSymbol(classDeclaration)!.ContainingNamespace.ToString();

        var builder = new StringBuilder();

        builder.AppendLine($"namespace {@namespace}");
        builder.AppendLine($"{{");

        GenerateBaseClass(builder, className, handlerTypes);
        GenerateBoilerplateClass(builder, className, handlerTypes);
        
        builder.AppendLine($"}}");

        string generatedText = builder.ToString();
        context.AddSource($"{className}Handler.generated.cs", SourceText.From(generatedText, Encoding.UTF8));
    }

    private void GenerateBaseClass(StringBuilder builder, string className, HandlerType[] handlerTypes)
    {
        builder.AppendLine($"   public abstract class {className}Base");
        builder.AppendLine($"   {{");

        foreach (HandlerType handlerType in handlerTypes)
        {
            builder.AppendLine($"       protected abstract string Handle{handlerType.Label}({handlerType.TypeName} value);");
            builder.AppendLine();
        }

        builder.AppendLine($"   }}");
        builder.AppendLine();
    }

    private void GenerateBoilerplateClass(StringBuilder builder, string className, HandlerType[] handlerTypes)
    {
        builder.AppendLine($"   public partial class {className} : {className}Base");
        builder.AppendLine($"   {{");

        // Constructor
        var constructorArgs = handlerTypes.Select(handlerType => $"{handlerType.TypeName} value{handlerType.Label}");
        var constructorArgString = string.Join(", ", constructorArgs);

        builder.AppendLine($"       public {className}({constructorArgString})");
        builder.AppendLine($"       {{");

        foreach (HandlerType handlerType in handlerTypes)
        {
            builder.AppendLine($"           {handlerType.Label} = value{handlerType.Label};");
        }

            
        builder.AppendLine($"       }}");
        builder.AppendLine();

        builder.AppendLine($"       public string Handle()");
        builder.AppendLine($"       {{");

        foreach (HandlerType handlerType in handlerTypes)
        {
            builder.AppendLine($"           string handled{handlerType.Label} = Handle{handlerType.Label}({handlerType.Label});");
        }

        var handledOutputSegment = handlerTypes.Select(handlerType => $"{{handled{handlerType.Label}}};");
        var handledOutput = string.Join(" ", handledOutputSegment);

        builder.AppendLine($"           return $\"{handledOutput}\";");
        builder.AppendLine($"       }}");
        builder.AppendLine();

        builder.AppendLine($"   }}");
        builder.AppendLine();
    }
}


