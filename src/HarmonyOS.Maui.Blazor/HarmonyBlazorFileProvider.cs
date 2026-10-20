#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Interop;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace HarmonyOS.Maui.Blazor;

/// <summary>
/// 从鸿蒙 rawfile（ResourceManager）读取 Blazor 静态资产。
/// 对齐官方 StaticContentProvider 的分层：宿主页目录 → 应用 wwwroot → RCL `_content/{PackageId}/`
/// （构建期由 HarmonyOS.Maui.Blazor.targets 把 publish 输出 wwwroot 整体 stage 进 rawfile，
/// RCL 资产的 _content 前缀在 stage 后即为真实路径，因此这里按 /_content/&lt;...&gt; 直读）。
/// </summary>
public sealed class HarmonyBlazorFileProvider : IFileProvider
{
    private readonly string _contentRoot;
    private readonly List<string> _candidateRoots;
    private static ResourceManagerObject? _resourceManager;

    public HarmonyBlazorFileProvider(string contentRoot)
    {
        if (string.IsNullOrWhiteSpace(contentRoot))
            throw new ArgumentException("Content root must not be empty.", nameof(contentRoot));
        _contentRoot = contentRoot.Trim('/');
        _candidateRoots = new List<string> { _contentRoot };
    }

    /// <summary>追加候选内容根（如发布产物的 wwwroot 与 RCL 的 _content 根）。</summary>
    public void AddContentRoot(string contentRoot)
    {
        if (string.IsNullOrWhiteSpace(contentRoot))
            return;
        var normalized = contentRoot.Trim('/');
        if (!_candidateRoots.Contains(normalized, StringComparer.Ordinal))
            _candidateRoots.Add(normalized);
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        // rawfile 无目录枚举通道：目录请求统一返回 NotFound（WebViewManager 仅对文件路径查 IFileInfo）
        return NotFoundDirectoryContents.Singleton;
    }

    public IFileInfo GetFileInfo(string subpath)
    {
        var relative = Normalize(subpath);
        if (relative.Length == 0)
            return new NotFoundFileInfo(subpath);

        var rm = GetResourceManager();
        if (rm is null)
            return new NotFoundFileInfo(subpath);

        // 内容根候选：HostPage 所在目录（通常 wwwroot）→ 追加的发布产物根。
        // RCL 静态资产以 /_content/{PackageId}/... 形态请求，stage 后位于 wwwroot/_content 下，命中同一根。
        foreach (var root in _candidateRoots)
        {
            if (TryRead(rm, root, relative, out var file))
                return file;
        }

        return new NotFoundFileInfo(subpath);
    }

    public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

    private static bool TryRead(ResourceManagerObject rm, string root, string relative, out IFileInfo file)
    {
        var rawPath = string.Join('/', root, relative);
        try
        {
            var bytes = rm.GetRawFileContentBytesSync(rawPath);
            if (bytes is null)
            {
                file = new NotFoundFileInfo(relative);
                return false;
            }
            file = new HarmonyBlazorFileInfo(relative, rawPath, bytes);
            return true;
        }
        catch
        {
            file = new NotFoundFileInfo(relative);
            return false;
        }
    }

    private static ResourceManagerObject? GetResourceManager()
    {
        if (_resourceManager is not null)
            return _resourceManager;

        try
        {
            var global = NodeApi.GetGlobal();
            var context = NodeApi.GetProperty(global, "abilityContext");
            if (context == IntPtr.Zero)
                return null;
            var resourceManager = NodeApi.GetProperty(context, "resourceManager");
            if (resourceManager == IntPtr.Zero)
                return null;
            return _resourceManager = new ResourceManagerObject(resourceManager);
        }
        catch
        {
            return null;
        }
    }

    private static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;
        var separator = path.IndexOfAny(new[] { '?', '#' });
        if (separator >= 0)
            path = path[..separator];
        return path.Trim('/').Replace('\\', '/');
    }

    private sealed class HarmonyBlazorFileInfo : IFileInfo
    {
        private readonly byte[] _bytes;
        private readonly string _rawPath;

        public HarmonyBlazorFileInfo(string name, string rawPath, byte[] bytes)
        {
            _bytes = bytes;
            _rawPath = rawPath;
            Name = name;
        }

        public bool Exists => true;
        public bool IsDirectory => false;
        public long Length => _bytes.Length;
        public string Name { get; }
        public string PhysicalPath => _rawPath;
        public DateTimeOffset LastModified => DateTimeOffset.MinValue;

        public Stream CreateReadStream()
            => new MemoryStream(_bytes, writable: false);
    }
}
