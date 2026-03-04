using System.Net;

namespace Sisk.Monitoring;

/// <summary>
/// Defines a monitored resource identified by a label and an instance of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the instance being monitored.</typeparam>
public sealed class MonitoringDefinition<T> : IEquatable<T>, IEquatable<MonitoringDefinition<T>> where T : notnull {

    /// <summary>
    /// Gets the instance being monitored.
    /// </summary>
    public T Instance { get; }

    /// <summary>
    /// Gets or sets the label that identifies the monitored resource.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the name of the group associated with this instance.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this monitoring definition should be pinned to the dashboard for easy access.
    /// </summary>
    public bool DashboardPinned { get; set; }

    /// <summary>
    /// Gets a sanitized version of the label, suitable for use in URLs or other contexts where special characters may need to be encoded.
    /// </summary>
    public string SanitizedLabel => WebUtility.UrlEncode ( Label );

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitoringDefinition{T}"/> class.
    /// </summary>
    /// <param name="label">The label that identifies the monitored resource.</param>
    /// <param name="instance">The instance to be monitored.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="label"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is <see langword="null"/>.</exception>
    public MonitoringDefinition ( string label, T instance ) {
        ArgumentException.ThrowIfNullOrWhiteSpace ( label );
        ArgumentNullException.ThrowIfNull ( instance );

        Instance = instance;
        Label = label;
    }

    /// <summary>
    /// Initializes a new instance of the MonitoringDefinition class with the specified label, group, and monitored
    /// instance.
    /// </summary>
    /// <param name="label">The display label used to identify the monitoring definition. Cannot be null, empty, or consist only of white
    /// space.</param>
    /// <param name="group">An optional group name used to categorize the monitoring definition. Can be null to indicate no group.</param>
    /// <param name="instance">The instance to be monitored. Cannot be null.</param>
    public MonitoringDefinition ( string label, string? group, T instance ) {
        ArgumentException.ThrowIfNullOrWhiteSpace ( label );
        ArgumentNullException.ThrowIfNull ( instance );

        Instance = instance;
        Label = label;
        Group = group;
    }

    /// <summary>
    /// Returns a hash code for this instance based on the label and instance.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode () {
        return HashCode.Combine ( Label, Instance );
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals ( object? obj ) {
        if (obj is T instance)
            return Instance!.Equals ( instance );
        else if (obj is MonitoringDefinition<T> definition)
            return Equals ( definition );

        return false;
    }

    /// <summary>
    /// Determines whether the current instance is equal to another object of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="other">An object of type <typeparamref name="T"/> to compare with this instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals ( T? other ) {
        if (other is null)
            return false;
        return Instance!.Equals ( other );
    }

    /// <summary>
    /// Determines whether the current instance is equal to another <see cref="MonitoringDefinition{T}"/>.
    /// </summary>
    /// <param name="other">A <see cref="MonitoringDefinition{T}"/> to compare with this instance.</param>
    /// <returns><see langword="true"/> if the current instance is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals ( MonitoringDefinition<T>? other ) {
        if (other is null)
            return false;
        return GetHashCode () == other.GetHashCode ();
    }
}