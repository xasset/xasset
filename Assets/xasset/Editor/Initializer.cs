using UnityEngine;

namespace xasset.editor
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            Assets.Platform = Settings.Platform;
            Assets.SimulationMode = Settings.GetDefaultSettings().simulationMode;
            Assets.OfflineMode = Settings.GetDefaultSettings().offlineMode;

            if (Assets.SimulationMode)
            {
                InitializeRequest.CreateHandler = EditorInitializeHandler.CreateInstance;
                AssetRequest.CreateHandler = EditorAssetHandler.CreateInstance;
                SceneRequest.CreateHandler = EditorSceneHandler.CreateInstance;
                References.GetFunc = Settings.GetDependencies;
                References.Enabled = true;
            }

            if (!Downloader.SimulationMode) return;
            Assets.UpdateInfoURL = $"{Assets.Protocol}{Settings.GetCachePath(UpdateInfo.Filename)}";
            Assets.DownloadURL = $"{Assets.Protocol}{Settings.PlatformDataPath}";
        }
    }
}