// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   UriBuilder.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using Sisk.Core.Entity;

namespace Sisk.Core.Helpers;

/// <summary>
/// Provides a way to build URLs by adding segments, queries, and other components.
/// </summary>
public sealed class UrlBuilder {

    private readonly List<string> segments = new ();
    private readonly StringKeyStoreCollection query = new ();

    /// <summary>
    /// Gets or sets the fragment part of the URL.
    /// </summary>
    /// <remarks>
    /// The fragment is the part of the URL after the <see langword="null"/> character.
    /// </remarks>
    public string? Fragment { get; set; }

    /// <summary>
    /// Gets or sets the authority part of the URL.
    /// </summary>
    /// <remarks>
    /// The authority is the part of the URL that contains the domain name or IP address.
    /// </remarks>
    public string Authority { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the scheme part of the URL.
    /// </summary>
    /// <remarks>
    /// The scheme is the part of the URL that specifies the protocol, such as <c>http</c> or <c>https</c>.
    /// </remarks>
    public string Scheme { get; set; } = "http";

    /// <summary>
    /// Gets the query parameters as an array of key-value pairs.
    /// </summary>
    public KeyValuePair<string, string []> [] Query => query.ToArray ();

    /// <summary>
    /// Gets the URL segments as an array of strings.
    /// </summary>
    /// <returns>An array of <see cref="string"/> representing the URL segments.</returns>
    public string [] Segments => segments.ToArray ();

    /// <summary>
    /// Gets or sets a value indicating whether to normalize separators in the URL.
    /// </summary>
    /// <remarks>
    /// If <see langword="true"/>, the URL builder will replace backslashes with forward slashes.
    /// </remarks>
    public bool NormalizeSeparators { get; set; } = true;

    /// <summary>
    /// Gets or sets the character used as the path separator.
    /// </summary>
    public char PathSeparator { get; set; } = '/';

    /// <summary>
    /// Gets the constructed URL as a <see cref="System.Uri"/> object.
    /// </summary>
    public Uri Uri => new ( Url );

    /// <summary>
    /// Gets the constructed URL as a string.
    /// </summary>
    /// <returns>The URL as a string, including scheme, authority, segments, query parameters, and fragment.</returns>
    public string Url {
        get {
            StringBuilder sb = new StringBuilder ();
            sb.Append ( Scheme );
            sb.Append ( "://" );
            sb.Append ( Authority );

            foreach (var segment in segments.Where ( s => !string.IsNullOrWhiteSpace ( s ) )) {
                sb.Append ( PathSeparator );
                sb.Append ( segment );
            }

            if (query.Count > 0) {
                sb.Append ( '?' );

                using var enumerator = query.GetEnumerator ();
                bool hasNext = enumerator.MoveNext ();

                while (hasNext) {

                    var queryItem = enumerator.Current;
                    foreach (var queryValue in queryItem.Value) {

                        // repeat values for same key
                        sb.Append ( Uri.EscapeDataString ( queryItem.Key ) );
                        sb.Append ( '=' );
                        sb.Append ( Uri.EscapeDataString ( queryValue ) );
                    }

                    hasNext = enumerator.MoveNext ();

                    if (hasNext) {
                        sb.Append ( '&' );
                    }
                }
            }

            if (!string.IsNullOrEmpty ( Fragment )) {
                sb.Append ( '#' );
                sb.Append ( Fragment.TrimStart ( '#' ) );
            }

            return sb.ToString ();
        }
    }

    /// <summary>
    /// Creates a new <see cref="UrlBuilder"/> instance from one or more combined URLs and/or paths.
    /// </summary>
    /// <param name="urls">The URLs to combine.</param>
    /// <returns>A new <see cref="UrlBuilder"/> instance initialized with the combined URL.</returns>
    public static UrlBuilder FromCombined ( params string [] urls ) {
        return new UrlBuilder ( PathHelper.CombinePaths ( urls ) );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlBuilder"/> class.
    /// </summary>
    public UrlBuilder () {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlBuilder"/> class from a URI string.
    /// </summary>
    /// <param name="uri">The URI string to parse.</param>
    /// <exception cref="ArgumentException">Thrown if the provided string is not a valid absolute URI.</exception>
    public UrlBuilder ( string uri ) {
        if (Uri.TryCreate ( uri, UriKind.Absolute, out var parsedUri )) {

            Scheme = parsedUri.Scheme;
            Authority = parsedUri.Authority;
            segments.AddRange ( parsedUri.Segments.Select ( s => s.Trim ( GetSplitPathSeparators () ) ).Where ( s => !string.IsNullOrWhiteSpace ( s ) ) );

            if (!string.IsNullOrEmpty ( parsedUri.Query )) {
                var queryString = parsedUri.Query.TrimStart ( '?' );
                query = StringKeyStoreCollection.FromQueryString ( queryString );
            }
        }
        else {
            throw new ArgumentException ( "The provided string is not a valid absolute URI.", nameof ( uri ) );
        }
    }

    private char [] GetSplitPathSeparators () {
        return NormalizeSeparators switch {
            true => [ '/', '\\' ],
            false => [ PathSeparator ],
        };
    }

    /// <summary>
    /// Sets the authority part of the URL.
    /// </summary>
    /// <param name="authority">The authority to set.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="authority"/> is <see langword="null"/>.</exception>
    public UrlBuilder SetAuthority ( string authority ) {
        ArgumentNullException.ThrowIfNullOrWhiteSpace ( authority );

        this.Authority = authority;
        return this;
    }

    /// <summary>
    /// Sets the scheme part of the URL.
    /// </summary>
    /// <param name="scheme">The scheme to set.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="scheme"/> is <see langword="null"/>.</exception>
    public UrlBuilder SetScheme ( string scheme ) {
        ArgumentNullException.ThrowIfNullOrWhiteSpace ( scheme );

        this.Scheme = scheme;
        return this;
    }

    /// <summary>
    /// Sets the fragment part of the URL.
    /// </summary>
    /// <param name="fragment">The fragment to set, or <see langword="null"/> to remove the fragment.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder SetFragment ( string? fragment ) {
        this.Fragment = fragment;
        return this;
    }

    /// <summary>
    /// Sets the path separator character.
    /// </summary>
    /// <param name="separator">The path separator character to set.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder SetPathSeparator ( char separator ) {
        PathSeparator = separator;
        return this;
    }

    /// <summary>
    /// Adds one or more segments to the URL path.
    /// </summary>
    /// <param name="segments">The segments to add, or an array containing <see langword="null"/> values to indicate relative URLs.</param>
    /// <param name="allowRelativeReturns">If <see langword="true"/>, allows the addition of relative URLs that return to a parent directory.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder AddSegment ( string? [] segments, bool allowRelativeReturns = false ) {
        ArgumentNullException.ThrowIfNull ( segments );

        if (segments.Length == 0) {
            return this;
        }

        var subSegments = segments
            .SelectMany ( s => s?.Split ( GetSplitPathSeparators (), StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries ) ?? Array.Empty<string> () );

        foreach (var segment in subSegments) {
            if (segment == "..") {
                if (allowRelativeReturns) {
                    if (this.segments.Count > 0) {
                        this.segments.RemoveAt ( this.segments.Count - 1 );
                    }
                }
                else {
                    throw new InvalidOperationException ( "Relative path returns are not allowed." );
                }
            }
            else if (segment == ".") {
                continue;
            }
            else if (!string.IsNullOrWhiteSpace ( segment )) {
                this.segments.Add ( segment );
            }
        }

        return this;
    }

