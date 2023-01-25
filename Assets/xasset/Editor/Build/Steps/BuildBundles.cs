using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace xasset.editor
{
    public class BuildBundles : IBuildJobStep
    {
        public void Start(BuildJob job)
        {
            if (job.bundledAssets.Count <= 0) return;
            BuildBundlesWithBundledAssets(job);
        }

        private static IAssetBundleManifest BuildAssetBundles(BuildJob job)
        {
            if (Builder.BuildPipeline != null)
                return Builder.BuildPipeline.BuildAssetBundles(Settings.PlatformCachePath, job.bundles.ConvertAll(
                    input =>
                        new AssetBundleBuild
                        {
                            assetNames = input.assets.ToArray(),
                            assetBundleName = input.group
                        }).ToArray(), job.parameters.options, EditorUserBuildSettings.activeBuildTarget);

            job.TreatError("Pipeline == null");
            return null;
        }

        private static void BuildBundlesWithBundledAssets(BuildJob job)
        {
            var nameWithBundles = job.bundles.ToDictionary(o => o.group);
            foreach (var asset in job.bundledAssets)
            {
                if (!nameWithBundles.TryGetValue(asset.bundle, out var bundle))
                {
                    var id = job.bundles.Count;
                    bundle = new BuildBundle
                    {
                        id = id,
                        group = asset.bundle,
                        assets = new List<string>()
                    };
                    job.bundles.Add(bundle);
                    nameWithBundles[asset.bundle] = bundle;
                }

                bundle.assets.Add(asset.path);
            }

            var manifest = BuildAssetBundles(job);

            if (manifest == null)
            {
                job.TreatError($"Failed to build AssetBundles with {job.parameters.name}.");
                return;
            }

            var settings = Settings.GetDefaultSettings().bundle;

            var assetBundles = manifest.GetAllAssetBundles();
            foreach (var assetBundle in assetBundles)
                if (nameWithBundles.TryGetValue(assetBundle, out var bundle))
                {
                    var path = Settings.GetCachePath(assetBundle);
                    bundle.deps = Array.ConvertAll(manifest.GetAllDependencies(assetBundle),
                        input => nameWithBundles[input].id);
                    var info = new FileInfo(path);
                    if (info.Exists)
                    {
                        var hash = Utility.ComputeHash(path);
                        var nameWithAppendHash = $"{hash}{Settings.extension}";
                        if (settings.saveBundleName)
                        {
                            var name = assetBundle.Replace(Settings.extension, string.Empty);
                            nameWithAppendHash = $"{name}_{nameWithAppendHash}";
                        }

                        bundle.size = (ulong) info.Length;
                        bundle.hash = hash;
                        bundle.file = nameWithAppendHash;
                        var newPath = Settings.GetDataPath(bundle.file);
                        if (File.Exists(newPath)) continue;
                        Utility.CreateDirectoryIfNecessary(newPath);
                        File.Copy(path, newPath, true);
                    }
                    else
                    {
                        job.TreatError($"File not found: {info}");
                        return;
                    }
                }
                else
                {
                    job.TreatError($"Bundle not found: {assetBundle}");
                    return;
                }
        }
    }
}