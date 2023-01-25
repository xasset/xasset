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
        ///     按 不带扩展名的文件名 加载。
        /// </summary>
        LoadByNameWithoutExtension,

        /// <summary>
        ///     按 依赖 加载。
        /// </summary>
        LoadByDependencies,
    }

    [Serializable]
    public class ManifestAsset
    {
        public int id { get; set; }
        public string name;
        public int bundle;
        public int dir;
        public int[] deps = Array.Empty<int>();
        public AddressMode addressMode = AddressMode.LoadByPath;
        public string path { get; set; }
        public Manifest manifest { get; set; }
    }

    [Serializable]
    public class ManifestBundle : Downloadable
    {
        public int[] deps = Array.Empty<int>();
        public Manifest manifest { get; set; }
    }

    public class Manifest : ScriptableObject, ISerializationCallbackReceiver
    {
        public string extension;
        public bool saveBundleName;
        public string build;
        public string[] dirs = Array.Empty<string>();
        public ManifestAsset[] assets = Array.Empty<ManifestAsset>();
        public ManifestBundle[] bundles = Array.Empty<ManifestBundle>();

        private readonly Dictionary<string, List<int>> directoryWithAssets = new Dictionary<string, List<int>>();
        private readonly Dictionary<string, ManifestAsset> addressWithAssets = new Dictionary<string, ManifestAsset>();
        private readonly Dictionary<string, ManifestBundle[]> nameWithDependencies = new Dictionary<string, ManifestBundle[]>();

        public void Clear()
        {
            dirs = Array.Empty<string>();

            bundles = Array.Empty<ManifestBundle>();
            assets = Array.Empty<ManifestAsset>();

            addressWithAssets.Clear();
            directoryWithAssets.Clear();
        }

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

            if (saveBundleName)
            {
                foreach (var bundle in bundles)
                {
                    var key = string.IsNullOrEmpty(extension)
                        ? $"{bundle.name}_{bundle.hash}"
                        : $"{bundle.name.Replace(extension, string.Empty)}_{bundle.hash}{extension}";
                    bundle.file = key;
                    bundle.manifest = this;
                }
            }
            else
            {
                foreach (var bundle in bundles)
                {
                    var key = string.IsNullOrEmpty(extension)
                        ? $"{bundle.hash}"
                        : $"{bundle.hash}{extension}";
                    bundle.file = key;
                    bundle.manifest = this;
                }
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
            switch (asset.addressMode)
            {
                case AddressMode.LoadByName:
                    addressWithAssets[asset.path] = asset;
                    SetAddress(asset, Path.GetFileName(asset.path));
                    break;
                case AddressMode.LoadByDependencies:
                    break;
                case AddressMode.LoadByNameWithoutExtension:
                    addressWithAssets[asset.path] = asset;
                    SetAddress(asset, Path.GetFileNameWithoutExtension(asset.path));
                    break;
                case AddressMode.LoadByPath:
                    addressWithAssets[asset.path] = asset;
                    break;
            }
        }

        private void SetAddress(ManifestAsset asset, string address)
        {
            if (addressWithAssets.TryGetValue(address, out var value))
            {
                if (value.path != asset.path)
                {
                    Logger.W($"{address} already exist {value.path}");
                }
            }
            else
                addressWithAssets[address] = asset;
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

        public ManifestBundle GetBundle(string assetPath)
        {
            return TryGetAsset(assetPath, out var value) ? bundles[value.bundle] : null;
        }

        public ManifestBundle[] GetDependencies(ManifestBundle bundle)
        {
            if (nameWithDependencies.TryGetValue(bundle.name, out var value)) return value;
            value = bundle.deps == null ? Array.Empty<ManifestBundle>() : Array.ConvertAll(bundle.deps, input => bundles[input]);
            nameWithDependencies[bundle.name] = value;
            return value;
        }
    }
}