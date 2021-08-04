using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VEngine.Editor.Builds
{
    public class PlayerBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public void OnPostprocessBuild(BuildReport report)
        {
            var directory = Settings.BuildPlayerDataPath;
            if (!Directory.Exists(directory)) return;

            Directory.Delete(directory, true);
            if (Directory.GetFiles(Application.streamingAssetsPath).Length == 0)
                Directory.Delete(Application.streamingAssetsPath);
        }

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildScript.CopyToStreamingAssets();
        }
    }
}