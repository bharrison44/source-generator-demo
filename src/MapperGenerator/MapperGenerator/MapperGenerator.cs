using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

#if DEBUGGEN
using System.Diagnostics;
#endif

namespace MapperGenerator;

/// <summary>
/// A source generator which generates extention methods for mapping one data object to another.
/// </summary>
[Generator]
public class MapperGenerator : ISourceGenerator
{
    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUGGEN
        // Attach debugger when using dedicated debugging configuration. Should not be used within VS.
        if (!Debugger.IsAttached) { Debugger.Launch(); }
#endif

        context.RegisterForSyntaxNotifications(() => new MappingReceiver());
    }

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReciever = context.SyntaxReceiver as MappingReceiver;
        
        if (syntaxReciever is null)
            throw new Exception("Null syntax reciever");

        var builder = new StringBuilder();

        // Generate the mappings registration function.
        builder.AppendLine("namespace MapperGenerator");
        builder.AppendLine("{");
        builder.AppendLine("    public static class Mappings");
        builder.AppendLine("    {");
        builder.AppendLine("        public static void Register<TIn, TOut>() where TOut : new() { }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine();

        if (syntaxReciever.Registrations.Any())
        {
            // Collect details needed from calls to Mappings.Register
            IEnumerable<IGrouping<string, MapDetails>> groupedDetails = GetMapDetails(context, syntaxReciever.Registrations).GroupBy(dets => dets.InTypeNamespace);

            foreach (IGrouping<string, MapDetails> detailsByNamespace in groupedDetails)
            {
                builder.AppendLine($"namespace {detailsByNamespace.Key}");
                builder.AppendLine($"{{");

                string extensionName = detailsByNamespace.Key.Replace(".", "_");

                builder.AppendLine($"    public static class MappingExtensions_{extensionName}");
                builder.AppendLine($"    {{");

                foreach (MapDetails details in detailsByNamespace)
                {
                    // Generate mapping extension methods in same namespace as the type being extended.
                    GenerateMappingMethod(builder, details);
                }

                builder.AppendLine($"    }}");
                builder.AppendLine();

                builder.AppendLine($"}}");
                builder.AppendLine();
            }
        }

        string generatedText = builder.ToString();

        context.AddSource($"GeneratedClass.cs", SourceText.From(generatedText, Encoding.UTF8));
    }

    private void GenerateMappingMethod(StringBuilder builder, MapDetails details)
    {
        builder.AppendLine($"        public static {details.OutTypeName} MapTo{details.OutTypeDisplay}(this {details.InTypeName} value, System.Action<{details.InTypeName}, {details.OutTypeName}>? additionalMappings = null)");
        builder.AppendLine($"        {{");
        builder.AppendLine($"           {details.OutTypeName} mapped = new {details.OutTypeName}();");

        foreach ((string inMember, string outMember) in details.MappableProperties)
        {
            builder.AppendLine($"           mapped.{outMember} = value.{inMember};");
        }

        builder.AppendLine($"           additionalMappings?.Invoke(value, mapped);");

        builder.AppendLine($"           return mapped;");
        builder.AppendLine($"        }}");
        builder.AppendLine();
    }

    private IEnumerable<MapDetails> GetMapDetails(GeneratorExecutionContext context, IEnumerable<(TypeSyntax InTypeSyntax, TypeSyntax OutTypeSyntax)> mappedTypeSyntax)
    {
        SemanticModel? semanticModel = null;

        foreach ((TypeSyntax inTypeSyntax, TypeSyntax outTypeSyntax) in mappedTypeSyntax)
        { 
            // Semantic model only needs to be retrieved once. All syntax objects have the same syntax tree.
            semanticModel ??= context.Compilation.GetSemanticModel(inTypeSyntax.SyntaxTree);

            // Use the in and out type syntax to get symbol information.
            SymbolInfo inTypeSymbolInfo = semanticModel.GetSymbolInfo(inTypeSyntax);
            SymbolInfo outTypeSymbolInfo = semanticModel.GetSymbolInfo(outTypeSyntax);

            if (inTypeSymbolInfo.Symbol is null || inTypeSymbolInfo.Symbol is not INamedTypeSymbol inTypeSymbol)
                throw new Exception("Invalid in type");

            if (outTypeSymbolInfo.Symbol is null || outTypeSymbolInfo.Symbol is not INamedTypeSymbol outTypeSymbol)
                throw new Exception("Invalid out type");

            // Member properties of the source type.
            var mappableInProperties = inTypeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetMethod is not null);

            // Member properties of the target type.
            var mappableOutProperties = outTypeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.SetMethod is not null);

            // The member properties which can be mapped based on their name. In this implementation,
            // non-matching properties would have to be mapped manually using the optional delegate parameter.
            (string, string)[] mappableProperties = mappableInProperties
                .Join(
                    mappableOutProperties,
                    inProp => inProp.Name,
                    outProp => outProp.Name,
                    (inProp, outProp) => (inProp.Name, outProp.Name))
                .ToArray();

            yield return new MapDetails(
                inTypeSymbol.ContainingNamespace.ToString(),
                inTypeSymbol.ToString(),
                outTypeSymbol.ToString(),
                outTypeSymbol.Name,
                mappableProperties);
        }
    }
}
