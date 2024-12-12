namespace Sisk.JsonRPC.Annotations;

/// <summary>
/// Specifies a description for a method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class MethodDescriptionAttribute : Attribute
{
    /// <summary>
    /// Gets the description of the method.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodDescriptionAttribute"/> class with the specified description.
    /// </summary>
    /// <param name="description">The description of the method.</param>
    public MethodDescriptionAttribute(string description)
    {
        Description = description;
    }
}
