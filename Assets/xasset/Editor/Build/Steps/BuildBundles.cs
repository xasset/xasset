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
                BuildBundlesWithBundledAssets(job);
        }

        private static ulong BuildAssetBundle(string path, string newPath)
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

        private static void BuildBundlesWithBundledAssets(BuildJob job)
        {
            var nameWithBundles = job.bundles.ToDictionary(o => o.name);
            foreach (var asset in job.bundledAssets)
            {
                if (!nameWithBundles.TryGetValue(asset.bundle, out var bundle))
                {
                    var id = job.bundles.Count;
                    bundle = new BuildBundle
                    {
                        id = id,
                        name = asset.bundle,
                        assets = new List<string>()
                    };
                    job.bundles.Add(bundle);
                    nameWithBundles[asset.bundle] = bundle;
                }

                bundle.assets.Add(asset.path);
            }

            var manifest = BuildPipeline.BuildAssetBundles(Settings.PlatformCachePath, job.bundles.ConvertAll(
                input =>
                    new AssetBundleBuild
                    {
                        assetNames = input.assets.ToArray(),
                        assetBundleName = input.name
                    }).ToArray(), job.parameters.options, EditorUserBuildSettings.activeBuildTarget);
            if (manifest == null)
            {
                job.TreatError($"Failed to build AssetBundles with {job.parameters.build}.");
                return;
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
                        var name = assetBundle.Replace(Bundler.BundleExtension, string.Empty);
                        var hash = Utility.ComputeHash(path);
                        var nameWithAppendHash = $"{name}_{hash}{Bundler.BundleExtension}";
                        var buildPath = Settings.GetCachePath(nameWithAppendHash);
                        bundle.nameWithAppendHash = nameWithAppendHash;
                        bundle.size = BuildAssetBundle(path, buildPath);
                        bundle.hash = Utility.ComputeHash(buildPath);
                        bundle.nameWithAppendHash = $"{name}_{bundle.hash}{Bundler.BundleExtension}";
                        var newPath = Settings.GetDataPath(bundle.nameWithAppendHash);
                        Utility.CreateDirectoryIfNecessary(newPath);
                        if (File.Exists(newPath)) File.Delete(newPath);
                        File.Move(buildPath, newPath);
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