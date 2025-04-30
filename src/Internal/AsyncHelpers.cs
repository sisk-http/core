// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   AsyncHelpers.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Internal;

static class AsyncHelpers {
    public static T GetSyncronizedResult<T> ( this ValueTask<T> task ) {
        if (task.IsCompleted) {
            return task.Result;
        }
        return task.AsTask ().GetAwaiter ().GetResult ();
    }

    public static void GetSyncronizedResult ( this ValueTask task ) {
        if (task.IsCompleted) {
            return;
        }
        task.AsTask ().GetAwaiter ().GetResult ();
    }

    public static Task WaitOneAsync ( this WaitHandle waitHandle ) {
        ArgumentNullException.ThrowIfNull ( waitHandle );

        var tcs = new TaskCompletionSource<bool> ();
        var rwh = ThreadPool.RegisterWaitForSingleObject ( waitHandle,
            delegate { tcs.TrySetResult ( true ); }, null, -1, true );
        var t = tcs.Task;
        t.ContinueWith ( ( antecedent ) => rwh.Unregister ( null ) );
        return t;
    }
}
