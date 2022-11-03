using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace xasset.example
{
    public class PreloadAsset : Loadable
    {
        private static readonly Dictionary<string, AssetRequest> Assets = new Dictionary<string, AssetRequest>();
        public bool all { get; set; }
        public Type type { get; set; }

        private LoadRequest _request { get; set; }

        public override bool isDone => _request == null || _request.isDone;

        protected override void OnLoad()
        {
            _request = LoadAsync(path, type, all);
        }

        public static void ClearAllAssets()
        {
            foreach (var pair in Assets) pair.Value.Release();
            Assets.Clear();
        }

        private static AssetRequest LoadAsync(string path, Type type, bool isAll = false)
        {
            if (Assets.TryGetValue(path, out var asset)) return asset;
            asset = isAll ? Asset.LoadAllAsync(path, type) : Asset.LoadAsync(path, type);
            Assets[path] = asset;
            return asset;
        }

        public static T GetAsset<T>(string path) where T : Object
        {
            return (T) (Assets.TryGetValue(path, out var value) ? value.asset : null);
        }
    }
}