// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonExampleTypeHandler.cs
// Repository:  https://github.com/sisk-http/core


using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Namotion.Reflection;

namespace Sisk.Documenting;

public class JsonExampleTypeHandler : IExampleBodyTypeHandler, IExampleParameterTypeHandler {

    public int EnumerationExampleCount { get; set; } = 1;
    public bool IncludeDescriptionAnnotations { get; set; } = true;

    private IJsonTypeInfoResolver _typeResolver;
    private JsonSerializerOptions _serializerOptions;

    public JsonExampleTypeHandler ( IJsonTypeInfoResolver typeResolver, JsonSerializerOptions serializerOptions ) {
        _typeResolver = typeResolver;
        _serializerOptions = serializerOptions;
    }

    [RequiresDynamicCode ( "This method calls the JsonSerializerOptions.Default, which requires dynamic code." )]
    [RequiresUnreferencedCode ( "This method calls the JsonSerializerOptions.Default, which requires unreferenced code." )]
    public JsonExampleTypeHandler ( JsonSerializerOptions serializerOptions ) {
        _typeResolver = new DefaultJsonTypeInfoResolver ();
        _serializerOptions = serializerOptions;
    }

    [RequiresDynamicCode ( "This method calls the JsonSerializerOptions.Default, which requires dynamic code." )]
    [RequiresUnreferencedCode ( "This method calls the JsonSerializerOptions.Default, which requires unreferenced code." )]
    public JsonExampleTypeHandler () {
        _typeResolver = new DefaultJsonTypeInfoResolver ();
        _serializerOptions = JsonSerializerOptions.Default;
    }

    string ConvertCase ( string name ) => _serializerOptions.PropertyNamingPolicy?.ConvertName ( name ) ?? name;

