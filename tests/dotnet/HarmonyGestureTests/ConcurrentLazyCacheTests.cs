using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyOS.Essentials;
using Xunit;

namespace HarmonyGestureTests;

public class ConcurrentLazyCacheTests
{
    [Fact]
    public async Task ConcurrentGetOrAdd_UsesOneFactoryExecution()
    {
        var cache = new ConcurrentLazyCache<string, int>();
        var calls = 0;

        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => cache.GetOrAdd(
                "same",
                _ =>
                {
                    Interlocked.Increment(ref calls);
                    return 42;
                })))
            .ToArray();

        var values = await Task.WhenAll(tasks);

        Assert.All(values, value => Assert.Equal(42, value));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void FactoryFailure_IsNotCached()
    {
        var cache = new ConcurrentLazyCache<string, int>();

        Assert.Throws<InvalidOperationException>(() => cache.GetOrAdd(
            "bad",
            _ => throw new InvalidOperationException()));
        Assert.Equal(0, cache.Count);

        Assert.Equal(7, cache.GetOrAdd("bad", _ => 7));
    }
}
