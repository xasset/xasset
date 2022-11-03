using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace xasset
{
    public class InitializeRequestHandlerRuntime : InitializeRequestHandler
    {
        private readonly Queue<Version> _queue = new Queue<Version>();
        private readonly List<UnityWebRequest> _requests = new List<UnityWebRequest>();
        private Versions _downloadVersions;
        private Step _step = Step.LoadPlayerVersions;
        private UnityWebRequest _unityWebRequest;
        private InitializeRequest request { get; set; }

        public void OnStart()
        {
            _unityWebRequest = UnityWebRequest.Get(Assets.GetPlayerDataURl(PlayerAssets.Filename));
            _unityWebRequest.SendWebRequest();
            _step = Step.LoadPlayerAssets;
        }


        public void OnUpdated()
        {
            switch (_step)
            {
                case Step.LoadPlayerAssets:
                    UpdateLoadingPlayerAssets();
                    break;
                case Step.LoadVersionsHeader:
                    UpdateLoadVersionsHeader();
                    break;
                case Step.LoadPlayerVersions:
                    UpdateLoadPlayerVersions();
                    break;
                case Step.LoadVersionsContent:
                    UpdateLoadVersions();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateLoadVersions()
        {
            while (_queue.Count > 0)
            {
                var version = _queue.Dequeue();
                var path = Assets.GetDownloadDataPath(version.file);
                var manifest = Utility.LoadFromFile<Manifest>(path);
                manifest.build = version.build;
                manifest.name = version.file;
                version.manifest = manifest;
                if (Scheduler.Busy) return;
            }

            request.SetResult(Request.Result.Success);
        }


        internal static InitializeRequestHandler CreateInstance(InitializeRequest initializeRequest)
        {
            return new InitializeRequestHandlerRuntime {request = initializeRequest};
        }

        private void UpdateLoadPlayerVersions()
        {
            for (var index = 0; index < _requests.Count; index++)
            {
                var unityWebRequest = _requests[index];
                if (!unityWebRequest.isDone) return;
                _requests.RemoveAt(index);
                index--;
                if (!string.IsNullOrEmpty(unityWebRequest.error))
                    request.SetResult(Request.Result.Failed, unityWebRequest.error);
                unityWebRequest.Dispose();
            }

            var path = Assets.GetDownloadDataPath(Versions.Filename);
            _downloadVersions = Utility.LoadFromFile<Versions>(path);
            if (_downloadVersions != null && _downloadVersions.timestamp > Assets.Versions.timestamp)
                Assets.Versions = _downloadVersions;
            foreach (var version in Assets.Versions.data)
                _queue.Enqueue(version);
            _step = Step.LoadVersionsContent;
        }

        private void UpdateLoadingPlayerAssets()
        {
            if (!_unityWebRequest.isDone) return;
            if (!string.IsNullOrEmpty(_unityWebRequest.error))
            {
                request.SetResult(Request.Result.Failed, _unityWebRequest.error);
                return;
            }

            Assets.PlayerAssets = Utility.LoadFromJson<PlayerAssets>(_unityWebRequest.downloadHandler.text);
            _unityWebRequest.Dispose();
            _unityWebRequest = UnityWebRequest.Get(Assets.GetPlayerDataURl(Versions.Filename));
            _unityWebRequest.SendWebRequest();
            _step = Step.LoadVersionsHeader;
        }

        private void UpdateLoadVersionsHeader()
        {
            if (!_unityWebRequest.isDone) return;
            if (!string.IsNullOrEmpty(_unityWebRequest.error))
            {
                request.SetResult(Request.Result.Failed, _unityWebRequest.error);
                return;
            }

            var json = _unityWebRequest.downloadHandler.text;
            Logger.D($"LoadVersionsHeader {json}");
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

    public interface InitializeRequestHandler
    {
        void OnStart();
        void OnUpdated();
    }
}