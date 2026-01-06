// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonContentTypeHandler.cs
// Repository:  https://github.com/sisk-http/core


using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using LightJson;
using LightJson.Schema;

namespace Sisk.Documenting;

/// <summary>
/// Provides JSON example generation for types used in documentation.
/// </summary>
public class JsonContentTypeHandler : IExampleBodyTypeHandler, IExampleParameterTypeHandler, IContentSchemaTypeHandler {

#pragma warning disable IL3050, IL2026
    static Lazy<JsonContentTypeHandler> shared = new Lazy<JsonContentTypeHandler> ( () => new JsonContentTypeHandler () );
#pragma warning restore IL3050, IL2026

    /// <summary>
    /// Gets the shared singleton instance of <see cref="JsonContentTypeHandler"/>.
    /// </summary>
    public static JsonContentTypeHandler Shared {
        [RequiresUnreferencedCode ( "This property uses the default JsonOptions and JsonTypeInfo, which requires dynamic code to run." )]
        get {
            return shared.Value;
        }
    }

    /// <summary>
    /// Gets or sets the number of items to include when generating examples for enumerable types.
    /// </summary>
    public int EnumerationExampleCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether to include XML documentation comments in the generated examples.
    /// </summary>
    public bool IncludeDescriptionAnnotations { get; set; } = true;

