using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

#if DEBUGGEN
using System.Diagnostics;
#endif

namespace SimpleGenerator;

/// <summary>
/// A simple generator for generating some static code.
/// </summary>
[Generator]
internal class SimpleGenerator : ISourceGenerator
{
    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUGGEN
        // Attach debugger when using dedicated debugging configuration. Should not be used within VS.
        if (!Debugger.IsAttached) { Debugger.Launch(); }
#endif
    }

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        // Generate code as a string.
        string generatedText = @"
namespace GeneratedNS
{
    public class GeneratedClass
    {
        public string? Name { get; set; }
    }
}
";
        
        // Save it under a file name.
        context.AddSource($"GeneratedClass.cs", SourceText.From(generatedText, Encoding.UTF8));
    }
}