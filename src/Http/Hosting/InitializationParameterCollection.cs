// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   InitializationParameterCollection.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Provides a collection of HTTP server initialization variables.
/// </summary>
/// <definition>
/// public class InitializationParameterCollection : IDictionary{{string, string?}}
/// </definition>
/// <type>
/// Class
/// </type>
public class InitializationParameterCollection : IDictionary<string, string?>
{
    private NameValueCollection _decorator = new NameValueCollection();
    private bool _isReadonly = false;

    /// <summary>
    /// Gets an instance of <see cref="NameValueCollection"/> with the values of this class.
    /// </summary>
    /// <definition>
    /// public NameValueCollection AsNameValueCollection()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public NameValueCollection AsNameValueCollection() => _decorator;

    /// <summary>
    /// Associates the parameters received in the service configuration to a managed object.
    /// </summary>
    /// <typeparam name="T">The type of the managed object that will have the service parameters mapped.</typeparam>
    /// <definition>
    /// public T MapTo{{T}}(NameValueCollection setupParameters)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075", Justification = "The return value of method 'System.Reflection.PropertyInfo.PropertyType.get' does not have matching annotations.")]
    public T Map<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new()
    {
        T parametersObject = new T();

        Type parameterType = typeof(T);
        PropertyInfo[] properties = parameterType.GetProperties();
        foreach (PropertyInfo property in properties)
        {
            object mappingValue;
            string? value = _decorator[property.Name];
            Type propType = property.PropertyType;

            if (value == null) continue;
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
#if NET6_0
                mappingValue = Parseable.ParseInternal(value, propType)!;
#elif NET7_0_OR_GREATER
                if (propType.IsAssignableTo(typeof(IParsable<>)))
                {
                    mappingValue = propType.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(string), typeof(IFormatProvider) })
                        !.Invoke(propType, new[] { value, null })!;
                }
                else
                {
                    mappingValue = Parseable.ParseInternal(value, propType)!;
                }
#endif
            } else
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
    /// <definition>
    /// public void EnsureNotNullOrEmpty(string parameterName)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void EnsureNotNullOrEmpty(string parameterName)
    {
        if (string.IsNullOrEmpty(_decorator[parameterName])) throw new ArgumentException(string.Format(SR.InitializationParameterCollection_NullOrEmptyParameter, parameterName));
    }

    /// <summary>
    /// Ensures that the parameter defined by name <paramref name="parameterName"/> is present in this collection.
    /// </summary>
    /// <remarks>
    /// If the parameter doens't meet the above requirements, an <see cref="ArgumentNullException"/> exception is thrown.
    /// </remarks>
    /// <param name="parameterName">The parameter name which will be evaluated.</param>
    /// <definition>
    /// public void EnsureNotNull(string parameterName)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void EnsureNotNull(string parameterName)
    {
        if (string.IsNullOrEmpty(_decorator[parameterName])) throw new ArgumentException(string.Format(SR.InitializationParameterCollection_NullParameter, parameterName));
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public string? this[string key]
    {
        get => _decorator[key];
        set
        {
            ThrowIfReadonly();
            _decorator[key] = value;
        }
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public ICollection<string> Keys
    {
        get
        {
            List<string> _keys = new List<string>();
            foreach (string s in _decorator.Keys)
                _keys.Add(s);
            return _keys.ToArray();
        }
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public ICollection<string?> Values
    {
        get
        {
            List<string?> _keys = new List<string?>();
            foreach (string s in _decorator.Keys)
                _keys.Add(_decorator[s]);
            return _keys.ToArray();
        }
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public int Count => _decorator.Count;

    /// <inheritdoc/>
    /// <nodoc/>
    public bool IsReadOnly => _isReadonly;

    /// <inheritdoc/>
    /// <nodoc/>
    public void Add(string key, string? value)
    {
        ThrowIfReadonly();
        _decorator[key] = value;
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public void Add(KeyValuePair<string, string?> item)
    {
        ThrowIfReadonly();
        _decorator[item.Key] = item.Value;
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public void Clear()
    {
        ThrowIfReadonly();
        _decorator.Clear();
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public bool Contains(KeyValuePair<string, string?> item)
    {
        return Keys.Contains(item.Key) && _decorator[item.Key] == item.Value;
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public bool ContainsKey(string key)
    {
        return Keys.Contains(key);
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public void CopyTo(KeyValuePair<string, string?>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
    {
        return (IEnumerator<KeyValuePair<string, string?>>)_decorator.GetEnumerator();
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public bool Remove(string key)
    {
        ThrowIfReadonly();
        _decorator.Remove(key);
        return true;
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public bool Remove(KeyValuePair<string, string?> item)
    {
        ThrowIfReadonly();
        _decorator.Remove(item.Key);
        return true;
    }

    /// <inheritdoc/>
    /// <nodoc/>
    public bool TryGetValue(string key, out string? value)
    {
        if (ContainsKey(key))
        {
            value = _decorator[key];
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    /// <inheritdoc/>
    /// <nodoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _decorator.GetEnumerator();
    }

    internal void MakeReadonly()
    {
        _isReadonly = true;
    }

    void ThrowIfReadonly()
    {
        if (_isReadonly)
        {
            throw new InvalidOperationException("Cannot modify this collection: it is read-only.");
        }
    }
}
