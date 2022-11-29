using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace xasset.example
{
    [RequireComponent(typeof(Downloader))]
    [RequireComponent(typeof(Scheduler))]
    [RequireComponent(typeof(Recycler))]
    [DisallowMultipleComponent]
    public class Startup : MonoBehaviour
    {
        public ExampleScene startWithScene;
        [SerializeField] private bool loggable = true;
        [SerializeField] private bool autoUpdate = true;
        [SerializeField] private bool simulationMode = true;
        [SerializeField] private bool offlineMode;
        [SerializeField] private bool benchmarkMode;
        [SerializeField] private string baseUpdateURL = $"http://127.0.0.1/{Assets.Bundles}";

        [Conditional("UNITY_EDITOR")]
        private void Awake()
        {
            Assets.SimulationMode = simulationMode;
        }

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            Assets.OfflineMode = offlineMode;
            if (!Assets.SimulationMode && !Downloader.SimulationMode)
                Assets.UpdateURL = $"{baseUpdateURL}/{Assets.Platform}/{UpdateInfo.Filename}";

            var initializeAsync = Assets.InitializeAsync();
            yield return initializeAsync;
            if (autoUpdate)
            {
                // 获取服务器的更新信息。 
                var getUpdateInfoAsync = Assets.GetUpdateInfoAsync();
                yield return getUpdateInfoAsync;
                if (getUpdateInfoAsync.result == Request.Result.Success)
                {
                    // 安装包完全不带资源的时候，可以从服务器下载版本文件和资源。
                    var getVersionsAsync = Assets.GetVersionsAsync(getUpdateInfoAsync.info);
                    yield return getVersionsAsync;
                    if (getVersionsAsync.versions != null)
                    {
                        getVersionsAsync.versions.Save(Assets.GetDownloadDataPath(Versions.Filename));
                        Assets.Versions = getVersionsAsync.versions;
                    }
                }
            }

            if (benchmarkMode)
            {
                gameObject.AddComponent<Benchmark>();
            }
            else
            {
                Scene.LoadAsync(startWithScene.ToString());
            }
        }

        private void Update()
        {
            Logger.Enabled = loggable;
        }
    }
}