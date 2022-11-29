using System.Collections.Generic;

namespace xasset.editor
{
    /// <summary>
    ///     清理重复设置打包分组的资源
    /// </summary>
    public class ClearDuplicateAssets : IBuildJobStep
    {
        public void Start(BuildJob job)
        {
            var pathWithAssets = new Dictionary<string, BuildAsset>();
            var bundledAssets = job.bundledAssets;
            for (var i = 0; i < bundledAssets.Count; i++)
            {
                var asset = bundledAssets[i];
                var path = asset.path;
                if (!pathWithAssets.TryGetValue(path, out var value))
                {
                    pathWithAssets[path] = asset;
                }
                else
                {
                    bundledAssets.RemoveAt(i);
                    i--;
                    Logger.W($"Can't pack {path} with {asset.bundle}, because already pack to {value.bundle}");
                }
            }
        }
    }
}