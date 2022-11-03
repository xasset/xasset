using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace xasset.editor
{
    public class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public void OnPostprocessBuild(BuildReport report)
        {
            if (!Directory.Exists(Assets.PlayerDataPath)) return;
            FileUtil.DeleteFileOrDirectory(Assets.PlayerDataPath);
            FileUtil.DeleteFileOrDirectory(Assets.PlayerDataPath + ".meta");
            if (Directory.GetFiles(Application.streamingAssetsPath).Length != 0) return;
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + ".meta");
        }

        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            BuildPlayerAssets.StartNew();
        }
    }
}