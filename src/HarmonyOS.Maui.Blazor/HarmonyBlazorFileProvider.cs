#nullable enable
using System;
using System.IO;
using System.Threading;
using HarmonyOS.Bindings.Api;
using HarmonyOS.Interop;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyBlazorFileProvider : IFileProvider
{
    private readonly string _contentRoot;
    private static ResourceManagerObject? _resourceManager;

    public HarmonyBlazorFileProvider(string contentRoot)
    {
        if (string.IsNullOrWhiteSpace(contentRoot))
            throw new ArgumentException("Content root must not be empty.", nameof(contentRoot));
        _contentRoot = contentRoot.Trim('/');
    }

    public IDirectoryContents GetDirectoryContents(string subpath)
        => NotFoundDirectoryContents.Singleton;

    public IFileInfo GetFileInfo(string subpath)
    {
        var relative = Normalize(subpath);
        if (relative.Length == 0)
            return new NotFoundFileInfo(subpath);

        var rawPath = string.Join('/', _contentRoot, relative);
        var rm = GetResourceManager();
        if (rm is null)
            return new NotFoundFileInfo(subpath);

        try
        {
            var bytes = rm.GetRawFileContentBytesSync(rawPath);
            if (bytes is null || bytes.Length == 0)
                return new NotFoundFileInfo(subpath);
            return new HarmonyBlazorFileInfo(relative, rawPath, bytes);
        }
        catch
        {
            return new NotFoundFileInfo(subpath);
        }
    }

    public IChangeToken Watch(string filter)
        => NullChangeToken.Singleton;

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
