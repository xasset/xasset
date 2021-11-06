using System;
using System.IO;
using UnityEngine;
using VEngine.Editor.Builds;
using VEngine.Editor.Simulation;

namespace VEngine.Editor
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Versions.DownloadDataPath = Path.Combine(Application.persistentDataPath, Utility.buildPath);
            Versions.PlatformName = Settings.GetPlatformName();
            var config = Settings.GetPlayerSettings();
            var settings = Settings.GetDefaultSettings();
            Versions.SimulationMode = settings.scriptPlayMode == ScriptPlayMode.Simulation;
            switch (settings.scriptPlayMode)
            {
                case ScriptPlayMode.Simulation:
                    Asset.Creator = EditorAsset.Create;
                    Scene.Creator = EditorScene.Create;
                    ManifestAsset.Creator = EditorManifestAsset.Create;
                    config.offlineMode = true;
                    break;
                case ScriptPlayMode.Preload:
                    Versions.PlayerDataPath = Path.Combine(Environment.CurrentDirectory, Settings.PlatformBuildPath);
                    config.offlineMode = true;
                    break;
                case ScriptPlayMode.Incremental:
                    if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath,
                        Utility.buildPath)))
                        config.assets.Clear();

                    config.offlineMode = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}