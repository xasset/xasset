using System;
using System.Collections.Generic;

namespace xasset
{
    /// <summary>
    ///     资源引用计数器。用来统计 Asset 层的引用计数。
    /// </summary>
    public static class ReferencesCounter
    {
        private static readonly Dictionary<string, string[]> assetWithDependencies = new Dictionary<string, string[]>();
        private static readonly Dictionary<string, int> assetWithReferences = new Dictionary<string, int>();
        public static Func<string, string[]> GetDependenciesFunc { get; set; } = GetDependenciesRuntime;

        /// <summary>
        ///     是否开启对资源的引用计数，编辑器下开启后可以及时释放被 AssetDatabase 加载的资源，
        ///     运行时如果资源打包粒度不合理，也可以开启这个选项提前释放 AssetBundle 里面加载的资源，打包粒度合理的话，关闭这个可以优化性能。
        /// </summary>
        public static bool Enabled { get; set; }

        private static string[] GetDependenciesRuntime(string path)
        {
            return Assets.TryGetAsset(ref path, out var asset)
                ? Array.ConvertAll(asset.deps, input => asset.manifest.assets[input].path)
                : Array.Empty<string>();
        }

        private static IEnumerable<string> GetDependencies(string path)
        {
            if (assetWithDependencies.TryGetValue(path, out var dependencies)) return dependencies;
            dependencies = GetDependenciesFunc(path);
            assetWithDependencies[path] = dependencies;
            return dependencies;
        }

        public static void Retain(string path)
        {
            if (!Enabled) return;

            RetainInternal(path);
        }

        private static void RetainInternal(string path)
        {
            _Retain(path);
            var children = GetDependencies(path);
            foreach (var child in children)
                _Retain(child);
        }

        private static void _Retain(string path)
        {
            if (!assetWithReferences.TryGetValue(path, out var value)) value = 0;
            value += 1;
            assetWithReferences[path] = value;
        }

        public static int Release(string path)
        {
            if (!Enabled) return 1;
            var result = ReleaseInternal(path);
            return result;
        }

        private static int ReleaseInternal(string path)
        {
            var children = GetDependencies(path);
            foreach (var child in children)
                _Release(child);
            return _Release(path);
        }

        private static int _Release(string path)
        {
            if (!assetWithReferences.TryGetValue(path, out var value)) return 0;
            value -= 1;
            assetWithReferences[path] = value;
            return value;
        }
    }
}