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
                InitializeRequest.CreateHandler = InitializeRequestHandlerSimulation.CreateInstance;
                AssetRequest.CreateHandler = AssetRequestHandlerSimulation.CreateInstance;
                SceneRequest.CreateHandler = SceneRequestHandlerSimulation.CreateInstance;
            }

            if (!Downloader.SimulationMode) return;
            Assets.UpdateURL = $"{Assets.Protocol}{Settings.GetDataPath(UpdateInfo.Filename)}";
            Assets.DownloadURL = $"{Assets.Protocol}{Settings.PlatformDataPath}";
        }
    }
}