    [SuppressMessage ( "Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "<Pending>" )]
    public virtual BodyExampleResult? GetBodyExampleForType ( Type type ) {
        StringBuilder sb = new StringBuilder ();
        int indentLevel = 0;

        HashSet<MemberInfo> documentedTypes = new HashSet<MemberInfo> ();

        void AppendComment ( string? text ) {
            if (text is null)
                return;
            foreach (var line in text.Split ( '\n' )) {
                AppendLine ( $"// {line.Trim ()}" );
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

        void AppendEnumerable ( JsonTypeInfo enumItem ) {
            AppendLine ( "[" );
            indentLevel++;

            Type? elementType = null;
            if (enumItem.Type.IsArray) {
                elementType = enumItem.Type.GetElementType ();
            }
            else {
                var genericArgs = enumItem.Type.GetGenericArguments ();
                if (genericArgs.Length > 0) {
                    elementType = genericArgs [ 0 ];
                }
            }

            AppendIndent ();
            if (elementType is null) {
                for (int i = 0; i < EnumerationExampleCount; i++) {
                    AppendLine ( "object," );
                    AppendIndent ();
                }

                AppendLine ();
                AppendLine ( "..." );
            }
            else {
                for (int i = 0; i < EnumerationExampleCount; i++) {
                    AppendType ( elementType );
                    AppendLine ( "," );
                    AppendIndent ();
                }

                AppendLine ( "..." );
            }

            indentLevel--;
            AppendIndent ();
            AppendText ( "]" );
        }

        void AppendObject ( JsonTypeInfo objectItem ) {

            Dictionary<string, string?> paramsDocs = new Dictionary<string, string?> ( new JsonPropertyNameComparer () );
            foreach (var prop in objectItem.Type.GetProperties ()) {
                string propName =
                    prop.GetCustomAttribute<JsonPropertyNameAttribute> ()?.Name ??
                    prop.Name;
                paramsDocs.Add ( propName, prop.GetXmlDocsSummary () );
            }

            AppendLine ( "{" );
            indentLevel++;

            bool lastItemAppendedDocs = false;
            for (int i = 0; i < objectItem.Properties.Count; i++) {
                JsonPropertyInfo property = objectItem.Properties [ i ];

                if (lastItemAppendedDocs) {
                    AppendLine ();
                    lastItemAppendedDocs = false;
                }

                if (paramsDocs.TryGetValue ( property.Name, out string? propertyDocs )) {
                    AppendIndent ();

                    List<string> docParts = new List<string> ();
                    if (property.IsRequired) {
                        docParts.Add ( "Required" );
                    }
                    if (property.AssociatedParameter?.IsNullable == true) {
                        docParts.Add ( "Nullable" );
                    }

                    if (docParts.Count > 0) {
                        AppendComment ( $"{string.Join ( ". ", docParts )}. {propertyDocs}" );
                    }
                    else {
                        AppendComment ( propertyDocs );
                    }
                    lastItemAppendedDocs = true;
                }

                AppendIndent ();
                AppendText ( $"\"{ConvertCase ( property.Name )}\": " );
                AppendType ( property.PropertyType );

                if (property.AssociatedParameter?.IsNullable == true) {
                    AppendText ( "?" );
                }

                if (i != objectItem.Properties.Count - 1) {
                    AppendText ( "," );
                }

                AppendLine ();
            }

            indentLevel--;
            AppendIndent ();
            AppendText ( "}" );
        }

        void AppendJsonType ( JsonTypeInfo item ) {
            switch (item.Kind) {
                case JsonTypeInfoKind.Object:
                    AppendObject ( item );
                    break;

                case JsonTypeInfoKind.Enumerable:
                    AppendEnumerable ( item );
                    break;

                default:
                    AppendText ( ConvertCase ( item.Type.Name ) );
                    break;
            }
        }

        void AppendType ( Type type ) {
            try {
                var typeInfo = _typeResolver.GetTypeInfo ( type, _serializerOptions );
                if (typeInfo is null) {
                    AppendText ( ConvertCase ( type.Name ) );
                }
                else {
                    AppendJsonType ( typeInfo );
                }
            }
            catch {
                AppendText ( ConvertCase ( type.Name ) );
            }
        }

        AppendType ( type );

        string result = sb.ToString ();
        return new BodyExampleResult ( result, "json" );
    }

    [SuppressMessage ( "Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "<Pending>" )]
    public ParameterExampleResult [] GetParameterExamplesForType ( Type type ) {

        List<ParameterExampleResult> parameters = new List<ParameterExampleResult> ();

        void AppendType ( Type type, string [] pathParts ) {
            var description = type.GetXmlDocsSummary ();
            JsonTypeInfo? typeInfo = _typeResolver.GetTypeInfo ( type, _serializerOptions );
            if (typeInfo is null) {
                parameters.Add ( new ParameterExampleResult ( $"{string.Join ( '.', pathParts )}.{ConvertCase ( type.Name )}", type.Name, false, description ) );
            }
            else {
                if (typeInfo.Kind == JsonTypeInfoKind.Object) {

                    Dictionary<string, string?> paramsDocs = new Dictionary<string, string?> ( new JsonPropertyNameComparer () );
                    foreach (var prop in typeInfo.Type.GetProperties ()) {
                        string propName =
                            prop.GetCustomAttribute<JsonPropertyNameAttribute> ()?.Name ??
                            prop.Name;
                        paramsDocs.Add ( propName, prop.GetXmlDocsSummary () );
                    }

                    foreach (var prop in typeInfo.Properties) {

                        paramsDocs.TryGetValue ( prop.Name, out string? propertyDocs );

                        string? typeName = ConvertCase ( prop.PropertyType.Name );
                        if (prop.AssociatedParameter?.IsNullable == true)
                            typeName += '?';

                        parameters.Add ( new ParameterExampleResult ( $"{string.Join ( '.', pathParts )}.{prop.Name}", typeName, prop.IsRequired, propertyDocs ) );
                        AppendType ( prop.PropertyType, [ .. pathParts, prop.Name ] );
                    }
                }
                else if (typeInfo.Kind == JsonTypeInfoKind.Enumerable) {
                    if (typeInfo.ElementType is { } elementType) {
                        AppendType ( typeInfo.ElementType, [ .. pathParts, "[*]" ] );
                    }
                    else {
                        parameters.Add ( new ParameterExampleResult ( $"{string.Join ( '.', pathParts )}.[*]", type.Name, true, description ) );
                    }
                }
            }
        }

        AppendType ( type, [ "$" ] );
        return parameters.ToArray ();
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
