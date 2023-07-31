using System.IO;
using UnityEngine;

namespace xasset.editor
{
    public class LoadBuildAssets : IBuildStep
    {
        public void Start(BuildTask task)
        {
            var path = Settings.GetCachePath($"{nameof(BuildCache)}{task.parameters.name}.json");
            if (!File.Exists(path))
            {
                task.error = $"File not found {path}.";
                return;
            }

            var assets = ScriptableObject.CreateInstance<BuildCache>();
            JsonUtility.FromJsonOverwrite(File.ReadAllText(path), assets);
            foreach (var asset in assets.data)
                task.AddAsset(asset);
        }
    }
}