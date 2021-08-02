using System;
using UnityEngine;

namespace Versions
{
    public class BundledAsset : Asset
    {
        private Dependencies dependencies;
        private AssetBundleRequest request;

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
            dependencies = new Dependencies
            {
                pathOrURL = pathOrURL
            };
            dependencies.Load();
            status = LoadableStatus.DependentLoading;
        }

        protected override void OnUnload()
        {
            if (dependencies != null)
            {
                dependencies.Unload();
                dependencies = null;
            }

            request = null;
            asset = null;
        }

        protected override void OnUpdate()
        {
            if (status == LoadableStatus.Loading)
                UpdateLoading();
            else if (status == LoadableStatus.DependentLoading) UpdateDependencies();
        }

        private void UpdateLoading()
        {
            if (request == null)
            {
                Finish("request == null");
                return;
            }

            progress = 0.5f + request.progress * 0.5f;
            if (!request.isDone) return;

            asset = request.asset;
            if (asset == null)
            {
                Finish("asset == null");
                return;
            }

            Finish();
        }

        private void UpdateDependencies()
        {
            if (dependencies == null)
            {
                Finish("dependencies == null");
                return;
            }

            progress = 0.5f * dependencies.progress;
            if (!dependencies.isDone) return;

            var assetBundle = dependencies.assetBundle;
            if (assetBundle == null)
            {
                Finish("assetBundle == null");
                return;
            }

            request = assetBundle.LoadAssetAsync(pathOrURL, type);
            status = LoadableStatus.Loading;
        }
    }
}