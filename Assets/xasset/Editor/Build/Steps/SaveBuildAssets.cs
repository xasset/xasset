using System.IO;
using UnityEngine;

namespace xasset.editor
{
    public class SaveBuildAssets : IBuildJobStep
    {
        public void Start(BuildJob job)
        {
            var buildAssets = ScriptableObject.CreateInstance<BuildAssets>();
            buildAssets.bundledAssets = job.bundledAssets.ToArray();
            var json = JsonUtility.ToJson(buildAssets);
            var path = Settings.GetCachePath(job.parameters.name + ".json");
            File.WriteAllText(path, json);
        }
    }
}