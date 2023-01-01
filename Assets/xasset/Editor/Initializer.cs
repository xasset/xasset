using UnityEngine;

namespace xasset.editor
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad()
        {
            var settings = Settings.GetDefaultSettings();
            Assets.Platform = Settings.Platform;
            Assets.SimulationMode = settings.simulationMode;
            Assets.OfflineMode = settings.offlineMode;
            if (!Assets.SimulationMode) return;
            InitializeRequest.CreateHandler = EditorInitializeHandler.CreateInstance;
            AssetRequest.CreateHandler = EditorAssetHandler.CreateInstance;
            SceneRequest.CreateHandler = EditorSceneHandler.CreateInstance;
        }
    }
}