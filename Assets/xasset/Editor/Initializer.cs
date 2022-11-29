using UnityEngine;

namespace xasset.editor
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            Assets.Platform = Settings.Platform;
            
            if (Assets.SimulationMode)
            {
                InitializeRequest.CreateHandler = EditorInitializeHandler.CreateInstance;
                AssetRequest.CreateHandler = EditorAssetHandler.CreateInstance;
                SceneRequest.CreateHandler = EditorSceneHandler.CreateInstance;
                References.GetFunc = Settings.GetDependencies;
            }

            if (!Downloader.SimulationMode) return;
            Assets.UpdateURL = $"{Assets.Protocol}{Settings.GetCachePath(UpdateInfo.Filename)}";
            Assets.DownloadURL = $"{Assets.Protocol}{Settings.PlatformDataPath}";
        }
    }
}