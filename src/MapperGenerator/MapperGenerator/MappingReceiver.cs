using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapperGenerator;

internal class MappingReceiver : ISyntaxReceiver
{
    public IList<(TypeSyntax, TypeSyntax)> Calls = 
        new List<(TypeSyntax, TypeSyntax)>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {

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
                            Identifier.Text: "SimpleMapper"
                        }
                        or
                        MemberAccessExpressionSyntax 
                        {
                            Name.Identifier.Text: "SimpleMapper"
                        }
                }
            })
        {
            Calls.Add((typeArgs[0], typeArgs[1]));
        }
    }
}
