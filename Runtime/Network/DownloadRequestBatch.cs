using System;
using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public sealed class DownloadRequestBatch : DownloadRequestBase
    {
        private static readonly List<DownloadRequestBatch> Progressing = new List<DownloadRequestBatch>();
        private static readonly Queue<DownloadRequestBatch> Unused = new Queue<DownloadRequestBatch>();

        // ReSharper disable once MemberCanBePrivate.Global
        public readonly Dictionary<string, DownloadContent> contents = new Dictionary<string, DownloadContent>();
        private readonly Queue<DownloadContent> queue = new Queue<DownloadContent>();
        private readonly List<DownloadRequest> working = new List<DownloadRequest>();

        private int _retryTimes;

        private ulong _successDownloadedBytes;

        // ReSharper disable once MemberCanBePrivate.Global
        public Action<DownloadRequestBatch> completed { get; set; }

        public static DownloadRequestBatch Create()
        {
            if (Unused.Count <= 0) return new DownloadRequestBatch();
            var item = Unused.Dequeue();
            item.contents.Clear();
            item.working.Clear();
            item._successDownloadedBytes = 0;
            item._retryTimes = 0;
            item.bandwidth = 0;
            item.Reset();
            return item;
        }

        private void Complete()
        {
            var saved = completed;
            completed?.Invoke(this);
            completed -= saved;
            if (result == Result.Success || result == Result.Cancelled) Unused.Enqueue(this);
        }

        private void OnStart()
        {
            while (queue.Count > 0)
            {
                var content = queue.Dequeue();
                var request = Downloader.DownloadAsync(content);
                working.Add(request);
            }

            BeganSample();
        }

        private bool Update()
        {
            if (status == Status.Paused) return true;
            if (status != Status.Progressing) return false;

            var size = 0UL;
            bandwidth = 0;
            downloadSize = 0L;
            foreach (var pair in contents)
            {
                var content = pair.Value;
                downloadSize += content.size;
            }

            var failed = 0;
            for (var index = 0; index < working.Count; index++)
            {
                var request = working[index];
                bandwidth += request.bandwidth;
                if (request.result == Result.Failed) failed++;
                if (request.result != Result.Success)
                {
                    size += request.downloadedBytes;
                    continue;
                }

                _successDownloadedBytes += request.downloadedBytes;
                working.RemoveAt(index);
                index--;
            }

            downloadedBytes = size + _successDownloadedBytes;
            progress = downloadedBytes * 1f / downloadSize;

            if (working.Count > failed) return true;

            if (failed == 0)
            {
                SetResult(Result.Success);
            }
            else
            {
                // 网络可达才自动 Retry
                if (Application.internetReachability != NetworkReachability.NotReachable
                    && _retryTimes < Assets.MaxRetryTimes)
                {
                    Retry();
                    _retryTimes++;
                    return true;
                }

                SetResult(Result.Failed, string.Format(DownloadErrors.FailedToDownloadSomeFiles, failed));
            }

            Complete();
            return false;
        }

        public void AddContent(DownloadContent content)
        {
            if (contents.ContainsKey(content.url)) return;
            contents.Add(content.url, content);
            downloadedBytes += content.downloadedBytes;
            downloadSize += content.size;
            queue.Enqueue(content);
        }

        protected override void OnPause(bool paused)
        {
            if (paused)
                foreach (var file in working)
                    file.Pause();
            else
                foreach (var file in working)
                    file.UnPause();
        }

        protected override void OnCancel()
        {
            foreach (var request in working) request.Cancel();
        }

        public void Clear()
        {
            queue.Clear();
            foreach (var request in working) request.Clear();
            working.Clear();
            foreach (var pair in contents) pair.Value.Clear();
            contents.Clear();
            progress = 0;
            downloadedBytes = 0;
            _successDownloadedBytes = 0;
            bandwidth = 0;
            SetResult(Result.Default);
        }

        public static void UpdateAll()
        {
            for (var index = 0; index < Progressing.Count; index++)
            {
                var batch = Progressing[index];
                if (batch.Update()) continue;
                Progressing.RemoveAt(index);
                index--;
                batch.Complete();
            }
        }

        public static void CancelAll()
        {
            foreach (var batch in Progressing) batch.Cancel();
            Progressing.Clear();
            Unused.Clear();
        }

        public void SendRequest()
        {
            Progressing.Add(this);
            OnStart();
            status = Status.Progressing;
        }

        public override void Retry()
        {
            error = string.Empty;
            foreach (var request in working) request.Retry();
            status = Status.Wait;
            SendRequest();
        }
    }
}