    private IJsonTypeInfoResolver _typeResolver;
    private JsonSerializerOptions _serializerOptions;
    private JsonOptions _jsoptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContentTypeHandler"/> class with the specified type resolver and serializer options.
    /// </summary>
    /// <param name="typeResolver">The JSON type info resolver.</param>
    /// <param name="serializerOptions">The JSON serializer options.</param>
    public JsonContentTypeHandler ( IJsonTypeInfoResolver typeResolver, JsonSerializerOptions serializerOptions ) {
        _typeResolver = typeResolver;
        _serializerOptions = serializerOptions;
        _jsoptions = new JsonOptions () {
            SerializerContext = serializerOptions
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContentTypeHandler"/> class with the specified serializer options and a default type resolver.
    /// </summary>
    /// <param name="serializerOptions">The JSON serializer options.</param>
    [RequiresDynamicCode ( "This method calls the JsonSerializerOptions.Default, which requires dynamic code." )]
    [RequiresUnreferencedCode ( "This method calls the JsonSerializerOptions.Default, which requires unreferenced code." )]
    public JsonContentTypeHandler ( JsonSerializerOptions serializerOptions ) {
        _typeResolver = new DefaultJsonTypeInfoResolver ();
        _serializerOptions = serializerOptions;
        _jsoptions = new JsonOptions () {
            SerializerContext = serializerOptions
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContentTypeHandler"/> class with default serializer options and type resolver.
    /// </summary>
    [RequiresDynamicCode ( "This method calls the JsonSerializerOptions.Default, which requires dynamic code." )]
    [RequiresUnreferencedCode ( "This method calls the JsonSerializerOptions.Default, which requires unreferenced code." )]
    public JsonContentTypeHandler () {
        _typeResolver = new DefaultJsonTypeInfoResolver ();
        _serializerOptions = JsonSerializerOptions.Default;
        _jsoptions = JsonOptions.Default;
    }

    string ConvertCase ( string name ) => (_jsoptions.NamingPolicy ?? _serializerOptions.PropertyNamingPolicy)?.ConvertName ( name ) ?? name;

    /// <summary>
    /// Generates a JSON body example for the specified type.
    /// </summary>
    /// <param name="type">The type to generate an example for.</param>
    /// <returns>A <see cref="BodyExampleResult"/> containing the JSON example, or <see langword="null"/> if the type cannot be handled.</returns>
    public virtual BodyExampleResult? GetBodyExampleForType ( Type type ) {
        StringBuilder sb = new StringBuilder ();
        int indentLevel = 0;

        void AppendComment ( string? text ) {
            if (text is null || !IncludeDescriptionAnnotations)
                return;

            var lines = text.Split ( '\n' );
            for (int i = 0; i < lines.Length; i++) {
                var line = lines [ i ];
                AppendLine ( $"// {line.Trim ()}" );
                if (i != lines.Length - 1) {
                    AppendIndent ();
                }
            }
        }

        void AppendLine ( string? text = null ) {
            sb.AppendLine ( text );
        }

        void AppendIndent () {
            sb.Append ( new string ( ' ', indentLevel * 4 ) );
        }

        void AppendText ( string text ) {
            sb.Append ( text );
        }

        void AppendSchema ( JsonValue schemaValue, bool isRequired = false, bool isNullable = false ) {
            if (indentLevel > 128)
                return;

            if (!schemaValue.IsJsonObject) {
                AppendText ( "any" );
                return;
            }

            var schema = schemaValue.GetJsonObject ();
            var schemaType = schema [ "type" ];

            string? typeStr = null;
            bool isNullableType = false;

            if (schemaType.IsString) {
                typeStr = schemaType.GetString ();
            }
            else if (schemaType.IsJsonArray) {
                var types = schemaType.GetJsonArray ();
                var nonNullTypes = types.Where ( t => t.GetString () != "null" ).ToArray ();
                isNullableType = types.Any ( t => t.GetString () == "null" );
                if (nonNullTypes.Length > 0) {
                    typeStr = nonNullTypes [ 0 ].GetString ();
                }
            }

            if (typeStr is null) {
                AppendText ( "any" );
                return;
            }

            switch (typeStr) {
                case "object":
                    AppendObjectSchema ( schema );
                    break;

                case "array":
                    AppendArraySchema ( schema );
                    break;

                case "string":
                    AppendStringExample ( schema );
                    break;

                case "number":
                case "integer":
                    AppendText ( "number" );
                    break;

                case "boolean":
                    AppendText ( "boolean" );
                    break;

                default:
                    AppendText ( typeStr );
                    break;
            }

            if (isNullableType || isNullable) {
                AppendText ( "?" );
            }
        }

        void AppendStringExample ( JsonObject schema ) {
            var enumValues = schema [ "enum" ];
            if (enumValues.IsJsonArray) {
                var arr = enumValues.GetJsonArray ();
                if (arr.Count > 0) {
                    AppendText ( $"\"{arr [ 0 ].GetString ()}\"" );
                    return;
                }
            }

            var format = schema [ "format" ];
            if (format.IsString) {
                AppendText ( $"\"{format.GetString ()}\"" );
                return;
            }

            AppendText ( "string" );
        }

        void AppendArraySchema ( JsonObject schema ) {
            AppendLine ( "[" );
            indentLevel++;

            var itemsSchema = schema [ "items" ];
            if (itemsSchema.IsJsonObject) {
                for (int i = 0; i < EnumerationExampleCount; i++) {
                    AppendIndent ();
                    AppendSchema ( itemsSchema );
                    AppendLine ( "," );
                }
            }

            AppendIndent ();
            AppendLine ( "..." );
            indentLevel--;
            AppendIndent ();
            AppendText ( "]" );
        }

        void AppendObjectSchema ( JsonObject schema ) {
            var properties = schema [ "properties" ];
            if (!properties.IsJsonObject) {
                AppendText ( "{}" );
                return;
            }

            var propsObj = properties.GetJsonObject ();
            if (propsObj.Count == 0) {
                AppendText ( "{}" );
                return;
            }

            var requiredProps = new HashSet<string> ();
            var requiredArray = schema [ "required" ];
            if (requiredArray.IsJsonArray) {
                foreach (var req in requiredArray.GetJsonArray ()) {
                    if (req.IsString) {
                        requiredProps.Add ( req.GetString () );
                    }
                }
            }

            AppendLine ( "{" );
            indentLevel++;

            bool lastItemAppendedDocs = false;
            int propIndex = 0;
            int propCount = propsObj.Count;

            foreach (var prop in propsObj) {
                if (lastItemAppendedDocs) {
                    AppendLine ();
                    lastItemAppendedDocs = false;
                }

                var propSchema = prop.Value.GetJsonObject ();
                var description = propSchema [ "description" ];
                bool isRequired = requiredProps.Contains ( prop.Key );

                var propType = propSchema [ "type" ];
                bool isNullable = propType.IsJsonArray && propType.GetJsonArray ().Any ( t => t.GetString () == "null" );

                if (description.IsString && IncludeDescriptionAnnotations) {
                    AppendIndent ();

                    List<string> docParts = new List<string> ();
                    if (isRequired) {
                        docParts.Add ( "Required" );
                    }
                    if (isNullable) {
                        docParts.Add ( "Nullable" );
                    }

                    if (docParts.Count > 0) {
                        AppendComment ( $"{string.Join ( ". ", docParts )}. {description.GetString ()}" );
                    }
                    else {
                        AppendComment ( description.GetString () );
                    }
                    lastItemAppendedDocs = true;
                }

                AppendIndent ();
                AppendText ( $"\"{prop.Key}\": " );
                AppendSchema ( prop.Value, isRequired, isNullable );

                if (propIndex < propCount - 1) {
                    AppendText ( "," );
                }

                AppendLine ();
                propIndex++;
            }

            indentLevel--;
            AppendIndent ();
            AppendText ( "}" );
        }

        var jsonSchema = JsonSchema.CreateFromType ( type, _jsoptions );
        AppendSchema ( jsonSchema.AsJsonValue () );

        string result = sb.ToString ();
        return new BodyExampleResult ( result, "json" );
    }

    /// <summary>
    /// Generates parameter examples for the specified type.
    /// </summary>
    /// <param name="type">The type to generate parameter examples for.</param>
    /// <returns>An array of <see cref="ParameterExampleResult"/> representing the parameters.</returns>
    public ParameterExampleResult [] GetParameterExamplesForType ( Type type ) {

        List<ParameterExampleResult> parameters = new List<ParameterExampleResult> ();

        void AppendSchema ( JsonValue schemaValue, string [] pathParts ) {
            if (!schemaValue.IsJsonObject)
                return;

            var schema = schemaValue.GetJsonObject ();
            var schemaType = schema [ "type" ];

            string? typeStr = null;
            bool isNullable = false;

            if (schemaType.IsString) {
                typeStr = schemaType.GetString ();
            }
            else if (schemaType.IsJsonArray) {
                var types = schemaType.GetJsonArray ();
                var nonNullTypes = types.Where ( t => t.GetString () != "null" ).ToArray ();
                isNullable = types.Any ( t => t.GetString () == "null" );
                if (nonNullTypes.Length > 0) {
                    typeStr = nonNullTypes [ 0 ].GetString ();
                }
            }

            if (typeStr is null)
                return;

            if (typeStr == "object") {
                var properties = schema [ "properties" ];
                if (!properties.IsJsonObject)
                    return;

                var propsObj = properties.GetJsonObject ();
                var requiredProps = new HashSet<string> ();
                var requiredArray = schema [ "required" ];
                if (requiredArray.IsJsonArray) {
                    foreach (var req in requiredArray.GetJsonArray ()) {
                        if (req.IsString) {
                            requiredProps.Add ( req.GetString () );
                        }
                    }
                }

                foreach (var prop in propsObj) {
                    var propSchema = prop.Value.GetJsonObject ();
                    var description = propSchema [ "description" ];
                    bool isRequired = requiredProps.Contains ( prop.Key );

                    var propType = propSchema [ "type" ];
                    bool propIsNullable = propType.IsJsonArray && propType.GetJsonArray ().Any ( t => t.GetString () == "null" );

                    string typeName = GetTypeNameFromSchema ( propSchema );
                    if (propIsNullable) {
                        typeName += "?";
                    }

                    parameters.Add ( new ParameterExampleResult (
                        $"{string.Join ( '.', pathParts )}.{prop.Key}",
                        typeName,
                        isRequired,
                        description.IsString ? description.GetString () : null
                    ) );

                    AppendSchema ( prop.Value, [ .. pathParts, prop.Key ] );
                }
            }
            else if (typeStr == "array") {
                var itemsSchema = schema [ "items" ];
                if (itemsSchema.IsJsonObject) {
                    AppendSchema ( itemsSchema, [ .. pathParts, "[*]" ] );
                }
            }
        }

        string GetTypeNameFromSchema ( JsonObject schema ) {
            var schemaType = schema [ "type" ];

            if (schemaType.IsString) {
                return schemaType.GetString () ?? "any";
            }
            else if (schemaType.IsJsonArray) {
                var types = schemaType.GetJsonArray ();
                var nonNullTypes = types.Where ( t => t.GetString () != "null" ).ToArray ();
                if (nonNullTypes.Length > 0) {
                    return nonNullTypes [ 0 ].GetString () ?? "any";
                }
            }

            return "any";
        }

        var jsonSchema = JsonSchema.CreateFromType ( type, _jsoptions );
        AppendSchema ( jsonSchema.AsJsonValue (), [ "$" ] );
        return parameters.ToArray ();
    }

    /// <inheritdoc/>
    public JsonSchema GetJsonSchemaForType ( Type type ) {

        return JsonSchema.CreateFromType ( type, _jsoptions );
    }

    class JsonPropertyNameComparer : IEqualityComparer<string> {

        [return: NotNullIfNotNull ( nameof ( s ) )]
        static string? Sanitize ( string? s ) {
            if (s is null)
                return null;
            return new string ( s.Where ( char.IsLetterOrDigit ).ToArray () );
        }

        public bool Equals ( string? x, string? y ) {
            return string.Equals ( Sanitize ( x ), Sanitize ( y ), StringComparison.OrdinalIgnoreCase );
        }

        public int GetHashCode ( [DisallowNull] string obj ) {
            return Sanitize ( obj ).ToLowerInvariant ().GetHashCode ();
        }
    }
}