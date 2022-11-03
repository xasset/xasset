using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset
{
    public class Manifest : ScriptableObject, ISerializationCallbackReceiver
    {
        public string build;
        public string[] dirs = Array.Empty<string>();
        public ManifestAsset[] assets = Array.Empty<ManifestAsset>();
        public ManifestBundle[] bundles = Array.Empty<ManifestBundle>();
        private readonly Dictionary<string, List<int>> directoryWithAssets = new Dictionary<string, List<int>>();
        private readonly Dictionary<string, ManifestAsset> nameWithAssets = new Dictionary<string, ManifestAsset>();

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

            foreach (var bundle in bundles)
            {
                var extension = Path.GetExtension(bundle.name);
                var nameWithAppendHash = string.IsNullOrEmpty(extension)
                    ? $"{bundle.name}_{bundle.hash}"
                    : $"{bundle.name.Replace(extension, string.Empty)}_{bundle.hash}{extension}";
                bundle.nameWithAppendHash = nameWithAppendHash;
            }

            foreach (var asset in assets)
            {
                var dir = dirs[asset.dir];
                var path = $"{dir}/{asset.name}";
                asset.path = path;
                asset.manifest = this;
                AddAsset(asset);
                if (directoryWithAssets.TryGetValue(dir, out var value)) value.Add(asset.id);
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
            nameWithAssets[asset.path] = asset;
            // 场景和预设默认生成短链接
            if (asset.auto) return;
            if (!asset.path.EndsWith(".unity") && !asset.path.EndsWith(".prefab")) return;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asset.path);
            if (nameWithAssets.TryGetValue(fileNameWithoutExtension, out var value))
                Logger.W($"{fileNameWithoutExtension} already exist {value.path}");
            else
                nameWithAssets[fileNameWithoutExtension] = asset;
        }

        public bool IsDirectory(string path)
        {
            return directoryWithAssets.ContainsKey(path);
        }

        public bool ContainsAsset(string path)
        {
            return nameWithAssets.ContainsKey(path);
        }

        public bool TryGetAsset(string path, out ManifestAsset asset)
        {
            return nameWithAssets.TryGetValue(path, out asset);
        }

        public ManifestBundle GetBundle(string assetPath)
        {
            return TryGetAsset(assetPath, out var value) ? bundles[value.bundle] : null;
        }

        public ManifestBundle[] GetDependencies(ManifestBundle bundle)
        {
            return bundle.deps == null
                ? Array.Empty<ManifestBundle>()
                : Array.ConvertAll(bundle.deps, input => bundles[input]);
        }
    }
}