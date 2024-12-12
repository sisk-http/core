namespace Sisk.JsonRPC.Annotations;

/// <summary>
/// Represents an attribute which holds the class name for a group of
/// JSON-RPC methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class WebNameAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name associated with the method group.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Creates an new instance of the <see cref="WebMethodAttribute"/> attribute.
    /// </summary>
    /// <param name="name">The method-group name.</param>
    public WebNameAttribute(string name)
    {
        Name = name;
    }
}
