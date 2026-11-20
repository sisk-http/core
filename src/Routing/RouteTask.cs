// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouteTask.cs
// Repository:  https://github.com/sisk-http/core

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents the non-generic base of <see cref="RouteTask{T}"/>.
/// </summary>
public abstract class RouteTask {
    internal readonly Task<object> Inner;

    private protected RouteTask ( Task<object> inner ) {
        Inner = inner;
    }
}

/// <summary>
/// Represents an asynchronous route action result which can hold both reference and value types, allowing
/// <c>async RouteTask&lt;int&gt;</c> instead of <c>async Task&lt;ValueResult&lt;int&gt;&gt;</c>.
/// </summary>
/// <typeparam name="T">The type of the route action result.</typeparam>
[AsyncMethodBuilder ( typeof ( RouteTaskMethodBuilder<> ) )]
public sealed class RouteTask<T> : RouteTask where T : notnull {
    internal RouteTask ( Task<object> inner ) : base ( inner ) {
    }
}

/// <summary>
/// Represents the asynchronous method builder for <see cref="RouteTask{T}"/>. This type is used by the compiler.
/// </summary>
/// <typeparam name="T">The type of the route action result.</typeparam>
[EditorBrowsable ( EditorBrowsableState.Never )]
public struct RouteTaskMethodBuilder<T> where T : notnull {
    private AsyncTaskMethodBuilder<object> _inner;

    /// <exclude />
    [SuppressMessage ( "Design", "CA1000", Justification = "The async method builder pattern requires a static Create method." )]
    public static RouteTaskMethodBuilder<T> Create () => new () { _inner = AsyncTaskMethodBuilder<object>.Create () };

    /// <exclude />
    public RouteTask<T> Task => new ( _inner.Task );

    /// <exclude />
    public void Start<TStateMachine> ( ref TStateMachine stateMachine )
        where TStateMachine : IAsyncStateMachine => _inner.Start ( ref stateMachine );

    /// <exclude />
    public void SetStateMachine ( IAsyncStateMachine stateMachine ) => _inner.SetStateMachine ( stateMachine );

    /// <exclude />
    public void SetResult ( T result ) => _inner.SetResult ( result );

    /// <exclude />
    public void SetException ( Exception exception ) => _inner.SetException ( exception );

    /// <exclude />
    public void AwaitOnCompleted<TAwaiter, TStateMachine> ( ref TAwaiter awaiter, ref TStateMachine stateMachine )
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine => _inner.AwaitOnCompleted ( ref awaiter, ref stateMachine );

    /// <exclude />
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine> ( ref TAwaiter awaiter, ref TStateMachine stateMachine )
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine => _inner.AwaitUnsafeOnCompleted ( ref awaiter, ref stateMachine );
}
