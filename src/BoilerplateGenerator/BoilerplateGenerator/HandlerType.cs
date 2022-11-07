using Microsoft.CodeAnalysis;

#if DEBUGGEN
using System.Diagnostics;
#endif

namespace BoilerplateGenerator;

public record HandlerType(string Label, string TypeName);


