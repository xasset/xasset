using System.Collections;
using UnityEngine;

namespace VEngine.Example
{
    [RequireComponent(typeof(Updater))]
    [DisallowMultipleComponent]
    public class Startup : MonoBehaviour
    {
        [Tooltip("资源下载地址，指向平台目录的父目录")] public string downloadURL = "http://127.0.0.1/Bundles/";
        [Tooltip("是否启动后更新服务器版本信息")] public bool autoUpdate;
        [Tooltip("是否开启日志")] public bool loggable;
        public string nextScene = "Splash.unity";

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            var operation = Versions.InitializeAsync();
            yield return operation;
            Logger.I("Initialize: {0}", operation.status);
            Versions.DownloadURL = downloadURL;
            Logger.I("API Version: {0}", Versions.APIVersion);
            Logger.I("Manifests Version: {0}", Versions.ManifestsVersion);
            Logger.I("PlayerDataPath: {0}", Versions.PlayerDataPath);
            Logger.I("DownloadDataPath: {0}", Versions.DownloadDataPath);
            Logger.I("DownloadURL: {0}", Versions.DownloadURL);
            if (autoUpdate && !Versions.OfflineMode)
            {
                var update = Versions.UpdateAsync(operation.file);
                yield return update;
                if (update.status == OperationStatus.Success)
                {
                    update.Override();
                    Logger.I("Success to update versions with version: {0}", Versions.ManifestsVersion);
                }

                update.Dispose();
            }

            var preloadManager = gameObject.AddComponent<PreloadManager>();
            var instantiateAsync = InstantiateObject.InstantiateAsync(Res.GameObject_LoadingScreen);
            yield return instantiateAsync;
            DontDestroyOnLoad(instantiateAsync.result);
            instantiateAsync.result.SetActive(true);
            yield return preloadManager.assetManager.Preload(Res.GameObject_MessageBox, typeof(GameObject));
            var loadingScreen = instantiateAsync.result.GetComponent<LoadingScreen>();
            preloadManager.progressBar = loadingScreen;
            preloadManager.SetVisible(false);
            yield return Scene.LoadAsync(Res.GetScene(nextScene));
        }

        private void Update()
        {
            Logger.Loggable = loggable;
        }
    }
}