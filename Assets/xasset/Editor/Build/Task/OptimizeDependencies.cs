using System.Collections.Generic;

namespace xasset.editor
{
    public class OptimizeDependencies : IBuildStep
    {
        private readonly List<BuildEntry> assets = new List<BuildEntry>();
        private readonly Dictionary<string, BuildEntry> entries = new Dictionary<string, BuildEntry>();
        private readonly Dictionary<string, List<BuildEntry>> references = new Dictionary<string, List<BuildEntry>>();

        public void Start(BuildTask task)
        {
            var bundledAssets = task.assets;
            foreach (var entry in bundledAssets)
                entries[entry.asset] = entry;

            foreach (var entry in bundledAssets)
                CollectDependencies(entry);

            var group = new BuildGroup { name = "Auto", build = task.parameters.name, enabled = true };
            foreach (var entry in assets)
            {
                if (!references.TryGetValue(entry.asset, out var value))
                    continue;
                if (value.Count <= 1) continue;
                // 公共依赖按文件为单位打包
                entry.owner = group;
                entry.bundleMode = BundleMode.PackByFile;
                entry.addressMode = AddressMode.LoadByDependencies;
                entry.bundle = $"auto_{Settings.PackAsset(entry)}";
                task.AddAsset(entry);
            }
        }

        private void CollectDependencies(BuildEntry entry)
        {
            foreach (var dependency in Settings.GetDependencies(entry.asset))
            {
                if (!references.TryGetValue(dependency, out var value))
                {
                    value = new List<BuildEntry>();
                    references[dependency] = value;
                }

                value.Add(entry);
                // Unity 会存在场景依赖场景的情况。 
                if (entries.ContainsKey(dependency))
                    continue;
                var asset = Settings.GetPackedAsset(dependency);
                entries.Add(dependency, asset);
                assets.Add(asset);
                CollectDependencies(asset);
            }
        }
    }
}