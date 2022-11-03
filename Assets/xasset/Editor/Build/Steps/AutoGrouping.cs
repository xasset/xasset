using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace xasset.editor
{
    public class AutoGrouping : IBuildJobStep
    {
        public void Start(BuildJob job)
        {
            var bundledAssets = job.bundledAssets;
            var pathWithAssets = new Dictionary<string, BuildAsset>();
            foreach (var bundledAsset in bundledAssets) pathWithAssets[bundledAsset.path] = bundledAsset;

            var dependencyWithReferences = new Dictionary<string, List<BuildAsset>>();
            foreach (var asset in bundledAssets)
            {
                if (!Bundler.FindReferences(asset)) continue;

                var dependencies = Bundler.GetDependencies(asset.path);
                foreach (var dependency in dependencies)
                {
                    if (pathWithAssets.ContainsKey(dependency)) continue;

                    if (!dependencyWithReferences.TryGetValue(dependency, out var assets))
                    {
                        assets = new List<BuildAsset>();
                        dependencyWithReferences[dependency] = assets;
                    }

                    assets.Add(asset);
                }
            }

            if (dependencyWithReferences.Count <= 0) return;

            var autoGroup = ScriptableObject.CreateInstance<Group>();
            autoGroup.bundleMode = BundleMode.PackTogether;
            autoGroup.name = "auto";

            foreach (var pair in dependencyWithReferences)
            {
                var path = pair.Key;
                var assets = pair.Value;
                if (pathWithAssets.ContainsKey(path)) continue;
                if (assets.Count <= 1) continue;
                var bundles = new HashSet<string>(assets.ConvertAll(input => input.bundle)).ToList();
                if (bundles.Count <= 1) continue;
                var asset = Bundler.CreateAsset(path, autoGroup, true);
                asset.entry = path;
                bundles.Sort();
                var hash = Utility.ComputeHash(Encoding.UTF8.GetBytes(string.Join("_", bundles.ToArray())));
                asset.bundle = Bundler.PackAsset(asset.path, $"auto_{hash}", job.parameters.build);
                job.AddAsset(asset);
                pathWithAssets.Add(path, asset);
            }
        }
    }
}