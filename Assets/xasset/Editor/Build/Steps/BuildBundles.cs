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
            if (job.bundledAssets.Count > 0)
                if (!BuildBundlesWithBundledAssets(job))
                    return;

            if (job.rawAssets.Count == 0) return;
            BuildBundlesWithRawAssets(job);
        }

        private static void BuildBundlesWithRawAssets(BuildJob job)
        {
            foreach (var asset in job.rawAssets)
            {
                if (string.IsNullOrEmpty(asset.path))
                {
                    Logger.E($"RawAsset not found:{asset.path}");
                    continue;
                }

                var file = new FileInfo(asset.path);
                var hash = Utility.ComputeHash(asset.path);
                var name = asset.bundle.Replace(Settings.extension, string.Empty);
                var nameWithAppendHash = $"{name}_{hash}{Settings.extension}";
                var bundle = new BuildBundle
                {
                    group = asset.bundle,
                    hash = hash,
                    file = nameWithAppendHash,
                    assets = new List<string> {asset.path},
                    size = (ulong) file.Length
                };

                var path = Settings.GetDataPath(bundle.file);
                Utility.CreateDirectoryIfNecessary(path);
                if (!File.Exists(path)) file.CopyTo(path, true);

                job.bundles.Add(bundle);
            }
        }

        private static ulong BuildBundle(string path, string newPath)
        {
            var bytes = File.ReadAllBytes(path);
            var size = bytes.Length;
            Utility.CreateDirectoryIfNecessary(newPath);
            using (var writer = new BinaryWriter(File.OpenWrite(newPath)))
            {
                writer.Write(File.ReadAllBytes(path));
            }

            return (ulong) size;
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

        private static bool BuildBundlesWithBundledAssets(BuildJob job)
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
                return false;
            }

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
                        var name = assetBundle.Replace(Settings.extension, string.Empty);
                        var hash = Utility.ComputeHash(path);
                        var nameWithAppendHash = $"{name}_{hash}{Settings.extension}";
                        var buildPath = Settings.GetCachePath(nameWithAppendHash);
                        bundle.file = nameWithAppendHash;
                        bundle.size = BuildBundle(path, buildPath);
                        bundle.hash = Utility.ComputeHash(buildPath);
                        bundle.file = $"{name}_{bundle.hash}{Settings.extension}";
                        var newPath = Settings.GetDataPath(bundle.file);
                        Utility.CreateDirectoryIfNecessary(newPath);
                        if (File.Exists(newPath)) File.Delete(newPath);
                        File.Move(buildPath, newPath);
                    }
                    else
                    {
                        job.TreatError($"File not found: {info}");
                        return false;
                    }
                }
                else
                {
                    job.TreatError($"Bundle not found: {assetBundle}");
                    return false;
                }

            return true;
        }
    }
}