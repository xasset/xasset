using System;
using UnityEngine;

namespace VEngine
{
    public class BundledAsset : Asset
    {
        private Dependencies _dependencies;
        private AssetBundleRequest _request;

        internal static BundledAsset Create(string path, Type type)
        {
            return new BundledAsset
            {
                pathOrURL = path,
                type = type
            };
        }

        protected override void OnLoad()
        {
            _dependencies = Dependencies.Load(pathOrURL, mustCompleteOnNextFrame);
            status = LoadableStatus.DependentLoading;
        }

        protected override void OnUnload()
        {
            if (_dependencies != null)
            {
                _dependencies.Release();
                _dependencies = null;
            }

            _request = null;
            asset = null;

            base.OnUnload();
        }

        public override void LoadImmediate()
        {
            if (isDone) return;

            if (_dependencies == null)
            {
                Finish("dependencies == null");
                return;
            }

            if (!_dependencies.isDone) _dependencies.LoadImmediate();

            if (_dependencies.assetBundle == null)
            {
                Finish("dependencies.assetBundle == null");
                return;
            }

            OnLoaded(_dependencies.assetBundle.LoadAsset(pathOrURL, type));
        }

        protected override void OnUpdate()
        {
            if (status == LoadableStatus.Loading)
                UpdateLoading();
            else if (status == LoadableStatus.DependentLoading)
                UpdateDependencies();
        }

        private void UpdateLoading()
        {
            if (_request == null)
            {
                Finish("request == null");
                return;
            }

            progress = 0.5f + _request.progress * 0.5f;
            if (!_request.isDone) return;

            OnLoaded(_request.asset);
        }

        private void UpdateDependencies()
        {
            if (_dependencies == null)
            {
                Finish("dependencies == null");
                return;
            }

            progress = 0.5f * _dependencies.progress;
            if (!_dependencies.isDone) return;

            var assetBundle = _dependencies.assetBundle;
            if (assetBundle == null)
            {
                Finish("assetBundle == null");
                return;
            }

            _request = assetBundle.LoadAssetAsync(pathOrURL, type);
            status = LoadableStatus.Loading;
        }
    }
}