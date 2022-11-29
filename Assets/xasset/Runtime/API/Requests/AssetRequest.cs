using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace xasset
{
    public class AssetRequest : LoadRequest
    {
        private static readonly Queue<AssetRequest> Unused = new Queue<AssetRequest>();
        private static readonly Dictionary<string, AssetRequest> Loaded = new Dictionary<string, AssetRequest>();
        public IAssetHandler handler { get; } = CreateHandler();

        public Object asset { get; set; }
        public Object[] assets { get; set; }
        public bool isAll { get; private set; }
        public ManifestAsset info { get; private set; }
        public Type type { get; private set; }

        public static Func<IAssetHandler> CreateHandler { get; set; } = RuntimeAssetHandler.CreateInstance;

        protected override void OnStart()
        {
            handler.OnStart(this);
            References.Retain(path);
        }

        protected override void OnWaitForCompletion()
        {
            handler.WaitForCompletion(this);
        }

        protected override void OnUpdated()
        {
            handler.Update(this);
        }


        protected override void OnDispose()
        {
            Remove(this);
            handler.Dispose(this);

            if (References.Release(path) <= 0)
            {
                if (isAll)
                {
                    if (assets != null)
                    {
                        foreach (var o in assets)
                        {
                            if (!(o is GameObject))
                            {
                                Recycler.UnloadAsset(o);
                            }
                        }
                    }
                }
                else
                {
                    if (asset != null && !(asset is GameObject))
                    {
                        Recycler.UnloadAsset(asset);
                    }
                }
            }

            asset = null;
            assets = null;
            isAll = false;
        }

        private static void Remove(AssetRequest request)
        {
            Loaded.Remove($"{request.path}[{request.type.Name}]");
            Unused.Enqueue(request);
        }

        internal static T Get<T>(string path) where T : Object
        {
            if (!Assets.TryGetAsset(ref path, out _)) return null;
            if (!Loaded.TryGetValue($"{path}[{typeof(T).Name}]", out var request)) return null;
            return request.asset as T;
        }

        internal static T[] GetAll<T>(string path) where T : Object
        {
            if (!Assets.TryGetAsset(ref path, out _)) return null;
            if (!Loaded.TryGetValue($"{path}[{typeof(T).Name}]", out var request)) return null;
            return request.assets as T[];
        }

        internal static AssetRequest Load(string path, Type type, bool isAll = false)
        {
            if (!Assets.TryGetAsset(ref path, out var info))
            {
                Logger.E($"File not found:{path}");
                return null;
            }

            // TODO: 如果不用 SubAssets 可以把 key 改成 path 减少 GC。
            var key = $"{path}[{type.Name}]";
            if (!Loaded.TryGetValue(key, out var request))
            {
                request = Unused.Count > 0 ? Unused.Dequeue() : new AssetRequest();
                request.Reset();
                request.type = type;
                request.info = info;
                request.isAll = isAll;
                request.path = path;
                Loaded[key] = request;
            }

            request.LoadAsync();
            return request;
        }
    }
}