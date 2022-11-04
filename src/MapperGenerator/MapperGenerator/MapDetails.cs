#if DEBUGGEN
using System.Diagnostics;
#endif

namespace MapperGenerator;

/// <summary>
/// Details of mapped types.
/// </summary>
/// <param name="InTypeNamespace">The in type namespace.</param>
/// <param name="InTypeName">The name of the in type.</param>
/// <param name="OutTypeName">The name of the out type.</param>
/// <param name="OutTypeDisplay">Display name for the out type.</param>
/// <param name="MappableProperties">The mappable properties.</param>
internal record MapDetails(string InTypeNamespace, string InTypeName, string OutTypeName, string OutTypeDisplay, (string InProp, string OutProp)[] MappableProperties);
