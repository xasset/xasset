using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    public static class Builder
    {
        public static Action<Build[], Settings> PreprocessBuildBundles { get; set; } = null;
        public static Action<string[]> PostprocessBuildBundles { get; set; } = null;

        public static void BuildBundles(params Build[] builds)
        {
            BuildBundlesInternal(false, builds);
        }

        public static void BuildBundlesWithLastBuild(params Build[] builds)
        {
            BuildBundlesInternal(true, builds);
        }

        private static void BuildBundlesInternal(bool withLastBuild, params Build[] builds)
        {
            var settings = Settings.GetDefaultSettings();
            Bundler.Initialize(settings);

            if (builds.Length == 0) builds = FindAssets<Build>();

            PreprocessBuildBundles?.Invoke(builds, settings);

            if (settings.bundleSettings.checkReference && FindReferences()) return;

            CreateDirectories();

            var paths = Array.ConvertAll(builds, AssetDatabase.GetAssetPath);

            if (paths.Length == 0)
            {
                Logger.I("Nothing to build.");
                return;
            }

            var watch = new Stopwatch();
            watch.Start();
            var changes = new List<string>();
            foreach (var item in paths)
            {
                var build = AssetDatabase.LoadAssetAtPath<Build>(item);
                var parameters = build.parameters;
                parameters.build = build.name;
                var task = withLastBuild
                    ? BuildJob.StartNew(parameters, new LoadBuildAssets(), new BuildBundles(), new BuildVersions())
                    : parameters.autoGrouping
                        ? BuildJob.StartNew(parameters, new CollectAssets(), new ClearDuplicateAssets(),
                            new AutoGrouping(),
                            new SaveBuildAssets(), new BuildBundles(), new BuildVersions())
                        : BuildJob.StartNew(parameters, new CollectAssets(), new ClearDuplicateAssets(),
                            new SaveBuildAssets(), new BuildBundles(), new BuildVersions());
                if (!string.IsNullOrEmpty(task.error))
                {
                    Logger.E(task.error);
                }
                else
                {
                    if (task.changes.Count <= 0) continue;
                    foreach (var change in task.changes) changes.Add(Settings.GetDataPath(change));
                }
            }

            watch.Stop();
            Logger.I($"Finish {nameof(BuildBundles)} with {watch.ElapsedMilliseconds / 1000f}s.");
            if (changes.Count <= 0) return;
            PostprocessBuildBundles?.Invoke(changes.ToArray());
            SaveVersions(changes);
        }

        private static void SaveVersions(List<string> changes)
        {
            var versions = Settings.GetDefaultVersions();
            var path = Settings.GetDataPath(versions.GetFilename()); 
            versions.Save(Settings.GetCachePath(Versions.Filename));
            versions.Save(path);
            changes.Add(path);
            PostprocessBuildBundles?.Invoke(changes.ToArray());
            var file = new FileInfo(path);
            BuildUpdateInfo(versions, Utility.ComputeHash(path), file.Length);
            PrintChanges(changes.ToArray());
        }

        public static void BuildUpdateInfo(Versions versions, string hash, long size)
        {
            var settings = Settings.GetDefaultSettings();
            var downloadURL = $"{settings.downloadURL}{Assets.Bundles}/{Settings.Platform}/";
            var updateInfoPath = Settings.GetDataPath(UpdateInfo.Filename);
            var updateInfo = Utility.LoadFromFile<UpdateInfo>(updateInfoPath);
            updateInfo.hash = hash;
            updateInfo.size = (ulong) size;
            updateInfo.timestamp = versions.timestamp;
            updateInfo.file = versions.GetFilename();
            updateInfo.downloadURL = downloadURL;
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

        public static bool FindReferences()
        {
            var builds = FindAssets<Build>();
            if (builds.Length == 0)
            {
                Logger.I("Nothing to build.");
                return false;
            }

            var assets = new List<BuildAsset>();
            foreach (var build in builds)
            {
                var item = build.parameters;
                item.build = build.name;
                var task = item.autoGrouping
                    ? BuildJob.StartNew(item, new CollectAssets(), new AutoGrouping())
                    : BuildJob.StartNew(item, new CollectAssets());
                if (!string.IsNullOrEmpty(task.error)) return true;

                foreach (var asset in task.bundledAssets) assets.Add(asset); 
            }

            var assetWithGroups = new Dictionary<string, HashSet<string>>();
            foreach (var asset in assets)
            {
                if (!assetWithGroups.TryGetValue(asset.path, out var refs))
                {
                    refs = new HashSet<string>();
                    assetWithGroups.Add(asset.path, refs);
                }

                refs.Add($"{asset.group.build}-{asset.group.name}");
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

        public static void RemoveBundles(params string[] assetPaths)
        {
            var bundles = new HashSet<ManifestBundle>();
            var versions = Settings.GetDefaultVersions();
            foreach (var version in versions.data)
            {
                var manifest = Utility.LoadFromFile<Manifest>(Settings.GetDataPath(version.file));
                foreach (var assetPath in assetPaths)
                {
                    var bundle = manifest.GetBundle(assetPath);
                    if (bundle != null) bundles.Add(bundle);
                }
            }

            var files = new List<string>();
            foreach (var bundle in bundles)
            {
                files.Add(Settings.GetCachePath(bundle.name));
                files.Add(Settings.GetCachePath(bundle.name + ".manifest"));
                files.Add(Settings.GetDataPath(bundle.nameWithAppendHash));
            }

            var sb = new StringBuilder();
            sb.AppendLine("Delete Files:");
            foreach (var file in files)
            {
                if (!File.Exists(file)) continue;
                File.Delete(file);
                sb.AppendLine($"Delete: {file}");
            }

            Logger.I(sb.ToString());
        }

        public static ManifestBundle[] GetBundlesInBuild(Versions versions)
        {
            var set = new HashSet<ManifestBundle>();
            foreach (var version in versions.data)
            {
                var path = Settings.GetDataPath(version.file);
                var manifest = Utility.LoadFromFile<Manifest>(path);
                set.UnionWith(manifest.bundles);
            }
            return set.ToArray();
        } 
        
        private static void PrintChanges(IEnumerable<string> changes)
        {
            var sb = new StringBuilder();
            var size = 0UL;
            var files = new List<FileInfo>();
            foreach (var change in changes)
            {
                var file = new FileInfo(change);
                if (!file.Exists) continue;
                size += (ulong) file.Length;
                files.Add(file);
            }

            files.Sort((a, b) => b.Length.CompareTo(a.Length));
            foreach (var file in files) sb.AppendLine($"{file.FullName}({Utility.FormatBytes((ulong) file.Length)})");

            Logger.I(size > 0
                ? $"GetChanges with following files({Utility.FormatBytes(size)}):\n {sb}"
                : "Nothing changed.");
        }

        private static string GetTimeForNow()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private static string GetBuildTargetName(BuildTarget target)
        {
            var targetName =
                $"{PlayerSettings.productName}-v{PlayerSettings.bundleVersion}-{GetTimeForNow()}";
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

        public static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Utility.CreateDirectoryIfNecessary(path);
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static void ClearHistory()
        {
            var usedFiles = new List<string>
            {
                Settings.GetCachePath(Settings.Platform.ToString()),
                Settings.GetCachePath($"{Settings.Platform}.manifest"),
                Settings.GetCachePath(Versions.Filename),
                Settings.GetDataPath(UpdateInfo.Filename)
            };

            var updateInfo = Utility.LoadFromFile<UpdateInfo>(Settings.GetDataPath(UpdateInfo.Filename));
            usedFiles.Add(Settings.GetDataPath(updateInfo.file));
            var versions = Utility.LoadFromFile<Versions>(Settings.GetDataPath(updateInfo.file));

            foreach (var version in versions.data)
            {
                usedFiles.Add(Settings.GetCachePath($"{version.build}.json"));
                usedFiles.Add(Settings.GetDataPath(version.file));
                var manifest = Utility.LoadFromFile<Manifest>(Settings.GetDataPath(version.file));
                foreach (var bundle in manifest.bundles)
                {
                    usedFiles.Add(Settings.GetCachePath(bundle.name));
                    usedFiles.Add(Settings.GetCachePath($"{bundle.name}.manifest"));
                    usedFiles.Add(Settings.GetDataPath(bundle.nameWithAppendHash));
                } 
            }

            var files = new List<string>();
            var dirs = new[] {Settings.PlatformDataPath, Settings.PlatformDataPath};
            foreach (var dir in dirs)
                if (Directory.Exists(dir))
                    files.AddRange(Directory.GetFiles(dir, "*", SearchOption.AllDirectories));

            var sb = new StringBuilder();
            sb.AppendLine("Delete files:");
            foreach (var file in files)
            {
                var path = file.Replace("\\", "/");
                if (usedFiles.Exists(path.Equals))
                    continue;

                File.Delete(path);
                sb.AppendLine(path);
            }

            Logger.I(sb);
        }
    }
}