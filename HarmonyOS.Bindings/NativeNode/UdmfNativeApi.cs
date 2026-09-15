// UDMF（统一数据管理框架）C API 桥：拖拽载荷（drag_and_drop）所需子集。
// 文本载荷构造（DragStarting → DragEvent SetData）与读取（DragEvent GetUdmfData → GetPrimaryPlainText）。
// 官方用法（udmf.h docs）：AddPlainText/AddRecord 之后 record/plainText 即可销毁（Add 拷贝内容），
// data 由调用方持有（DestroyData 释放）。
#nullable enable
using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.NativeNode;

internal static unsafe partial class UdmfNativeApi
{
    private const string UdmfLib = "libudmf.z.so";

    [LibraryImport(UdmfLib)]
    internal static partial IntPtr OH_UdmfData_Create();

    [LibraryImport(UdmfLib)]
    internal static partial void OH_UdmfData_Destroy(IntPtr data);

    [LibraryImport(UdmfLib)]
    internal static partial int OH_UdmfData_AddRecord(IntPtr data, IntPtr record);

    [LibraryImport(UdmfLib)]
    internal static partial int OH_UdmfData_GetPrimaryPlainText(IntPtr data, IntPtr plainText);

    [LibraryImport(UdmfLib)]
    internal static partial IntPtr OH_UdmfRecord_Create();

    [LibraryImport(UdmfLib)]
    internal static partial void OH_UdmfRecord_Destroy(IntPtr record);

    [LibraryImport(UdmfLib)]
    internal static partial int OH_UdmfRecord_AddPlainText(IntPtr record, IntPtr plainText);

    [LibraryImport(UdmfLib)]
    internal static partial IntPtr OH_UdsPlainText_Create();

    [LibraryImport(UdmfLib)]
    internal static partial void OH_UdsPlainText_Destroy(IntPtr plainText);

    [LibraryImport(UdmfLib)]
    private static partial int OH_UdsPlainText_SetContent(IntPtr plainText, byte* content);

    [LibraryImport(UdmfLib)]
    private static partial IntPtr OH_UdsPlainText_GetContent(IntPtr plainText);

    /// <summary>构造携带纯文本的 UDMF 数据（拖拽 DragStarting → DragEventSetData）。返回句柄由调用方 DestroyData。</summary>
    internal static IntPtr CreateTextData(string text)
    {
        var data = OH_UdmfData_Create();
        var record = OH_UdmfRecord_Create();
        var plain = OH_UdsPlainText_Create();
        if (data == IntPtr.Zero || record == IntPtr.Zero || plain == IntPtr.Zero)
        {
            OH_UdsPlainText_Destroy(plain);
            OH_UdmfRecord_Destroy(record);
            OH_UdmfData_Destroy(data);
            return IntPtr.Zero;
        }
        try
        {
            var utf8 = System.Text.Encoding.UTF8.GetBytes(text);
            var buffer = new byte[utf8.Length + 1];
            utf8.CopyTo(buffer, 0);
            var status = 0;
            fixed (byte* p = buffer)
            {
                status = OH_UdsPlainText_SetContent(plain, p);
                if (status != 0)
                    throw new InvalidOperationException($"OH_UdsPlainText_SetContent failed: {status}");
            }
            status = OH_UdmfRecord_AddPlainText(record, plain);
            if (status != 0)
                throw new InvalidOperationException($"OH_UdmfRecord_AddPlainText failed: {status}");
            status = OH_UdmfData_AddRecord(data, record);
            if (status != 0)
                throw new InvalidOperationException($"OH_UdmfData_AddRecord failed: {status}");
            return data;
        }
        finally
        {
            // Add 拷贝内容：item/record 即刻可销毁（官方 docs 用法）
            OH_UdsPlainText_Destroy(plain);
            OH_UdmfRecord_Destroy(record);
        }
    }

    /// <summary>读取 UDMF 数据的主纯文本（GetPrimaryPlainText）。无文本返回 null。</summary>
    internal static string? ReadPrimaryText(IntPtr data)
    {
        var plain = OH_UdsPlainText_Create();
        if (plain == IntPtr.Zero)
            return null;
        try
        {
            var status = OH_UdmfData_GetPrimaryPlainText(data, plain);
            if (status != 0)
                return null;
            var p = OH_UdsPlainText_GetContent(plain);
            return p == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(p);
        }
        finally
        {
            OH_UdsPlainText_Destroy(plain);
        }
    }

    internal static void DestroyData(IntPtr data)
    {
        if (data != IntPtr.Zero)
            OH_UdmfData_Destroy(data);
    }
}
