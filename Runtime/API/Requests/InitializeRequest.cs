using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace xasset
{
    public class InitializeRequest : Request
    {
        private static InitializeRequest _request;

        public static Func<InitializeRequest, IEnumerator> Initializer = request => request.RuntimeInitializeAsync();

        private float _startTime;

        public static InitializeRequest InitializeAsync(Action<Request> completed = null)
        {
            if (_request == null)
            {
                _request = new InitializeRequest();
                _request.SendRequest();
                _request.completed = completed;
            }

            if (_request.isDone) ActionRequest.CallAsync(_request.Complete);

            return _request;
        }

        protected override void OnStart()
        {
            var scheduler = Object.FindObjectOfType<Scheduler>();
            scheduler.StartCoroutine(Initializer(this));
            _startTime = Time.realtimeSinceStartup;
        }

        public IEnumerator RuntimeInitializeAsync()
        {
            var www = UnityWebRequest.Get(Assets.GetPlayerDataURl(PlayerAssets.Filename));
            yield return www.SendWebRequest();
            if (!string.IsNullOrEmpty(www.error))
            {
                SetResult(Result.Failed, $"{www.error}({www.url})");
                www.Dispose();
            }
            else
            {
                var json = www.downloadHandler.text;
                www.Dispose();
                var settings = Utility.LoadFromJson<PlayerAssets>(json);
                Assets.LoadPlayerAssets(settings);
            }

            progress = 0.2f;
            // step2: load player versions.json 
            www = UnityWebRequest.Get(Assets.GetPlayerDataURl(Versions.Filename));
            yield return www.SendWebRequest();
            if (!string.IsNullOrEmpty(www.error))
            {
                SetResult(Result.Failed, $"{www.error}({www.url})");
                www.Dispose();
            }
            else
            {
                var json = www.downloadHandler.text;
                Assets.Versions = Utility.LoadFromJson<Versions>(json);
                www.Dispose();

                foreach (var version in Assets.Versions.data)
                {
                    if (Assets.IsDownloaded(version, false)) continue;
                    var url = Assets.GetPlayerDataURl(version.file);
                    var savePath = Assets.GetDownloadDataPath(version.file);
                    var webRequest = UnityWebRequest.Get(url);
                    webRequest.downloadHandler = new DownloadHandlerFile(savePath);
                    yield return webRequest.SendWebRequest();
                }
            }

            progress = 0.3f;
            // step3: load server versions.json
            var serverVersions = Utility.LoadFromFile<Versions>(Assets.GetDownloadDataPath(Versions.Filename));
            if (serverVersions != null && serverVersions.IsNew(Assets.Versions))
            {
                Assets.Versions = serverVersions;
            }
            else
            {
                if (Assets.PlayerAssets.splitMode == PlayerAssetsSplitMode.IncludeAllAssets)
                    PlayerPrefs.SetString(Assets.kBundlesVersions, Assets.Versions.GetFilename());
            }

            // step4: load manifests.json
            foreach (var version in Assets.Versions.data)
                version.Load(Assets.GetDownloadDataPath(version.file));

            progress = 1f;
            SetResult(Result.Success);
        }

        protected override void OnCompleted()
        {
            if (!Application.isEditor && Assets.IsWebGLPlatform)
                Assets.DownloadURL = Assets.PlayerDataPath;

            if (result == Result.Success)
            {
                var elapsed = Time.realtimeSinceStartup - _startTime;
                Logger.D($"Initialize {result} with {elapsed:F2} seconds");
            }
            else
            {
                Logger.E($"Initialize {result} with error {error}.");
            }

            Logger.D($"API Version:{Assets.APIVersion}");
            Logger.D($"Unity Version:{Application.unityVersion}");
            Logger.D($"Realtime Mode: {Assets.RealtimeMode}");
            Logger.D($"Updatable: {Assets.Updatable}");
            Logger.D($"Versions: {Assets.Versions}");
            Logger.D($"Download URL: {Assets.DownloadURL}");
            Logger.D($"Player Data Path: {Assets.PlayerDataPath}");
        }
    }
}