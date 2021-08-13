using UnityEngine;

namespace VEngine
{
    internal class LocalBundle : Bundle
    {
        private AssetBundleCreateRequest request;

        protected override void OnLoad()
        {
            request = AssetBundle.LoadFromFileAsync(pathOrURL);
        }

        public override void LoadImmediate()
        {
            if (isDone) return;
            OnLoaded(request.assetBundle);
            request = null;
        }

        protected override void OnUpdate()
        {
            if (status != LoadableStatus.Loading) return;
            progress = request.progress;
            if (request.isDone) OnLoaded(request.assetBundle);
        }
    }
}