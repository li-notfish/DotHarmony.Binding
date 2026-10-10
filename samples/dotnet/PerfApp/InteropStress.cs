#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyOS.ArkUI;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;

namespace PerfApp;

internal static class InteropStress
{
    private const int StressIterations = 10_000;

    internal enum TestEnum
    {
        Zero = 0,
        One = 1,
        Two = 2,
    }

    internal static async Task RunAsync()
    {
        RunReferenceStress();
        RunCallbackExceptionStress();
        RunCallbackAllocationStress();
        RunEnumConversionStress();
        RunMainThreadGuardStress();
        RunDispatchBackpressureStress();
        RunComponentFinalizerStress();
        RunEventRemoveFailureStress();
        await RunGestureDisposeStress();
    }

    private static void RunReferenceStress()
    {
        var env = NapiEnv.Current;
        NativeNodeApi.napi_create_object(env, out var value).ThrowIfFailed();
        using var keepAlive = new NapiReference(value);

        for (var i = 0; i < StressIterations; i++)
        {
            using var reference = new NapiReference(value);
        }

        for (var i = 0; i < StressIterations; i++)
        {
            var reference = new NapiReference(value);
            reference = null!;
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        NapiFinalizationQueue.Drain();

        HiLog.Info("InteropStress", "INTEROP_REF_STRESS_OK");
    }

    private static void RunCallbackExceptionStress()
    {
        var (jsFunc, handle) = NodeApi.CreateCallbackFunction(
            (Action<ReadOnlySpan<IntPtr>>)(_ => throw new InvalidOperationException("callback stress")));

        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_global(env, out var global).ThrowIfFailed();
        NativeNodeApi.napi_call_function(
            env,
            global,
            jsFunc,
            0,
            ReadOnlySpan<IntPtr>.Empty,
            out _).ThrowIfFailed();

        NodeApi.FreeEventHandle(handle);
        HiLog.Info("InteropStress", "INTEROP_CALLBACK_EXCEPTION_OK");
    }

    private static void RunMainThreadGuardStress()
    {
        var background = new Thread(() =>
        {
            try
            {
                _ = new TestNode();
                HiLog.Error("InteropStress", "INTEROP_MAIN_THREAD_GUARD_FAILED");
            }
            catch (InvalidOperationException)
            {
                HiLog.Info("InteropStress", "INTEROP_MAIN_THREAD_GUARD_OK");
            }
            catch (Exception ex)
            {
                HiLog.Error("InteropStress", $"INTEROP_MAIN_THREAD_GUARD_FAILED: {ex.GetType().Name}: {ex.Message}");
            }
        })
        {
            IsBackground = true,
            Name = "interop-main-thread-guard",
        };

        background.Start();
        background.Join();
    }

    private static async Task RunGestureDisposeStress()
    {
        using var gesture = new ArkTapGesture();
        gesture.TestDisposeDuringDispatch();

        await Task.Delay(100);

        if (gesture.IsTestDisposed)
            HiLog.Info("InteropStress", "INTEROP_GESTURE_DISPOSE_OK");
        else
            HiLog.Error("InteropStress", "INTEROP_GESTURE_DISPOSE_FAILED");
    }

    private static void RunDispatchBackpressureStress()
    {
        var failures = 0;
        var worker = new Thread(() =>
        {
            for (var i = 0; i < StressIterations * 2; i++)
            {
                if (!MainThreadDispatcher.TryPost(static () => { }))
                                       Interlocked.Increment(ref failures);
            }
        })
        {
            IsBackground = true,
            Name = "interop-dispatch-backpressure",
        };

        worker.Start();
        worker.Join();

        MainThreadDispatcher.Drain();

        if (failures > 0)
            HiLog.Info("InteropStress", $"INTEROP_DISPATCH_BACKPRESSURE_OK failures={failures}");
        else
            HiLog.Error("InteropStress", "INTEROP_DISPATCH_BACKPRESSURE_FAILED");
    }

    private static void RunComponentFinalizerStress()
    {
        var weak = CreateComponentWithTrackedHandle();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        if (!weak.IsAlive)
            HiLog.Info("InteropStress", "INTEROP_COMPONENT_FINALIZER_OK");
        else
            HiLog.Error("InteropStress", "INTEROP_COMPONENT_FINALIZER_FAILED");
    }

    private static WeakReference CreateComponentWithTrackedHandle()
    {
        var dummy = new object();
        var gch = GCHandle.Alloc(dummy);
        var component = new TestComponent(IntPtr.Zero);
        component.TrackForTest(gch);
        return new WeakReference(dummy);
    }

    private static void RunEventRemoveFailureStress()
    {
        var registry = new EventListenerRegistry();
        registry.Add("stress-key", static _ => { }, static _ => { });

        var first = registry.Remove("stress-key", static _ => throw new InvalidOperationException("off failed"));
        var second = registry.Remove("stress-key", static _ => { });

        if (!first && second)
            HiLog.Info("InteropStress", "INTEROP_EVENT_REMOVE_FAILURE_OK");
        else
            HiLog.Error("InteropStress", "INTEROP_EVENT_REMOVE_FAILURE_FAILED");
    }

    private static void RunCallbackAllocationStress()
    {
        var (jsFunc, handle) = NodeApi.CreateCallbackFunction(
            (Action<ReadOnlySpan<IntPtr>>)(static _ => { }));

        var env = NapiEnv.Current;
        NativeNodeApi.napi_get_global(env, out var global).ThrowIfFailed();

        var before = GC.GetTotalAllocatedBytes();
        for (var i = 0; i < StressIterations; i++)
        {
            NativeNodeApi.napi_call_function(
                env,
                global,
                jsFunc,
                0,
                ReadOnlySpan<IntPtr>.Empty,
                out _).ThrowIfFailed();
        }
        var after = GC.GetTotalAllocatedBytes();

        NodeApi.FreeEventHandle(handle);
        var perCall = (after - before) / StressIterations;
        if (perCall < 64)
            HiLog.Info("InteropStress", $"INTEROP_CALLBACK_ALLOC_OK bytes_per_call={perCall}");
        else
            HiLog.Error("InteropStress", $"INTEROP_CALLBACK_ALLOC_FAILED bytes_per_call={perCall}");
    }

    private static void RunEnumConversionStress()
    {
        var value = NativeValue.From(2);
        var before = GC.GetTotalAllocatedBytes();
        for (var i = 0; i < StressIterations; i++)
        {
            var result = ValueConverter.Convert<TestEnum>(value);
            if (result != TestEnum.Two)
                throw new InvalidOperationException("enum conversion mismatch");
        }
        var after = GC.GetTotalAllocatedBytes();

        var perCall = (after - before) / StressIterations;
        if (perCall < 8)
            HiLog.Info("InteropStress", $"INTEROP_ENUM_CONVERT_OK bytes_per_call={perCall}");
        else
            HiLog.Error("InteropStress", $"INTEROP_ENUM_CONVERT_FAILED bytes_per_call={perCall}");
    }

    private sealed class TestNode : ArkUINodeBase
    {
        public TestNode()
            : base(ArkUI_NodeType.ARKUI_NODE_TEXT)
        {
        }
    }

    private sealed class TestComponent : ArkUIComponentBase
    {
        public TestComponent(IntPtr jsObject)
            : base(jsObject)
        {
        }

        public void TrackForTest(GCHandle handle) => TrackEventHandle(handle);
    }
}
