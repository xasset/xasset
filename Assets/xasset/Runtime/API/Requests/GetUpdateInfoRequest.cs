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

                // Web GL 直接读取 PlayerDataPath
                if (Assets.IsWebGLPlatform && !Application.isEditor)
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