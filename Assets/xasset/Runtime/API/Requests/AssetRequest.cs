using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace xasset
{
    public class AssetRequest : LoadRequest, IReloadable
    {
        private static readonly Queue<AssetRequest> Unused = new Queue<AssetRequest>();
        internal static readonly Dictionary<string, AssetRequest> Loaded = new Dictionary<string, AssetRequest>();

        // ReSharper disable once MemberCanBePrivate.Global
        public IAssetHandler handler { get; } = CreateHandler();

        public Object asset { get; set; }
        public Object[] assets { get; set; }
        public bool isAll { get; private set; }
        public ManifestAsset info { get; internal set; }
        public Type type { get; private set; }
        public override int priority => 1;

        public static Func<IAssetHandler> CreateHandler { get; set; } = RuntimeAssetHandler.CreateInstance;

        // ReSharper disable once MemberCanBePrivate.Global
        public Action reloaded { get; set; }

        private bool reloadNow;
        
        public void ReloadAsync()
        {
            reloadNow = true; 
        }

        public void OnReloaded()
        {
            reloaded?.Invoke();
            reloaded = null;
        }

        public bool IsReloaded()
        {
            if (reloadNow)
            {
                status = Status.Processing;
                handler.OnReload(this);
                reloadNow = false;
            }
            OnUpdated();
            return isDone;
        }

        protected override void OnStart()
        {
            handler.OnStart(this);
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
            if (ReferencesCounter.Release(path) <= 0)
            {
                if (isAll)
                {
                    if (assets != null)
                        foreach (var o in assets)
                            if (!(o is GameObject))
                                Recycler.UnloadAsset(o);
                }
                else
                {
                    if (asset != null && !(asset is GameObject)) Recycler.UnloadAsset(asset);
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
            if (!Assets.TryGetAsset(ref path, out _)) 
            {
                Logger.E($"File not found {path}.");    
                return null;
            }
            if (!Loaded.TryGetValue($"{path}[{typeof(T).Name}]", out var request)) return null;
            return request.asset as T;
        }

        internal static T[] GetAll<T>(string path) where T : Object
        {
            if (!Assets.TryGetAsset(ref path, out _)) 
            {
                Logger.E($"File not found {path}.");    
                return null;
            }
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

            if (request.refCount == 1)
                ReferencesCounter.Retain(path);

            request.LoadAsync();
            return request;
        }
    }
}