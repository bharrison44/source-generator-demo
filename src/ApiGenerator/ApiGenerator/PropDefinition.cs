#nullable disable

namespace ApiGenerator;

/// <summary>
/// JSON model for a property denfifion.
/// </summary>
public class PropDefinition
{
    /// <summary>
    /// The name of the property.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type of the property.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// If the property is key.
    /// </summary>
    public bool Key { get; set; }
}