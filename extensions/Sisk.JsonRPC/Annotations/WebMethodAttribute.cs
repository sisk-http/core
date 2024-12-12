namespace Sisk.JsonRPC.Annotations;

/// <summary>
/// Represents an JSON-RPC method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class WebMethodAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the method name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Creates an new <see cref="WebMethodAttribute"/> with no parameters.
    /// </summary>
    public WebMethodAttribute()
    {
    }

    /// <summary>
    /// Creates an new <see cref="WebMethodAttribute"/> with given
    /// parameters.
    /// </summary>
    /// <param name="methodName">The method name.</param>
    public WebMethodAttribute(string methodName)
    {
        Name = methodName;
    }
}
