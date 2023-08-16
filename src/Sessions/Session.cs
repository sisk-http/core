// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Session.cs
// Repository:  https://github.com/sisk-http/core

using System.Text.Json;

namespace Sisk.Core.Sessions;

/// <summary>
/// Represents a session object that stores data referring to an HTTP state.
/// </summary>
/// <definition>
/// public sealed class UserSession
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class Session
{
    internal bool willDestroy = false;
    internal DateTime memAccessAt = DateTime.Now;

    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    /// <definition>
    /// public Guid Id { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public Guid Id { get; set; }

    /// <summary>
    /// Represents the session storage, which allows you to store objects in the form of a value and key.
    /// </summary>
    /// <definition>
    /// public Hashtable Bag { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public Dictionary<string, object?> Bag { get; set; } = new();

    /// <summary>
    /// Creates a new session with a random ID.
    /// </summary>
    /// <definition>
    /// public Session()
    /// </definition>
    /// <type>
    /// Constructor
    /// </type>
    public Session()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Destroys the session, deletes all its stored data and sends a cookie to the client to invalidate the stored cookie.
    /// </summary>
    /// <definition>
    /// public void Destroy()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Destroy()
    {
        willDestroy = true;
    }

    /// <summary>
    /// Gets or sets a bag property for this session.
    /// </summary>
    /// <nodocs/>
    /// <param name="index">The bag property key.</param>
    public object? this[string index]
    {
        get
        {
            return Bag[index];
        }
        set
        {
            Bag[index] = value;
        }
    }

    /// <summary>
    /// Gets an managed object from the session bag through it's type.
    /// </summary>
    /// <typeparam name="T">The type of object which is stored in the session bag.</typeparam>
    /// <definition>
    /// public T? Get{{T}}()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public T? Get<T>()
    {
        Type contextType = typeof(T);
        string key = contextType.FullName ?? "";
        if (Bag.ContainsKey(key))
        {
            var val = Bag[key];
            if (val is JsonElement json)
            {
                return json.Deserialize<T>();
            }
            else
            {
                return (T?)Bag[key];
            }
        }
        return default;
    }

    /// <summary>
    /// Stores a managed object in the session bag through it's type.
    /// </summary>
    /// <typeparam name="T">The type of object that will be stored in the session bag.</typeparam>
    /// <param name="value">The object which will be stored.</param>
    /// <returns>Returns the stored object.</returns>
    /// <definition>
    /// public void Set{{T}}(T value)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Set<T>(T value)
    {
        Type contextType = typeof(T);
        string key = contextType.FullName ?? "";
        Bag[key] = value;
    }

    /// <summary>
    /// Removes a managed object in the session bag through it's type.
    /// </summary>
    /// <typeparam name="T">The type of object that will be removed from the session bag.</typeparam>
    /// <definition>
    /// public void Remove{{T}}()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Remove<T>()
    {
        Type contextType = typeof(T);
        string key = contextType.FullName ?? "";
        Bag.Remove(key);
    }

    /// <inheritdoc />
    /// <nodocs/>
    public override Boolean Equals(Object? obj)
    {
        if (obj is Session s)
        {
            return s.Id == this.Id;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    /// <nodocs/>
    public override Int32 GetHashCode()
    {
        return Id.GetHashCode();
    }
}
