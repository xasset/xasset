using UnityEditor;
using UnityEngine;

namespace libx
{
    public class EditorRuntimeInitializeOnLoad
    {
        [RuntimeInitializeOnLoadMethod]
        private static void OnInitialize()
        {
            EditorUtility.ClearProgressBar();
            var settings = BuildScript.GetSettings();
            if (settings.localServer)
            {
                bool isRunning = LaunchLocalServer.IsRunning();
                if (!isRunning)
                {
                    LaunchLocalServer.Run();
                }
                Assets.dataPath = string.Empty; 
            }
            else
            {
                bool isRunning = LaunchLocalServer.IsRunning();
                if (isRunning)
                {
                    LaunchLocalServer.KillRunningAssetBundleServer();
                }
                Assets.dataPath = System.Environment.CurrentDirectory;
            }
            Assets.assetBundleMode = settings.runtimeMode;
            Assets.getPlatformDelegate = BuildScript.GetPlatformName;
            Assets.loadDelegate = AssetDatabase.LoadAssetAtPath;
        }

        [InitializeOnLoadMethod]
        private static void OnEditorInitialize()
        {
            BuildScript.GetManifest();
            BuildScript.GetSettings();
            BuildScript.GetBuildRules();
        }
    }
}
