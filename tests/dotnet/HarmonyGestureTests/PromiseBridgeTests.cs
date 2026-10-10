using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyOS.Interop;
using Xunit;

namespace HarmonyGestureTests;

public class PromiseBridgeTests
{
    [Fact]
    public async Task AtomicCompletion_AllowsOnlyOneWinner()
    {
        var tcs = new TaskCompletionSource<int>();
        var completion = new AtomicCompletion<int>(tcs);

        var results = await Task.WhenAll(
            Enumerable.Range(0, 100)
                .Select(_ => Task.Run(() => completion.TrySetResult(42))));

        Assert.Equal(1, results.Count(success => success));
        Assert.Equal(42, await tcs.Task);
    }

    [Fact]
    public void AtomicCompletion_CancelWinsOverLaterResult()
    {
        var tcs = new TaskCompletionSource<int>();
        var completion = new AtomicCompletion<int>(tcs);

        Assert.True(completion.TrySetCanceled());
        Assert.False(completion.TrySetResult(42));
        Assert.Equal(TaskStatus.Canceled, tcs.Task.Status);
    }

    [Fact]
    public void RichNapiException_ContainsOperationCodeAndMessage()
    {
        var exception = PromiseTaskBridge.CreateRichNapiException(
            napi_status.napi_generic_failure,
            "promise.then",
            401,
            "boom");

        Assert.Contains("promise.then", exception.Message);
        Assert.Contains("401", exception.Message);
        Assert.Contains("boom", exception.Message);
        Assert.Equal("promise.then", exception.Operation);
        Assert.Equal(401, exception.ErrorCode);
    }
}
