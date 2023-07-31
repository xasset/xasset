using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset
{
    /// <summary>
    ///     寻址模式
    /// </summary>
    public enum AddressMode
    {
        /// <summary>
        ///     按 Assets 路径加载，始终有效。
        /// </summary>
        LoadByPath,

        /// <summary>
        ///     按 文件名 加载。
        /// </summary>
        LoadByName,

        /// <summary>
        ///     按 不带扩张名的文件名 加载。
        /// </summary>
        LoadByNameWithoutExtension,

        /// <summary>
        ///     按 依赖 加载，不主动生成加载地址。
        /// </summary>
        LoadByDependencies
    }

    /// <summary>
    ///     交付模式
    /// </summary>
    public enum DeliveryMode
    {
        InstallTime, // 在安装包中
        FastFollow, // 不在安装包中，启动后立即下载
        OnDemand // 按需更新，点更新才下载
    }
    
    [Serializable]
    public class ManifestAsset
    {
        public string name;
        public int[] deps = Array.Empty<int>();
        public int bundle;
        public int dir;
        public AddressMode addressMode = AddressMode.LoadByPath;
        public int id { get; set; }
        public string path { get; set; }
        public Manifest manifest { get; set; }
    }

    [Serializable]
    public class ManifestBundle : Downloadable
    {
        public int[] deps = Array.Empty<int>();
        public Manifest manifest { get; set; }
    } 
    
    [Serializable]
    public class ManifestGroup
    {
        public DeliveryMode deliveryMode = DeliveryMode.OnDemand;
        public List<int> assets = new List<int>();
        private Dictionary<string, int> _assets = new Dictionary<string, int>();
        public string desc;
        public string name;
        public Manifest manifest { get; set; }

        public void OnDeserialize()
        {
            var bundles = manifest.bundles;
            for (var index = 0; index < assets.Count; index++)
            {
                var asset = assets[index];
                var id = asset;
                if (id >= 0 && id < bundles.Length)
                {
                    _assets[bundles[id].name] = id;
                }
                else
                {
                    assets.RemoveAt(index);
                    index--;
                    Logger.W($"Asset {id} not exist in {manifest.name}.");
                }
            }
        }

        public bool TryGetAsset(string asset, out int result)
        {
            return _assets.TryGetValue(asset, out result);
        }

        public bool Contains(string asset)
        {
            return _assets.ContainsKey(asset);
        } 
    }

    public class Manifest : ScriptableObject, ISerializationCallbackReceiver
    {
        internal static Action<ManifestAsset> OnReadAsset;
        public string[] dirs = Array.Empty<string>();
        public ManifestAsset[] assets = Array.Empty<ManifestAsset>();
        public ManifestBundle[] bundles = Array.Empty<ManifestBundle>();
        public ManifestGroup[] groups = Array.Empty<ManifestGroup>();
        private readonly Dictionary<string, ManifestAsset> addressWithAssets = new Dictionary<string, ManifestAsset>();
        private readonly Dictionary<string, List<int>> directoryWithAssets = new Dictionary<string, List<int>>();
        private readonly Dictionary<string, ManifestBundle> nameWithBundles = new Dictionary<string, ManifestBundle>();

        private readonly Dictionary<string, ManifestBundle[]> nameWithDependencies =
            new Dictionary<string, ManifestBundle[]>();

        private readonly Dictionary<string, ManifestGroup> nameWithGroups = new Dictionary<string, ManifestGroup>();
        public long time;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            foreach (var item in dirs)
            {
                var dir = item;
                if (!directoryWithAssets.TryGetValue(dir, out _)) directoryWithAssets.Add(dir, new List<int>());

                int pos;
                while ((pos = dir.LastIndexOf('/')) != -1)
                {
                    dir = dir.Substring(0, pos);
                    if (!directoryWithAssets.TryGetValue(dir, out _)) directoryWithAssets.Add(dir, new List<int>());
                }
            }

            foreach (var group in groups)
            {
                group.manifest = this;
                group.OnDeserialize();
                nameWithGroups[group.name] = group;
            }

            foreach (var bundle in bundles)
            {
                if (string.IsNullOrEmpty(bundle.name))
                    continue;
                var extension = Path.GetExtension(bundle.name);
                var key = string.IsNullOrEmpty(extension)
                    ? $"{bundle.hash}"
                    : $"{bundle.hash}{extension}";
                bundle.file = key;
                bundle.manifest = this;
                nameWithBundles[bundle.name] = bundle;
            }

            for (var index = 0; index < assets.Length; index++)
            {
                var asset = assets[index];
                var dir = dirs[asset.dir];
                var path = $"{dir}/{asset.name}";
                asset.path = path;
                asset.manifest = this;
                AddAsset(asset);
                if (directoryWithAssets.TryGetValue(dir, out var value)) value.Add(index);
            }
        }

        public void Clear()
        {
            dirs = Array.Empty<string>();

            groups = Array.Empty<ManifestGroup>();
            bundles = Array.Empty<ManifestBundle>();
            assets = Array.Empty<ManifestAsset>();

            nameWithGroups.Clear();
            addressWithAssets.Clear();
            directoryWithAssets.Clear();
            nameWithDependencies.Clear();
            nameWithBundles.Clear();
        }

        public int[] GetAssets(string dir, bool recursion)
        {
            if (!recursion)
                return directoryWithAssets.TryGetValue(dir, out var value)
                    ? value.ToArray()
                    : Array.Empty<int>();

            var keys = new List<string>();
            foreach (var item in directoryWithAssets.Keys)
                if (item.StartsWith(dir)
                    && (item.Length == dir.Length || (item.Length > dir.Length && item[dir.Length] == '/')))
                    keys.Add(item);

            if (keys.Count <= 0) return Array.Empty<int>();

            var get = new List<int>();
            foreach (var item in keys) get.AddRange(GetAssets(item, false));

            return get.ToArray();
        }

        private void AddAsset(ManifestAsset asset)
        {
            if (asset.addressMode == AddressMode.LoadByDependencies) return;
            addressWithAssets[asset.path] = asset;
            OnReadAsset?.Invoke(asset);
        }

        public bool IsDirectory(string path)
        {
            return directoryWithAssets.ContainsKey(path);
        }

        public bool ContainsAsset(string path)
        {
            return addressWithAssets.ContainsKey(path);
        }

        public bool TryGetAsset(string path, out ManifestAsset asset)
        {
            return addressWithAssets.TryGetValue(path, out asset);
        }

        public ManifestBundle GetBundleWithAsset(string assetPath)
        {
            return TryGetAsset(assetPath, out var value) ? bundles[value.bundle] : null;
        }

        public ManifestBundle GetBundle(string assetPath)
        {
            return nameWithBundles.TryGetValue(assetPath, out var bundle) ? bundle : null;
        }

        public ManifestBundle[] GetDependencies(ManifestBundle bundle)
        {
            if (nameWithDependencies.TryGetValue(bundle.name, out var value)) return value;
            value = bundle.deps == null
                ? Array.Empty<ManifestBundle>()
                : Array.ConvertAll(bundle.deps, input => bundles[input]);
            nameWithDependencies[bundle.name] = value;
            return value;
        }

        public bool TryGetGroups(string group, out ManifestGroup result)
        {
            return nameWithGroups.TryGetValue(group, out result);
        }
    }
}