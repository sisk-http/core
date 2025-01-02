namespace Sisk.Documenting;

/// <summary>
/// Defines a contract for exporting API documentation content.
/// </summary>
public interface IApiDocumentationExporter {

    /// <summary>
    /// Exports the specified API documentation content.
    /// </summary>
    /// <param name="documentation">The API documentation to export.</param>
    /// <returns>An <see cref="HttpContent"/> representing the exported documentation.</returns>
    public HttpContent ExportDocumentationContent ( ApiDocumentation documentation );
}