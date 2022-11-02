using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Collections.Immutable;

#if DEBUGGEN
using System.Diagnostics;
#endif

namespace MapperGenerator;

[Generator]
public class MapperGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUGGEN
        if (!Debugger.IsAttached) { Debugger.Launch(); }
#endif
        context.RegisterForSyntaxNotifications(() => new MappingReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxReciever = context.SyntaxReceiver as MappingReceiver;
        
        if (syntaxReciever is null)
            throw new Exception("Null syntax reciever");

        var builder = new StringBuilder();

        builder.AppendLine("namespace GeneratedNS");
        builder.AppendLine("{");
        builder.AppendLine("    public static class SimpleMapper");
        builder.AppendLine("    {");
        builder.AppendLine("        public static void Register<TIn, TOut>() where TOut : new() { }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        builder.AppendLine();

        if (syntaxReciever.Calls.Any())
        {
            IEnumerable<IGrouping<string, MapDetails>> groupedDetails = GetMapDetails(context, syntaxReciever.Calls).GroupBy(dets => dets.InTypeNamespace);

            foreach (IGrouping<string, MapDetails> detailsByNamespace in groupedDetails)
            {
                builder.AppendLine($"namespace {detailsByNamespace.Key}");
                builder.AppendLine($"{{");

                string extensionName = detailsByNamespace.Key.Replace(".", "_");

                builder.AppendLine($"    public static class MappingExtensions_{extensionName}");
                builder.AppendLine($"    {{");

                foreach (MapDetails details in detailsByNamespace)
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

                builder.AppendLine($"    }}");
                builder.AppendLine();

                builder.AppendLine($"}}");
                builder.AppendLine();
            }
        }

        string generatedText = builder.ToString();

        SourceText sourceText = SourceText.From(generatedText, Encoding.UTF8);
        context.AddSource($"GeneratedClass.cs", sourceText);
    }

    private IEnumerable<MapDetails> GetMapDetails(GeneratorExecutionContext context, IEnumerable<(TypeSyntax InTypeSyntax, TypeSyntax OutTypeSyntax)> mappedTypeSyntax)
    {
        foreach ((TypeSyntax inTypeSyntax, TypeSyntax outTypeSyntax) in mappedTypeSyntax)
        { 
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(inTypeSyntax.SyntaxTree);

            SymbolInfo inTypeSymbolInfo = semanticModel.GetSymbolInfo(inTypeSyntax);
            SymbolInfo outTypeSymbolInfo = semanticModel.GetSymbolInfo(outTypeSyntax);

            if (inTypeSymbolInfo.Symbol is null || inTypeSymbolInfo.Symbol is not INamedTypeSymbol inTypeSymbol)
                throw new Exception("Invalid in type");

            if (outTypeSymbolInfo.Symbol is null || outTypeSymbolInfo.Symbol is not INamedTypeSymbol outTypeSymbol)
                throw new Exception("Invalid out type");

            var mappableInProperties = inTypeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.GetMethod is not null);

            var mappableOutProperties = outTypeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.SetMethod is not null);

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

internal record MapDetails(string InTypeNamespace, string InTypeName, string OutTypeName, string OutTypeDisplay, (string InProp, string OutProp)[] MappableProperties);

