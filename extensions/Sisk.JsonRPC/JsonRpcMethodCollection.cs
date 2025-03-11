// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcMethodCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.JsonRPC;

/// <summary>
/// Represents a collection of JSON-RPC methods, allowing for dynamic addition and removal of methods.
/// </summary>
public sealed class JsonRpcMethodCollection {
    internal Dictionary<string, RpcDelegate> methods = new Dictionary<string, RpcDelegate> ( StringComparer.OrdinalIgnoreCase );

    /// <summary>
    /// Adds a method to the collection with the specified name.
    /// </summary>
    /// <param name="name">The name of the method to add.</param>
    /// <param name="method">The delegate representing the method to add.</param>
    public void AddMethod ( string name, Delegate method ) {
        lock ((methods as ICollection).SyncRoot) {
            methods.Add ( name, new RpcDelegate ( method.Method, method.Target ) );
        }
    }

    /// <summary>
    /// Adds methods from the specified type to the collection, optionally prefixing method names with the type name.
    /// </summary>
    /// <typeparam name="T">The type from which to scan and add methods.</typeparam>
    /// <param name="target">The target object instance containing the methods.</param>
    /// <param name="prefixTypes">Indicates whether to prefix method names with the type name.</param>
    public void AddMethodsFromType<[DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.PublicMethods )] T> ( T target, bool prefixTypes = false ) where T : notnull {
        lock ((methods as ICollection).SyncRoot) {
            foreach (var method in MethodScanner.ScanMethods ( typeof ( T ), prefixTypes, target )) {
                methods.Add ( method.Item1, method.Item2 );
            }
        }
    }

    /// <summary>
    /// Adds methods from the specified type to the collection, optionally prefixing method names with the type name.
    /// </summary>
    /// <param name="type">The type from which to scan and add methods.</param>
    /// <param name="target">The target object instance containing the methods.</param>
    /// <param name="prefixTypes">Indicates whether to prefix method names with the type name.</param>
    public void AddMethodsFromType ( [DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.PublicMethods )] Type type, object? target, bool prefixTypes ) {
        lock ((methods as ICollection).SyncRoot) {
            foreach (var method in MethodScanner.ScanMethods ( type, prefixTypes, target )) {
                methods.Add ( method.Item1, method.Item2 );
            }
        }
    }

    /// <summary>
    /// Adds methods from the specified type to the collection without prefixing method names.
    /// </summary>
    /// <param name="type">The type from which to scan and add methods.</param>
    /// <param name="target">The target object instance containing the methods.</param>
    public void AddMethodsFromType ( [DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.PublicMethods )] Type type, object? target ) => AddMethodsFromType ( type, target, false );

    /// <summary>
    /// Adds methods from the specified type to the collection without prefixing method names.
    /// </summary>
    /// <param name="type">The type from which to scan and add methods.</param>
    public void AddMethodsFromType ( [DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.PublicMethods )] Type type ) => AddMethodsFromType ( type, null, false );

    /// <summary>
    /// Removes a method from the collection by its name.
    /// </summary>
    /// <param name="name">The name of the method to remove.</param>
    public void RemoveMethod ( string name ) {
        lock ((methods as ICollection).SyncRoot) {
            methods.Remove ( name );
        }
    }

    internal RpcDelegate? GetMethod ( string name ) {
        lock ((methods as ICollection).SyncRoot) {
            if (methods.TryGetValue ( name, out RpcDelegate? result )) {
                return result;
            }
            else {
                return null;
            }
        }
    }
}
