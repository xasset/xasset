using UnityEngine;
using UnityEngine.Networking;

namespace xasset
{
    public sealed class GetUpdateInfoRequest : Request
    {
        private UnityWebRequest _request;
        public UpdateInfo info { get; private set; }

        protected override void OnStart()
        {
            if (!Assets.Updatable)
            {
                SetResult(Result.Failed);
                return;
            }

            _request = UnityWebRequest.Get(Assets.UpdateInfoURL);
            _request.certificateHandler = new DownloadCertificateHandler();
            _request.SendWebRequest();
        }

        protected override void OnUpdated()
        {
            progress = _request.downloadProgress;
            if (!_request.isDone)
                return;

            if (string.IsNullOrEmpty(_request.error))
            {
                info = Utility.LoadFromJson<UpdateInfo>(_request.downloadHandler.text);
                // Web GL 直接读取 Player Data Path
                if (Application.isEditor || !Assets.IsWebGLPlatform)
                    Assets.DownloadURL = info.downloadURL; // 更新下载地址
                // 版本文件未发生更新
                if (info.timestamp <= Assets.Versions.timestamp)
                {
                    SetResult(Result.Failed, "Nothing to update.");
                    return;
                }

                SetResult(Result.Success);
                return;
            }

            SetResult(Result.Failed, _request.error);
        }

        protected override void OnCompleted()
        {
            _request?.Dispose();
            _request = null;
        }

        public VersionsRequest GetVersionsAsync()
        {
            var request = new VersionsRequest
                { url = Assets.GetDownloadURL(info.file), hash = info.hash, size = info.size };
            request.SendRequest();
            return request;
        }
    }
}