using System.IO;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoad()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            var settings = Settings.GetDefaultSettings();

            Assets.Platform = Settings.Platform;
            Assets.SimulationMode = settings.simulationMode;
            Assets.OfflineMode = settings.offlineMode;

            if (Assets.SimulationMode)
            {
                InitializeRequest.CreateHandler = EditorInitializeHandler.CreateInstance;
                AssetRequest.CreateHandler = EditorAssetHandler.CreateInstance;
                SceneRequest.CreateHandler = EditorSceneHandler.CreateInstance;
                // 编辑器仿真模式开启 引用技术回收资源优化内存 可能对性能有影响 
                References.GetFunc = Settings.GetDependencies;
                References.Enabled = true;
            }

            Downloader.SimulationMode = settings.simulationDownload;
            Downloader.MaxRetryTimes = settings.maxRetryTimes;
            Downloader.MaxDownloads = settings.maxDownloads;

            if (Downloader.SimulationMode)
            {
                Assets.UpdateInfoURL = $"{Assets.Protocol}{Settings.GetCachePath(UpdateInfo.Filename)}";
                Assets.DownloadURL = $"{Assets.Protocol}{Settings.PlatformDataPath}";
            }
            else
            {
                // 把文件 copy 到 CDN 目录
                var path = Settings.GetCachePath(UpdateInfo.Filename);
                if (File.Exists(path))
                    File.Copy(path, Settings.GetDataPath(UpdateInfo.Filename), true);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj != PlayModeStateChange.ExitingPlayMode) return;
            Assets.PlayerDataPath = $"{Application.streamingAssetsPath}/{Assets.Bundles}";
            var path = Settings.GetDataPath(UpdateInfo.Filename);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}