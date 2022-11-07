using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if DEBUGGEN
using System.Diagnostics;
#endif

namespace BoilerplateGenerator;

public class HandlerClassRetciever : ISyntaxReceiver
{
    public IList<ClassDeclarationSyntax> HandlerClasses = new List<ClassDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax
            {
                AttributeLists: { Count: > 0 } attrLists,
                Modifiers: { } mods,
            } classDec)
        {
            bool hasHandlerAttr = attrLists.SelectMany(list => list.Attributes).Where(attr => attr.Name.ToString() == "Handler").Any();
            bool isPartial = mods.Any(mod => mod.Text == "partial");

            if (hasHandlerAttr && isPartial)
            {
                HandlerClasses.Add(classDec);
            }
        }
    }
}


