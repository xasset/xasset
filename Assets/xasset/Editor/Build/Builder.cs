using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace xasset.editor
{
    public class Builder : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public const string ErrorFile = "BuildErrors.txt";

        public static IBuildPipeline Pipeline { get; set; } = new DefaultBuildPipeline();
        public static Action<Build[], Settings> PreprocessBuildBundles { get; set; }
        public static Action<BuildTask[], string[]> PostprocessBuildBundles { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static Action<Versions> CustomPlayerAssetsBuilder { get; set; }

        public void OnPostprocessBuild(BuildReport report)
        {
            var dataPath = $"{Application.streamingAssetsPath}/{Assets.Bundles}";
            if (!Directory.Exists(dataPath)) return;
            FileUtil.DeleteFileOrDirectory(dataPath);
            FileUtil.DeleteFileOrDirectory(dataPath + ".meta");
            if (Directory.GetFiles(Application.streamingAssetsPath).Length != 0) return;
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + ".meta");
        }

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildPlayerAssets();
        }

        public static bool HasError()
        {
            return File.Exists(ErrorFile);
        }

        public static void BuildBundles(params Build[] builds)
        {
            BuildBundlesInternal(false, builds);
        }

        public static void BuildBundlesWithCache(params Build[] builds)
        {
            BuildBundlesInternal(true, builds);
        }

        private static void BuildBundlesInternal(bool withCache, params Build[] builds)
        {
            var settings = Settings.GetDefaultSettings();
            if (settings.bundle.forceUseBuiltinPipeline)
                Pipeline = new DefaultBuildPipeline();

            if (builds.Length == 0) builds = Settings.FindAssets<Build>();
            for (var index = 0; index < builds.Length; index++)
            {
                var build = builds[index];
                if (build.enabled) continue;
                ArrayUtility.RemoveAt(ref builds, index);
                index--;
            }

            Array.Sort(builds, (a, b) => a.id.CompareTo(b.id));

            if (File.Exists(ErrorFile)) File.Delete(ErrorFile);

            ClearBuildCache();

            PreprocessBuildBundles?.Invoke(builds, settings);

            if (settings.bundle.checkReference && FindReferences()) return;

            CreateDirectories();

            var assets = Array.ConvertAll(builds, AssetDatabase.GetAssetPath);

            if (assets.Length == 0)
            {
                Logger.I("Nothing to build.");
                return;
            }

            var tasks = new List<BuildTask>();
            var watch = new Stopwatch();
            watch.Start();
            var changes = new List<string>();
            var errors = new List<string>();
            foreach (var asset in assets)
            {
                var build = AssetDatabase.LoadAssetAtPath<Build>(asset);
                var parameters = build.parameters;
                parameters.name = build.name;
                var jobs = new List<IBuildStep>();
                if (withCache)
                {
                    jobs.Add(new LoadBuildAssets());
                    jobs.Add(new BuildBundles());
                    jobs.Add(new BuildVersions());
                }
                else
                {
                    jobs.Add(new CollectAssets());
                    if (parameters.optimizeDependencies)
                        jobs.Add(new OptimizeDependencies());
                    jobs.Add(new SaveBuildAssets());
                    jobs.Add(new BuildBundles());
                    jobs.Add(new BuildVersions());
                }

                var task = BuildTask.StartNew(build, jobs.ToArray());
                tasks.Add(task);
                if (!string.IsNullOrEmpty(task.error))
                {
                    if (!task.nothingToBuild)
                    {
                        Logger.E(task.error);
                        errors.Add($"Failed to build {task.parameters.name} with error {task.error}.");
                    }
                    else
                    {
                        Logger.E(task.error);
                    }
                }
                else
                {
                    if (task.changes.Count <= 0) continue;
                    foreach (var change in task.changes) changes.Add(change);
                }
            }

            if (errors.Count > 0)
                File.WriteAllText(ErrorFile, string.Join("\n", errors));

            watch.Stop();
            Logger.I($"Finish {nameof(BuildBundles)} with {watch.ElapsedMilliseconds / 1000f}s.");
            if (changes.Count <= 0) return;
            SaveVersions(changes);
            PostprocessBuildBundles?.Invoke(tasks.ToArray(), changes.ToArray());
        }

        private static void ClearBuildCache()
        {
            var versions = Settings.GetDefaultVersions();
            var builds = Settings.FindAssets<Build>();
            var removed = 0;
            for (var index = 0; index < versions.data.Count; index++)
            {
                var version = versions.data[index];
                if (Array.Exists(builds, build => version.name.Equals(build.name))) continue;
                versions.data.RemoveAt(index);
                index--;
                removed++;
            }

            if (removed <= 0) return;
            versions.Save(Settings.GetCachePath(Versions.Filename));
            versions.Save(Settings.GetCachePath(versions.GetFilename()));
        }

        private static void SaveVersions(List<string> changes)
        {
            var versions = Settings.GetDefaultVersions(); 
            versions.Save(Settings.GetCachePath(Versions.Filename));
            versions = BuildBundleVersions(versions, changes);
            changes.Add(versions.GetFilename());
            var path = Settings.GetDataPath(versions.GetFilename());
            var file = new FileInfo(path);
            BuildUpdateInfo(versions, Utility.ComputeHash(path), file.Length);
            // updateInfo.
            var size = GetChanges(changes.ToArray(), versions.GetFilename());
            var savePath = Settings.GetCachePath(BuildChanges.Filename);
            var records = Utility.LoadFromFile<BuildChanges>(savePath);
            records.Set(versions.GetFilename(), changes.ToArray(), size);
            var json = JsonUtility.ToJson(records);
            File.WriteAllText(savePath, json);
        }

        private static Versions BuildBundleVersions(Versions versions, List<string> changes)
        {
            var builds = new List<AssetBundleBuild>();
            var versionList = new List<Version>();
            foreach (var version in versions.data)
            {
                var manifest = Utility.LoadFromFile<Manifest>(Settings.GetCachePath(version.file));
                var assetPath = $"Assets/{version.name}.asset";
                AssetDatabase.CreateAsset(manifest, assetPath);
                builds.Add(new AssetBundleBuild
                {
                    assetNames = new[] { assetPath },
                    assetBundleName = $"{version.name.ToLower()}.json"
                });
                versionList.Add(version);
            }

            var target = EditorUserBuildSettings.activeBuildTarget;
            var options = Settings.GetDefaultSettings().bundle.options;
            Pipeline.BuildAssetBundles(Settings.PlatformCachePath, builds.ToArray(), options, target);
            foreach (var version in versionList)
            {
                var bundle = $"{version.name.ToLower()}.json";
                var src = new FileInfo(Settings.GetCachePath(bundle));
                var assetPath = $"Assets/{version.name}.asset";
                AssetDatabase.DeleteAsset(assetPath);
                if (src.Exists)
                {
                    version.size = (ulong)src.Length;
                    version.hash = Utility.ComputeHash(src.FullName);
                    version.file = version.GetFilename();
                    var to = Settings.GetDataPath(version.file);
                    if (File.Exists(to)) continue;
                    src.CopyTo(to, true);
                    changes.Add(version.file);
                }
                else
                {
                    Debug.LogError($"File not found {bundle}.");
                }
            }

            versions.data = versionList;
            versions.Save(Settings.GetDataPath(versions.GetFilename()));
            versions.Save(Settings.GetCachePath(Versions.BundleFilename));
            return versions;
        }

        public static void BuildUpdateInfo(Versions versions, string hash, long size)
        {
            var settings = Settings.GetDefaultSettings();
            var downloadURL = $"{settings.player.downloadURL}/{Settings.Platform}";
            var updateInfoPath = Settings.GetCachePath(UpdateInfo.Filename);
            var updateInfo = Utility.LoadFromFile<UpdateInfo>(updateInfoPath);
            updateInfo.hash = hash;
            updateInfo.size = (ulong)size;
            updateInfo.timestamp = versions.timestamp;
            updateInfo.file = versions.GetFilename();
            updateInfo.downloadURL = downloadURL;
            updateInfo.playerURL = settings.player.playerURL;
            updateInfo.version = PlayerSettings.bundleVersion;
            var version = System.Version.Parse(updateInfo.version);
            Logger.I($"Bundle Version:{version}");
            File.WriteAllText(updateInfoPath, JsonUtility.ToJson(updateInfo));
        }

        private static void CreateDirectories()
        {
            var directories = new List<string>
            {
                Settings.PlatformCachePath, Settings.PlatformDataPath
            };

            foreach (var directory in directories)
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
        }

        [MenuItem("xasset/Check References")]
        public static void CheckReferences()
        {
            FindReferences();
        }

        public static bool FindReferences()
        {
            var builds = Settings.FindAssets<Build>();
            if (builds.Length == 0)
            {
                Logger.I("Nothing to build.");
                return false;
            }

            var assetWithGroups = new Dictionary<string, HashSet<string>>();
            foreach (var build in builds)
            {
                var item = build.parameters;
                item.name = build.name;
                var task = item.optimizeDependencies
                    ? BuildTask.StartNew(build, new CollectAssets(), new OptimizeDependencies())
                    : BuildTask.StartNew(build, new CollectAssets());
                if (!string.IsNullOrEmpty(task.error)) return true;

                foreach (var entry in task.assets)
                {
                    if (!assetWithGroups.TryGetValue(entry.asset, out var refs))
                    {
                        refs = new HashSet<string>();
                        assetWithGroups.Add(entry.asset, refs);
                    } 
                    refs.Add($"{entry.owner.build}-{entry.owner.name}");
                }
            } 

            var sb = new StringBuilder();
            foreach (var pair in assetWithGroups.Where(pair => pair.Value.Count > 1))
            {
                sb.AppendLine(pair.Key);
                foreach (var s in pair.Value) sb.AppendLine(" - " + s);
            }

            var content = sb.ToString();
            if (string.IsNullOrEmpty(content))
            {
                Logger.I("Checking completed, Everything is ok.");
                return false;
            }

            const string filename = "MultipleReferences.txt";
            File.WriteAllText(filename, content);
            if (EditorUtility.DisplayDialog("提示", "检测到多重引用关系，是否打开查看？", "确定"))
                EditorUtility.OpenWithDefaultApp(filename);
            return true;
        }

        public static ManifestBundle[] GetBundlesInBuild(Settings settings, Versions versions)
        {
            var set = new HashSet<ManifestBundle>();
            if (settings.player.assetsSplitMode != PlayerAssetsSplitMode.ExcludeAllAssets)
                foreach (var version in versions.data)
                    version.Load(Settings.GetDataPath(version.file));

            switch (settings.player.assetsSplitMode)
            {
                case PlayerAssetsSplitMode.IncludeInstallTimeAssetsOnly:
                    if (EditorUtility.DisplayDialog("Tips", "This feature is pro only. goto https://xasset.cc see more info.", "Ok"))
                        MenuItems.OpenAbout();
                    break;
                case PlayerAssetsSplitMode.ExcludeAllAssets:
                    break;
                case PlayerAssetsSplitMode.IncludeAllAssets:
                    foreach (var version in versions.data) set.UnionWith(version.manifest.bundles);

                    break;
            }

            return set.ToArray();
        }

        public static ulong GetChanges(IEnumerable<string> changes, string name)
        {
            var sb = new StringBuilder();
            var size = 0UL;
            var files = new List<FileInfo>();
            foreach (var change in changes)
            {
                var file = new FileInfo(Settings.GetDataPath(change));
                if (!file.Exists) continue;
                size += (ulong)file.Length;
                files.Add(file);
            }

            foreach (var file in files) sb.AppendLine($"{file.FullName}({Utility.FormatBytes((ulong)file.Length)})");

            Logger.I(size > 0
                ? $"GetChanges from {name} with following files({Utility.FormatBytes(size)}):\n{sb}"
                : "Nothing changed.");
            return size;
        }

        /// <summary>
        ///     清理历史打包文件
        /// </summary>
        public static void ClearHistory()
        {
            AssetBundle.UnloadAllAssetBundles(true);

            var usedFiles = new List<string>
            {
                Settings.GetCachePath(Settings.Platform.ToString()),
                Settings.GetCachePath($"{Settings.Platform}.manifest"),
                Settings.GetCachePath(Versions.Filename),
                Settings.GetCachePath(Versions.BundleFilename),
                Settings.GetCachePath(UpdateInfo.Filename),
                Settings.GetCachePath(BuildChanges.Filename),
                Settings.GetCachePath("buildlogtep.json")
            };

            var versions = Utility.LoadFromFile<Versions>(Settings.GetCachePath(Versions.BundleFilename));
            usedFiles.Add(Settings.GetDataPath(versions.GetFilename()));
            foreach (var version in versions.data)
            {
                usedFiles.Add(Settings.GetCachePath($"{version.name.ToLower()}.json"));
                var manifestFile = Settings.GetCachePath($"{version.name.ToLower()}.json.manifest");
                if (File.Exists(manifestFile))
                    usedFiles.Add(manifestFile);
                usedFiles.Add(
                    Settings.GetCachePath($"{nameof(BuildCache)}{version.name}.json")); // build assets.json
                usedFiles.Add(Settings.GetDataPath(version.file));
                version.Load(Settings.GetDataPath(version.file));
                var manifest = version.manifest;
                foreach (var bundle in manifest.bundles)
                {
                    usedFiles.Add(Settings.GetCachePath(bundle.name));
                    manifestFile = Settings.GetCachePath($"{bundle.name}.manifest");
                    if (File.Exists(manifestFile))
                        usedFiles.Add(manifestFile);
                    usedFiles.Add(Settings.GetDataPath(bundle.file));
                } 
            }

            versions = Settings.GetDefaultVersions();
            foreach (var version in versions.data)
                usedFiles.Add(Settings.GetCachePath(version.file));

            var files = new List<string>();
            var dirs = new[] { Settings.PlatformDataPath, Settings.PlatformCachePath };
            foreach (var dir in dirs)
                if (Directory.Exists(dir))
                {
                    var result = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
                    foreach (var file in result)
                        files.Add(file.Replace("\\", "/"));
                }

            var sb = new StringBuilder();
            files.RemoveAll(input => usedFiles.Contains(input));
            sb.AppendLine($"Delete files({files.Count}):");
            foreach (var file in files)
            {
                File.Delete(file);
                sb.AppendLine(file);
            }

            Logger.I(sb);
        }

        private static string GetBuildTargetName(BuildTarget target)
        {
            var timeForNow = DateTime.Now.ToString("yyyyMMddHHmmss");
            var targetName =
                $"{PlayerSettings.productName}-v{PlayerSettings.bundleVersion}-{timeForNow}";
            switch (target)
            {
                case BuildTarget.Android:
                    return targetName + (EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk");
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return targetName + ".exe";
                case BuildTarget.StandaloneOSX:
                    return targetName + ".app";
                default:
                    return targetName;
            }
        }

        public static void BuildPlayer(string path = null)
        {
            if (string.IsNullOrEmpty(path)) path = $"Build/{Settings.Platform}";

            var levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled)
                    levels.Add(scene.path);

            if (levels.Count == 0)
            {
                Logger.I("Nothing to build.");
                return;
            }

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetName = GetBuildTargetName(buildTarget);
            if (targetName == null) return;

            var options = new BuildPlayerOptions
            {
                scenes = levels.ToArray(),
                locationPathName = $"{path}/{targetName}",
                target = buildTarget,
                options = EditorUserBuildSettings.development
                    ? BuildOptions.Development
                    : BuildOptions.None
            };
            BuildPipeline.BuildPlayer(options);
            EditorUtility.OpenWithDefaultApp(path);
        }

        public static void BuildPlayerAssets(Versions versions = null)
        {
            if (versions == null)
                versions = Utility.LoadFromFile<Versions>(Settings.GetCachePath(Versions.BundleFilename));
            if (CustomPlayerAssetsBuilder != null)
            {
                CustomPlayerAssetsBuilder.Invoke(versions);
                return;
            }

            Assets.PlayerDataPath = $"{Application.streamingAssetsPath}/{Assets.Bundles}";
            var settings = Settings.GetDefaultSettings();
            if (Settings.Platform == Platform.iOS || Settings.Platform == Platform.WebGL)
                // iOS 下散文件 IO 效率更高
            {
            }

            var playerAssets = settings.GetPlayerAssets();
            if (Directory.Exists(Assets.PlayerDataPath))
            {
                FileUtil.DeleteFileOrDirectory(Assets.PlayerDataPath);
                FileUtil.DeleteFileOrDirectory($"{Assets.PlayerDataPath}.meta");
            }

            var bundles = GetBundlesInBuild(settings, versions);
            if (bundles.Length > 0)
                CopyBundles(bundles, playerAssets);

            // 保存版本文件
            foreach (var version in versions.data)
            {
                var from = Settings.GetDataPath(version.file);
                var to = Assets.GetPlayerDataPath(version.file);
                Utility.CreateDirectoryIfNecessary(to);
                File.Copy(from, to, true);
            }

            // WebGL 不需要搞 Player Assets。
            if (Settings.Platform == Platform.WebGL)
                playerAssets.data.Clear();

            var json = JsonUtility.ToJson(playerAssets);
            // settings.json
            var path = Assets.GetPlayerDataPath(PlayerAssets.Filename);
            Utility.CreateDirectoryIfNecessary(path);
            File.WriteAllText(path, json);
            // versions.json
            path = Assets.GetPlayerDataPath(Versions.Filename);
            Utility.CreateDirectoryIfNecessary(path);
            versions.Save(path);
        }

        private static void CopyBundles(IEnumerable<ManifestBundle> bundles, PlayerAssets playerAssets)
        {  
            foreach (var bundle in bundles)
            {
                var from = Settings.GetDataPath(bundle.file);
                var to = Assets.GetPlayerDataPath(bundle.file);
                var file = new FileInfo(from);
                if (file.Exists)
                {
                    Utility.CreateDirectoryIfNecessary(to);
                    file.CopyTo(to, true);
                    playerAssets.data.Add(bundle.hash);
                }
                else
                {
                    Logger.E($"File not found: {from}");
                }
            }
        }
    }
}