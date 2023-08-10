using System.Collections.Generic;
using System.IO;

namespace xasset.editor
{
    public class CollectAssets : IBuildStep
    {
        public void Start(BuildTask task)
        {
            var children = new List<BuildEntry>();
            foreach (var group in task.groups)
            {
                if (group == null)
                {
                    Logger.W($"Group is missing in build {task.parameters.name}");
                    continue;
                }

                if (!group.enabled) continue;
                group.build = task.parameters.name;
                foreach (var entry in group.assets)
                {
                    entry.owner = group;
                    if (Directory.Exists(entry.asset))
                    {
                        // 允许目录下的单个文件拎出去打包
                        foreach (var child in Settings.GetChildren(entry))
                        {
                            var asset =  Settings.GetPackedAsset(child);
                            asset.owner = entry.owner;
                            asset.bundleMode = entry.bundleMode;
                            asset.addressMode = entry.addressMode;
                            asset.parent = entry.asset; 
                            children.Add(asset);
                        }
                    }
                    else
                    {
                        if (!File.Exists(entry.asset))
                        {
                            Logger.W($"Asset is missing in build {task.parameters.name} with group {group.name}.");
                            continue;
                        } 
                        var asset =  Settings.GetPackedAsset(entry.asset);
                        asset.owner = entry.owner;
                        asset.bundleMode = entry.bundleMode;
                        asset.addressMode = entry.addressMode;
                        asset.parent = Settings.GetDirectoryName(asset.asset);
                        if (task.AddAsset(asset))
                            asset.bundle = Settings.PackAsset(asset);
                    }
                }
            }

            foreach (var asset in children)
                if (task.AddAsset(asset))
                    asset.bundle = Settings.PackAsset(asset);
        }
    }
}