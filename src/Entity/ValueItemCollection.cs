// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   KeyValueCollection.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Entity;

public sealed class ValueItemCollection : IEnumerable<ValueItem>
{
    internal Dictionary<string, string?> items;
    internal string paramName;

    internal static ValueItemCollection FromNameValueCollection(string paramName, NameValueCollection col)
    {
        ValueItemCollection vcol = new ValueItemCollection(paramName);

        foreach (string key in col.Keys)
        {
            vcol.items.Add(key, col[key]);
        }

        return vcol;
    }

    internal ValueItemCollection(string paramName)
    {
        items = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        this.paramName = paramName;
    }

    internal void SetItem(string key, string? value)
    {
        items[key] = value;
    }

    public Dictionary<string, string?> AsDictionary() => items;
    public NameValueCollection AsNameValueCollection()
    {
        NameValueCollection n = new NameValueCollection();

        foreach (string key in items.Keys)
        {
            n.Add(key, items[key]);
        }

        return n;
    }

    public int Count { get => items.Count; }
    public ValueItem this[string name] { get => GetItem(name); }

    public ValueItem GetItem(string name)
    {
        items.TryGetValue(name, out string? value);
        return new ValueItem(name, paramName, value);
    }

    public IEnumerator<ValueItem> GetEnumerator()
    {
        foreach (string key in items.Keys)
        {
            yield return new ValueItem(key, paramName, items[key]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public static implicit operator Dictionary<string, string>(ValueItemCollection vcol)
    {
        return vcol.AsDictionary();
    }

    public static implicit operator NameValueCollection(ValueItemCollection vcol)
    {
        return vcol.AsNameValueCollection();
    }
}
