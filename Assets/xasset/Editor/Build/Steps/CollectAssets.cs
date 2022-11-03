using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace xasset.editor
{
    public class CollectAssets : IBuildJobStep
    {
        public void Start(BuildJob job)
        {
            foreach (var group in job.parameters.groups)
            {
                if (group == null)
                {
                    Logger.W($"Group is missing in build {job.parameters.build}");
                    continue;
                }

                group.build = job.parameters.build;
                var assets = Collect(group);
                foreach (var asset in assets)
                {
                    asset.bundle = Bundler.PackAsset(asset);
                    job.AddAsset(asset);
                }
            }
        }

        public static BuildAsset[] Collect(Group group)
        {
            var assets = new List<BuildAsset>();
            if (group.entries == null) return assets.ToArray();
            foreach (var entry in group.entries)
            {
                var path = AssetDatabase.GetAssetPath(entry);
                if (string.IsNullOrEmpty(path)) continue;
                if (!Directory.Exists(path))
                {
                    AddAsset(assets, group, path, path);
                }
                else
                {
                    var guilds = AssetDatabase.FindAssets(group.filter, new[] {path});
                    var set = new HashSet<string>();
                    foreach (var guild in guilds)
                    {
                        var child = AssetDatabase.GUIDToAssetPath(guild);
                        if (string.IsNullOrEmpty(child) || Directory.Exists(child) || set.Contains(child)) continue;
                        set.Add(child);
                        AddAsset(assets, group, child, path);
                    }
                }
            }

            return assets.ToArray();
        }

        private static void AddAsset(List<BuildAsset> assets, Group group, string path, string entry)
        {
            if (assets.Exists(a => a.path.Equals(path)))
            {
                Logger.W(
                    $"Failed to add {path} to assets with group {group.name} with entry {entry}, because which is already exist.");
                return;
            }

            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            assets.Add(new BuildAsset
            {
                auto = false,
                entry = entry,
                group = group,
                path = path,
                type = type != null ? type.Name : "MissType"
            });
        }
    }
}