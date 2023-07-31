using System.IO;

namespace xasset.editor
{
    public class CollectAssets : IBuildStep
    {
        public void Start(BuildTask task)
        {
            foreach (var group in task.groups)
            {
                if (group == null)
                {
                    Logger.W($"Group is missing in build {task.parameters.name}");
                    continue;
                }

                if (group.enabled == false) continue;
                
                group.build = task.parameters.name;
                foreach (var entry in group.assets)
                { 
                    entry.owner = group;
                    if (Directory.Exists(entry.asset))
                    {
                        foreach (var child in Settings.GetChildren(entry))
                        {
                            var asset = new BuildEntry
                            {
                                asset = child,
                                owner = entry.owner,
                                bundleMode = entry.bundleMode,
                                addressMode = entry.addressMode,
                                parent = entry.asset
                            };
                            if (task.AddAsset(asset))
                                asset.bundle = Settings.PackAsset(asset);
                        }
                    }
                    else
                    {
                        if (!File.Exists(entry.asset))
                        {
                            Logger.W($"Asset is missing in build {task.parameters.name} with group {group.name}.");
                            continue;
                        }

                        var asset = new BuildEntry
                        {
                            asset = entry.asset,
                            owner = entry.owner,
                            bundleMode = entry.bundleMode,
                            addressMode = entry.addressMode
                        };
                        asset.parent = Settings.GetDirectoryName(asset.asset);
                        if (task.AddAsset(asset))
                            asset.bundle = Settings.PackAsset(asset);
                    }
                }
            }
        }
    }
}