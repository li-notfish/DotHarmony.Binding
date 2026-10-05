#nullable enable
using System;
using System.Threading.Tasks;

namespace HarmonyOS.Interop;

internal sealed class AtomicCompletion<T>
{
    private readonly TaskCompletionSource<T> _tcs;
    private readonly object _gate = new();
    private bool _done;

    public AtomicCompletion(TaskCompletionSource<T> tcs)
    {
        ArgumentNullException.ThrowIfNull(tcs);
        _tcs = tcs;
    }

    public bool TrySetResult(T value)
        => Complete(() => _tcs.TrySetResult(value));

    public bool TrySetException(Exception exception)
        => Complete(() => _tcs.TrySetException(exception));

    public bool TrySetCanceled()
        => Complete(() => _tcs.TrySetCanceled());

    private bool Complete(Func<bool> complete)
    {
        lock (_gate)
        {
            if (_done)
            {
                return false;
            }

            _done = true;
            return complete();
        }
    }
}
