using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace xasset.editor
{
    public class BuildBundles : IBuildStep
    {
        public void Start(BuildTask task)
        {
            if (task.assets.Count <= 0) return;
            BuildBundledAssets(task);
        }

        private static IAssetBundleManifest BuildAssetBundles(BuildTask task)
        {
            if (Builder.Pipeline == null)
                Builder.Pipeline = new DefaultBuildPipeline();

            var outputPath = Settings.PlatformCachePath;
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var options = task.parameters.options;
            var builds = task.bundles.ConvertAll(
                input =>
                    new AssetBundleBuild
                    {
                        assetNames = input.assets.ToArray(),
                        assetBundleName = input.group
                    }).ToArray();
            return Builder.Pipeline.BuildAssetBundles(outputPath, builds, options, buildTarget);
        }

        private static void BuildBundledAssets(BuildTask task)
        {
            var bundles = GetBundles(task);
            var manifest = BuildAssetBundles(task);
            if (manifest == null)
            {
                task.TreatError($"Failed to build AssetBundles with {task.parameters.name}. manifest == null");
                return;
            }

            var assetBundles = manifest.GetAllAssetBundles();
            foreach (var assetBundle in assetBundles)
                if (bundles.TryGetValue(assetBundle, out var bundle))
                {
                    var path = Settings.GetCachePath(assetBundle);
                    bundle.deps = Array.ConvertAll(manifest.GetDependencies(assetBundle),
                        input => bundles[input].id);
                    var info = new FileInfo(path);
                    if (info.Exists)
                    { 
                        bundle.size = (ulong)info.Length;
                        bundle.hash = Utility.ComputeHash(path);
                        bundle.file = $"{bundle.hash}{Settings.Extension}";
                        var newPath = Settings.GetDataPath(bundle.file);
                        Utility.CreateDirectoryIfNecessary(newPath);
                        if (File.Exists(newPath)) File.Delete(newPath);
                        info.CopyTo(newPath, true);
                    }
                    else
                    {
                        task.TreatError($"File not found: {info}");
                        return;
                    }
                }
                else
                {
                    task.TreatError($"Bundle not found: {assetBundle}");
                    return;
                }
        }

        private static Dictionary<string, BuildBundle> GetBundles(BuildTask task)
        {
            var bundles = new Dictionary<string, BuildBundle>();
            foreach (var bundle in task.bundles)
                bundles[bundle.group] = bundle;
            foreach (var entry in task.assets)
            {
                if (!bundles.TryGetValue(entry.bundle, out var bundle))
                {
                    bundle = new BuildBundle
                    {
                        id = task.bundles.Count,
                        group = entry.bundle
                    };
                    task.bundles.Add(bundle);
                    bundles[entry.bundle] = bundle;
                }

                bundle.assets.Add(entry.asset);
            }

            return bundles;
        }
    }
}