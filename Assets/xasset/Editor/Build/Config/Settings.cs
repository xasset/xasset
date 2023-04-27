using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace xasset.editor
{
    public enum PlayerAssetsSplitMode
    {
        SplitByAssetPacksWithInstallTime,
        IncludeAllAssets,
        ExcludeAllAssets
    }

    [CreateAssetMenu(fileName = nameof(Settings), menuName = "xasset/" + nameof(Settings))]
    public class Settings : ScriptableObject
    {
        /// <summary>
        ///     更新信息地址
        /// </summary>
        [Header("Player")] [Tooltip("更新信息地址")] public string updateInfoURL = "http://127.0.0.1/";

        /// <summary>
        ///     资源下载地址
        /// </summary>
        [Tooltip("资源下载地址")] public string bundleDownloadURL = "http://127.0.0.1/Bundles";

        /// <summary>
        ///     安装包下载地址
        /// </summary>
        [Tooltip("安装包下载地址")] public string playerDownloadURL = "http://127.0.0.1/Build/xasset.apk";

        /// <summary>
        ///     安装包资源分包模式
        /// </summary>
        [Tooltip("安装包资源分包模式")]
        public PlayerAssetsSplitMode playerAssetsSplitMode = PlayerAssetsSplitMode.IncludeAllAssets;

        /// <summary>
        ///     是否将安装包内的资源二次打包，Android 设备上开启这个选项可以优化 IO 效率。
        /// </summary>
        [Tooltip("是否将安装包内的资源二次打包，Android 设备上开启这个选项可以优化 IO 效率")]
        public bool packPlayerAssets = true;

        /// <summary>
        ///     离线模式，运行时有效，开启后不会触发更新。
        /// </summary>
        [Tooltip("离线模式，运行时有效，开启后不会触发更新。")] public bool offlineMode;

        /// <summary>
        ///     日志级别
        /// </summary>
        [Tooltip("日志级别")] public LogLevel logLevel = LogLevel.Debug;

        /// <summary>
        ///     打包 bundle 的设置
        /// </summary>
        [Tooltip("打包 bundle 的设置")] public BundleSettings bundle = new BundleSettings();

        /// <summary>
        ///     编辑器仿真模式，开启后，无需打包可以进入播放模式。
        /// </summary>
        [Header("Development")] [Tooltip("编辑器仿真模式，开启后，无需打包可以进入播放模式")]
        public bool simulationMode = true;

        /// <summary>
        ///     编辑器下是否开启仿真下载模式，开启后，无需把资源部署到服务器，就行运行真机的更新过程。
        /// </summary>
        [Tooltip("编辑器下是否开启仿真下载模式，开启后，无需把资源部署到服务器，就行运行真机的更新过程")]
        public bool simulationDownload = true;

        /// <summary>
        ///     编辑器仿真模式下，是否采集所有资源，需要更长的初始化耗时，但是可以及时对未采集的内容进行预警。
        /// </summary>
        [Tooltip("编辑器仿真模式下，是否采集所有资源，需要更长的初始化耗时，但是可以及时对未采集的内容进行预警。")]
        public bool collectAllAssets;

        /// <summary>
        ///     最大并行下载数量
        /// </summary>
        [Header("Download")] [Range(1, 10)] [Tooltip("最大并行下载数量")]
        public byte maxDownloads = 5;

        /// <summary>
        ///     最大错误重试次数
        /// </summary>
        [Range(0, 5)] [Tooltip("最大错误重试次数")] public byte maxRetryTimes = 3;

        /// <summary>
        ///     每个队列最大单帧更新数量。
        /// </summary>
        [Header("Scheduler")] [Range(1, 30)] [Tooltip("每个队列最大单帧更新数量")]
        public byte maxRequests = 10;

        /// <summary>
        ///     自动切片时间，值越大处理的请求数量越多，值越小处理请求的数量越小，可以根据目标帧率分配。
        /// </summary>
        [Tooltip("自动切片时间，值越大处理的请求数量越多，值越小处理请求的数量越小，可以根据目标帧率分配")]
        public float autoSliceTimestep = 1 / 16f;

        /// <summary>
        ///     是否开启自动切片
        /// </summary>
        [Tooltip("是否开启自动切片")] public bool autoSlicing = true;

        /// <summary>
        ///     自动回收的时间步长
        /// </summary>
        [Header("Recycler")] [Tooltip("自动回收的时间步长")]
        public float autoRecycleTimestep = 0.7f;

        private static string Filename => $"Assets/xasset/Config/{nameof(Settings)}.asset";

        public static BuildGroup GetAutoGroup()
        {
            var group = GetOrCreateAsset<BuildGroup>($"Assets/xasset/Config/Auto.asset");
            group.bundleMode = BundleMode.PackByCustom;
            group.addressMode = AddressMode.LoadByDependencies;
            return group;
        }

        public PlayerAssets GetPlayerAssets()
        {
            var assets = CreateInstance<PlayerAssets>();
            assets.version = PlayerSettings.bundleVersion;
            assets.updateInfoURL = $"{updateInfoURL}{Assets.Bundles}/{Platform}/{UpdateInfo.Filename}";
            assets.downloadURL = $"{bundleDownloadURL}{Assets.Bundles}/{Platform}";
            assets.offlineMode = offlineMode;
            assets.maxDownloads = maxDownloads;
            assets.maxRetryTimes = maxRetryTimes;
            assets.logLevel = logLevel;
            assets.maxRequests = maxRequests;
            assets.autoSliceTimestep = autoSliceTimestep;
            assets.autoSlicing = autoSlicing;
            assets.autoRecycleTimestep = autoRecycleTimestep;
            return assets;
        }

        public static string PlatformCachePath =>
            $"{Environment.CurrentDirectory}/{Assets.Bundles}Cache/{Platform}".Replace('\\', '/');

        public static string PlatformDataPath =>
            $"{Environment.CurrentDirectory.Replace('\\', '/')}/{Assets.Bundles}/{Platform}";

        public static Platform Platform => GetPlatform();

        private static Settings defaultSettings;

        public static Settings GetDefaultSettings()
        {
            if (defaultSettings != null) return defaultSettings;
            var assets = FindAssets<Settings>();
            defaultSettings = assets.Length > 0
                ? assets[0]
                : GetOrCreateAsset<Settings>(Filename);
            return defaultSettings;
        }

        public static Versions GetDefaultVersions()
        {
            var versions = Utility.LoadFromFile<Versions>(GetCachePath(Versions.Filename));
            return versions;
        }

        public static string GetDataPath(string filename)
        {
            return $"{PlatformDataPath}/{filename}";
        }

        public static string GetCachePath(string filename)
        {
            return $"{PlatformCachePath}/{filename}";
        }

        private static Platform GetPlatform()
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return Platform.Android;
                case BuildTarget.StandaloneOSX:
                    return Platform.OSX;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return Platform.Windows;
                case BuildTarget.iOS:
                    return Platform.iOS;
                case BuildTarget.WebGL:
                    return Platform.WebGL;
                case BuildTarget.StandaloneLinux64:
                    return Platform.Linux;
                default:
                    return Platform.Default;
            }
        }

        public static long GetLastWriteTime(string path)
        {
            var file = new FileInfo(path);
            return file.Exists ? file.LastAccessTime.ToFileTime() : 0;
        }

        public static string[] GetDependenciesWithoutCache(string assetPath)
        {
            var set = new HashSet<string>();
            set.UnionWith(AssetDatabase.GetDependencies(assetPath));
            set.Remove(assetPath);
            var exclude = GetDefaultSettings().bundle.excludeFiles;
            // Unity 会存在场景依赖场景的情况。
            set.RemoveWhere(s => s.EndsWith(".unity") || exclude.Exists(s.EndsWith));
            return set.ToArray();
        }

        public static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Utility.CreateDirectoryIfNecessary(path);
            asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static T[] FindAssets<T>() where T : ScriptableObject
        {
            var builds = new List<T>();
            var guilds = AssetDatabase.FindAssets("t:" + typeof(T).FullName);
            foreach (var guild in guilds)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guild);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset == null) continue;

                builds.Add(asset);
            }

            return builds.ToArray();
        }

        public static BundleSettings BundleSettings => GetDefaultSettings().bundle;
        public static string extension => BundleSettings.extension;

        /// <summary>
        ///     [path, entry, bundle, group, return(bundle)]
        /// </summary>
        public static Func<string, string, string, BuildGroup, string> customPacker { get; set; } = null;

        public static Func<string, bool> customFilter { get; set; } = s => true;

        private static string GetDirectoryName(string path)
        {
            var dir = Path.GetDirectoryName(path);
            return dir?.Replace("\\", "/");
        }

        public static string PackAsset(BuildAsset asset)
        {
            var assetPath = asset.path;
            var group = asset.group;
            if (group == null)
            {
                asset.addressMode = AddressMode.LoadByDependencies;
                return "auto";
            }

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
                    bundle = assetPath;
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

            return PackAsset(assetPath, bundle, group.build);
        }

        public static string PackAsset(string assetPath, string bundle, string build)
        {
            var settings = GetDefaultSettings().bundle;

            if (settings.packTogetherForAllShaders && settings.shaderExtensions.Exists(assetPath.EndsWith))
                bundle = "shaders";

            if (settings.packByFileForAllScenes && assetPath.EndsWith(".unity"))
                bundle = assetPath;

            bundle = $"{FixedName(bundle)}{settings.extension}";

            if (!string.IsNullOrEmpty(build) && settings.splitBundleNameWithBuild)
                return $"{build.ToLower()}/{bundle}";

            return $"{bundle}";
        }

        private static string FixedName(string bundle)
        {
            return bundle.Replace(" ", "").Replace("/", "_").Replace("-", "_").Replace(".", "_").ToLower();
        }

        public static bool FindReferences(BuildAsset asset)
        {
            var type = asset.type;
            if (type == null) return false;
            return !type.Contains("TextAsset") && !type.Contains("Texture");
        }

        public static BuildAsset GetAsset(string path)
        {
            return GetBuildAssetCache().GetAsset(path);
        }

        private static BuildAssetCache BuildAssetCache;

        public static string[] GetDependencies(string assetPath)
        {
            return GetBuildAssetCache().GetDependencies(assetPath);
        }

        public static BuildAssetCache GetBuildAssetCache()
        {
            if (BuildAssetCache == null)
            {
                BuildAssetCache = GetOrCreateAsset<BuildAssetCache>(BuildAssetCache.Filename);
            }

            return BuildAssetCache;
        }

        private static BuildEntryCache BuildEntryCache;

        public static BuildEntryCache GetBuildEntryCache()
        {
            if (BuildEntryCache == null)
            {
                BuildEntryCache = GetOrCreateAsset<BuildEntryCache>(BuildEntryCache.Filename);
            }

            return BuildEntryCache;
        }

        public static BuildAsset[] Collect(BuildGroup group)
        {
            var assets = new List<BuildAsset>();
            if (group.entries == null) return assets.ToArray();
            foreach (var entry in group.entries)
            {
                GetAssets(group, entry, assets);
            }

            return assets.ToArray();
        }

        private static void GetAssets(BuildGroup group, Object entry, List<BuildAsset> assets)
        {
            var path = AssetDatabase.GetAssetPath(entry);
            if (string.IsNullOrEmpty(path)) return;
            var cache = GetBuildEntryCache();
            var entryAsset = cache.GetEntry(path, group);
            assets.AddRange(entryAsset.assets);
        }
    }
}