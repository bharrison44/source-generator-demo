using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;

namespace SimpleGenerator;

/// <summary>
/// A simple generator for generating some static code.
/// </summary>
[Generator]
internal class SimpleGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUGGEN
        if (!Debugger.IsAttached) { Debugger.Launch(); }
#endif
    }

    public void Execute(GeneratorExecutionContext context)
    {
        string generatedText = @"
namespace GeneratedNS
{
    public class GeneratedClass
    {
        public string? Name { get; set; }
    }
}
";

        SourceText sourceText = SourceText.From(generatedText, Encoding.UTF8);
        context.AddSource($"GeneratedClass.cs", sourceText);
    }
}