    /// <summary>
    /// Adds one or more segments to the URL path.
    /// </summary>
    /// <param name="segments">The segments to add.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="segments"/> is <see langword="null"/>.</exception>
    public UrlBuilder AddSegment ( params string? [] segments ) {
        ArgumentNullException.ThrowIfNull ( segments );
        return AddSegment ( segments, false );
    }

    /// <summary>
    /// Adds a single segment to the URL path.
    /// </summary>
    /// <param name="segment">The segment to add.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder AddSegment ( string? segment ) {
        return AddSegment ( [ segment ] );
    }

    /// <summary>
    /// Conditionally adds a segment to the URL path.
    /// </summary>
    /// <param name="segment">The segment to add.</param>
    /// <param name="condition">The condition under which the segment should be added.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance, with the segment added if <paramref name="condition"/> is <see langword="true"/>.</returns>
    public UrlBuilder AddSegmentIf ( string segment, bool condition ) {
        if (condition) {
            return AddSegment ( [ segment ] );
        }
        return this;
    }

    /// <summary>
    /// Adds a query parameter to the URL.
    /// </summary>
    /// <param name="key">The key of the query parameter.</param>
    /// <param name="value">The value of the query parameter, or <see langword="null"/> to add an empty value.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder AddQuery ( string key, string? value ) {
        query.Add ( key, value ?? string.Empty );
        return this;
    }

    /// <summary>
    /// Adds a query parameter with multiple values to the URL.
    /// </summary>
    /// <param name="key">The key of the query parameter.</param>
    /// <param name="values">The values of the query parameter, or an array containing <see langword="null"/> values to add empty values.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder AddQuery ( string key, params string? [] values ) {
        query.Add ( key, values.Select ( s => s ?? string.Empty ) );
        return this;
    }

    /// <summary>
    /// Conditionally adds a query parameter with multiple values to the URL.
    /// </summary>
    /// <param name="key">The key of the query parameter.</param>
    /// <param name="values">The values of the query parameter, or an array containing <see langword="null"/> values to add empty values.</param>
    /// <param name="condition">The condition under which the query parameter should be added.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance, with the query parameter added if <paramref name="condition"/> is <see langword="true"/>.</returns>
    public UrlBuilder AddQueryIf ( string key, string? [] values, bool condition ) {
        if (condition) {
            query.Add ( key, values.Select ( s => s ?? string.Empty ) );
        }
        return this;
    }
    /// <summary>
    /// Sets (removes and adds) a query parameter with multiple values in the URL.
    /// </summary>
    /// <param name="key">The key of the query parameter.</param>
    /// <param name="values">The values of the query parameter.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance, with the query parameter set.</returns>
    public UrlBuilder SetQuery ( string key, params string? [] values ) {
        RemoveQuery ( key );
        return AddQuery ( key, values );
    }

    /// <summary>
    /// Sets (removes and adds) a query parameter with a single value in the URL.
    /// </summary>
    /// <param name="key">The key of the query parameter.</param>
    /// <param name="value">The value of the query parameter, or <see langword="null"/> to add an empty value.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance, with the query parameter set.</returns>
    public UrlBuilder SetQuery ( string key, string? value ) {
        RemoveQuery ( key );
        return AddQuery ( key, value );
    }

    /// <summary>
    /// Conditionally sets (removes and adds) a query parameter with a single value in the URL.
    /// </summary>
    /// <param name="condition">The condition under which the query parameter should be set.</param>
    /// <param name="key">The key of the query parameter.</param>
    /// <param name="value">The value of the query parameter, or <see langword="null"/> to add an empty value.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance, with the query parameter set if <paramref name="condition"/> is <see langword="true"/>.</returns>
    public UrlBuilder SetQueryIf ( bool condition, string key, string? value ) {
        if (condition) {
            RemoveQuery ( key );
            AddQuery ( key, value );
        }
        return this;
    }

    /// <summary>
    /// Conditionally sets (removes and adds) a query parameter with multiple values in the URL.
    /// </summary>
    /// <param name="condition">The condition under which the query parameter should be set.</param>
    /// <param name="key">The key of the query parameter.</param>
    /// <param name="values">The values of the query parameter, or an array containing <see langword="null"/> values to add empty values.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance, with the query parameter set if <paramref name="condition"/> is <see langword="true"/>.</returns>
    public UrlBuilder SetQueryIf ( bool condition, string key, params string? [] values ) {
        if (condition) {
            RemoveQuery ( key );
            AddQuery ( key, values );
        }
        return this;
    }

    /// <summary>
    /// Removes a query parameter from the URL.
    /// </summary>
    /// <param name="key">The key of the query parameter to remove.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder RemoveQuery ( string key ) {
        query.Remove ( key );
        return this;
    }

    /// <summary>
    /// Removes the last segment(s) from the URL path.
    /// </summary>
    /// <param name="amount">The number of segments to remove. Defaults to 1 if not specified.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="amount"/> is negative.</exception>
    public UrlBuilder Pop ( int amount = 1 ) {
        ArgumentOutOfRangeException.ThrowIfNegative ( amount );

        for (int i = 0; i < amount; i++) {
            if (segments.Count == 0) {
                break;
            }
            segments.RemoveAt ( segments.Count - 1 );
        }

        return this;
    }

    /// <summary>
    /// Applies a transformation function to each URL segment.
    /// </summary>
    /// <param name="fn">A function that takes a segment as input and returns the transformed segment, or <see langword="null"/> to remove the segment.</param>
    /// <returns>The current <see cref="UrlBuilder"/> instance, with the segments transformed.</returns>
    public UrlBuilder TransformSegments ( Func<string, string?> fn ) {
        for (int i = 0; i < segments.Count; i++) {
            string segment = segments [ i ];
            segments [ i ] = fn ( segment )!;
        }

        // remove null or empty segments
        segments.RemoveAll ( string.IsNullOrWhiteSpace );

        return this;
    }

    /// <summary>
    /// Clears all query parameters from the URL.
    /// </summary>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder ClearQuery () {
        query.Clear ();
        return this;
    }

    /// <summary>
    /// Clears all segments from the URL path.
    /// </summary>
    /// <returns>The current <see cref="UrlBuilder"/> instance.</returns>
    public UrlBuilder ClearSegments () {
        segments.Clear ();
        return this;
    }

    /// <inheritdoc/>
    public override string ToString () {
        return Url;
    }
}
