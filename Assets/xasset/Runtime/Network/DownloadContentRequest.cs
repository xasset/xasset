using System;
using System.IO;
using UnityEngine.Networking;

namespace xasset
{
    public sealed class DownloadContentRequest : DownloadRequest
    {
        private UnityWebRequest _content;
        private UnityWebRequest _header;
        private ulong _lastRequestDownloadedBytes;
        private Step _step;

        public Action<DownloadContentRequest> completed { get; set; }
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
            if (paused)
            {
                _content?.Abort();
                _header?.Abort();
                Dispose();
            }
            else
            {
                StartDownload();
            }
        }

        protected override void OnCancel()
        {
            Dispose();
        }

        public void SendRequest()
        {
            Downloader.Queue.Enqueue(this);
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

            if (file.Length != (long) content.size)
            {
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

            SetResult(Result.Failed,
                string.Format(DownloadErrors.DownloadHashMismatch, computeHash, content.hash));
        }

        private void OnStart()
        {
            content.status = DownloadContent.Status.Downloading;
            _step = Step.GetHeader;
            StartDownload();
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

        public void Update()
        {
            if (isDone || status == Status.Paused || status != Status.Progressing) return;

            switch (_step)
            {
                case Step.GetHeader:
                    UpdateHeaderRequest();
                    break;
                case Step.GetContent:
                    UpdateContentRequest();
                    break;
            }
        }

        private void StartDownload()
        {
            if (Downloader.SimulationMode)
            {
                downloadedBytes = 0;
                var path = url.Replace(Assets.Protocol, string.Empty);
                var file = new FileInfo(path);
                if (file.Exists) OnGetDownloadSize((ulong) file.Length);
                GetContentRequest();
            }
            else
            {
                GetHeadRequest();
            }

            status = Status.Progressing;
        }

        private void GetHeadRequest()
        {
            _header = UnityWebRequest.Head(url);
            _header.SendWebRequest();
            _step = Step.GetHeader;
        }

        private void UpdateHeaderRequest()
        {
            if (!_header.isDone) return;
            if (!string.IsNullOrEmpty(_header.error))
            {
                SetResult(Result.Failed, _header.error);
                return;
            }

            const string key = "Content-Length";
            var value = _header.GetResponseHeader(key);
            if (ulong.TryParse(value, out var size))
            {
                OnGetDownloadSize(size);
                if (size == downloadedBytes)
                {
                    SetResult(Result.Success, DownloadErrors.NothingToDownload);
                    return;
                }

                GetContentRequest();
                return;
            }

            SetResult(Result.Success, DownloadErrors.NothingToDownload);
        }

        private void UpdateContentRequest()
        {
            var deltaBytes = _content.downloadedBytes - _lastRequestDownloadedBytes;
            OnReceiveBytes(deltaBytes);
            _lastRequestDownloadedBytes = _content.downloadedBytes;
            if (!_content.isDone) return;
            downloadedBytes = content.GetDownloadedBytes();
            error = _content.error;
            VerifyContent();
            _step = Step.Ended;
            Dispose();
        }

        private void GetContentRequest()
        {
            _content = UnityWebRequest.Get(url);
            if (downloadedBytes > 0)
            {
#if UNITY_2019_1_OR_NEWER
                _content.SetRequestHeader("Range", $"bytes={downloadedBytes}-");
                _content.downloadHandler = new DownloadHandlerFile(savePath, true);
#else
                downloadedBytes = 0;
                _content.downloadHandler = new DownloadHandlerFile(savePath);
#endif
            }
            else
            {
                _content.downloadHandler = new DownloadHandlerFile(savePath);
            }

            _content.certificateHandler = new DownloadCertificateHandler();
            _content.disposeDownloadHandlerOnDispose = true;
            _content.disposeCertificateHandlerOnDispose = true;
            _content.disposeUploadHandlerOnDispose = true;
            _content.SendWebRequest();
            _lastRequestDownloadedBytes = 0;
            _step = Step.GetContent;
            BeganSample();
        }

        private void Dispose()
        {
            if (_header != null)
            {
                _header.Dispose();
                _header = null;
            }

            if (_content == null) return;
            _content.Dispose();
            _content = null;
        }

        private enum Step
        {
            GetHeader,
            GetContent,
            Ended
        }
    }
}