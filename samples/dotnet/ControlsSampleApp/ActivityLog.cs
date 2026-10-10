using System;
using System.Collections.ObjectModel;
using HarmonyOS.Interop;
using Microsoft.Maui.ApplicationModel;

namespace ControlsSampleApp;

/// <summary>Cross-page activity feed so control interactions are visible on the Overview tab.</summary>
public static class ActivityLog
{
    private const int MaxEntries = 8;

    public static ObservableCollection<string> Entries { get; } = new();

    public static void Record(string text)
    {
        void Add()
        {
            Entries.Insert(0, $"{DateTime.Now:HH:mm:ss}  {text}");
            while (Entries.Count > MaxEntries)
                Entries.RemoveAt(Entries.Count - 1);
        }

        MainThreadDispatcher.Post(Add);
    }
}
