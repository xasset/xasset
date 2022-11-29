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
            if (Assets.SimulationMode || Assets.OfflineMode)
            {
                SetResult(Result.Failed);
                return;
            }

            _request = UnityWebRequest.Get(Assets.UpdateURL);
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

                // WebGL 下运行时读取 Streaming Assets 目录的值作为下载地址
                if (Assets.IsWebGLPlatform && !Application.isEditor)
                    info.downloadURL = Assets.PlayerDataPath;

                if (!Downloader.SimulationMode)
                    Assets.DownloadURL = info.downloadURL;

                SetResult(Result.Success);
                return;
            }

            SetResult(Result.Failed, _request.error);
        }

        protected override void OnCompleted()
        {
        }
    }
}