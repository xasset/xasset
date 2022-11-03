using System;
using System.Collections.Generic;

namespace xasset
{
    public class VersionsRequest : Request
    {
        private static readonly List<VersionsRequest> AllRequests = new List<VersionsRequest>();
        private readonly Queue<Version> _queue = new Queue<Version>();
        private DownloadContentRequestBatch _contents;
        private DownloadContentRequest _downloadContent;
        private int _retryCount;
        private Step _step = Step.DownloadHeader;
        public byte retryTimes { get; set; } = 2;
        public Versions versions { get; private set; }
        public string url { get; set; }
        public string hash { get; set; }
        public ulong size { get; set; }

        protected override void OnStart()
        {
            AllRequests.Add(this);

            if (Assets.SimulationMode)
            {
                SetResult(Result.Success);
                return;
            }

            if (string.IsNullOrEmpty(url)) url = Assets.GetDownloadURL(Versions.Filename);
            var savePath = Assets.GetTemporaryCachePath(Versions.Filename);
            var content = DownloadContent.Get(url, savePath, hash, size);
            content.Clear();
            _downloadContent = Downloader.DownloadAsync(content);
            _step = Step.DownloadHeader;
        }

        protected override void OnUpdated()
        {
            // 防止并行执行多个请求。
            if (AllRequests.Count > 1 && AllRequests[0] != this) return;

            switch (_step)
            {
                case Step.DownloadHeader:
                    UpdateDownloadHeader();
                    break;
                case Step.DownloadContents:
                    UpdateDownloadContents();
                    break;
                case Step.LoadContents:
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
                if (Assets.Versions.TryGetVersion(version.build, out var value) && value.hash.Equals(version.hash))
                {
                    version.manifest = value.manifest;
                }
                else
                {
                    var path = Assets.GetDownloadDataPath(version.file);
                    var manifest = Utility.LoadFromFile<Manifest>(path);
                    manifest.build = version.build;
                    manifest.name = version.file;
                    version.manifest = manifest;
                }

                if (Scheduler.Busy) return;
            }

            SetResult(Result.Success);
        }

        private void UpdateDownloadContents()
        {
            progress = 0.3f + _contents.progress * 0.7f;
            if (!_contents.isDone) return;
            if (_contents.result == DownloadRequest.Result.Failed)
            {
                Logger.W($"Failed to download versions with error {_contents.error}.");
                if (_retryCount > retryTimes)
                {
                    SetResult(Result.Failed, _contents.error);
                    return;
                }

                _contents.Retry();
                _retryCount++;
                return;
            }

            foreach (var version in versions.data) _queue.Enqueue(version);

            _step = Step.LoadContents;
        }

        private void UpdateDownloadHeader()
        {
            progress = _downloadContent.progress * 0.3f;
            if (!_downloadContent.isDone) return;

            if (!string.IsNullOrEmpty(_downloadContent.error)) SetResult(Result.Failed, _downloadContent.error);
            if (!string.IsNullOrEmpty(hash))
            {
                var _hash = Utility.ComputeHash(_downloadContent.savePath);
                if (_hash != hash)
                {
                    SetResult(Result.Failed, $"download hash {_hash} mismatch {hash} ");
                    return;
                }
            }

            versions = Utility.LoadFromFile<Versions>(_downloadContent.savePath);
            var changes = new List<Version>();
            foreach (var item in versions.data)
            {
                if (Assets.IsDownloaded(item)) continue;
                changes.Add(item);
            }

            if (changes.Count > 0)
            {
                _contents = DownloadContentRequestBatch.Create();
                foreach (var item in changes)
                {
                    var downloadURL = Assets.GetDownloadURL(item.file);
                    var savePath = Assets.GetDownloadDataPath(item.file);
                    var content = DownloadContent.Get(downloadURL, savePath, item.hash, item.size);
                    _contents.AddContent(content);
                    _contents.downloadSize += content.downloadSize;
                }

                _contents.SendRequest();
                _step = Step.DownloadContents;
            }
            else
            {
                foreach (var version in versions.data) _queue.Enqueue(version);
                _step = Step.LoadContents;
            }
        }

        protected override void OnCompleted()
        {
            AllRequests.Remove(this);
        }

        private enum Step
        {
            DownloadHeader,
            DownloadContents,
            LoadContents
        }
    }
}