using System.IO;
using UnityEngine;

namespace xasset.editor
{
    public class SaveBuildAssets : IBuildStep
    {
        public void Start(BuildTask task)
        {
            var buildAssets = ScriptableObject.CreateInstance<BuildCache>();
            buildAssets.data = task.assets;
            var json = JsonUtility.ToJson(buildAssets);
            var path = Settings.GetCachePath($"{nameof(BuildCache)}{task.parameters.name}.json");
            Utility.CreateDirectoryIfNecessary(path);
            File.WriteAllText(path, json);
        }
    }
}