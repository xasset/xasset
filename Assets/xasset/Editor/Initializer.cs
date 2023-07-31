using System;
using System.Collections;
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
            var settings = Settings.GetDefaultSettings();
            Assets.Platform = Settings.Platform;
            Assets.SimulationMode = settings.player.simulationMode;
            Assets.MaxRetryTimes = settings.player.maxRetryTimes;
            Assets.MaxDownloads = settings.player.maxDownloads;

            if (Assets.SimulationMode && !settings.player.updatable)
            {
                Assets.Updatable = false;
                InitializeRequest.Initializer = InitializeAsync;
                AssetRequest.CreateHandler = EditorAssetHandler.CreateInstance;
                SceneRequest.CreateHandler = EditorSceneHandler.CreateInstance;
                // 编辑器仿真模式开启 通过引用计数回收资源优化内存 可能对性能有影响 
                ReferencesCounter.GetDependenciesFunc = Settings.GetDependencies;
                ReferencesCounter.Enabled = true;
            }
            else
            {
                Assets.Updatable = settings.player.updatable;
                if (Assets.Updatable)
                {
                    var file = Assets.GetPlayerDataPath(PlayerAssets.Filename);
                    if (!File.Exists(file))
                    {
                        if (EditorUtility.DisplayDialog("Tips",
                                "Please build player assets before enter playmode.", "Build", "Cancel"))
                        {
                            MenuItems.BuildPlayerAssetsWithSelection();
                            EditorApplication.isPlaying = false;
                        }
                    } 
                    
                    if (Assets.SimulationMode)
                    {
                        Assets.UpdateInfoURL = $"{Assets.Protocol}{Settings.GetCachePath(UpdateInfo.Filename)}";
                        Assets.DownloadURL = $"{Assets.Protocol}{Settings.PlatformDataPath}";
                    }

                    InitializeRequest.Initializer = request => request.RuntimeInitializeAsync();
                }
                else
                {
                    InitializeRequest.Initializer = InitializeAsyncWithOfflineMode;
                }

                AssetRequest.CreateHandler = RuntimeAssetHandler.CreateInstance;
                SceneRequest.CreateHandler = RuntimeSceneHandler.CreateInstance;
            }
        }

        private static IEnumerator InitializeAsyncWithOfflineMode(InitializeRequest request)
        {
            Assets.DownloadDataPath = Settings.PlatformDataPath;
            Assets.PlayerAssets = Settings.GetDefaultSettings().GetPlayerAssets();
            yield return null;
            var file = Settings.GetCachePath(Versions.BundleFilename);
            if (!File.Exists(file))
            {
                const string message = "The versions.json not found! please run build bundles before enter in playmode.";
                request.SetResult(Request.Result.Failed, message);
                EditorUtility.DisplayDialog("Error", message, "Ok");
                EditorApplication.isPlaying = false;
            }
            Assets.Versions = Utility.LoadFromFile<Versions>(file);
            yield return null;
            foreach (var version in Assets.Versions.data)
                version.Load(Settings.GetDataPath(version.file));
            request.SetResult(Request.Result.Success);
        }

        private static IEnumerator InitializeAsync(InitializeRequest request)
        {
            Assets.Versions = ScriptableObject.CreateInstance<Versions>();
            Assets.PlayerAssets = ScriptableObject.CreateInstance<PlayerAssets>();
            Assets.ContainsFunc = ContainsAsset;
            var builds = Settings.FindAssets<Build>();
            foreach (var build in builds)
            {
                if (! build.enabled) continue;
                foreach (var group in build.groups)
                {
                    if (! group.enabled) continue;
                    foreach (var entry in group.assets)
                    {
                        if (entry.addressMode != AddressMode.LoadByName &&
                            entry.addressMode != AddressMode.LoadByNameWithoutExtension) continue;
                        Func<string, string> addressFunc = Path.GetFileName;
                        if (entry.addressMode == AddressMode.LoadByNameWithoutExtension)
                            addressFunc = Path.GetFileNameWithoutExtension;
                        if (!Directory.Exists(entry.asset))
                        {
                            Assets.SetAddress(entry.asset, addressFunc(entry.asset));
                            continue;
                        }

                        var children = Settings.GetChildren(entry);
                        foreach (var child in children)
                            Assets.SetAddress(child, addressFunc(child));
                    }
                }  
                yield return null;
            }

            request.SetResult(Request.Result.Success);
        }

        private static bool ContainsAsset(string path)
        {
            var result = File.Exists(path);
            if (result) return true;
            var message = $"File not found:{path}";
            EditorUtility.DisplayDialog("Error", message, "Ok");
            Logger.E(message);
            return false;
        }
    }
}