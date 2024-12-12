namespace Sisk.JsonRPC.Annotations;

/// <summary>
/// Specifies a description for a method parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ParamDescriptionAttribute : Attribute
{
    /// <summary>
    /// Gets the description of the method parameter.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the target parameter name.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParamDescriptionAttribute"/> class with the specified description.
    /// </summary>
    /// <param name="paramName">The parameter name.</param>
    /// <param name="description">The description of the method parameter.</param>
    public ParamDescriptionAttribute(string paramName, string description)
    {
        ParameterName = paramName;
        Description = description;
    }
}
