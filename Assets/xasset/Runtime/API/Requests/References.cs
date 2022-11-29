using System;
using System.Collections.Generic;

namespace xasset
{
    /// <summary>
    ///     资源引用计数器。用来统计 Asset 层的引用计数。
    /// </summary>
    public static class References
    {
        private static readonly Dictionary<string, string[]> assetWithDependencies = new Dictionary<string, string[]>();
        public static readonly Dictionary<string, int> assetWithReferences = new Dictionary<string, int>();
        public static Func<string, string[]> GetFunc { get; set; } = RuntimeGet;

        /// <summary>
        ///     是否开启对资源的引用计数，编辑器下开启后可以及时释放被 AssetDatabase 加载的资源，运行时如果资源打包粒度不合理，也可以开启这个选项提前释放 AssetBundle 里面加载的资源，打包粒度合理的化，关闭这个可以优化性能。
        /// </summary>
        public static bool Enabled { get; set; } = true;

        private static string[] RuntimeGet(string path)
        {
            return Assets.TryGetAsset(ref path, out var asset) ? Array.ConvertAll(asset.deps, input => asset.manifest.assets[input].path) : Array.Empty<string>();
        }

        private static string[] Get(string path)
        {
            if (assetWithDependencies.TryGetValue(path, out var dependencies)) return dependencies;
            dependencies = GetInternal(path);
            assetWithDependencies[path] = dependencies;
            return dependencies;
        }

        private static string[] GetInternal(string path)
        {
            return GetFunc(path);
        }

        public static void Retain(string path)
        {
            if (!Enabled)
            {
                return;
            }

            RetainInternal(path);
            var children = Get(path);
            foreach (var child in children)
            {
                RetainInternal(child);
            }
        }

        private static void RetainInternal(string path)
        {
            if (!assetWithReferences.TryGetValue(path, out var value))
            {
                value = 0;
            }

            value += 1;
            assetWithReferences[path] = value;
        }

        public static int Release(string path)
        {
            if (!Enabled)
            {
                return 1;
            }

            var result = ReleaseInternal(path);
            var children = Get(path);
            foreach (var child in children)
            {
                ReleaseInternal(child);
            }

            return result;
        }

        private static int ReleaseInternal(string path)
        {
            if (!assetWithReferences.TryGetValue(path, out var value))
            {
                return 0;
            }

            value -= 1;
            assetWithReferences[path] = value;
            return value;
        }
    }
}