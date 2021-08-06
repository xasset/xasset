using System;
using UnityEngine;

namespace VEngine
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

        public override void LoadImmediate()
        {
            if (isDone) return;

            if (dependencies == null)
            {
                Finish("dependencies == null");
                return;
            }

            if (!dependencies.isDone) dependencies.LoadImmediate();

            if (dependencies.assetBundle == null)
            {
                Finish("dependencies.assetBundle == null");
                return;
            }

            asset = dependencies.assetBundle.LoadAsset(pathOrURL, type);
            if (asset == null)
            {
                Finish("target == null");
                return;
            }

            Finish();
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