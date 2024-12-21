namespace Sisk.Helpers.Swagger;

[AttributeUsage ( AttributeTargets.Method, AllowMultiple = false )]
public sealed class ApiDefinitionAttribute : Attribute {
    public string? Description { get; set; }

    public ApiDefinitionAttribute () {
    }
}
