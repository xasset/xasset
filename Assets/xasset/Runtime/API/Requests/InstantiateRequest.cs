using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public class InstantiateRequest : LoadRequest, IAutorelease
    {
        private static readonly Queue<InstantiateRequest> Unused = new Queue<InstantiateRequest>();

        private AssetRequest _asset;
        public override int priority => 2;
        public Transform parent { get; set; }
        public GameObject gameObject { get; private set; }
        public bool worldPositionStays { get; set; }

        public bool CanRelease()
        {
            return isDone && gameObject == null;
        }

        protected override void OnStart()
        {
            _asset = Asset.LoadAsync(path, typeof(GameObject));
        }

        protected override void OnCompleted()
        {
            Recycler.Autorelease(this);
        }

        protected override void OnUpdated()
        {
            if (_asset == null)
            {
                SetResult(Result.Failed, "_assetRequest == null");
                return;
            }

            progress = _asset.progress;
            if (!_asset.isDone) return;

            if (_asset.result == Result.Failed)
            {
                SetResult(Result.Failed, _asset.error);
                gameObject = null;
                return;
            }

            var prefab = _asset.asset;
            if (prefab == null)
            {
                SetResult(Result.Failed, "gameObject == null");
                gameObject = null;
            }
            else
            {
                gameObject = Object.Instantiate(prefab, parent, worldPositionStays) as GameObject;
                if (gameObject == null)
                    SetResult(Result.Failed, "gameObject == null");
                else
                    SetResult(Result.Success);
            }
        }

        protected override void OnWaitForCompletion()
        {
            if (!_asset.isDone) _asset.WaitForCompletion();

            while (!isDone) OnUpdated();
        }

        protected override void OnDispose()
        {
            _asset?.Release();
            _asset = null;
            if (gameObject != null)
            {
                Object.DestroyImmediate(gameObject);
                gameObject = null;
            }

            Unused.Enqueue(this);
        }

        internal static InstantiateRequest InstantiateAsync(string path, Transform parent = null,
            bool worldPositionStays = false)
        {
            if (!Assets.TryGetAsset(ref path, out _))
            {
                Logger.E($"File not found {path}.");    
                return null;
            }
            var request = Unused.Count > 0 ? Unused.Dequeue() : new InstantiateRequest();
            request.Reset();
            request.path = path;
            request.parent = parent;
            request.worldPositionStays = worldPositionStays;
            request.LoadAsync();
            return request;
        }
    }
}