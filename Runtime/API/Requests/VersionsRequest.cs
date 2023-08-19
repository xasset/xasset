using System;
using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public class VersionsRequest : Request
    {
        private static readonly List<VersionsRequest> AllRequests = new List<VersionsRequest>();
        private readonly Queue<Version> queue = new Queue<Version>();
        private DownloadRequestBatch _contents;
        private DownloadRequest _download;
        private int _retryTimes;
        private Step _step = Step.HeaderDownloading;
        public Versions versions { get; private set; }
        public string url { get; set; }
        public string hash { get; set; }
        public ulong size { get; set; }

        protected override void OnStart()
        {
            AllRequests.Add(this);
            if (!Assets.Updatable)
            {
                SetResult(Result.Success);
                return;
            }

            _retryTimes = 0;
            if (string.IsNullOrEmpty(url)) url = Assets.GetDownloadURL(Versions.Filename);
            var savePath = Assets.GetTemporaryCachePath(Versions.Filename);
            var content = DownloadContent.Get(url, savePath, hash, size);
            content.Clear();
            _download = Downloader.DownloadAsync(content);
            _step = Step.HeaderDownloading;
        }

        protected override void OnUpdated()
        {
            // 防止并行执行多个请求。
            if (AllRequests.Count > 1 && AllRequests[0] != this) return;

            switch (_step)
            {
                case Step.HeaderDownloading:
                    UpdateDownloadHeader();
                    break;
                case Step.ContentsDownloading:
                    UpdateDownloadContents();
                    break;
                case Step.ContentsLoading:
                    UpdateLoadVersions();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateLoadVersions()
        {
            while (queue.Count > 0)
            {
                var version = queue.Dequeue();
                if (Assets.Versions.TryGetVersion(version.name, out var value) && value.hash.Equals(version.hash))
                    version.manifest = value.manifest;
                else
                    version.Load(Assets.GetDownloadDataPath(version.file));


                if (Scheduler.Busy) return;
            }

            SetResult(Result.Success);
        }

        private void UpdateDownloadContents()
        {
            progress = 0.3f + _contents.progress * 0.7f;
            if (!_contents.isDone) return;
            if (_contents.result == DownloadRequestBase.Result.Failed)
            {
                Logger.W($"Failed to download versions with error {_contents.error}.");

                if (Application.internetReachability != NetworkReachability.NotReachable
                    && _retryTimes < Assets.MaxRetryTimes)
                {
                    _contents.Retry();
                    _retryTimes++;
                    return;
                }

                SetResult(Result.Failed, _contents.error);
                return;
            }

            foreach (var version in versions.data) queue.Enqueue(version);

            _step = Step.ContentsLoading;
        }

        private void UpdateDownloadHeader()
        {
            progress = _download.progress * 0.3f;
            if (!_download.isDone) return;

            if (!string.IsNullOrEmpty(_download.error)) SetResult(Result.Failed, _download.error);
            if (!string.IsNullOrEmpty(hash))
            {
                var _hash = Utility.ComputeHash(_download.savePath);
                if (_hash != hash)
                {
                    SetResult(Result.Failed, $"download hash {_hash} mismatch {hash} ");
                    return;
                }
            }

            versions = Utility.LoadFromFile<Versions>(_download.savePath);
            var changes = new List<Version>();
            foreach (var item in versions.data)
            {
                if (Assets.IsDownloaded(item)) continue;
                changes.Add(item);
            }

            if (changes.Count > 0)
            {
                _contents = DownloadRequestBatch.Create();
                foreach (var item in changes)
                {
                    var downloadURL = Assets.GetDownloadURL(item.file);
                    var savePath = Assets.GetDownloadDataPath(item.file);
                    var content = DownloadContent.Get(downloadURL, savePath, item.hash, item.size);
                    _contents.AddContent(content);
                    _contents.downloadSize += content.downloadSize;
                }

                _contents.SendRequest();
                _step = Step.ContentsDownloading;
            }
            else
            {
                foreach (var version in versions.data) queue.Enqueue(version);
                _step = Step.ContentsLoading;
            }
        }

        protected override void OnCompleted()
        {
            AllRequests.Remove(this);
        }

        private enum Step
        {
            HeaderDownloading,
            ContentsDownloading,
            ContentsLoading
        }
    }
}