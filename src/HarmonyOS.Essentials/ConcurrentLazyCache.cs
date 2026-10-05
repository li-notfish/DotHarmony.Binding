#nullable enable
using System;
using System.Collections.Concurrent;

namespace HarmonyOS.Essentials;

internal sealed class ConcurrentLazyCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, LazyValue> _items = new();

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (_items.TryGetValue(key, out var cached))
        {
            return cached.GetValue();
        }

        var created = new LazyValue(key, factory);
        var winner = _items.GetOrAdd(key, created);
        if (!ReferenceEquals(winner, created))
        {
            return winner.GetValue();
        }

        try
        {
            return created.GetValue();
        }
        catch
        {
            _items.TryRemove(key, out _);
            throw;
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_items.TryGetValue(key, out var lazy))
        {
            value = lazy.GetValue();
            return true;
        }

        value = default!;
        return false;
    }

    public int Count => _items.Count;

    private sealed class LazyValue
    {
        private readonly TKey _key;
        private readonly Func<TKey, TValue> _factory;
        private readonly object _gate = new();
        private TValue? _value;
        private bool _created;

        public LazyValue(TKey key, Func<TKey, TValue> factory)
        {
            _key = key;
            _factory = factory;
        }

        public TValue GetValue()
        {
            lock (_gate)
            {
                if (_created)
                {
                    return _value!;
                }

                _value = _factory(_key);
                _created = true;
                return _value;
            }
        }
    }
}
