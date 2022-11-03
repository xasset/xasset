using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace xasset.editor
{
    public static class Bundler
    {
        private static BundleSettings _setting = new BundleSettings();

        public static string BundleExtension => ".bundle";

        public static List<string> ExcludeFiles => _setting.excludeFiles;

        /// <summary>
        ///     [path, entry, bundle, group, return(bundle)]
        /// </summary>
        public static Func<string, string, string, Group, string> customPacker { get; set; } = null;

        public static void Initialize(Settings settings)
        {
            _setting = settings.bundleSettings;
        }

        private static string GetDirectoryName(string path)
        {
            var dir = Path.GetDirectoryName(path);
            return !string.IsNullOrEmpty(dir) ? dir?.Replace("\\", "/") : string.Empty;
        }

        public static string PackAsset(BuildAsset asset)
        {
            var assetPath = asset.path;
            var group = asset.group;
            var entry = asset.entry;
            var bundle = group.name.ToLower();

            var dir = GetDirectoryName(entry) + "/";
            assetPath = assetPath.Replace(dir, "");
            entry = entry.Replace(dir, "");

            switch (group.bundleMode)
            {
                case BundleMode.PackTogether:
                    bundle = group.name;
                    break;
                case BundleMode.PackByFolder:
                    bundle = GetDirectoryName(assetPath);
                    break;
                case BundleMode.PackByFile:
                    bundle = assetPath.Replace(dir, "");
                    break;
                case BundleMode.PackByTopSubFolder:
                    if (!string.IsNullOrEmpty(entry))
                    {
                        var pos = assetPath.IndexOf("/", entry.Length + 1, StringComparison.Ordinal);
                        bundle = pos != -1 ? assetPath.Substring(0, pos) : entry;
                    }
                    else
                    {
                        Logger.E($"invalid rootPath {assetPath}");
                    }

                    break;
                case BundleMode.PackByEntry:
                    bundle = Path.GetFileNameWithoutExtension(entry);
                    break;
                case BundleMode.PackByCustom:
                    if (customPacker != null) bundle = customPacker?.Invoke(assetPath, entry, bundle, group);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return PackAsset(assetPath, bundle, asset.group.build);
        }


        public static string PackAsset(string assetPath, string bundle, string build)
        {
            if (build == null) build = "unnamed";
            if (_setting.packTogetherForAllShaders && _setting.shaderExtensions.Exists(assetPath.EndsWith))
                bundle = "shaders";
            if (_setting.packByFileForAllScenes && assetPath.EndsWith(".unity")) bundle = assetPath;
            bundle = _setting.applyBundleNameWithHash
                ? Utility.ComputeHash(Encoding.UTF8.GetBytes(bundle))
                : bundle.Replace(" ", "").Replace("/", "_").Replace("-", "_").Replace(".", "_").ToLower();
            bundle += _setting.bundleExtension;
            return _setting.splitBundleNameWithBuild ? $"{build.ToLower()}/{bundle}" : $"{bundle}";
        }

        public static IEnumerable<string> GetDependencies(string assetPath)
        {
            var set = new HashSet<string>();
            set.UnionWith(AssetDatabase.GetDependencies(assetPath));
            set.Remove(assetPath);
            set.RemoveWhere(s => s.EndsWith(".cs") || s.EndsWith(".dll") || s.EndsWith(".unity"));
            return set;
        }

        public static bool FindReferences(BuildAsset asset)
        {
            var type = asset.type;
            if (type == null) return false;
            return !type.Contains("TextAsset") && !type.Contains("Texture");
        }

        public static BuildAsset CreateAsset(string path, Group group, bool auto = false)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (type != null)
                return new BuildAsset
                {
                    path = path,
                    type = type.Name,
                    auto = auto,
                    group = group
                };

            Logger.W($"Invalid type for {path}");
            return new BuildAsset
            {
                path = path,
                type = "MissType",
                auto = auto,
                group = group
            };
        }
    }
}