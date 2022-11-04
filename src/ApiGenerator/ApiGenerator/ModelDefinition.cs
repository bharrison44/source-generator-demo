#nullable disable

namespace ApiGenerator;

/// <summary>
/// JSON model for a defined model.
/// </summary>
public class ModelDefinition
{
    /// <summary>
    /// The name of the model.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The props the model has.
    /// </summary>
    public PropDefinition[] Props { get; set; }
}