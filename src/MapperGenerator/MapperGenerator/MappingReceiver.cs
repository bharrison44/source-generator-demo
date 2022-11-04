using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapperGenerator;

/// <summary>
/// Syntax reciver for collection register types used in calls to Mapper.Register{TIn, TOut}
/// </summary>
internal class MappingReceiver : ISyntaxReceiver
{
    /// <summary>
    /// The types register by calls to Mapper.Register{TIn, TOut}
    /// </summary>
    public ISet<(TypeSyntax, TypeSyntax)> Registrations = new HashSet<(TypeSyntax, TypeSyntax)>();

    /// <inheritdoc />
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // Pattern matching on parsed syntax, looking for calls to Mapper.Register{TIn, TOut} or MapperGenerator.Mapper.Register{TIn, TOut}
        // No expensive work should be done in this method as it will run over every single node in the syntax tree. Simply collect the required
        // information and perform any more indepth operations in the generator.
        if (syntaxNode is InvocationExpressionSyntax 
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Name: GenericNameSyntax
                    {
                        Identifier.Text: "Register",
                        TypeArgumentList.Arguments:
                        {
                            Count: 2
                        } typeArgs
                    },
                    Expression: 
                        IdentifierNameSyntax 
                        {
                            Identifier.Text: "Mappings"
                        }
                        or
                        MemberAccessExpressionSyntax 
                        {
                            Name.Identifier.Text: "Mappings",
                            Expression: IdentifierNameSyntax
                            {
                                Identifier.Text: "MapperGenerator"
                            }
                        }
                }
            })
        {
            (TypeSyntax, TypeSyntax) types = (typeArgs[0], typeArgs[1]);

            if (!Registrations.Contains(types))
                Registrations.Add(types);
        }
    }
}
