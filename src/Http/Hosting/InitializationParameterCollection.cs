// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   InitializationParameterCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Sisk.Core.Internal;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Provides a collection of HTTP server initialization variables.
/// </summary>
public sealed class InitializationParameterCollection : IDictionary<string, string?>
{
    private readonly NameValueCollection _decorator = new NameValueCollection();
    private bool _isReadonly = false;

    /// <summary>
    /// Gets an instance of <see cref="NameValueCollection"/> with the values of this class.
    /// </summary>
    public NameValueCollection AsNameValueCollection() => this._decorator;

    /// <summary>
    /// Associates the parameters received in the service configuration to a managed object.
    /// </summary>
    /// <typeparam name="T">The type of the managed object that will have the service parameters mapped.</typeparam>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075", Justification = "The return value of method 'System.Reflection.PropertyInfo.PropertyType.get' does not have matching annotations.")]
    [Obsolete("This method is deprecated and will be removed in next Sisk versions.")]
    public T Map<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new()
    {
        T parametersObject = new T();

        Type parameterType = typeof(T);
        PropertyInfo[] properties = parameterType.GetProperties();
        foreach (PropertyInfo property in properties)
        {
            object mappingValue;
            string? value = this._decorator[property.Name];
            Type propType = property.PropertyType;

            if (value is null) continue;
            if (propType.IsEnum)
            {
                mappingValue = Enum.Parse(propType, value, true);
            }
            else if (propType == typeof(string))
            {
                mappingValue = value;
            }
            else if (propType.IsValueType)
            {
                if (propType.IsAssignableTo(typeof(IParsable<>)))
                {
                    mappingValue = propType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(string), typeof(IFormatProvider) })
                        !.Invoke(propType, new[] { value, null })!;
                }
                else
                {
                    mappingValue = Parseable.ParseInternal(value, propType)!;
                }
            }
            else
            {
                throw new InvalidCastException(string.Format(SR.InitializationParameterCollection_MapCastException, value, propType.FullName));
            }

            property.SetValue(parametersObject, mappingValue);
        }

        return parametersObject;
    }

    /// <summary>
    /// Ensures that the parameter defined by name <paramref name="parameterName"/> is present and not empty in this collection.
    /// </summary>
    /// <remarks>
    /// If the parameter doens't meet the above requirements, an <see cref="ArgumentNullException"/> exception is thrown.
    /// </remarks>
    /// <param name="parameterName">The parameter name which will be evaluated.</param>
    public void EnsureNotNullOrEmpty(string parameterName)
    {
        if (string.IsNullOrEmpty(this._decorator[parameterName])) throw new ArgumentException(string.Format(SR.InitializationParameterCollection_NullOrEmptyParameter, parameterName));
    }

    /// <summary>
    /// Ensures that the parameter defined by name <paramref name="parameterName"/> is present in this collection.
    /// </summary>
    /// <remarks>
    /// If the parameter doens't meet the above requirements, an <see cref="ArgumentNullException"/> exception is thrown.
    /// </remarks>
    /// <param name="parameterName">The parameter name which will be evaluated.</param>
    public void EnsureNotNull(string parameterName)
    {
        if (string.IsNullOrEmpty(this._decorator[parameterName])) throw new ArgumentException(string.Format(SR.InitializationParameterCollection_NullParameter, parameterName));
    }

    /// <inheritdoc/>
    /// <exclude/>
    public string? this[string key]
    {
        get => this._decorator[key];
        set
        {
            this.ThrowIfReadonly();
            this._decorator[key] = value;
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public ICollection<string> Keys
    {
        get
        {
            List<string> _keys = new List<string>();
            foreach (string s in this._decorator.Keys)
                _keys.Add(s);
            return _keys.ToArray();
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public ICollection<string?> Values
    {
        get
        {
            List<string?> _keys = new List<string?>();
            foreach (string s in this._decorator.Keys)
                _keys.Add(this._decorator[s]);
            return _keys.ToArray();
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public int Count => this._decorator.Count;

    /// <inheritdoc/>
    /// <exclude/>
    public bool IsReadOnly => this._isReadonly;

    /// <inheritdoc/>
    /// <exclude/>
    public void Add(string key, string? value)
    {
        this.ThrowIfReadonly();
        this._decorator[key] = value;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public void Add(KeyValuePair<string, string?> item)
    {
        this.ThrowIfReadonly();
        this._decorator[item.Key] = item.Value;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public void Clear()
    {
        this.ThrowIfReadonly();
        this._decorator.Clear();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Contains(KeyValuePair<string, string?> item)
    {
        return this.Keys.Contains(item.Key) && this._decorator[item.Key] == item.Value;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool ContainsKey(string key)
    {
        return this.Keys.Contains(key);
    }

    /// <inheritdoc/>
    /// <exclude/>
    public void CopyTo(KeyValuePair<string, string?>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
    {
        return (IEnumerator<KeyValuePair<string, string?>>)this._decorator.GetEnumerator();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Remove(string key)
    {
        this.ThrowIfReadonly();
        this._decorator.Remove(key);
        return true;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Remove(KeyValuePair<string, string?> item)
    {
        this.ThrowIfReadonly();
        this._decorator.Remove(item.Key);
        return true;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool TryGetValue(string key, out string? value)
    {
        if (this.ContainsKey(key))
        {
            value = this._decorator[key];
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this._decorator.GetEnumerator();
    }

    internal void MakeReadonly()
    {
        this._isReadonly = true;
    }

    void ThrowIfReadonly()
    {
        if (this._isReadonly)
        {
            throw new InvalidOperationException("Cannot modify this collection: it is read-only.");
        }
    }
}
