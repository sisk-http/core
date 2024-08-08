using Sisk.IniConfiguration.Parser;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.IniConfiguration;

/// <summary>
/// Represents an INI section, which contains it's own properties.
/// </summary>
public sealed class IniSection : IReadOnlyDictionary<string, string[]>
{
    internal (string, string)[] items;

    /// <summary>
    /// Gets the INI section name.
    /// </summary>
    public string Name { get; }

    internal IniSection(string name, (string, string)[] items)
    {
        this.items = items;
        Name = name;
    }

    /// <summary>
    /// Gets all values associated with the specified property name, performing an case-insensitive search.
    /// </summary>
    /// <param name="key">The property name.</param>
    public string[] this[string key]
    {
        get
        {
            return items
                .Where(k => IniParser.IniNamingComparer.Compare(key, k.Item1) == 0)
                .Select(k => k.Item2)
                .ToArray();
        }
    }

    /// <summary>
    /// Gets all keys defined in this INI section, without duplicates.
    /// </summary>
    public IEnumerable<string> Keys
    {
        get
        {
            return items.Select(i => i.Item1).Distinct().ToArray();
        }
    }

    /// <summary>
    /// Gets all values defined in this INI section.
    /// </summary>
    public IEnumerable<string[]> Values
    {
        get
        {
            using (var e = GetEnumerator())
            {
                while (e.MoveNext())
                {
                    yield return e.Current.Value;
                }
            }
        }
    }

    /// <summary>
    /// Gets the number of properties in this INI section.
    /// </summary>
    public int Count => items.Length;

    /// <summary>
    /// Gets the last value defined in this INI section by their property name.
    /// </summary>
    /// <param name="key">The property name.</param>
    /// <returns>The last value associated with the specified property name, or null if nothing is found.</returns>
    public string? GetOne(string key)
    {
        return items
            .Where(k => IniParser.IniNamingComparer.Compare(key, k.Item1) == 0)
            .Select(k => k.Item2)
            .LastOrDefault();
    }

    /// <summary>
    /// Gets all values defined in this INI section by their property name.
    /// </summary>
    /// <param name="key">The property name.</param>
    /// <returns>All values associated with the specified property name.</returns>
    public string[] GetMany(string key)
    {
        return this[key];
    }

    /// <summary>
    /// Gets an boolean indicating if the specified key/property name is
    /// defined in this <see cref="IniSection"/>.
    /// </summary>
    /// <param name="key">The property name.</param>
    /// <returns>An <see cref="bool"/> indicating if the specified property name is defined or not.</returns>
    public bool ContainsKey(string key)
    {
        for (int i = 0; i < items.Length; i++)
        {
            (string, string?) item = items[i];

            if (IniParser.IniNamingComparer.Compare(item.Item1, key) == 0)
                return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
    {
        string[] keysDistinct = items.Select(i => i.Item1).Distinct().ToArray();

        foreach (string key in keysDistinct)
        {
            string[] valuesByKey = items
                .Where(i => i.Item1 == key)
                .Select(i => i.Item2)
                .ToArray();

            yield return new KeyValuePair<string, string[]>(key, valuesByKey);
        }
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string[] value)
    {
        value = this[key];
        return value.Length > 0;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
