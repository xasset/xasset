using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace xasset
{
    public class AssetRequest : LoadRequest
    {
        private static readonly Queue<AssetRequest> Unused = new Queue<AssetRequest>();
        private static readonly Dictionary<string, AssetRequest> Loaded = new Dictionary<string, AssetRequest>();
        private AssetRequestHandler handler { get; set; }

        public Object asset { get; set; }
        public Object[] assets { get; set; }
        public bool isAll { get; private set; }
        public ManifestAsset info { get; private set; }
        public Type type { get; private set; }

        public static Func<AssetRequest, AssetRequestHandler> CreateHandler { get; set; } = AssetRequestHandlerRuntime.CreateInstance;

        protected override void OnStart()
        {
            handler.OnStart();
        }

        protected override void OnWaitForCompletion()
        {
            handler.WaitForCompletion();
        }

        protected override void OnUpdated()
        {
            handler.Update();
        }

        protected override void OnDispose()
        {
            Remove(this);
            handler.Dispose();
            asset = null;
            assets = null;
            isAll = false;
        }

        private static void Remove(AssetRequest request)
        {
            Loaded.Remove($"{request.info.path}[{request.type.Name}]");
            Unused.Enqueue(request);
        }

        internal static T Get<T>(string path) where T : Object
        {
            if (!Assets.Versions.TryGetAsset(path, out var info)) return null;
            if (!Loaded.TryGetValue($"{info.path}[{typeof(T).Name}]", out var request)) return null;
            return request.asset as T;
        }

        internal static T[] GetAll<T>(string path) where T : Object
        {
            if (!Assets.Versions.TryGetAsset(path, out var info)) return null;
            if (!Loaded.TryGetValue($"{info.path}[{typeof(T).Name}]", out var request)) return null;
            return request.assets as T[];
        }

        internal static AssetRequest Load(string path, Type type, bool isAll = false)
        {
            if (!Assets.Versions.TryGetAsset(path, out var info))
            {
                Logger.E($"File not found:{path}");
                return null;
            }

            var key = $"{info.path}[{type.Name}]";
            if (!Loaded.TryGetValue(key, out var request))
            {
                request = Unused.Count > 0 ? Unused.Dequeue() : new AssetRequest();
                request.Reset();
                request.type = type;
                request.info = info;
                request.isAll = isAll;
                request.path = info.path;
                request.handler = CreateHandler(request);
                Loaded[key] = request;
            }

            request.LoadAsync();
            return request;
        }
    }
}