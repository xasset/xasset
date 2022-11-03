using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public class InstantiateRequest : LoadRequest
    {
        private static readonly Queue<InstantiateRequest> Unused = new Queue<InstantiateRequest>();

        private AssetRequest _assetRequest;
        public Transform parent { get; set; }
        public GameObject gameObject { get; private set; }
        public bool worldPositionStays { get; set; }

        protected override void OnStart()
        {
            _assetRequest = Asset.LoadAsync(path, typeof(GameObject));
        }

        protected override void OnUpdated()
        {
            if (_assetRequest == null)
            {
                SetResult(Result.Failed, "_assetRequest == null");
                return;
            }

            progress = _assetRequest.progress;
            if (!_assetRequest.isDone) return;

            if (_assetRequest.result == Result.Failed)
            {
                SetResult(Result.Failed, _assetRequest.error);
                return;
            }

            var prefab = _assetRequest.asset;

            if (prefab == null)
            {
                SetResult(Result.Failed, "gameObject == null");
                _assetRequest.Release();
                _assetRequest = null;
            }
            else
            {
                gameObject = Object.Instantiate(prefab, parent, worldPositionStays) as GameObject;

                if (gameObject == null)
                    SetResult(Result.Failed, "gameObject == null");
                else
                    SetResult(Result.Success);

                AutoreleaseCache.Get(gameObject).Add(this);
            }
        }

        protected override void OnDispose()
        {
            _assetRequest?.Release();
            _assetRequest = null;
            AutoreleaseCache.Get(gameObject).Remove(this);
            if (gameObject == null) return;
            Object.DestroyImmediate(gameObject);
            gameObject = null;
            Unused.Enqueue(this);
            Reset();
        }

        internal static InstantiateRequest InstantiateAsync(string path, Transform parent = null,
            bool worldPositionStays = false)
        {
            if (!Assets.Versions.TryGetAsset(path, out var info)) return null;
            var request = Unused.Count > 0 ? Unused.Dequeue() : new InstantiateRequest();
            request.path = info.path;
            request.parent = parent;
            request.worldPositionStays = worldPositionStays;
            request.LoadAsync();
            return request;
        }
    }
}