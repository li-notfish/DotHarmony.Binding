#nullable enable
using System;
using System.Threading.Tasks;
using BlazorDispatcher = Microsoft.AspNetCore.Components.Dispatcher;
using MauiDispatcher = Microsoft.Maui.Dispatching.IDispatcher;
using Microsoft.Maui.Dispatching;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyBlazorDispatcher : BlazorDispatcher
{
    private readonly MauiDispatcher _dispatcher;

    public HarmonyBlazorDispatcher(MauiDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dispatcher = dispatcher;
    }

    public override bool CheckAccess()
        => !_dispatcher.IsDispatchRequired;

    public override Task InvokeAsync(Action workItem)
        => ObserveFaults(_dispatcher.DispatchAsync(workItem));

    public override Task InvokeAsync(Func<Task> workItem)
        => ObserveFaults(_dispatcher.DispatchAsync(workItem));

    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
        => _dispatcher.DispatchAsync(workItem);

    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
        => _dispatcher.DispatchAsync(workItem);

    private static async Task ObserveFaults(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HarmonyOS.Interop.HiLog.Error("HarmonyWeb",
                $"[blazor-dispatcher] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }
}
