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
        public static IBuildPipeline BuildPipeline { get; set; } = new BuiltinBuildPipeline();
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
            if (builds.Length == 0) builds = Settings.FindAssets<Build>();

            PreprocessBuildBundles?.Invoke(builds, settings);

            if (settings.bundle.checkReference && FindReferences()) return;

            CreateDirectories();

            var assets = Array.ConvertAll(builds, AssetDatabase.GetAssetPath);

            if (assets.Length == 0)
            {
                Logger.I("Nothing to build.");
                return;
            }

            var watch = new Stopwatch();
            watch.Start();
            var changes = new List<string>();
            foreach (var asset in assets)
            {
                var build = AssetDatabase.LoadAssetAtPath<Build>(asset);
                var parameters = build.parameters;
                parameters.name = build.name;
                var task = withLastBuild
                    ? BuildJob.StartNew(parameters, new LoadBuildAssets(), new BuildBundles(), new BuildVersions())
                    : parameters.optimizeDependentAssets
                        ? BuildJob.StartNew(parameters, new CollectAssets(), new ClearDuplicateAssets(),
                            new OptimizeDependentAssets(),
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
            // updateInfo.
            var size = GetChanges(changes.ToArray(), versions.GetFilename());
            var savePath = Settings.GetCachePath(BuildRecords.Filename);
            var records = Utility.LoadFromFile<BuildRecords>(savePath);
            records.Set(versions.GetFilename(), changes.ToArray(), size);
            var json = JsonUtility.ToJson(records);
            File.WriteAllText(savePath, json);
        }

        public static void BuildUpdateInfo(Versions versions, string hash, long size)
        {
            var settings = Settings.GetDefaultSettings();
            var downloadURL = $"{settings.downloadURL}{Assets.Bundles}/{Settings.Platform}/";
            var updateInfoPath = Settings.GetCachePath(UpdateInfo.Filename);
            var updateInfo = Utility.LoadFromFile<UpdateInfo>(updateInfoPath);
            updateInfo.hash = hash;
            updateInfo.size = (ulong) size;
            updateInfo.file = versions.GetFilename();
            updateInfo.version = PlayerSettings.bundleVersion;
            updateInfo.downloadURL = downloadURL;
            updateInfo.playerDownloadURL = settings.playerDownloadURL;
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
            var builds = Settings.FindAssets<Build>();
            if (builds.Length == 0)
            {
                Logger.I("Nothing to build.");
                return false;
            }

            var assets = new List<BuildAsset>();
            foreach (var build in builds)
            {
                var item = build.parameters;
                item.name = build.name;
                var task = item.optimizeDependentAssets
                    ? BuildJob.StartNew(item, new CollectAssets(), new OptimizeDependentAssets())
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

        public static ManifestBundle[] GetBundlesInBuild(Settings settings, Versions versions)
        {
            var set = new HashSet<ManifestBundle>();
            switch (settings.playerAssetsSplitMode)
            {
                case PlayerAssetsSplitMode.SplitByAssetPacksWithInstallTime:
                    if (EditorUtility.DisplayDialog("提示", "开源版本不提供分包机制，购买专业版可以解锁这个功能，立即前往？", "确定"))
                    {
                        MenuItems.OpenAbout();
                    }

                    break;
                case PlayerAssetsSplitMode.ExcludeAllAssets:
                    break;
                case PlayerAssetsSplitMode.IncludeAllAssets:
                    foreach (var version in versions.data)
                    {
                        var path = Settings.GetDataPath(version.file);
                        var manifest = Utility.LoadFromFile<Manifest>(path);
                        set.UnionWith(manifest.bundles);
                    }

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
                var file = new FileInfo(change);
                if (!file.Exists) continue;
                size += (ulong) file.Length;
                files.Add(file);
            }

            files.Sort((a, b) => b.Length.CompareTo(a.Length));
            foreach (var file in files) sb.AppendLine($"{file.FullName}({Utility.FormatBytes((ulong) file.Length)})");

            Logger.I(size > 0
                ? $"GetChanges from {name} with following files({Utility.FormatBytes(size)}):\n {sb}"
                : "Nothing changed.");
            return size;
        }
    }
}