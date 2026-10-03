using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Sisk.ModelContextProtocol;

/// <summary>
/// Represents an in-memory skill and its complete, immutable file manifest.
/// </summary>
public sealed class McpSkill {

    internal byte[] EntryJson { get; }
    internal IReadOnlyDictionary<string, JsonObject> Contents { get; }

    /// <summary>
    /// Gets the URI of this skill's SKILL.md resource.
    /// </summary>
    public string Uri { get; }

    /// <summary>
    /// Creates a skill, generating SKILL.md from JSON-compatible YAML frontmatter and a Markdown body.
    /// </summary>
    /// <param name="path">The skill path, ending in the frontmatter name, for example acme/refunds.</param>
    /// <param name="frontmatter">All frontmatter fields, including name and description.</param>
    /// <param name="body">The Markdown instructions, without frontmatter.</param>
    /// <param name="files">Optional supporting files, keyed by relative path. Their bytes are copied at registration.</param>
    public McpSkill ( string path, JsonObject frontmatter, string body, IReadOnlyDictionary<string, byte[]>? files = null ) {
        ArgumentNullException.ThrowIfNull ( path );
        ArgumentNullException.ThrowIfNull ( frontmatter );
        ArgumentNullException.ThrowIfNull ( body );

        JsonValue nameValue = frontmatter [ "name" ];
        JsonValue descriptionValue = frontmatter [ "description" ];
        if (nameValue.Type != JsonValueType.String
            || !Regex.IsMatch ( nameValue.GetString (), @"\A[a-z0-9]+(-[a-z0-9]+)*\z" )
            || nameValue.GetString ().Length > 64
            || descriptionValue.Type != JsonValueType.String
            || string.IsNullOrWhiteSpace ( descriptionValue.GetString () )
            || descriptionValue.GetString ().Length > 1024)
            throw new ArgumentException ( "Frontmatter requires a valid skill name and a description of 1 to 1024 characters.", nameof ( frontmatter ) );

        if (path.Split ( '/' ).Any ( segment => string.IsNullOrEmpty ( segment )
            || segment is "." or ".."
            || !Regex.IsMatch ( segment, @"\A[a-zA-Z0-9._~-]+\z" ) )
            || path.Split ( '/' ) [ ^1 ] != nameValue.GetString ())
            throw new ArgumentException ( "The skill path must contain URI-safe segments and end in the skill name.", nameof ( path ) );

        Uri = $"skill://{path}/SKILL.md";
        string frontmatterJson = Encoding.UTF8.GetString ( McpProvider.Json.SerializeUtf8Bytes ( frontmatter ) );
        string markdown = $"---\n{frontmatterJson}\n---\n{body}";
        var contents = new Dictionary<string, JsonObject> ( StringComparer.Ordinal );
        JsonArray manifest = [];
        var allFiles = new Dictionary<string, byte[]> ( StringComparer.Ordinal ) {
            [ "SKILL.md" ] = Encoding.UTF8.GetBytes ( markdown )
        };
        if (files is { }) {
            foreach (var file in files) {
                if (string.IsNullOrEmpty ( file.Key )
                    || file.Key.Split ( '/' ).Any ( segment => string.IsNullOrEmpty ( segment )
                        || segment is "." or ".."
                        || segment.Any ( c => char.IsControl ( c ) || c is '\\' or ':' ) )
                    || file.Value is null
                    || !allFiles.TryAdd ( file.Key, file.Value.ToArray () ))
                    throw new ArgumentException ( "Supporting files must have unique relative paths and cannot replace SKILL.md.", nameof ( files ) );
            }
        }
        if (allFiles.Count > 512 || allFiles.Sum ( file => (long) file.Value.Length ) > 16_777_216)
            throw new ArgumentException ( "A skill cannot exceed 512 files or 16 MiB.", nameof ( files ) );

        foreach (var file in allFiles) {
            string uri = $"skill://{path}/{string.Join ( '/', file.Key.Split ( '/' ).Select ( System.Uri.EscapeDataString ) )}";
            manifest.Add ( new JsonObject () {
                [ "uri" ] = uri,
                [ "digest" ] = "sha256:" + Convert.ToHexString ( SHA256.HashData ( file.Value ) ).ToLowerInvariant (),
                [ "size" ] = file.Value.Length
            } );
            JsonObject content = new JsonObject () {
                [ "uri" ] = uri,
                [ "mimeType" ] = file.Key == "SKILL.md" ? "text/markdown" : "application/octet-stream"
            };
            if (file.Key == "SKILL.md")
                content [ "text" ] = markdown;
            else
                content [ "blob" ] = Convert.ToBase64String ( file.Value );
            contents.Add ( uri, content );
        }
        Contents = contents;
        EntryJson = McpProvider.Json.SerializeUtf8Bytes ( new JsonObject () {
            [ "uri" ] = Uri,
            [ "frontmatter" ] = frontmatter,
            [ "resources" ] = manifest
        } );
    }
}
