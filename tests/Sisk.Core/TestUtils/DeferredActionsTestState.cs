using System.Collections.Concurrent;

namespace tests.TestUtils;

public sealed class DeferredActionsTestCase {
    public string Id { get; }

    public ConcurrentQueue<string> Log { get; } = new ();

    public TaskCompletionSource<bool> AllowFinish { get; } =
        new ( TaskCreationOptions.RunContinuationsAsynchronously );

    public TaskCompletionSource<bool> DeferredExecuted { get; } =
        new ( TaskCreationOptions.RunContinuationsAsynchronously );

    public DeferredActionsTestCase ( string id ) {
        Id = id;
    }
}

public static class DeferredActionsTestState {
    private static readonly ConcurrentDictionary<string, DeferredActionsTestCase> Cases = new ( StringComparer.Ordinal );

    public static DeferredActionsTestCase GetOrCreate ( string id ) {
        if (string.IsNullOrWhiteSpace ( id )) {
            throw new ArgumentException ( "id must be a non-empty string.", nameof ( id ) );
        }

        return Cases.GetOrAdd ( id, static ( key ) => new DeferredActionsTestCase ( key ) );
    }

    public static void Remove ( string id ) {
        if (string.IsNullOrWhiteSpace ( id )) {
            return;
        }

        Cases.TryRemove ( id, out _ );
    }
}
