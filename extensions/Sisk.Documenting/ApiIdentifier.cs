namespace Sisk.Documenting;

/// <summary>
/// Represents an identifier for an API, including application details such as name, version, and description.
/// </summary>
public sealed class ApiIdentifier {

    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the version of the application.
    /// </summary>
    public string? ApplicationVersion { get; set; }

    /// <summary>
    /// Gets or sets the description of the application.
    /// </summary>
    public string? ApplicationDescription { get; set; }
}
