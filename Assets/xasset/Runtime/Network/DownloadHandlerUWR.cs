using System.IO;
using UnityEngine.Networking;

namespace xasset
{
    public class DownloadCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public struct DownloadHandlerUWR : IDownloadHandler
    {
        private UnityWebRequest _content;
        private UnityWebRequest _header;
        private ulong _lastRequestDownloadedBytes;
        private Step _step;
        private readonly DownloadRequest _request;

        public DownloadHandlerUWR(DownloadRequest request)
        {
            _header = null;
            _content = null;
            _lastRequestDownloadedBytes = 0;
            _step = Step.GetHeader;
            _request = request;
        }

        public void OnStart()
        {
            _step = Step.GetHeader;
            StartDownload();
        }

        public void OnPause(bool paused)
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

        public bool Update()
        {
            if (_request.isDone) return false;
            if (_request.status == DownloadRequestBase.Status.Paused) return true;
            if (_request.status != DownloadRequestBase.Status.Progressing) return false;
            if (_step == Step.GetHeader) return UpdateHeaderRequest();
            return _step == Step.GetContent && UpdateContentRequest();
        }

        public void OnCancel()
        {
            Dispose();
        }

        private void StartDownload()
        {
            if (Downloader.SimulationMode)
            {
                // 本地仿真的时候不支持断点续传。
                _request.downloadedBytes = 0;
                var path = _request.url.Replace(Assets.Protocol, string.Empty);
                var file = new FileInfo(path);
                if (file.Exists) _request.OnGetDownloadSize((ulong) file.Length);
                GetContentRequest();
            }
            else
            {
                GetHeadRequest();
            }

            _request.status = DownloadRequestBase.Status.Progressing;
        }

        private void GetHeadRequest()
        {
            _header = UnityWebRequest.Head(_request.url);
            _header.SendWebRequest();
            _step = Step.GetHeader;
        }

        private bool UpdateHeaderRequest()
        {
            if (!_header.isDone) return true;
            if (!string.IsNullOrEmpty(_header.error))
            {
                _request.SetResult(DownloadRequestBase.Result.Failed, _header.error);
                return false;
            }

            const string key = "Content-Length";
            var value = _header.GetResponseHeader(key);
            if (ulong.TryParse(value, out var size))
            {
                _request.OnGetDownloadSize(size);
                if (size == _request.downloadedBytes)
                {
                    _request.SetResult(DownloadRequestBase.Result.Success, DownloadErrors.NothingToDownload);
                    return false;
                }

                GetContentRequest();
                return true;
            }

            _request.SetResult(DownloadRequestBase.Result.Success, DownloadErrors.NothingToDownload);
            return false;
        }

        private bool UpdateContentRequest()
        {
            var deltaBytes = _content.downloadedBytes - _lastRequestDownloadedBytes;
            _request.OnReceiveBytes(deltaBytes);
            _lastRequestDownloadedBytes = _content.downloadedBytes;
            if (!_content.isDone) return true;
            _request.downloadedBytes = _request.content.GetDownloadedBytes();
            _request.error = _content.error;
            _request.VerifyContent();
            _step = Step.Ended;
            Dispose();
            return false;
        }

        private void GetContentRequest()
        {
            _content = UnityWebRequest.Get(_request.url);
            if (_request.downloadedBytes > 0)
            {
#if UNITY_2019_1_OR_NEWER
                _content.SetRequestHeader("Range", $"bytes={_request.downloadedBytes}-");
                _content.downloadHandler = new DownloadHandlerFile(_request.savePath, true);
#else
                _request.downloadedBytes = 0;
                _content.downloadHandler = new DownloadHandlerFile(_request.savePath);
#endif
            }
            else
            {
                _content.downloadHandler = new DownloadHandlerFile(_request.savePath);
            }

            _content.certificateHandler = new DownloadCertificateHandler();
            _content.disposeDownloadHandlerOnDispose = true;
            _content.disposeCertificateHandlerOnDispose = true;
            _content.disposeUploadHandlerOnDispose = true;
            _content.SendWebRequest();
            _lastRequestDownloadedBytes = 0;
            _step = Step.GetContent;
            _request.BeganSample();
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