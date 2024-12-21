using System.Net;

namespace Sisk.Helpers.Swagger;

[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public sealed class ApiResponseAttribute : Attribute {
    public HttpStatusCode StatusCode { get; set; }
    public string? Description { get; set; }
    public string? ContentExample { get; set; }
    public string? ContentType { get; set; }
}
