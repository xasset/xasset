using System;
using System.IO;
using UnityEngine;
using Versions.Editor.Builds;
using Versions.Editor.Simulation;

namespace Versions.Editor
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
                    Versions.FuncCreateAsset = EditorAsset.Create;
                    Versions.FuncCreateScene = EditorScene.Create;
                    Versions.FuncCreateManifest = EditorManifestAsset.Create;
                    config.offlineMode = true;
                    break;
                case ScriptPlayMode.Preload:
                    Versions.PlayerDataPath = Path.Combine(Environment.CurrentDirectory, Settings.PlatformBuildPath);
                    config.offlineMode = true;
                    break;
                case ScriptPlayMode.Incremental:
                    if (!Directory.Exists(Path.Combine(Application.streamingAssetsPath,
                        Utility.buildPath)))
                    {
                        config.assets.Clear();
                    }

                    config.offlineMode = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}