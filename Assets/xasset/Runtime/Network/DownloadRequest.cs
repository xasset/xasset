using System;
using System.IO;

namespace xasset
{
    public sealed class DownloadRequest : DownloadRequestBase
    {
        public static bool Resumable { get; set; } = true;
        public IDownloadHandler handler { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public Action<DownloadRequest> completed { get; set; }
        public DownloadContent content { get; set; }
        public string savePath => content.savePath;
        public string url => content.url;

        internal void OnGetDownloadSize(ulong size)
        {
            downloadSize = size;
            content.size = size;
        }

        public void Start()
        {
            if (status == Status.Progressing) return;

            downloadedBytes = content.GetDownloadedBytes();
            if (downloadedBytes > 0 && downloadedBytes == content.size)
            {
                SetResult(Result.Success, DownloadErrors.NothingToDownload);
                return;
            }

            OnStart();
            status = Status.Progressing;
        }

        protected override void OnPause(bool paused)
        {
            handler.OnPause(paused);
        }

        protected override void OnCancel()
        {
            handler.OnCancel();
            Downloader.Remove(this);
        }

        public void SendRequest()
        {
            Downloader.Queue.Enqueue(this);
        }

        public void WaitForCompletion()
        {
            if (isDone) return;
            if (Assets.IsWebGLPlatform)
            {
                SetResult(Result.Failed, "WaitForCompletion is not supported on WebGL.");
                return;
            }

            if (status == Status.Wait) Start();

            while (!isDone) Update();
        }

        public override void Retry()
        {
            Reset();
            SendRequest();
        }

        internal void VerifyContent()
        {
            if (result == Result.Failed || result == Result.Cancelled) return;

            if (!string.IsNullOrEmpty(error))
            {
                SetResult(Result.Failed, error);
                return;
            }

            var file = new FileInfo(savePath);
            if (!file.Exists)
            {
                SetResult(Result.Failed, DownloadErrors.FileNotExist);
                return;
            }

            if (content.size != 0 && file.Length != (long)content.size)
            {
                if (file.Length > (long)content.size)
                    file.Delete();
                SetResult(Result.Failed, string.Format(DownloadErrors.DownloadSizeMismatch, file.Length, content.size));
                return;
            }

            if (string.IsNullOrEmpty(content.hash))
            {
                content.status = DownloadContent.Status.Downloaded;
                SetResult(Result.Success);
                return;
            }

            var computeHash = Utility.ComputeHash(savePath);
            if (content.hash.Equals(computeHash))
            {
                content.status = DownloadContent.Status.Downloaded;
                SetResult(Result.Success);
                return;
            }

            file.Delete();
            SetResult(Result.Failed,
                string.Format(DownloadErrors.DownloadHashMismatch, computeHash, content.hash));
        }

        private void OnStart()
        {
            content.status = DownloadContent.Status.Downloading;
            handler.OnStart();
        }

        public void Update()
        {
            handler.Update();
        }

        public void Complete()
        {
            content.downloadedBytes = downloadedBytes;
            Logger.D($"Download {url} {result} {error}");
            var saved = completed;
            completed?.Invoke(this);
            completed -= saved;
        }

        public void Clear()
        {
            Cancel();
            downloadedBytes = 0;
            progress = 0;
            bandwidth = 0;
            content.Clear();
        }
    }
}