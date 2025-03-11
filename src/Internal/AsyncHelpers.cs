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
        return task.GetAwaiter ().GetResult ();
    }
}
