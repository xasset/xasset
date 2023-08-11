using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    [Serializable]
    public class PlayerConfig
    {
        /// <summary>
        ///     安装包资源分包模式
        /// </summary>
        [Tooltip("安装包资源分包模式")] public PlayerAssetsSplitMode assetsSplitMode = PlayerAssetsSplitMode.IncludeAllAssets;

        /// <summary>
        ///     是否开启更新，运行时有效。
        /// </summary>
        [Tooltip("是否开启更新，运行时有效。")] public bool updatable;

        /// <summary>
        ///     更新信息地址
        /// </summary>
        [Tooltip("更新信息地址")] public string updateInfoURL = "http://127.0.0.1/";

        /// <summary>
        ///     资源下载地址
        /// </summary>
        [Tooltip("资源下载地址")] public string downloadURL = "http://127.0.0.1/Bundles";

        /// <summary>
        ///     安装包下载地址
        /// </summary>
        [Tooltip("安装包下载地址")] public string playerURL = "http://127.0.0.1/Build/xasset.apk";

        /// <summary>
        ///     日志级别
        /// </summary>
        [Tooltip("日志级别")] public LogLevel logLevel = LogLevel.Debug;

        /// <summary>
        ///     最大并行下载数量
        /// </summary>
        [Range(1, 10)] [Tooltip("最大并行下载数量")] public byte maxDownloads = 5;

        /// <summary>
        ///     最大错误重试次数
        /// </summary>
        [Range(0, 5)] [Tooltip("最大错误重试次数")] public byte maxRetryTimes = 3;

        /// <summary>
        ///     每个队列最大单帧更新数量。
        /// </summary>
        [Range(0, 30)] [Tooltip("每个队列最大单帧更新数量")]
        public byte maxRequests = 10;

        /// <summary>
        ///     是否开启自动切片
        /// </summary>
        [Tooltip("是否开启自动切片")] public bool autoslicing = true;

        /// <summary>
        ///     自动切片时间，值越大处理的请求数量越多，值越小处理请求的数量越小，可以根据目标帧率分配。
        /// </summary>
        [Tooltip("自动切片时间，值越大处理的请求数量越多，值越小处理请求的数量越小，可以根据目标帧率分配")]
        public float autoslicingTimestep = 1 / 60f * 0.6f;

        /// <summary>
        ///     自动回收的时间步长
        /// </summary>
        [Tooltip("自动回收的时间步长")] public float autoreleaseTimestep = 0.7f;
    }

    [Serializable]
    public class BundleConfig
    {
        /// <summary>
        ///     打包时先检查引用关系，如果有异常会弹窗提示。
        /// </summary>
        [Tooltip("打包时先检查引用关系，如果有异常会弹窗提示。")] public bool checkReference = true;

        /// <summary>
        ///     打包清单的选项
        /// </summary>
        [Tooltip("打包清单的选项")] public BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression |
                                                                      BuildAssetBundleOptions
                                                                          .DisableLoadAssetByFileName |
                                                                      BuildAssetBundleOptions
                                                                          .DisableLoadAssetByFileNameWithExtension;

        /// <summary>
        ///     将所有场景按文件为单位打包。
        /// </summary>
        [Tooltip("将所有场景按文件为单位打包。")] public bool packByFileForAllScenes = true;

        /// <summary>
        ///     将所有 Shader 打包到一起。
        /// </summary>
        [Tooltip("将所有 Shader 打包到一起。")] public bool packTogetherForAllShaders = true;

        /// <summary>
        ///     强制使用内置管线。
        /// </summary>
        [Tooltip("强制使用内置管线。")] public bool forceUseBuiltinPipeline;

        /// <summary>
        ///     bundle 的扩展名
        /// </summary>
        [Tooltip("bundle 的扩展名")] public string extension = ".bundle";

        /// <summary>
        ///     Shader 的后缀
        /// </summary>
        [Tooltip("Shader 的后缀")] public List<string> shaders = new List<string>
            { ".shader", ".shadervariants", ".compute" };

        /// <summary>
        ///     不参与打包的文件
        /// </summary>
        [Tooltip("不参与打包的文件")] public List<string> excludeFiles = new List<string>
        {
            ".cs",
            ".cginc",
            ".hlsl",
            ".spriteatlas",
            ".dll"
        };
    }
    
    public enum PlayMode
    {
        FastPlayWithoutBuild,                // Only for Editor
        PlayByUpdateWithSimulation,          // Only for Editor
        PlayByUpdateWithRealtime,            // Update by file server
        PlayWithoutUpdate,                   // Offline Mode
    }

    [CreateAssetMenu(fileName = nameof(Settings), menuName = "xasset/" + nameof(Settings))]
    public class Settings : ScriptableObject
    {
        private static Settings _defaultSettings;
        /// <summary>
        ///     代码运行模式，
        /// </summary>
        [Tooltip("代码运行模式")]
        public PlayMode playMode = PlayMode.FastPlayWithoutBuild;
        /// <summary>
        ///     播放器设置
        /// </summary>
        [Tooltip("播放器设置")] public PlayerConfig player = new PlayerConfig();

        /// <summary>
        ///     打包 bundle 的设置
        /// </summary>
        [Tooltip("打包 bundle 的设置")] public BundleConfig bundle = new BundleConfig();

        private static string Filename => $"Assets/xasset/Config/{nameof(Settings)}.asset";

        public static string PlatformCachePath =>
            $"{Environment.CurrentDirectory}/{Assets.Bundles}Cache/{Platform}".Replace('\\', '/');

        public static string PlatformDataPath =>
            $"{Environment.CurrentDirectory.Replace('\\', '/')}/{Assets.Bundles}/{Platform}";

        public static Platform Platform => GetPlatform();

        public static string Extension => GetDefaultSettings().bundle.extension;

        /// <summary>
        ///     [path, entry, bundle, group, return(bundle)]
        /// </summary>
        public static Func<BuildEntry, string> CustomPacker { get; set; }

        public static Func<string, bool> CustomFilter { get; set; }

        public PlayerAssets GetPlayerAssets()
        {
            var assets = CreateInstance<PlayerAssets>();
            assets.version = PlayerSettings.bundleVersion;
            assets.updateInfoURL = $"{player.updateInfoURL}/{Platform}/{UpdateInfo.Filename}";
            assets.downloadURL = $"{player.downloadURL}/{Platform}";
            assets.updatable = player.updatable;
            assets.maxDownloads = player.maxDownloads;
            assets.maxRetryTimes = player.maxRetryTimes;
            assets.splitMode = player.assetsSplitMode;
            assets.logLevel = player.logLevel;
            assets.maxRequests = player.maxRequests;
            assets.autoslicingTimestep = player.autoslicingTimestep;
            assets.autoslicing = player.autoslicing;
            assets.autoreleaseTimestep = player.autoreleaseTimestep;
            if (Platform != Platform.WebGL) return assets;
            assets.splitMode = PlayerAssetsSplitMode.IncludeAllAssets;
            assets.updatable = true;
            return assets;
        }

        public static Settings GetDefaultSettings()
        {
            if (_defaultSettings != null) return _defaultSettings;
            var assets = FindAssets<Settings>();
            _defaultSettings = assets.Length > 0
                ? assets[0]
                : GetOrCreateAsset<Settings>(Filename);
            return _defaultSettings;
        }
        
        private static BuildCache _packedAssets;

        public static BuildCache GetPackedAssets()
        {
            if (_packedAssets != null)
                return _packedAssets;

            var path = AssetDatabase.GetAssetPath(GetDefaultSettings());
            var dir = Path.GetDirectoryName(path);
            _packedAssets = GetOrCreateAsset<BuildCache>($"{dir}/PackedAssets.asset");
            return _packedAssets;
        }

        public static BuildEntry GetPackedAsset(string asset)
        {
            return GetPackedAssets().GetAsset(asset);
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
            if (Directory.Exists(path))
                return new DirectoryInfo(path).LastAccessTime.ToFileTime();
            return File.Exists(path) ? new FileInfo(path).LastAccessTime.ToFileTime() : 0;
        }

        private static T GetOrCreateAsset<T>(string path, Action<T> onCreate = null) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Utility.CreateDirectoryIfNecessary(path);
            asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            onCreate?.Invoke(asset);
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

        public static string GetDirectoryName(string path)
        {
            var dir = Path.GetDirectoryName(path);
            return !string.IsNullOrEmpty(dir) ? dir.Replace("\\", "/") : string.Empty;
        }

        public static string PackAsset(BuildEntry entry)
        {
            var assetPath = entry.asset;
            if (Directory.Exists(assetPath))
                return string.Empty;
            var groupName = entry.owner.name;
            var bundle = groupName.ToLower();
            if (string.IsNullOrEmpty(entry.parent))
                entry.parent = GetDirectoryName(assetPath);

            var entryPath = entry.parent;
            var dir = GetDirectoryName(entryPath) + "/";
            assetPath = assetPath.Replace(dir, "");
            entryPath = entryPath.Replace(dir, "");
            var bundleMode = entry.bundleMode;
            switch (bundleMode)
            {
                case BundleMode.PackTogether:
                    bundle = groupName;
                    break;
                case BundleMode.PackByFolder:
                    bundle = GetDirectoryName(assetPath);
                    break;
                case BundleMode.PackByFile:
                    bundle = assetPath;
                    break;
                case BundleMode.PackByFolderTopOnly:
                    if (!string.IsNullOrEmpty(entryPath))
                    {
                        var pos = assetPath.Length < entryPath.Length + 1
                            ? -1
                            : assetPath.IndexOf("/", entryPath.Length + 1, StringComparison.Ordinal);
                        bundle = pos != -1 ? assetPath.Substring(0, pos) : entryPath;
                    }
                    else
                    {
                        Logger.E($"invalid rootPath {assetPath}");
                    }

                    break;
                case BundleMode.PackByRaw:
                    bundle = assetPath;
                    break;
                case BundleMode.PackByFileWithoutExtension:
                    var ext = Path.GetExtension(assetPath);
                    bundle = assetPath.Replace(ext, "");
                    break;
                case BundleMode.PackByCustom:
                    if (CustomPacker != null) bundle = CustomPacker?.Invoke(entry);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return PackAsset(assetPath, bundle);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static string PackAsset(string assetPath, string bundle)
        {
            var settings = GetDefaultSettings().bundle;
            if (settings.packTogetherForAllShaders && settings.shaders.Exists(assetPath.EndsWith))
                bundle = "shaders";
            if (settings.packByFileForAllScenes && assetPath.EndsWith(".unity"))
                bundle = assetPath;
            bundle = bundle.Replace(" ", "").Replace("/", "_").Replace("-", "_").Replace(".", "_").ToLower();
            return $"{bundle}{settings.extension}";
        }

        public static string[] GetDependencies(string path)
        { 
            return AssetDatabase.GetDependencies(path, true);
        }

        public static void RemoveBundles(params string[] assetPaths)
        {
            var bundles = new HashSet<ManifestBundle>();
            var versions = GetDefaultVersions();
            foreach (var version in versions.data)
            {
                var manifest = Utility.LoadFromFile<Manifest>(GetDataPath(version.file));
                foreach (var assetPath in assetPaths)
                {
                    var bundle = manifest.GetBundleWithAsset(assetPath);
                    if (bundle != null) bundles.Add(bundle);
                }
            }

            var files = new List<string>();
            foreach (var bundle in bundles)
            {
                files.Add(GetCachePath(bundle.name));
                files.Add(GetCachePath(bundle.name + ".manifest"));
                files.Add(GetDataPath(bundle.file));
            }

            var sb = new StringBuilder();
            sb.AppendLine("Delete Files:");
            foreach (var file in files)
            {
                if (!File.Exists(file)) continue;
                File.Delete(file);
                sb.AppendLine($" - {file}");
            }

            Logger.I(sb.ToString());
        }

        public static IEnumerable<string> GetChildren(BuildEntry entry)
        {
            // TODO：考虑加个缓存？
            var path = entry.asset;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return Array.Empty<string>();
            var children = new List<string>();
            var guilds = AssetDatabase.FindAssets(entry.filter, new[] { path });
            var set = new HashSet<string>();
            var exclude = GetDefaultSettings().bundle.excludeFiles;
            foreach (var guild in guilds)
            {
                var child = AssetDatabase.GUIDToAssetPath(guild);
                if (string.IsNullOrEmpty(child) || exclude.Exists(child.EndsWith)
                                                || Directory.Exists(child)
                                                || set.Contains(child)) continue;
                set.Add(child);
                if (CustomFilter != null && !CustomFilter(child))
                    continue;
                children.Add(child);
            }

            return children.ToArray();
        }

        public static void MoveAsset(string movedAsset, string movedFromAssetPath)
        {
            var builds = FindAssets<Build>();
            foreach (var build in builds)
            foreach (var group in build.groups)
            foreach (var asset in group.assets)
            {
                if (asset.asset != movedFromAssetPath) continue;
                asset.asset = movedAsset;
                EditorUtility.SetDirty(build);
                return;
            }
        }

        public static void DeleteAsset(string deletedAsset)
        {
            var builds = FindAssets<Build>();
            foreach (var build in builds)
            foreach (var group in build.groups)
                for (var index = 0; index < group.assets.Length; index++)
                {
                    var asset = group.assets[index];
                    if (asset.asset != deletedAsset) continue;
                    ArrayUtility.RemoveAt(ref group.assets, index);
                    EditorUtility.SetDirty(build);
                    return;
                }
        }

        public static void MakeSelectionAssetsGroupTo(string buildName, string groupName)
        {
            MakeSelectionAssetsGroupTo(Array.ConvertAll(Selection.assetGUIDs, AssetDatabase.GUIDToAssetPath), buildName, groupName);
        }
        
        public static void MakeSelectionAssetsGroupTo(IEnumerable<string> selectionAssets, string buildName, string groupName)
        {
            var builds = FindAssets<Build>();
            var assets = new Dictionary<string, BuildEntry>(); 
            BuildGroup selected = null;
            Build selectedBuild = null;
            foreach (var build in builds)
            {
                if (build.name == buildName)
                    selectedBuild = build;
                foreach (var group in build.groups)
                {
                    group.owner = build;
                    foreach (var entry in group.assets)
                    {
                        assets[entry.asset] = entry;
                        entry.owner = group;
                    }
    
                    if (groupName == group.name && buildName == build.name) selected = group;
                }
            } 
            
            if (selected == null)
            {
                if (selectedBuild == null)
                    selectedBuild = GetOrCreateAsset<Build>($"Assets/xasset/Config/{buildName}.asset");
                selected = new BuildGroup { name = groupName, owner = selectedBuild };
                ArrayUtility.Add(ref selectedBuild.groups, selected);
            }
            
            foreach (var path in selectionAssets)
            {
                if (!assets.TryGetValue(path, out var entry))
                { 
                    ArrayUtility.Add(ref selected.assets, new BuildEntry { asset = path, owner = selected});
                }
                else
                {
                    ArrayUtility.Add(ref selected.assets, entry);
                    ArrayUtility.Remove(ref entry.owner.assets, entry);
                    EditorUtility.SetDirty(entry.owner.owner);
                }
            }        
            EditorUtility.SetDirty(selected.owner);
            Selection.activeObject = selected.owner;
            EditorGUIUtility.PingObject(Selection.activeObject);
            EditorUtility.FocusProjectWindow();
        }
    }
}