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
                    Logger.W($"Group is missing in build {job.parameters.name}");
                    continue;
                }

                group.build = job.parameters.name;
                var assets = Settings.Collect(group);
                foreach (var asset in assets)
                {
                    asset.bundle = Settings.PackAsset(asset);
                    job.AddAsset(asset);
                }
            }
        }
    }
}