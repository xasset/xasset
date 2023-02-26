using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace xasset
{
    public struct RuntimeInitializeHandler : IInitializeHandler
    {
        public static IInitializeHandler CreateInstance()
        {
            return new RuntimeInitializeHandler();
        }

        private Queue<Version> _queue;
        private List<UnityWebRequest> _requests;
        private Versions _downloadVersions;
        private Step _step;
        private UnityWebRequest _unityWebRequest;

        public void OnStart(InitializeRequest request)
        {
            _step = Step.LoadPlayerVersions;
            _queue = new Queue<Version>();
            _requests = new List<UnityWebRequest>();

            AssetRequest.CreateHandler = RuntimeAssetHandler.CreateInstance;

            if (Application.isEditor && Assets.OfflineMode)
            {
                Assets.PlayerDataPath = Environment.CurrentDirectory + $"/Bundles/{Assets.Platform}";
                LoadVersionsHeader(Assets.GetPlayerDataURl(Versions.Filename).Replace("/Bundles/", "/BundlesCache/"));
                return;
            }

            _unityWebRequest = UnityWebRequest.Get(Assets.GetPlayerDataURl(PlayerAssets.Filename));
            _unityWebRequest.SendWebRequest();
            _step = Step.LoadPlayerAssets;
        }


        public void OnUpdated(InitializeRequest request)
        {
            switch (_step)
            {
                case Step.LoadPlayerAssets:
                    UpdateLoadingPlayerAssets(request);
                    break;
                case Step.LoadVersionsHeader:
                    UpdateLoadVersionsHeader(request);
                    break;
                case Step.LoadPlayerVersions:
                    UpdateLoadPlayerVersions(request);
                    break;
                case Step.LoadVersionsContent:
                    UpdateLoadVersions(request);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LoadVersionsHeader(string url)
        {
            _unityWebRequest = UnityWebRequest.Get(url);
            _unityWebRequest.SendWebRequest();
            _step = Step.LoadVersionsHeader;
        }

        private void UpdateLoadVersions(InitializeRequest request)
        {
            while (_queue.Count > 0)
            {
                var version = _queue.Dequeue();
                var path = Assets.GetDownloadDataPath(version.file);
                var manifest = Utility.LoadFromFile<Manifest>(path);
                manifest.build = version.name;
                manifest.name = version.file;
                version.manifest = manifest;
                foreach (var asset in manifest.assets)
                {
                    switch (asset.addressMode)
                    {
                        case AddressMode.LoadByDependencies:
                        case AddressMode.LoadByPath:
                            break;
                        case AddressMode.LoadByName:
                            Assets.SetAddress(asset.path, Path.GetFileName(asset.path));
                            break;
                        case AddressMode.LoadByNameWithoutExtension:
                            Assets.SetAddress(asset.path, Path.GetFileNameWithoutExtension(asset.path));
                            break;
                    }
                }

                if (Scheduler.Busy) return;
            }

            request.SetResult(Request.Result.Success);
        }


        private void UpdateLoadPlayerVersions(InitializeRequest request)
        {
            for (var index = 0; index < _requests.Count; index++)
            {
                var unityWebRequest = _requests[index];
                if (!unityWebRequest.isDone) return;
                _requests.RemoveAt(index);
                index--;
                if (!string.IsNullOrEmpty(unityWebRequest.error))
                {
                    TreatError(request, unityWebRequest);
                }

                unityWebRequest.Dispose();
            }

            var path = Assets.GetDownloadDataPath(Versions.Filename);
            _downloadVersions = Utility.LoadFromFile<Versions>(path);
            if (_downloadVersions != null && _downloadVersions.IsNew(Assets.Versions))
                Assets.Versions = _downloadVersions;
            foreach (var version in Assets.Versions.data)
                _queue.Enqueue(version);
            _step = Step.LoadVersionsContent;
        }

        private static void TreatError(Request request, UnityWebRequest unityWebRequest)
        {
            request.SetResult(Request.Result.Failed, unityWebRequest.error);
            Logger.E($"Failed to load {unityWebRequest.url} with error {unityWebRequest.error}");
        }

        private void UpdateLoadingPlayerAssets(InitializeRequest request)
        {
            if (!_unityWebRequest.isDone) return;
            if (!string.IsNullOrEmpty(_unityWebRequest.error))
            {
                request.SetResult(Request.Result.Failed, _unityWebRequest.error);
                return;
            }

            Assets.PlayerAssets = Utility.LoadFromJson<PlayerAssets>(_unityWebRequest.downloadHandler.text);

            // TODO: 这里在正式环境，可以在初始化之后，自己重写 UpdateInfoURL 的地址。
            if (!Downloader.SimulationMode)
            {
                Assets.UpdateInfoURL = Assets.PlayerAssets.updateInfoURL;
                Assets.DownloadURL = Assets.PlayerAssets.downloadURL;
            }

            Assets.OfflineMode = Assets.PlayerAssets.offlineMode;
            Downloader.MaxRetryTimes = Assets.PlayerAssets.maxRetryTimes;
            Downloader.MaxDownloads = Assets.PlayerAssets.maxDownloads;

            _unityWebRequest.Dispose();
            LoadVersionsHeader(Assets.GetPlayerDataURl(Versions.Filename));
        }

        private void UpdateLoadVersionsHeader(InitializeRequest request)
        {
            if (!_unityWebRequest.isDone) return;
            if (!string.IsNullOrEmpty(_unityWebRequest.error))
            {
                TreatError(request, _unityWebRequest);
                return;
            }

            var json = _unityWebRequest.downloadHandler.text;
            Assets.Versions = Utility.LoadFromJson<Versions>(json);
            _unityWebRequest.Dispose();
            foreach (var version in Assets.Versions.data)
            {
                if (Assets.IsDownloaded(version)) continue;
                var url = Assets.GetPlayerDataURl(version.file);
                var savePath = Assets.GetDownloadDataPath(version.file);
                var unityWebRequest = UnityWebRequest.Get(url);
                unityWebRequest.downloadHandler = new DownloadHandlerFile(savePath);
                unityWebRequest.SendWebRequest();
                _requests.Add(unityWebRequest);
            }

            _step = Step.LoadPlayerVersions;
        }

        private enum Step
        {
            LoadPlayerVersions,
            LoadPlayerAssets,
            LoadVersionsHeader,
            LoadVersionsContent
        }
    }

    public interface IInitializeHandler
    {
        void OnStart(InitializeRequest request);
        void OnUpdated(InitializeRequest request);
    }
}